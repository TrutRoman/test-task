import { randomBytes } from "node:crypto";
import { promisify } from "node:util";
import type { NextFunction, Request, Response } from "express";
import { findUserById } from "./db.ts";
import { renderError } from "./errors.ts";
import { logWarn } from "./logger.ts";

export const SESSION_COOKIE = "sid";

export function loadUser(req: Request, res: Response, next: NextFunction): void {
  const { userId } = req.session;
  res.locals.user = userId === undefined ? undefined : findUserById(userId);
  next();
}

// Токен живе в сесії; кожна POST-форма має повернути його в прихованому полі _csrf
export function csrfProtection(req: Request, res: Response, next: NextFunction): void {
  req.session.csrfToken ??= randomBytes(32).toString("hex");
  res.locals.csrfToken = req.session.csrfToken;
  if (req.method === "POST" && req.body?._csrf !== req.session.csrfToken) {
    logWarn(`CSRF token rejected: ${req.method} ${req.originalUrl}`);
    renderError(res, 403, "Недійсний CSRF-токен. Оновіть сторінку й спробуйте ще раз.");
    return;
  }
  next();
}

// Гостя перенаправляє на вказану сторінку: кабінет веде на вхід, курси — на реєстрацію
export function requireAuth(guestRedirect: string) {
  return (_req: Request, res: Response, next: NextFunction): void => {
    if (!res.locals.user) {
      res.redirect(guestRedirect);
      return;
    }
    next();
  };
}

export function requireAdmin(req: Request, res: Response, next: NextFunction): void {
  if (res.locals.user?.role !== "admin") {
    const who = res.locals.user ? `user ${res.locals.user.email}` : "guest";
    logWarn(`Admin access denied for ${who}: ${req.originalUrl}`);
    renderError(res, 403, "Доступ лише для адміністратора.");
    return;
  }
  next();
}

// Новий ідентифікатор сесії після входу захищає від session fixation
export async function logIn(req: Request, userId: number): Promise<void> {
  await promisify(req.session.regenerate.bind(req.session))();
  req.session.userId = userId;
  await promisify(req.session.save.bind(req.session))();
}

export async function logOut(req: Request, res: Response): Promise<void> {
  await promisify(req.session.destroy.bind(req.session))();
  res.clearCookie(SESSION_COOKIE);
}
