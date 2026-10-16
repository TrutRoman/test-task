import bcrypt from "bcrypt";
import { createUser, findUserByEmail, updatePasswordHash } from "./db.ts";
import { requireEnv } from "./env.ts";
import { logInfo } from "./logger.ts";
import { normalizeEmail, validateRegistration } from "./validation.ts";

const BCRYPT_SALT_ROUNDS = 10;
const ADMIN_NAME = "Адміністратор";

const email = normalizeEmail(requireEnv("ADMIN_EMAIL"));
const password = requireEnv("ADMIN_PASSWORD");

const errors = validateRegistration({ email, name: ADMIN_NAME, password });
if (errors.length > 0) {
  throw new Error(`Invalid ADMIN_EMAIL / ADMIN_PASSWORD: ${errors.join(" ")}`);
}

const existing = findUserByEmail(email);
if (existing && existing.role !== "admin") {
  throw new Error(`User ${email} already exists and is not an admin`);
}

const passwordHash = await bcrypt.hash(password, BCRYPT_SALT_ROUNDS);

// Пароль адміна завжди відповідає ADMIN_PASSWORD з .env, навіть якщо його змінили після першого seed
if (existing) {
  updatePasswordHash(existing.id, passwordHash);
  logInfo(`Seed: admin ${email} already exists, password updated from .env`);
} else {
  createUser({ email, name: ADMIN_NAME, passwordHash, role: "admin" });
  logInfo(`Seed: admin ${email} created`);
}
