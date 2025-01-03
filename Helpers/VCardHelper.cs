using Cardrly.Mode_s.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Helpers
{
    public static class VCardHelper
    {
        public static string GenerateVCard(CardResponse Card)
        {
            return "BEGIN:VCARD\n" +
                "VERSION:3.0\n" +
                $"FN:{Card.PersonName + " " + Card.PersonNikeName}\n" +
                $"ADR:{Card.location}\n" +
                $"URL:{Card.CardUrl}\n" +
                "END:VCARD"; 
        }

        public static async Task<string> SaveVCardAsync(string vCardContent, string fileName)
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            await File.WriteAllTextAsync(filePath, vCardContent);
            return filePath;
        }
    }
}
