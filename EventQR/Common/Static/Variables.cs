using System.Security.Cryptography;
using System.Text;
using System.Web;
namespace EventQR.Common.Static
{
    public static class Variables
    {
        private static string HostUrl = "http://foodcoupon.bitprosofttech.com/";

        public static string OrgProfilePicsPath = $"/ApplicationDocs/Organizer/ProfilePics/";
        public static string OrgLogoPath = $"/ApplicationDocs/Organizer/Logos/";

        private static IConfiguration Configuration { get; }

        static Variables()
        {
            // Build configuration Setting path for appsetting.jsonfile
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
        }
        private static string EncryptionKey => Configuration["ConnectionStrings:EncryptionKey"];
        public static string GetMyTicketUri(Guid guestId, Guid eventId)
        {
            return $"/Admin/Guests/ShowMyTicket?guestId={guestId}&eventId={eventId}";
        }

        public static string GetQrCodeUriStr(Guid guestId, Guid eventId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(HostUrl);
            sb.Append("/Scanner/CheckIn/AllowGuest?");
            var parameters = $"guestId={guestId}&eventId={eventId}";
            var param = Encrypt(parameters);
            sb.Append(param);
            return sb.ToString();
        }


        public static string Encrypt(string clearText)
        {

            byte[] clearBytes = Encoding.UTF8.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            clearText = clearText.Replace("/", "_");
            return clearText;

        }

        public static string Decrypt(string cipherText)
        {
            cipherText = cipherText.Replace("_", "/");
            byte[] cipherBytes = Convert.FromBase64String(cipherText); // Convert from Base64 to byte array
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }


        public static string GenerateTicketKey(string guestId, string eventId) => guestId + "|" + eventId;
        public static string GetMerchantLogoUrl(string OrganizerId) => $"{OrgLogoPath.Replace("/", "\\")}\\{OrganizerId}";
        public static string GetMerchantProfilePicUrl(string OrganizerId) => $"{OrgProfilePicsPath.Replace("/", "\\")}\\{OrganizerId}";

        public static List<string> TicketViewNamesList
        {
            get
            {
                return new List<string>()
                {
                    "ShowMyTicket",
                    "ShowMyTicket1",
                    "ShowMyTicket2",
                    "ShowMyTicket3",
                    "ShowMyTicket4"
                };
            }
        }

    }
}
