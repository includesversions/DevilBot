using System;
using DevilBot;
using Telegram.Bot;

TelegramBotClient bot = new("9013422708:AAHVjsjBQ1WadzQ2d6Domq1Kn46_fCXBjyo");

UpdateHandler.BotClient = bot;
SenderHandler.BotClient = bot;

bot.OnUpdate += UpdateHandler.OnUpdate;

bot.OnMessage += SenderHandler.Sender;

Console.WriteLine("Bot Running.");

await bot.SendMessage(6124324134, "دوباره اجرا شدم");

Console.ReadLine();

