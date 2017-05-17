namespace Platform.Util
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public static class TypeUtil
    {
        public static List<string> GetNamesWithDateTime(Type type)
        {
            List<string> names = new List<string>();

            // get all public static properties of MyClass type
            PropertyInfo[] propertyInfos;
            propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if(propertyInfo.PropertyType == typeof(DateTime))
                {
                    names.Add(propertyInfo.Name);
                }
            }

            return names;
        }
    }
}
