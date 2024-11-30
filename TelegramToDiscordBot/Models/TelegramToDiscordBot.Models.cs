using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramToDiscordBot.Models
{
    public class FileDetails
    {
        public required string FileId { get; set; }
        public required string FileName { get; set; }
        public required string FileType { get; set; }
        public required Stream FileStream { get; set; }
    }
}
