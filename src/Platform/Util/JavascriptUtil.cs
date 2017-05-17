namespace Platform.Util
{
    using System.Collections.Generic;

    public static class JavascriptUtil
    {
        public static string GetArrays(List<string> items)
        {
            var result = "[";

            foreach (var item in items)
            {
                result += "\"" + item + "\",";
            }

            result = result.TrimEnd();
            result = result.TrimEnd(',');

            result += "]";

            return result;
        }

        public static string GetArrays(List<double> items)
        {
            var result = "[";

            foreach (var item in items)
            {
                result +=  item + ",";
            }

            result = result.TrimEnd();
            result = result.TrimEnd(',');

            result += "]";

            return result;
        }
    }
}
