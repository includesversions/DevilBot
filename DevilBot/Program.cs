using System;
using System.Text;
using DevilBot;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.ObjectModel;

TelegramBotClient bot = new("9292779843:AAFrYCURT5IsT0NqHAM0WgpjqaRdOifoiZM");

UpdateHandler.BotClient = bot;
SenderHandler.BotClient = bot;

bot.OnUpdate += UpdateHandler.OnUpdate;

bot.OnMessage += SenderHandler.Sender;

Console.WriteLine("bot runing.");

Console.ReadLine();

