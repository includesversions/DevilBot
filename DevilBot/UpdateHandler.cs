using DevilBot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DevilBot
{
    public static class UpdateHandler
    {
        public static ITelegramBotClient? BotClient;
        public static int LastMessageId;
        static long? userId;
        public static async Task OnUpdate(Update update)
        {
            var data = update.CallbackQuery?.Data;
            userId = update.CallbackQuery?.From.Id;

            if (data == "add_channel")
            {
                
            }
            else if(data == "add_video")
            {
                await BotClient!.EditMessageText(userId!, LastMessageId!, "خب مشخص کن چند تا ویدیو میخوای اضافه کنی", replyMarkup: new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("یکی", "oneMovie"), InlineKeyboardButton.WithCallbackData("چند تا", "manyMovie") }, new[] { InlineKeyboardButton.WithCallbackData("نظرم عوض شد, دیگه نمیخوام", "sendMovieCancel") } } ));

            }

            if(data is not null)
                CheckCallbackData(data);


            if (data == "addCaption")
            {
                SenderHandler.addCaption = true;
                await BotClient!.EditMessageText(userId!, LastMessageId!, "بفرست");

            }


        }

        private static void CheckCallbackData(string data)
        {
            Action run = data switch
            {
                "oneMovie" => OneMovieIntro, 
                "manyMovie" => ManyMovieIntro,
                _ => () => { }
            };

            run();
        }

        private static async void OneMovieIntro()
        {
            SenderHandler.oneMovie = true;
            await BotClient!.EditMessageText(userId!, LastMessageId!, "بفرستش");
        }
        private static async void ManyMovieIntro()
        {
            SenderHandler.oneMovie = false;
            await BotClient!.EditMessageText(userId!, LastMessageId!, "یکی یکی برام ارسال کن یا فور بزن");
        }

        private static async void OneMovieHandler()
        {
            
        }
        
    }
}
