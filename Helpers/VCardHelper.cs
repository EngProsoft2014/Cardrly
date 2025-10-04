using Cardrly.Models.Card;

namespace Cardrly.Helpers
{
    public static class VCardHelper
    {
        public static string GenerateVCard(CardDetailsResponse Card)
        {
            return "BEGIN:VCARD\n" +
                "VERSION:3.0\n" +
                $"FN:{Card.PersonName + " " + Card.PersonNikeName}\n" +
                $"ADR:{Card.location}\n" +
                $"URL:{Card.CardUrlVM}\n" +
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
