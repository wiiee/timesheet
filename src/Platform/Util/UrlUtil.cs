namespace Platform.Util
{
    public static class UrlUtil
    {
        public static string GetRelativeUrl(string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                return url;
            }
            else
            {
                return "/";
            }
        }
    }
}
