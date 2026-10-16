import { bot } from "./bot.ts";
import { OPERATOR_ID } from "./config.ts";
import { logInfo } from "./logger.ts";

function stop(signal: string): void {
  logInfo(`Received ${signal}, stopping bot`);
  bot.stop();
}

process.once("SIGINT", () => stop("SIGINT"));
process.once("SIGTERM", () => stop("SIGTERM"));

await bot.start({
  onStart: (me) => logInfo(`Bot @${me.username} started, operator ID: ${OPERATOR_ID}`),
});
logInfo("Bot stopped");
