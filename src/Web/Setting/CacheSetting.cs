namespace Web.Setting
{
    using Platform.Setting;

    public static class CacheSetting
    {
        public static bool IsUseCache(string serviceName, ISetting setting)
        {
            var names = setting.Get("CacheSetting:CacheNames");

            if (!string.IsNullOrEmpty(names))
            {
                return names.Contains(serviceName);
            }

            return false;
        }
    }
}
