using DevilBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DevilBot
{
    public class SenderHandler
    {
        public static ITelegramBotClient BotClient { get; set; }
        public static bool oneMovie;
        public static bool addChannel;
        static Dictionary<long, UserSession> sessions = new();
        public static List<UserInfo> Users = new();

        public static string? LastMoiveId = null;
        public static ObservableCollection<string> Channel = new() { "@global_currency_iran" };
        public static int lastMessageId;
        static string apiUrl = "https://www.eporner.com/api/v2/video/search/?query=anal&per_page=100";

        public static Dictionary<string, string> categoryKeys = new()
        {
           ["Amateur"] = "آماتور",
           ["Anal"] = "مقعدی",
           ["Solo"] = "تک نفره",
           ["Cosplay"] = "کاسپلی",
           ["Lesbian"] = "لزبین",
           ["Outdoor"] = "فضای باز",
           ["MILF"] = "میلف",
           ["Hentai"] = "هنتای",
           ["POV"] = "موقعیت",
           ["Romantic"] = "عاشقانه",
           ["Hardcore"] = "هارد کور",
           ["Softcore"] = "سافت کور",
           ["Mature"] = "بالغانه",
           ["Couples"] = "کاپل",
           ["ایرانی"] = "ایرانی",
           ["Foot fetish"] = "فوت فتیش",
           ["Footjob"] = "Footjob",
           ["Handjob"] = "Handjob",
           ["BDSM"] = "BDSM",
        };

        public static async Task Sender(Message message, UpdateType type)
        {
            long chatId = message.Chat.Id;
            if(UpdateHandler.SendUsers && (chatId == 6124324134) || chatId == 2103359553)
            {
                if (message.Text is null) return;
                var info = await BotClient.SendMessage(message.Chat.Id, "درحال ارسال پیام");
                foreach (UserSession users in sessions.Values)
                {
                    if (users.ChatId == message.Chat.Id) continue;
                    await BotClient.SendMessage(users.ChatId, message.Text, ParseMode.Markdown);
                }
                await BotClient!.EditMessageText(message.Chat?.Id, info.MessageId, "پیام با موفقیت ارسال شد.");

                UpdateHandler.SendUsers = false;
                return;
            }

            var buttons = new List<InlineKeyboardButton[]>();

            foreach (string channel in Channel)
            {
                var status = await BotClient.GetChatMember(channel, message!.From!.Id);

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

            buttons.Add(new[]
            {
    InlineKeyboardButton.WithCallbackData("عضو شدم", "joined")
});

            if (buttons.Count > 1)
            {
               var info =  await BotClient.SendMessage(
                    message!.Chat.Id,
                    "درود برای استفاده ابتدا در کانال‌های زیر عضو شوید:",
                    replyMarkup: new InlineKeyboardMarkup(buttons)
                );

                lastMessageId = info.MessageId;

                return;
            }

            if (addChannel)
            {
                if (message.Text.StartsWith("@"))
                {
                    Channel.Add(message.Text);
                    addChannel = false;
                    await BotClient.SendMessage(message.Chat.Id, $"کانال {message.Text} با موفقیت ثبت شد");
                }
                else
                {
                    await BotClient.SendMessage(message.Chat.Id, "کانال باید با @ شروع شود");
		    addChannel = false;
                }
            }
            UserSession userInfo;

                lock (sessions)
                {
                    if (!sessions.TryGetValue(message.Chat.Id, out userInfo))
                    {
                        userInfo = new UserSession
                        {
                            ApiUrl = apiUrl,
                            StepNumber = 0,
                            Cts = new(),
                            Response = new(),
                            Semaphore = new(1, 1),
                            ChatId = message.Chat.Id,
                            FirstName = message.From?.FirstName,
                            UserName = message.From?.Username
                        };

                        sessions.Add(message.Chat.Id, userInfo);
                        Users.Add(new() { ChatId = message.Chat.Id, FirstName = message.From?.FirstName, UserName = $"https://t.me/{message.From?.Username}" });
                }
            }

                await userInfo.Semaphore.WaitAsync();

                try
                {
                    string text = message.Text;

                    if (!userInfo.IsInitialized)
                    {
                        userInfo.Response = await RequestPop(userInfo.ApiUrl);
                        userInfo.IsInitialized = true;
                    }
                if (message.Text == "بعدی")
                {
                    await SendInfo(message, userInfo);
                }
                else if (text == "/admin" && (message.Chat.Id == 2103359553 || message.Chat.Id == 6124324134))
                {
                    InlineKeyboardMarkup inlineKeyboard = new(new[]
                    {
                   new[] { InlineKeyboardButton.WithCallbackData("افزودن کانال", "add_channel") },
                   new[] {  InlineKeyboardButton.WithCallbackData("مشاهده تعداد کل کاربران", "show_users") },
                   new[] {  InlineKeyboardButton.WithCallbackData("ارسال پیام به تمامی کاربران", "sendmessage_allusers")},    
                    });
                    

                    var info = await BotClient.SendMessage(message.Chat.Id, "چطوری عشق, در خدمتتم", replyMarkup: inlineKeyboard);
                    UpdateHandler.LastMessageId = info.MessageId;

                }
                else if (message.Text == "دسته بندی ها")
                {
                    await SendCategory(message, null);
                }
                else if (message.Text?.ToLower() == "/start")
                {
                    await BotClient.SendMessage(
                        message.Chat.Id,
                        "درود \r\nیکی از گزینه های زیر رو انتخاب کن",
                        replyMarkup: new KeyboardButton[]
                        {
                    new() { Text = "دسته بندی ها", Style = KeyboardButtonStyle.Primary },
                    new() { Text = "بعدی", Style = KeyboardButtonStyle.Success }
                        });
                }
                else
                {
                    var value = categoryKeys.Where(n => n.Value == message.Text).FirstOrDefault();
                    if (value.Value == message.Text)
                    {
                        await BotClient.SendMessageDraft(
                            chatId: message.Chat.Id,
                            draftId: 10111,
                            text: "درحال جستوجو "
                        );

                        await Task.Delay(500);

                        SelectCategory(value.Key ?? "", 500, userInfo);

                        if (userInfo.ApiUrl is null)
                        {
                            await SendCategory(message, "موردی یافت نشد لطفا از دسته بندی های تهیه شده استفاده کنید"); 
                            return;
                        }

                        userInfo.Response = await RequestPop(userInfo.ApiUrl);
                        userInfo.StepNumber = 0;
                       
                        await BotClient.SendMessage(
                           message.Chat.Id,
                           $"دسته بندی {message.Text} تایید شد گزینه بعدی رو بزنید \r\nتا به یک بایگانی بی نهایت از فیلم ها وصل شید.",
                           replyMarkup: new KeyboardButton[]
                           {
                    new() { Text = "بعدی", Style = KeyboardButtonStyle.Success },
                    new() { Text = "دسته بندی ها", Style = KeyboardButtonStyle.Primary }
                           });
                    }
                }

                }
                finally
                {
                    userInfo.Semaphore.Release();
                }
            }
        private static async Task SendCategory(Message message, string? txtMessage)
        {
            ReplyKeyboardMarkup targetButtons = new(new[]
            {
        new KeyboardButton[]
        {
            new KeyboardButton() { Text = "ایرانی" },
            new KeyboardButton() { Text = "بعدی", Style = KeyboardButtonStyle.Success },
        },
        new KeyboardButton[]
        {
            new KeyboardButton() { Text = "آماتور" },
            new KeyboardButton() { Text = "مقعدی" },
            new KeyboardButton() { Text = "تک نفره" }
        },
        new KeyboardButton[]
        {
            new KeyboardButton() { Text = "کاپل" },
            new KeyboardButton() { Text = "لزبین" },
            new KeyboardButton() { Text = "فضای باز" },
        },
        new KeyboardButton[]
        {

            new KeyboardButton() { Text = "عاشقانه" },
            new KeyboardButton() { Text = "کاسپلی" },
            new KeyboardButton() { Text = "میلف" }
        },
        new KeyboardButton[]
        {
            new KeyboardButton() { Text = "هارد کور" },
            new KeyboardButton() { Text = "سافت کور" },
            new KeyboardButton() { Text = "بالغانه" },
        },
        new KeyboardButton[]
        {
            new KeyboardButton() { Text = "هنتای" },
            new KeyboardButton() { Text = "موقعیت" },
            new KeyboardButton() { Text = "فوت فتیش" },
        },

        new KeyboardButton[]
        {
            new KeyboardButton() { Text = "Footjob" },
            new KeyboardButton() { Text = "Handjob" },
            new KeyboardButton() { Text = "BDSM" },
        },
    })
            {
                ResizeKeyboard = true
            };

            await BotClient.SendMessage(message.Chat.Id, txtMessage ?? "یک گزینه را انتخاب کنید", replyMarkup: targetButtons);

        }

        private static async Task SendInfo(Message message, UserSession session)
        {
            if (session.StepNumber >= session.Response.Videos.Count)
            {
                await BotClient.SendMessage(
                    message.Chat.Id,
                    "ویدیوی دیگری وجود ندارد لطفا از دسته بندی ی دیگر بهره مند شوید");
                return;
            }

            var current = session.Response.Videos[session.StepNumber++];

            string text = $"""
    *عنوان*: _{current?.Title}_
    *امتیاز*: _{current?.Rate}_
    *بازدید ها*: _{current?.Views}_
    *زمان*: _{current?.LengthMin}_
    ---
    [برای مشاهده کلیک کنید
    برای مشاهده کلیک کنید]({current?.Embed})
    
    لطفاً برای دسترسی بهتر از* VPN استفاده کنید*
    
    [وطنی هامون اینجاست](https://t.me/setmethhj)
    """;

            var media = new IAlbumInputMedia[]
            {
        new InputMediaPhoto(InputFile.FromUri(current.Thumbs.First().Src.ToString()))
        {
            Caption = text,
            ParseMode = ParseMode.Markdown
        },
        new InputMediaPhoto(InputFile.FromUri(current.Thumbs[1].Src.ToString())),
        new InputMediaPhoto(InputFile.FromUri(current.Thumbs.Last().Src.ToString()))
            };
      
            await BotClient.SendMediaGroup(
                chatId: message.Chat.Id,
                media: media,
                cancellationToken: session.Cts.Token);
        }

       private static async Task<VideoResponse> RequestPop(string url)
        {
            HttpClient client = new();

            string res = await client.GetStringAsync(url);

            return JsonConvert.DeserializeObject<VideoResponse>(res) ?? new();
        }

       private static void ChangeUrl(string? query, int count, UserSession session)
        {
            if (query is null)
            {
                session.ApiUrl = null;
                return;
            }
            session.ApiUrl = $"https://www.eporner.com/api/v2/video/search/?query={Uri.EscapeDataString(query)}&per_page={count}";
        }

        private static void SelectCategory(string category, int count, UserSession session)
        {
            Action change = category switch
            {
                "Amateur" => () => ChangeUrl(category.ToLower(), count, session),
                "Anal" => () => ChangeUrl(category.ToLower(), count, session),
                "Solo" => () => ChangeUrl(category.ToLower(), count, session),
                "Couples" => () => ChangeUrl(category.ToLower(), count, session),
                "Lesbian" => () => ChangeUrl(category.ToLower(), count, session),
                "BDSM" => () => ChangeUrl(category.ToLower(), count, session),
                "Cosplay" => () => ChangeUrl(category.ToLower(), count, session),
                "Softcore" => () => ChangeUrl(category.ToLower(), count, session),
                "POV" => () => ChangeUrl(category.ToLower(), count, session),
                "Romantic" => () => ChangeUrl(category.ToLower(), count, session),
                "Foot fetish" => () => ChangeUrl(category.ToLower(), count, session),
                "MILF" => () => ChangeUrl(category.ToLower(), count, session),
                "Mature" => () => ChangeUrl(category.ToLower(), count, session),
                "Hentai" => () => ChangeUrl(category.ToLower(), count, session),
                "Hardcore" => () => ChangeUrl(category.ToLower(), count, session),
                "Outdoor" => () => ChangeUrl(category.ToLower(), count, session),
                "Footjob" => () => ChangeUrl(category.ToLower(), count, session),
                "Handjob" => () => ChangeUrl(category.ToLower(), count, session),
                "ایرانی" => () => ChangeUrl(category.ToLower(), count, session),
                _ => () => ChangeUrl(null, count, session),
            };

            change();

        }
    }
}
