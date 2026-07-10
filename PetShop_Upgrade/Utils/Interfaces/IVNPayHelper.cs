namespace PetShop_Upgrade.Utils.Interfaces
{
    public interface IVNPayHelper
    {
        string HmacSHA512(string key, string inputData);
        string BuildQueryString(SortedDictionary<string, string> data, bool urlEncode);
    }
}
