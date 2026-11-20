using DevilBot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
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
        static long userId;
        public static async Task OnUpdate(Update update)
        {
            try
            {
                var data = update.CallbackQuery?.Data;
                userId = update.CallbackQuery?.From.Id ?? 0;

                if (data == "add_channel")
                {
                    SenderHandler.addChannel = true;
                    await BotClient!.EditMessageText(userId!, LastMessageId!, "لینکو بفرست به صورت زیر:\n@examplexxx");

                }
                if (data == "joined")
                {
                    var buttons = new List<InlineKeyboardButton[]>();

                    foreach (string channel in SenderHandler.Channel)
                    {
                        var status = await BotClient.GetChatMember(channel, userId);

                        if (status.Status is ChatMemberStatus.Left or ChatMemberStatus.Kicked)
                        {
                            buttons.Add(new[]
                            {
            InlineKeyboardButton.WithUrl(
                $"عضویت در {channel}",
                $"https://t.me/{channel.TrimStart('@')}"
            )
        });
                        }
                    }

                    buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("عضو شدم", "joined") });

                    if (buttons.Count > 1)
                    {
                        await BotClient.AnswerCallbackQuery(
                            update.CallbackQuery.Id,
                            "تو کانال نمیبینمت"
                        );

                        return;
                    }

                    
                    await BotClient!.SendMessage(userId!, "خوش اومدی عشق استفاده کن لذت ببر منو در اختیارته اگه نیاز به راهنمایی داری /help روش بزن بهت بگم چیکار کنی", replyMarkup: new KeyboardButton[] { new() { Text = "Category", Style = KeyboardButtonStyle.Primary }, new() { Text = "Next", Style = KeyboardButtonStyle.Success } });
                    
                }
            }
            catch
            { throw; }
        }
        
    }
}
