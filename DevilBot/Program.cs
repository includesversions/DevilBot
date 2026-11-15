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

string apiUrl = "https://www.eporner.com/api/v2/video/search/?query=anal&per_page=100";
UserSession defaultSession = new() { ApiUrl = apiUrl, StepNumber = 0, Cts = new(), Response = await RequestPop(apiUrl) };
Dictionary<long, UserSession> sessions = new();
TelegramBotClient bot = new("7634355485:AAGIUhX2x8w34DB3nL1ZOQNtIw-sfaDsssU");

await RequestPop(apiUrl);

bot.OnMessage += Sender;

Console.WriteLine("ربات در حال اجراس");

Console.ReadLine();

async Task Sender(Message message, UpdateType type)
{
        UserSession userInfo;

        // ایجاد یا دریافت Session به شکل امن
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
            // دریافت اطلاعات اولیه
            if (!userInfo.IsInitialized)
            {
                userInfo.Response = await RequestPop(userInfo.ApiUrl);
                userInfo.IsInitialized = true;
            }

            if (message.Text == "Next")
            {
                await SendInfo(message, userInfo);
            }
            else if (message.Text == "Category")
            {
                await SendCategory(message);
            }
            else if (message.Text?.ToLower() == "/start")
            {
                await bot.SendMessage(
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
                SelectCategory(message.Text ?? "", 1000, userInfo);

                userInfo.Response = await RequestPop(userInfo.ApiUrl);

                userInfo.StepNumber = 0;

                await bot.SendMessage(
                    message.Chat.Id,
                    $"New Target: {message.Text}.\n choose an option",
                    replyMarkup: new KeyboardButton[]
                    {
                    new() { Text = "Next", Style = KeyboardButtonStyle.Success },
                    new() { Text = "Category", Style = KeyboardButtonStyle.Primary }
                    });
            }
        }
        finally
        {
            userInfo.Semaphore.Release();
        }
    
    //if (!sessions.TryGetValue(message.Chat.Id, out var userInfo))
    //{
    //    userInfo = new UserSession
    //    {
    //        ApiUrl = apiUrl,
    //        StepNumber = 0,
    //        Cts = new(),
    //        Response = await RequestPop(apiUrl)
    //    };

    //    sessions.Add(message.Chat.Id, userInfo);
    //}

    //if (message.Text == "Next")
    //{
    //    await SendInfo(message, userInfo);
    //}
    //else if (message.Text == "Category")
    //{
    //    SendCategory(message);
    //}
    //else if (message.Text?.ToLower() == "/start")
    //{
    //    Console.WriteLine("پیام ارسال شد");

    //    await bot.SendMessage(
    //         message.Chat.Id,
    //         "Hey there, choose an option",
    //         replyMarkup: new KeyboardButton[]
    //         {
    //            new() { Text = "Category", Style = KeyboardButtonStyle.Primary },
    //            new() { Text = "Next", Style = KeyboardButtonStyle.Success },
    //         });
    //}
    //else
    //{
    //    SelectCategory(message.Text, 1000, userInfo);

    //    userInfo.Response = await RequestPop(userInfo.ApiUrl);

    //    await bot.SendMessage(
    //         message.Chat.Id,
    //         $"New Target: {message.Text}.\n choose an option",
    //         replyMarkup: new KeyboardButton[]
    //         {
    //            new() { Text = "Next", Style = KeyboardButtonStyle.Success },
    //            new() { Text = "Category", Style = KeyboardButtonStyle.Primary }
    //         });

    //    userInfo?.StepNumber = 0;
    //}
}

async Task SendCategory(Message message)
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

    await bot.SendMessage(
        message.Chat.Id,
        "یک گذینه را انتخاب کنید",
        replyMarkup: targetButtons);
}

async Task SendInfo(Message message, UserSession session)
{
    if (session.StepNumber >= session.Response.Videos.Count)
    {
        await bot.SendMessage(
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
        new InputMediaPhoto(
            InputFile.FromUri(current.Thumbs.First().Src.ToString()))
        {
            Caption = text,
            ParseMode = ParseMode.Markdown
        },
        new InputMediaPhoto(
            InputFile.FromUri(current.Thumbs[1].Src.ToString())),
        new InputMediaPhoto(
            InputFile.FromUri(current.Thumbs.Last().Src.ToString()))
    };

    await bot.SendMediaGroup(
        chatId: message.Chat.Id,
        media: media,
        cancellationToken: session.Cts.Token);
}

async Task<VideoResponse> RequestPop(string url)
{
    HttpClient client = new();

    string res = await client.GetStringAsync(url);

    return JsonConvert.DeserializeObject<VideoResponse>(res) ?? new();
}

void ChangeUrl(string query, int count, UserSession session)
{
    session.ApiUrl = $"https://www.eporner.com/api/v2/video/search/?query={Uri.EscapeDataString(query)}&per_page={count}";
}

void SelectCategory(string category, int count, UserSession session)
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
