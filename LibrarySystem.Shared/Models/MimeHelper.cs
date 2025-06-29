using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace LibrarySystem_Shared.Models
{
    public static class MimeHelper
    {
        private static readonly Dictionary<string, string> MimeMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".bmp", "image/bmp" },
            { ".webp", "image/webp" }
        };

        public static string GetMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            return MimeMappings.ContainsKey(ext) ? MimeMappings[ext] : "image/jpeg";
        }
    }
}