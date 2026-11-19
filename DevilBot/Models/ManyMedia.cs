using System;
using System.Collections.Generic;
using System.Text;

namespace DevilBot.Models
{
    public class ManyMedia
    {
        public Guid Id { get; set; }
        public string Link { get; set; }
        public List<string> FileId { get; set; } 
        public string Description { get; set; }
    }
}
