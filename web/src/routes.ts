import bcrypt from "bcrypt";
import { Router, type Response } from "express";
import { logIn, logOut, requireAdmin, requireAuth } from "./auth.ts";
import { COURSES, POPULAR_COURSES, TRACKS } from "./courses.ts";
import { type PublicUser, createUser, findUserByEmail, listUsers } from "./db.ts";
import { logInfo, logWarn } from "./logger.ts";
import {
  EMAIL_PATTERN,
  MAX_NAME_LENGTH,
  MIN_NAME_LENGTH,
  MIN_PASSWORD_LENGTH,
  normalizeEmail,
  readField,
  validateRegistration,
} from "./validation.ts";

const BCRYPT_SALT_ROUNDS = 10;
const WEEK_MS = 7 * 24 * 60 * 60 * 1000;

interface UserSummary {
  total: number;
  admins: number;
  regular: number;
  newThisWeek: number;
}

function summarizeUsers(users: PublicUser[]): UserSummary {
  const weekAgo = new Date(Date.now() - WEEK_MS).toISOString();
  const admins = users.filter((user) => user.role === "admin").length;
  return {
    total: users.length,
    admins,
    regular: users.length - admins,
    newThisWeek: users.filter((user) => user.created_at >= weekAgo).length,
  };
}

const REGISTRATION_RULES = {
  emailPattern: EMAIL_PATTERN.source,
  nameMin: MIN_NAME_LENGTH,
  nameMax: MAX_NAME_LENGTH,
  passwordMin: MIN_PASSWORD_LENGTH,
};

interface RegisterForm {
  errors: string[];
  email: string;
  name: string;
}

function renderRegister(res: Response, status: number, form: RegisterForm): void {
  res.status(status).render("register", { title: "Реєстрація", rules: REGISTRATION_RULES, ...form });
}

export const router = Router();

router.get("/", (_req, res) => {
  res.render("index", {
    title: "Головна",
    popularCourses: POPULAR_COURSES,
    tracks: TRACKS,
  });
});

router.get("/courses", requireAuth("/register"), (_req, res) => {
  res.render("courses", { title: "Курси", courses: COURSES });
});

router.get("/register", (_req, res) => {
  renderRegister(res, 200, { errors: [], email: "", name: "" });
});

router.post("/register", async (req, res) => {
  const input = {
    email: normalizeEmail(readField(req.body, "email")),
    name: readField(req.body, "name").trim(),
    password: readField(req.body, "password"),
  };
  const errors = validateRegistration(input);
  if (errors.length === 0 && findUserByEmail(input.email)) {
    errors.push("Користувач з таким email вже зареєстрований.");
  }
  if (errors.length > 0) {
    logWarn(`Registration rejected for "${input.email}": ${errors.join(" ")}`);
    renderRegister(res, 400, { errors, email: input.email, name: input.name });
    return;
  }

  const passwordHash = await bcrypt.hash(input.password, BCRYPT_SALT_ROUNDS);
  const userId = createUser({ email: input.email, name: input.name, passwordHash, role: "user" });
  await logIn(req, userId);
  logInfo(`User registered: ${input.email} (id ${userId})`);
  res.redirect("/profile");
});

router.get("/login", (_req, res) => {
  res.render("login", { title: "Вхід", error: "", email: "" });
});

router.post("/login", async (req, res) => {
  const email = normalizeEmail(readField(req.body, "email"));
  const password = readField(req.body, "password");
  const user = findUserByEmail(email);
  if (!user || !(await bcrypt.compare(password, user.password_hash))) {
    logWarn(`Failed login for "${email}"`);
    res.status(401).render("login", { title: "Вхід", error: "Невірний email або пароль.", email });
    return;
  }

  await logIn(req, user.id);
  logInfo(`User logged in: ${user.email} (${user.role})`);
  res.redirect("/profile");
});

router.post("/logout", async (req, res) => {
  logInfo(`User logged out: ${res.locals.user?.email}`);
  await logOut(req, res);
  res.redirect("/");
});

router.get("/profile", requireAuth("/login"),(_req, res) => {
  res.render("profile", { title: "Особистий кабінет" });
});

router.get("/admin", requireAdmin, (_req, res) => {
  const users = listUsers();
  res.render("admin", { title: "Адмін-панель", users, summary: summarizeUsers(users) });
});
