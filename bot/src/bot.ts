import { Bot, InlineKeyboard, Keyboard, type Context } from "grammy";
import { BOT_TOKEN, OPERATOR_ID } from "./config.ts";
import {
  type Client,
  type Stats,
  findClientIdByMessage,
  getClient,
  getStats,
  listBannedClients,
  listRecentClients,
  saveClient,
  saveMessage,
  setBanned,
} from "./db.ts";
import { logError, logInfo, logWarn } from "./logger.ts";

const STATS_BUTTON = "📊 Статистика";
const RECENT_BUTTON = "👥 Останні клієнти";
const BANNED_BUTTON = "🚫 Заблоковані";
const RECENT_CLIENTS_LIMIT = 10;

const CLIENT_WELCOME = [
  "👋 Вітаю! Це тестовий бот підтримки — частина тестового завдання.",
  "",
  "Як це працює:",
  "• напишіть своє запитання — текстом, фото чи файлом;",
  "• повідомлення одразу отримає оператор;",
  "• відповідь оператора прийде сюди, у цей чат.",
  "",
  "Кнопка /start внизу знову покаже це повідомлення.",
].join("\n");
const OPERATOR_WELCOME = [
  "👋 Вітаю! Це тестовий бот підтримки, ви — оператор.",
  "",
  "• повідомлення клієнтів пересилаються сюди, під кожним — дані клієнта і кнопка «🚫 Заблокувати»;",
  "• щоб відповісти, зробіть reply на переслане повідомлення — відповідь отримає саме цей клієнт;",
  "• кнопки внизу: статистика, останні клієнти, заблоковані.",
].join("\n");
const NO_REPLY_HINT = "Щоб відповісти клієнту, зробіть reply на його переслане повідомлення.";
const UNKNOWN_CLIENT = "Не можу визначити клієнта. Зробіть reply саме на переслане повідомлення клієнта.";
const BANNED_NOTICE = "Ви заблоковані: ваші повідомлення не доставляються оператору.";

const clientKeyboard = new Keyboard().text("/start").resized().persistent();
const operatorKeyboard = new Keyboard()
  .text(STATS_BUTTON)
  .text(RECENT_BUTTON)
  .row()
  .text(BANNED_BUTTON)
  .resized()
  .persistent();

function isOperator(ctx: Context): boolean {
  return ctx.from?.id === OPERATOR_ID;
}

function formatClient(client: Client): string {
  const username = client.username ? `@${client.username}` : "без username";
  return `${client.first_name} (${username}), ID ${client.id}`;
}

function formatStats(stats: Stats): string {
  return [
    STATS_BUTTON,
    `Клієнтів: ${stats.clients}`,
    `Повідомлень усього: ${stats.messages}`,
    `Повідомлень сьогодні: ${stats.messagesToday}`,
  ].join("\n");
}

function formatRecentClients(clients: Client[]): string {
  if (clients.length === 0) {
    return "Клієнтів ще немає.";
  }
  const lines = clients.map((client, index) => `${index + 1}. ${formatClient(client)}`);
  return [RECENT_BUTTON, ...lines].join("\n");
}

function startOfToday(): string {
  const date = new Date();
  date.setHours(0, 0, 0, 0);
  return date.toISOString();
}

export const bot = new Bot(BOT_TOKEN);
const privateChats = bot.chatType("private");
// Кожен обробник оператора зареєстрований тут, тому виконується лише для OPERATOR_ID
const operator = privateChats.filter(isOperator);
const client = privateChats.drop(isOperator);

operator.command("start", async (ctx) => {
  logInfo("Operator: /start");
  await ctx.reply(OPERATOR_WELCOME, { reply_markup: operatorKeyboard });
});

operator.hears(STATS_BUTTON, async (ctx) => {
  logInfo("Operator: requested stats");
  await ctx.reply(formatStats(getStats(startOfToday())));
});

operator.hears(RECENT_BUTTON, async (ctx) => {
  logInfo("Operator: requested recent clients");
  await ctx.reply(formatRecentClients(listRecentClients(RECENT_CLIENTS_LIMIT)));
});

operator.hears(BANNED_BUTTON, async (ctx) => {
  logInfo("Operator: requested banned clients");
  const banned = listBannedClients();
  if (banned.length === 0) {
    await ctx.reply("Заблокованих клієнтів немає.");
    return;
  }
  for (const bannedClient of banned) {
    await ctx.reply(formatClient(bannedClient), {
      reply_markup: new InlineKeyboard().text("✅ Розблокувати", `unban:${bannedClient.id}`),
    });
  }
});

operator.callbackQuery(/^ban:(\d+)$/, async (ctx) => {
  const bannedClient = getClient(Number(ctx.match[1]));
  setBanned(bannedClient.id, true);
  logInfo(`Operator: banned ${formatClient(bannedClient)}`);
  await ctx.answerCallbackQuery({ text: `Заблоковано: ${bannedClient.first_name}` });
});

operator.callbackQuery(/^unban:(\d+)$/, async (ctx) => {
  const unbannedClient = getClient(Number(ctx.match[1]));
  setBanned(unbannedClient.id, false);
  logInfo(`Operator: unbanned ${formatClient(unbannedClient)}`);
  await ctx.editMessageText(`✅ Розблоковано: ${formatClient(unbannedClient)}`);
  await ctx.answerCallbackQuery();
});

operator.on("message", async (ctx) => {
  const repliedId = ctx.msg.reply_to_message?.message_id;
  if (repliedId === undefined) {
    logWarn("Operator: message without reply, not delivered");
    await ctx.reply(NO_REPLY_HINT);
    return;
  }
  const clientId = findClientIdByMessage(repliedId);
  if (clientId === undefined) {
    logWarn(`Operator: reply to unknown message ${repliedId}, not delivered`);
    await ctx.reply(UNKNOWN_CLIENT);
    return;
  }
  await ctx.copyMessage(clientId);
  logInfo(`Operator: reply delivered to ${formatClient(getClient(clientId))}`);
});

client.command("start", async (ctx) => {
  saveClient(ctx.from);
  logInfo(`Client /start: ${formatClient(getClient(ctx.from.id))}`);
  await ctx.reply(CLIENT_WELCOME, { reply_markup: clientKeyboard });
});

// forwardMessage не приймає reply_markup, тому кнопку блокування додаємо окремим повідомленням-відповіддю
client.on("message", async (ctx) => {
  saveClient(ctx.from);
  const sender = getClient(ctx.from.id);
  if (sender.banned) {
    logWarn(`Message from banned client ${formatClient(sender)} rejected`);
    await ctx.reply(BANNED_NOTICE);
    return;
  }
  const forwarded = await ctx.forwardMessage(OPERATOR_ID);
  saveMessage(forwarded.message_id, sender.id);
  logInfo(`Message from ${formatClient(sender)} forwarded to operator`);
  await ctx.api.sendMessage(OPERATOR_ID, `👤 ${formatClient(sender)}`, {
    reply_parameters: { message_id: forwarded.message_id },
    reply_markup: new InlineKeyboard().text("🚫 Заблокувати", `ban:${sender.id}`),
  });
});

bot.catch((err) => {
  logError(`Error while handling update ${err.ctx.update.update_id}`, err.error);
});
