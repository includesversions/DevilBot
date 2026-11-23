using DevilBot.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Channels;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DevilBot
{
    public class UpdateHandler
    {
        public static ITelegramBotClient? BotClient;
        public static int LastMessageId;
        public static bool SendUsers;


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
                else if (data == "show_users")
                {
                    //await BotClient!.SendMessageDraft(userId, 10111, "درحال دریافت اطلاعات ");

                    StringBuilder text = new($"-- کاربران -- {Environment.NewLine}");
                    text.AppendLine($"تعداد کل کاربران: {SenderHandler.Users.Count()}");
                    text.AppendLine();
                    foreach (UserInfo user in SenderHandler.Users)
                    {
                        text.AppendLine($"نام: {user.FirstName}");
                        text.AppendLine($"نام کاربری: {user.UserName ?? "ندارد"}");
                        text.AppendLine($"شناسه ی کاربر: {user.ChatId}");
                        text.AppendLine(" - - - - - - ");
                        text.AppendLine();
                    }
                    await Task.Delay(500);
                   var info = await BotClient!.SendMessage(userId, text.ToString());

                    LastMessageId = info.MessageId;

                }
                else if(data == "sendmessage_allusers")
                {
                    if(SenderHandler.Users.Count == 0)
                    {
                        await BotClient!.SendMessage(userId, "کاربری وجود نداره که بخوای براش پیامی بفرستی");
                        return;
                    }
                    SendUsers = true;
                    await BotClient.EditMessageText(userId, LastMessageId, "پیام خود را بفرستید");
                }

                if (data == "joined")
                {
                    var buttons = new List<InlineKeyboardButton[]>();

                    foreach (string channel in SenderHandler.Channel)
                    {
                        var status = await BotClient!.GetChatMember(channel, userId);

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
                        await BotClient!.AnswerCallbackQuery(
                            update.CallbackQuery.Id,
                            "تو کانال نمیبینمت"
                        );

                        return;
                    }

                    
                    await BotClient!.SendMessage(userId!, "خوش اومدی حالا از گزینه های زیر استفاده کنید", replyMarkup: new KeyboardButton[] { new() { Text = "دسته بندی ها", Style = KeyboardButtonStyle.Primary }, new() { Text = "بعدی", Style = KeyboardButtonStyle.Success } });
                    
                }
            }
            catch
            { throw; }
        }
        
    }
}
