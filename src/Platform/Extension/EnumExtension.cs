namespace Platform.Extension
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using Model;

    public static class EnumExtension
    {
        //Get enum dictionary
        public static List<OptionModel> GetEnumsWithName(this Enum e)
        {
            var result = new List<OptionModel>();

            foreach (var item in Enum.GetValues(e.GetType()))
            {
                result.Add(new OptionModel(item.ToString(), (((int)item)).ToString(), Convert.ToInt32(e) == (int)item));
            }

            return result;
        }

        public static string GetDescription(this Enum e)
        {
            Type t = e.GetType();
            string name = Enum.GetName(t, e);
            FieldInfo fieldInfo = t.GetField(name);
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : name;
        }
    }
}
