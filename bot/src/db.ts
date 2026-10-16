import Database from "better-sqlite3";
import { DB_PATH } from "./config.ts";

export interface Client {
  id: number;
  username: string | null;
  first_name: string;
  banned: 0 | 1;
  created_at: string;
}

export interface TelegramUser {
  id: number;
  username?: string;
  first_name: string;
}

export interface Stats {
  clients: number;
  messages: number;
  messagesToday: number;
}

interface CountRow {
  count: number;
}

const db = new Database(DB_PATH);
db.pragma("foreign_keys = ON");

// id — Telegram ID клієнта
db.prepare(`
  CREATE TABLE IF NOT EXISTS clients (
    id         INTEGER PRIMARY KEY,
    username   TEXT,
    first_name TEXT NOT NULL,
    banned     INTEGER NOT NULL CHECK (banned IN (0, 1)),
    created_at TEXT NOT NULL
  )
`).run();

// id — message_id пересланого повідомлення в чаті оператора, за ним reply оператора знаходить клієнта
db.prepare(`
  CREATE TABLE IF NOT EXISTS messages (
    id         INTEGER PRIMARY KEY,
    client_id  INTEGER NOT NULL REFERENCES clients (id),
    created_at TEXT NOT NULL
  )
`).run();

const upsertClientStmt = db.prepare<[number, string | null, string, string]>(`
  INSERT INTO clients (id, username, first_name, banned, created_at) VALUES (?, ?, ?, 0, ?)
  ON CONFLICT (id) DO UPDATE SET username = excluded.username, first_name = excluded.first_name
`);
const clientByIdStmt = db.prepare<[number], Client>("SELECT * FROM clients WHERE id = ?");
const setBannedStmt = db.prepare<[0 | 1, number]>("UPDATE clients SET banned = ? WHERE id = ?");
const insertMessageStmt = db.prepare<[number, number, string]>(
  "INSERT INTO messages (id, client_id, created_at) VALUES (?, ?, ?)",
);
const clientIdByMessageStmt = db.prepare<[number], { client_id: number }>(
  "SELECT client_id FROM messages WHERE id = ?",
);
const countClientsStmt = db.prepare<[], CountRow>("SELECT COUNT(*) AS count FROM clients");
const countMessagesStmt = db.prepare<[], CountRow>("SELECT COUNT(*) AS count FROM messages");
const countMessagesSinceStmt = db.prepare<[string], CountRow>(
  "SELECT COUNT(*) AS count FROM messages WHERE created_at >= ?",
);
const recentClientsStmt = db.prepare<[number], Client>(
  "SELECT * FROM clients ORDER BY created_at DESC LIMIT ?",
);
const bannedClientsStmt = db.prepare<[], Client>(
  "SELECT * FROM clients WHERE banned = 1 ORDER BY created_at DESC",
);

export function saveClient(user: TelegramUser): void {
  upsertClientStmt.run(user.id, user.username ?? null, user.first_name, new Date().toISOString());
}

export function getClient(id: number): Client {
  const client = clientByIdStmt.get(id);
  if (!client) {
    throw new Error(`Client ${id} not found`);
  }
  return client;
}

export function setBanned(id: number, banned: boolean): void {
  const info = setBannedStmt.run(banned ? 1 : 0, id);
  if (info.changes === 0) {
    throw new Error(`Client ${id} not found`);
  }
}

export function saveMessage(forwardedMessageId: number, clientId: number): void {
  insertMessageStmt.run(forwardedMessageId, clientId, new Date().toISOString());
}

export function findClientIdByMessage(forwardedMessageId: number): number | undefined {
  return clientIdByMessageStmt.get(forwardedMessageId)?.client_id;
}

// COUNT(*) завжди повертає рівно один рядок
export function getStats(since: string): Stats {
  return {
    clients: countClientsStmt.get()!.count,
    messages: countMessagesStmt.get()!.count,
    messagesToday: countMessagesSinceStmt.get(since)!.count,
  };
}

export function listRecentClients(limit: number): Client[] {
  return recentClientsStmt.all(limit);
}

export function listBannedClients(): Client[] {
  return bannedClientsStmt.all();
}
