import "express-session";
import type { User } from "./db.ts";

declare module "express-session" {
  interface SessionData {
    userId: number;
    csrfToken: string;
  }
}

declare global {
  namespace Express {
    interface Locals {
      user: User | undefined;
      csrfToken: string;
      currentPath: string;
    }
  }
}
