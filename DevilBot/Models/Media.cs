using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Telegram.Bot.Types;

namespace DevilBot.Models
{
    public class Media
    {
        public Guid Id { get; set; }
        public string Link { get; set; }
        public string? FileId { get; set; }
        public string? Description { get; set; }

        public static Media Add(Message message)
        {
            Media media = new()
            {
                Id = Guid.NewGuid(),
                FileId = message?.Video?.FileId,
            };
            media.Link = $"https://t.me/DevliUplader_bot?start={media.Id}";

            return media;
        }
    }
}
