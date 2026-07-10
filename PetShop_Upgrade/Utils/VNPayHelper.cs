using PetShop_Upgrade.Utils.Interfaces;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace PetShop_Upgrade.Utils
{
    public class VNPayHelper : IVNPayHelper
    {
        public string BuildQueryString(SortedDictionary<string, string> data, bool urlEncode)
        {
            var sb = new StringBuilder();
            foreach (var kv in data)
            {
                if (string.IsNullOrEmpty(kv.Value)) 
                    continue;

                if (sb.Length > 0) 
                    sb.Append('&');

                sb.Append(kv.Key);
                sb.Append('=');

                if (urlEncode)
                {
                    sb.Append(WebUtility.UrlEncode(kv.Value));
                }
                else
                {
                    sb.Append(kv.Value);
                }
            }
            return sb.ToString();
        }

        public string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);

            using var hmac = new HMACSHA512(keyBytes);
            var hashValue = hmac.ComputeHash(inputBytes);
            foreach (var b in hashValue)
                hash.Append(b.ToString("x2"));

            return hash.ToString();
        }
    }
}
