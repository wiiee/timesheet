namespace Platform.Util
{
    using Newtonsoft.Json;

    public static class JsonUtil
    {
        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T FromJson<T>(string item)
        {
            return JsonConvert.DeserializeObject<T>(item);
        }
    }
}
