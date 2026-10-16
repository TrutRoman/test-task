import { loadEnvFile } from "node:process";

loadEnvFile();

function requireEnv(name: string): string {
  const value = process.env[name];
  if (!value) {
    throw new Error(`Environment variable ${name} is not set`);
  }
  return value;
}

function parseTelegramId(name: string): number {
  const id = Number(requireEnv(name));
  if (!Number.isSafeInteger(id)) {
    throw new Error(`Environment variable ${name} must be an integer Telegram ID`);
  }
  return id;
}

export const BOT_TOKEN = requireEnv("BOT_TOKEN");
export const OPERATOR_ID = parseTelegramId("OPERATOR_ID");
export const DB_PATH = requireEnv("DB_PATH");
export const LOG_PATH = requireEnv("LOG_PATH");
