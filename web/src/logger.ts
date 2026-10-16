import { appendFileSync, mkdirSync } from "node:fs";
import { dirname } from "node:path";
import { requireEnv } from "./env.ts";

type Level = "INFO" | "WARN" | "ERROR";

const LOG_PATH = requireEnv("LOG_PATH");
mkdirSync(dirname(LOG_PATH), { recursive: true });

function write(level: Level, message: string): void {
  const line = `${new Date().toISOString()} ${level.padEnd(5)} ${message}`;
  appendFileSync(LOG_PATH, `${line}\n`);
  if (level === "ERROR") {
    console.error(line);
  } else {
    console.log(line);
  }
}

export function logInfo(message: string): void {
  write("INFO", message);
}

export function logWarn(message: string): void {
  write("WARN", message);
}

export function logError(message: string, error: unknown): void {
  const details = error instanceof Error ? error.stack : String(error);
  write("ERROR", `${message}: ${details}`);
}
