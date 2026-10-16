import { join } from "node:path";
import express, { type ErrorRequestHandler, type NextFunction, type Request, type Response } from "express";
import session from "express-session";
import { SESSION_COOKIE, csrfProtection, loadUser } from "./auth.ts";
import type { Track } from "./courses.ts";
import type { Role } from "./db.ts";
import { requirePort, requireSecret } from "./env.ts";
import { renderError } from "./errors.ts";
import { logError, logInfo } from "./logger.ts";
import { router } from "./routes.ts";

const PORT = requirePort("PORT");
const SESSION_SECRET = requireSecret("SESSION_SECRET", 32);
const ONE_DAY_MS = 24 * 60 * 60 * 1000;
const ROOT_DIR = join(import.meta.dirname, "..");

function formatDate(iso: string): string {
  return new Date(iso).toLocaleString("uk-UA", { timeZone: "Europe/Kyiv" });
}

function initials(name: string): string {
  const words = name.trim().split(/\s+/).slice(0, 2);
  return words.map((word) => word[0]).join("").toUpperCase();
}

function roleLabel(role: Role): string {
  return role === "admin" ? "Адміністратор" : "Студент";
}

const TRACK_CLASSES: Record<Track, string> = {
  Frontend: "track-frontend",
  Backend: "track-backend",
  DevOps: "track-devops",
};

function trackClass(track: Track): string {
  return TRACK_CLASSES[track];
}

function exposePath(req: Request, res: Response, next: NextFunction): void {
  res.locals.currentPath = req.path;
  next();
}

function logRequest(req: Request, res: Response, next: NextFunction): void {
  const startedAt = Date.now();
  res.on("finish", () => {
    logInfo(`${req.method} ${req.originalUrl} ${res.statusCode} ${Date.now() - startedAt}ms`);
  });
  next();
}

const handleError: ErrorRequestHandler = (err, req, res, _next) => {
  logError(`${req.method} ${req.originalUrl} failed`, err);
  renderError(res, 500, "Внутрішня помилка сервера.");
};

const app = express();

app.set("view engine", "ejs");
app.set("views", join(ROOT_DIR, "views"));
app.locals.formatDate = formatDate;
app.locals.initials = initials;
app.locals.roleLabel = roleLabel;
app.locals.trackClass = trackClass;

app.use(express.static(join(ROOT_DIR, "public")));
// Браузери запитують /favicon.ico навіть за наявності <link rel="icon">
app.get("/favicon.ico", (_req, res) => res.redirect(301, "/favicon.svg"));
app.use(logRequest);
app.use(exposePath);
app.use(express.urlencoded());
app.use(
  session({
    name: SESSION_COOKIE,
    secret: SESSION_SECRET,
    resave: false,
    saveUninitialized: false,
    cookie: { httpOnly: true, sameSite: "lax", maxAge: ONE_DAY_MS },
  }),
);
app.use(loadUser);
app.use(csrfProtection);
app.use(router);
app.use((_req, res) => renderError(res, 404, "Сторінку не знайдено."));
app.use(handleError);

app.listen(PORT, (error) => {
  if (error) {
    throw error;
  }
  logInfo(`Site is running at http://localhost:${PORT}`);
});
