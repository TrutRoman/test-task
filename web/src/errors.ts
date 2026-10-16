import type { Response } from "express";

export function renderError(res: Response, status: number, message: string): void {
  res.status(status).render("error", { title: `Помилка ${status}`, status, message });
}
