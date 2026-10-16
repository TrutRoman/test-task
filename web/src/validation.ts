export interface RegistrationInput {
  email: string;
  name: string;
  password: string;
}

// Ці ж правила передаються у форму реєстрації для перевірки в браузері
export const MIN_PASSWORD_LENGTH = 8;
export const MIN_NAME_LENGTH = 2;
export const MAX_NAME_LENGTH = 30;
export const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
// bcrypt враховує лише перші 72 байти пароля
const MAX_PASSWORD_BYTES = 72;

// Поле форми може бути відсутнім або масивом (?a=1&a=2), тому беремо лише рядок
export function readField(body: Record<string, unknown>, name: string): string {
  const value = body[name];
  return typeof value === "string" ? value : "";
}

export function normalizeEmail(email: string): string {
  return email.trim().toLowerCase();
}

export function validateRegistration(input: RegistrationInput): string[] {
  const errors: string[] = [];
  if (!EMAIL_PATTERN.test(input.email)) {
    errors.push("Вкажіть коректний email.");
  }
  if (input.name.length < MIN_NAME_LENGTH || input.name.length > MAX_NAME_LENGTH) {
    errors.push(`Ім'я має містити від ${MIN_NAME_LENGTH} до ${MAX_NAME_LENGTH} символів.`);
  }
  if (input.password.length < MIN_PASSWORD_LENGTH) {
    errors.push(`Пароль має містити щонайменше ${MIN_PASSWORD_LENGTH} символів.`);
  }
  if (Buffer.byteLength(input.password) > MAX_PASSWORD_BYTES) {
    errors.push(`Пароль задовгий (максимум ${MAX_PASSWORD_BYTES} байти).`);
  }
  return errors;
}
