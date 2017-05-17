namespace Platform.Util
{
    using System;
    using System.Collections.Generic;
    using Extension;
    using System.Reflection;

    public static class EnumUtil
    {
        private const string INVALID_TYPE = "{0} not a Enum type, please check.";

        public static T ParseEnum<T>(string value)
        {
            Type t = typeof(T);
            if (t.GetTypeInfo().IsEnum)
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            else
            {
                throw new Exception(string.Format(INVALID_TYPE, t));
            }
        }

        // used for javascript
        public static string GetOptions<T>()
        {
            Type t = typeof(T);

            if (t.GetTypeInfo().IsEnum)
            {
                var result = "[";

                foreach (var item in Enum.GetValues(t))
                {
                    result += @"{text: """ + item.ToString() + @""", value: """ + (int)item + @"""}, ";
                }

                result = result.TrimEnd();
                result = result.TrimEnd(',');

                result += "]";

                return result;
            }
            else
            {
                throw new Exception(string.Format(INVALID_TYPE, t));
            }
        }

        // used for javascript
        public static string GetOptionsWithDescription<T>()
        {
            Type t = typeof(T);

            if (t.GetTypeInfo().IsEnum)
            {
                var result = "[";

                foreach (var item in Enum.GetValues(t))
                {
                    result += @"{text: """ + ((Enum)item).GetDescription() + @""", value: """ + (int)item + @"""}, ";
                }

                result = result.TrimEnd();
                result = result.TrimEnd(',');

                result += "]";

                return result;
            }
            else
            {
                throw new Exception(string.Format(INVALID_TYPE, t));
            }
        }

        public static Dictionary<int, string> GetEnumsWithDescription<T>()
        {
            Type t = typeof(T);

            if (t.GetTypeInfo().IsEnum)
            {
                Dictionary<int, string> result = new Dictionary<int, string>();
                
                foreach (var item in Enum.GetValues(t))
                {
                    result.Add((int)item, ((Enum)item).GetDescription());
                }

                return result;
            }
            else
            {
                throw new Exception(string.Format(INVALID_TYPE, t));
            }
        }
    }
}
