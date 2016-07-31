using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PCLCrypto;

namespace GPSOAuthSharp
{
    public class GPSOAuthClient
    {
        private const string Version = "0.0.5";
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

        private readonly string _email;
        private readonly string _password;

        public GPSOAuthClient(string email, string password)
        {
            _email = email;
            _password = password;
        }

        public async Task<Dictionary<string, string>> PerformMasterLogin(string service = "ac2dm", string deviceCountry = "us", string operatorCountry = "us", string lang = "en", int sdkVersion = 21)
        {
            string signature = GoogleKeyUtils.CreateSignature(_email, _password, AndroidKey);

            return await PerformAuthRequest(new Dictionary<string, string>
            {
                {"accountType", "HOSTED_OR_GOOGLE"},
                {"Email", _email},
                {"has_permission", 1.ToString()},
                {"add_account", 1.ToString()},
                {"EncryptedPasswd", signature},
                {"service", service},
                {"source", "android"},
                {"device_country", deviceCountry},
                {"operatorCountry", operatorCountry},
                {"lang", lang},
                {"sdk_version", sdkVersion.ToString()}
            }).ConfigureAwait(false);
        }

        public async Task<Dictionary<string, string>> PerformOAuth(string masterToken, string service, string app, string clientSig, string deviceCountry = "us", string operatorCountry = "us", string lang = "en", int sdkVersion = 21)
        {
            return await PerformAuthRequest(new Dictionary<string, string>
            {
                {"accountType", "HOSTED_OR_GOOGLE"},
                {"Email", _email},
                {"has_permission", 1.ToString()},
                {"EncryptedPasswd", masterToken},
                {"service", service},
                {"source", "android"},
                {"app", app},
                {"client_sig", clientSig},
                {"device_country", deviceCountry},
                {"operatorCountry", operatorCountry},
                {"lang", lang},
                {"sdk_version", sdkVersion.ToString()}
            }).ConfigureAwait(false);
        }

        private async Task<Dictionary<string, string>> PerformAuthRequest(Dictionary<string, string> data)
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
