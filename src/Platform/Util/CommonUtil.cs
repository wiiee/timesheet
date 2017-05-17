namespace Platform.Util
{
    using System;

    public static class CommonUtil
    {
        public static string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
