namespace Platform.Setting
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;

    public class Setting : ISetting
    {
        private Setting() { }
        private static readonly Lazy<Setting> lazy = new Lazy<Setting>(() => new Setting());
        public static Setting Instance { get { return lazy.Value; } }

        private Dictionary<string, string> settings;
        private IConfiguration config;

        private object lockObj = new object();

        private const string ERROR = "please init it before use it";

        public void Init(IConfiguration config)
        {
            this.config = config;
            this.settings = new Dictionary<string, string>();
        }

        public string Get(string key)
        {
            lock(lockObj)
            {
                if (config == null)
                {
                    throw new Exception(ERROR);
                }

                if (settings.ContainsKey(key))
                {
                    return settings[key];
                }

                var result = config.GetSection(key).Value;

                settings.Add(key, result);

                return result;
            }
        }
    }
}
