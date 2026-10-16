import Database from "better-sqlite3";
import { requireEnv } from "./env.ts";

export type Role = "user" | "admin";

export interface User {
  id: number;
  email: string;
  name: string;
  password_hash: string;
  role: Role;
  created_at: string;
}

export type PublicUser = Omit<User, "password_hash">;

export interface NewUser {
  email: string;
  name: string;
  passwordHash: string;
  role: Role;
}

const db = new Database(requireEnv("DB_PATH"));

db.prepare(`
  CREATE TABLE IF NOT EXISTS users (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    email         TEXT NOT NULL UNIQUE,
    name          TEXT NOT NULL,
    password_hash TEXT NOT NULL,
    role          TEXT NOT NULL CHECK (role IN ('user', 'admin')),
    created_at    TEXT NOT NULL
  )
`).run();

const insertUserStmt = db.prepare<[string, string, string, Role, string]>(
  "INSERT INTO users (email, name, password_hash, role, created_at) VALUES (?, ?, ?, ?, ?)",
);
const updatePasswordStmt = db.prepare<[string, number]>(
  "UPDATE users SET password_hash = ? WHERE id = ?",
);
const userByEmailStmt = db.prepare<[string], User>("SELECT * FROM users WHERE email = ?");
const userByIdStmt = db.prepare<[number], User>("SELECT * FROM users WHERE id = ?");
const allUsersStmt = db.prepare<[], PublicUser>(
  "SELECT id, email, name, role, created_at FROM users ORDER BY id",
);

export function createUser(user: NewUser): number {
  const info = insertUserStmt.run(
    user.email,
    user.name,
    user.passwordHash,
    user.role,
    new Date().toISOString(),
  );
  return Number(info.lastInsertRowid);
}

export function updatePasswordHash(id: number, passwordHash: string): void {
  const info = updatePasswordStmt.run(passwordHash, id);
  if (info.changes === 0) {
    throw new Error(`User ${id} not found`);
  }
}

export function findUserByEmail(email: string): User | undefined {
  return userByEmailStmt.get(email);
}

export function findUserById(id: number): User | undefined {
  return userByIdStmt.get(id);
}

export function listUsers(): PublicUser[] {
  return allUsersStmt.all();
}
