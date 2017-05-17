namespace Platform.Util
{
    using System.Text;

    public static class StringUtil
    {
        private static UTF8Encoding encoding = new UTF8Encoding();

        public static byte[] StrToByteArray(string str)
        {
            return encoding.GetBytes(str);
        }

        public static string ByteArrayToStr(byte[] bytes)
        {
            if(bytes == null)
            {
                return null;
            }

            return encoding.GetString(bytes);
        }
    }
}
