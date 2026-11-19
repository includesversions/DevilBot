using DevilBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public static bool addCaption;
        static Dictionary<long, UserSession> sessions = new();
        public static string? LastMoiveId = null;
        public static ObservableCollection<Media> Movies = new();
        public static ObservableCollection<ManyMedia> ManyMovies = new();
        public static ManyMedia? currenctMedia = null;
        public static int lastMessageId;
        static string apiUrl = "https://www.eporner.com/api/v2/video/search/?query=anal&per_page=100";

        public static async Task Sender(Message message, UpdateType type)
        {
            if (LastMoiveId != null)
            {
                if (oneMovie)
                {
                    try
                    {
                        ref Media found = ref FindMedia(Movies.ToArray(), LastMoiveId);

                        found.Description = message.Text;

                        LastMoiveId = null;

                        await BotClient.SendMessage(message.Chat.Id, $"*{found.Description}*\n\n[Watch now]({found.Link})", ParseMode.Markdown);

                    }
                    catch
                    {
                        LastMoiveId = null;
                        throw;
                    }
                }
                if(!oneMovie)
                {
                    try
                    {
                        if (addCaption)
                        {
                            if (currenctMedia != null)
                            {
                                ManyMovies.Add(currenctMedia);

                                ref ManyMedia found = ref FindManyMedia(ManyMovies.ToArray(), LastMoiveId!);

                                found?.Description = message?.Text ?? "";
                                LastMoiveId = null;

                                await BotClient.SendMessage(message.Chat.Id, $"*{found.Description}*\n\n[Watch now]({found.Link})", ParseMode.Markdown);
                            }
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
                
                static ref ManyMedia FindManyMedia(Span<ManyMedia> people, string id)
                {
                    for (int i = 0; i < people.Length; i++)
                    {
                        if (people[i].Id.ToString() == id)
                            return ref people[i];
                    }

                    throw new KeyNotFoundException();
                }

                static ref Media FindMedia(Span<Media> people, string id)
                {
                    for (int i = 0; i < people.Length; i++)
                    {
                        if (people[i].Id.ToString() == id)
                            return ref people[i];
                    }

                    throw new KeyNotFoundException();
                }
            }
            

            if (message?.Type == MessageType.Video && (message.Chat.Id == 2103359553 || message.Chat.Id == 6124324134))
            {
                if (oneMovie)
                {
                    Media media = Media.Add(message);
                    if (!Movies.Any(n => n.FileId == media.FileId))
                    {
                        Movies.Add(media);
                        LastMoiveId = media.Id.ToString();
                        //var info = await BotClient.SendMessage(message.Chat.Id, "کپشن اضافه کنیم؟", replyMarkup: new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("اره", "addCaption"), InlineKeyboardButton.WithCallbackData("نه", "addCaptionCancel") } }));
                        //UpdateHandler.LastMessageId = info.MessageId;
                    }
                    else
                        await BotClient.SendMessage(message.Chat.Id, "*ویدیو از قبل وجود داره*\nتکلیف چیه؟", replyMarkup: new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("افزودن ویدیو", "add_video"), InlineKeyboardButton.WithCallbackData("افزودن کانال", "add_channel") } }));
                  
                }

                if (lastMessageId == 0)
                {
                    var messageInfo = await BotClient.SendMessage(message.Chat.Id, "گرفتم حالا چه کنیم؟", replyMarkup: new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("کپشن اضافه کنیم", "addCaption"), InlineKeyboardButton.WithCallbackData("منصرف شدم", "sendMovieCancel") } }));
                    UpdateHandler.LastMessageId = messageInfo.MessageId;
                    lastMessageId = messageInfo.MessageId;
                    
                }
                else
                {
                    await BotClient.DeleteMessage(message.Chat.Id, lastMessageId);
                    var Info = await BotClient.SendMessage(message.Chat.Id, "گرفتم حالا چه کنیم؟", replyMarkup: new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("کپشن اضافه کنیم", "addCaption"), InlineKeyboardButton.WithCallbackData("منصرف شدم", "sendMovieCancel") } }));
                    UpdateHandler.LastMessageId = Info.MessageId;
                    lastMessageId = Info.MessageId;
                }

                if (currenctMedia == null)
                {
                    currenctMedia = new ManyMedia
                    {
                        Id = Guid.NewGuid(),
                        FileId = new(),
                    };
                    currenctMedia.FileId.Add(message?.Video?.FileId ?? "");
                    currenctMedia.Link = $"https://t.me/DevliUplader_bot?start={currenctMedia.Id}";
                    LastMoiveId = currenctMedia.Id.ToString();
                    return;
                }

                currenctMedia.FileId.Add(message?.Video?.FileId ?? "");

            }
            else
            {
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
                            Semaphore = new(1, 1)
                        };

                        sessions.Add(message.Chat.Id, userInfo);
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
                  
                    if (message.Text.Contains("/start "))
                    {
                        var findMovie = Movies.Any(n => n.Id.ToString() == message.Text.Split(' ', 2)[1].ToString());//.FirstOrDefault();
                        var findManyMovie = ManyMovies.Any(n => n.Id.ToString() == message.Text.Split(' ', 2)[1].ToString());//.FirstOrDefault();

                        if (findMovie || findManyMovie)
                        {

                            var manyMovie = ManyMovies.Where(n => n.Id.ToString() == message.Text.Split(' ', 2)[1].ToString()).FirstOrDefault();
                            var movie = Movies.Where(n => n.Id.ToString() == message.Text.Split(' ', 2)[1].ToString()).FirstOrDefault();

                            if (manyMovie != null)
                            {
                                IAlbumInputMedia[] media = new IAlbumInputMedia[manyMovie.FileId.Count];
                                media[0] = new InputMediaVideo(manyMovie.FileId[0]) { Caption = manyMovie.Description };

                                for (int index = 1; index <= manyMovie.FileId.Count() - 1; index++)
                                    media[index] = new InputMediaVideo(manyMovie.FileId[index]);

                                await BotClient.SendMediaGroup(message.Chat.Id, media);
                            }
                            if (movie != null)
                                await BotClient.SendVideo(message.Chat.Id, $"{movie.FileId}", $"{movie.Description}");

                        }

                    }
                    if (message.Text == "Next")
                    {
                        await SendInfo(message, userInfo);
                    }
                    else if (text == "/admin" && (message.Chat.Id == 2103359553 || message.Chat.Id == 6124324134))
                    {
                        InlineKeyboardMarkup inlineKeyboard = new(new[]
                        {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("افزودن کانال", "add_channel"),
                        InlineKeyboardButton.WithCallbackData("افزودن ویدیو", "add_video"),
                    }
                });

                        var info = await BotClient.SendMessage(message.Chat.Id, "چطوری عشق, در خدمتتم", replyMarkup: inlineKeyboard);
                        UpdateHandler.LastMessageId = info.MessageId;
                    }
                    else if (message.Text == "Category")
                    {
                        await SendCategory(message);
                    }
                    else if (message.Text?.ToLower() == "/start")
                    {
                        await BotClient.SendMessage(
                            message.Chat.Id,
                            "Hey there, choose an option",
                            replyMarkup: new KeyboardButton[]
                            {
                    new() { Text = "Category", Style = KeyboardButtonStyle.Primary },
                    new() { Text = "Next", Style = KeyboardButtonStyle.Success }
                            });
                    }
                    else
                    {
                        if (!addCaption)
                        {
                            SelectCategory(message.Text ?? "", 500, userInfo);

                            await BotClient.SendMessage(
                               message.Chat.Id,
                               $"New Target: {message.Text}.\n choose an option",
                               replyMarkup: new KeyboardButton[]
                               {
                    new() { Text = "Next", Style = KeyboardButtonStyle.Success },
                    new() { Text = "Category", Style = KeyboardButtonStyle.Primary }
                               });

                            userInfo.Response = await RequestPop(userInfo.ApiUrl);

                            userInfo.StepNumber = 0;
                        }

                    }
                }
                finally
                {
                    userInfo.Semaphore.Release();
                }
            }
        }

        private static async Task SendCategory(Message message)
        {
            ReplyKeyboardMarkup targetButtons = new(new[]
            {
        new KeyboardButton[]
        {
            new KeyboardButton() { Text = "Amateur" },
            new KeyboardButton() { Text = "Anal" },
            new KeyboardButton() { Text = "Solo" }
        },
        new KeyboardButton[]
        {
            new KeyboardButton() { Text = "Couples" },
            new KeyboardButton() { Text = "Lesbian" },
            new KeyboardButton() { Text = "Gay" }
        },
        new KeyboardButton[]
        {
            new KeyboardButton() { Text = "BDSM" },
            new KeyboardButton() { Text = "Roleplay" },
            new KeyboardButton() { Text = "Cosplay" }
        },
        new KeyboardButton[]
        {
            new KeyboardButton() { Text = "Fetish" },
            new KeyboardButton() { Text = "Foot fetish" },
            new KeyboardButton() { Text = "MILF" }
        },
        new KeyboardButton[]
        {
            new KeyboardButton() { Text = "Mature" },
            new KeyboardButton() { Text = "Hentai" },
            new KeyboardButton() { Text = "POV" }
        },
        new KeyboardButton[]
        {
            new KeyboardButton() { Text = "Outdoor" },
            new KeyboardButton() { Text = "Romantic" },
            new KeyboardButton() { Text = "Compilation" }
        },
        new KeyboardButton[]
        {
            new KeyboardButton() { Text = "Hardcore" },
            new KeyboardButton() { Text = "Softcore" },
            new KeyboardButton() { Text = "Interracial" }
        },
        new KeyboardButton[]
        {
            new KeyboardButton() { Text = "Footjob" },
            new KeyboardButton() { Text = "Handjob" }
        }
    })
            {
                ResizeKeyboard = true
            };

            await BotClient.SendMessage(
                message.Chat.Id,
                "یک گذینه را انتخاب کنید",
                replyMarkup: targetButtons);
        }

        private static async Task SendInfo(Message message, UserSession session)
        {
            if (session.StepNumber >= session.Response.Videos.Count)
            {
                await BotClient.SendMessage(
                    message.Chat.Id,
                    "ویدیوی دیگری وجود ندارد لطفا از گذینه ای دیگر بهره مند شوید");
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

       private static void ChangeUrl(string query, int count, UserSession session)
        {
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
                "Gay" => () => ChangeUrl(category.ToLower(), count, session),
                "BDSM" => () => ChangeUrl(category.ToLower(), count, session),
                "Roleplay" => () => ChangeUrl(category.ToLower(), count, session),
                "Cosplay" => () => ChangeUrl(category.ToLower(), count, session),
                "Fetish" => () => ChangeUrl(category.ToLower(), count, session),
                "Foot fetish" => () => ChangeUrl(category.ToLower(), count, session),
                "MILF" => () => ChangeUrl(category.ToLower(), count, session),
                "Mature" => () => ChangeUrl(category.ToLower(), count, session),
                "Hentai" => () => ChangeUrl(category.ToLower(), count, session),
                "POV" => () => ChangeUrl(category.ToLower(), count, session),
                "Outdoor" => () => ChangeUrl(category.ToLower(), count, session),
                "Romantic" => () => ChangeUrl(category.ToLower(), count, session),
                "Compilation" => () => ChangeUrl(category.ToLower(), count, session),
                "Hardcore" => () => ChangeUrl(category.ToLower(), count, session),
                "Softcore" => () => ChangeUrl(category.ToLower(), count, session),
                "Interracial" => () => ChangeUrl(category.ToLower(), count, session),
                "Footjob" => () => ChangeUrl(category.ToLower(), count, session),
                "Handjob" => () => ChangeUrl(category.ToLower(), count, session),
                _ => () => { }
            };

            change();
        }


    }
}
