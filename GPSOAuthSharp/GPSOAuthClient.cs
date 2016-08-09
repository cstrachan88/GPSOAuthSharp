using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PCLCrypto;

namespace GPSOAuthSharp
{
    public static class GPSOAuthClient
    {
        private const string Version = "1.0.0";
        private const string AuthUrl = "https://android.clients.google.com/auth";
        private const string UserAgent = "GPSOAuthSharp/" + Version;

        /// <summary>
        /// The key is distirbuted with Google Play Services. 
        /// This one is from version 7.3.29.
        /// </summary>
        private const string B64Key =
            "AAAAgMom/1a/v0lblO2Ubrt60J2gcuXSljGFQXgcyZWveWLEwo6prwgi3" +
            "iJIZdodyhKZQrNWp5nKJ3srRXcUW+F1BD3baEVGcmEgqaLZUNBjm057pK" +
            "RI16kB0YppeGx5qIQ5QjKzsR8ETQbKLNWgRY0QRNVz34kMJR3P/LgHax/" +
            "6rmf5AAAAAwEAAQ==";

        private static readonly RSAParameters AndroidKey = GoogleKeyUtils.KeyFromB64(B64Key);

        public static async Task<Dictionary<string, string>> PerformMasterLogin(string email, string password, string androidId,
            string service = "ac2dm", string deviceCountry = "us", string operatorCountry = "us",
            string lang = "en", int sdkVersion = 21)
        {
            return await PerformAuthRequest(new Dictionary<string, string>
            {
                {"accountType", "HOSTED_OR_GOOGLE"},
                {"Email", email},
                {"has_permission", "1"},
                {"add_account", "1"},
                {"EncryptedPasswd", GoogleKeyUtils.CreateSignature(email, password, AndroidKey)},
                {"service", service},
                {"source", "android"},
                {"androidId", androidId },
                {"device_country", deviceCountry},
                {"operatorCountry", operatorCountry},
                {"lang", lang},
                {"sdk_version", sdkVersion.ToString()}
            }).ConfigureAwait(false);
        }

        public static async Task<Dictionary<string, string>> PerformOAuth(string email, string masterToken, string androidId, string service, string app, string clientSig,
            string deviceCountry = "us", string operatorCountry = "us", string lang = "en", int sdkVersion = 21)
        {
            return await PerformAuthRequest(new Dictionary<string, string>
            {
                {"accountType", "HOSTED_OR_GOOGLE"},
                {"Email", email},
                {"has_permission", "1"},
                {"EncryptedPasswd", masterToken},
                {"service", service},
                {"source", "android"},
                {"androidId", androidId},
                {"app", app},
                {"client_sig", clientSig},
                {"device_country", deviceCountry},
                {"operatorCountry", operatorCountry},
                {"lang", lang},
                {"sdk_version", sdkVersion.ToString()}
            }).ConfigureAwait(false);
        }

        private static async Task<Dictionary<string, string>> PerformAuthRequest(Dictionary<string, string> data)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                string result;
                try
                {
                    var responseMessage = await client.PostAsync(AuthUrl, new FormUrlEncodedContent(data)).ConfigureAwait(false);
                    result = await responseMessage.Content.ReadAsStringAsync();
                }
                catch (WebException e)
                {
                    result = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                }
                return GoogleKeyUtils.ParseAuthResponse(result);
            }
        }
    }
}
