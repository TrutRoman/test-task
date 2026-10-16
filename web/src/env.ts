import { loadEnvFile } from "node:process";

loadEnvFile();

export function requireEnv(name: string): string {
  const value = process.env[name];
  if (!value) {
    throw new Error(`Environment variable ${name} is not set`);
  }
  return value;
}

export function requirePort(name: string): number {
  const port = Number(requireEnv(name));
  if (!Number.isInteger(port) || port < 1 || port > 65535) {
    throw new Error(`Environment variable ${name} must be a port number (1-65535)`);
  }
  return port;
}

export function requireSecret(name: string, minLength: number): string {
  const secret = requireEnv(name);
  if (secret.length < minLength) {
    throw new Error(`Environment variable ${name} must be at least ${minLength} characters long`);
  }
  return secret;
}
