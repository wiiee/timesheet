namespace Platform.Util
{
    using ImageProcessorCore;
    using System;
    using System.IO;

    public static class ImgUtil
    {
        public static byte[] Resize(byte[] content, int destHeight, int destWidth)
        {
            var srcStream = new MemoryStream(content);
            var output = new MemoryStream();
            var imgSource = new Image(srcStream);

            var height = Math.Min(imgSource.Height, destHeight);
            var width = Math.Min(imgSource.Width, destWidth);

            imgSource.Resize(width, height)
                .Save(output);

            return output.ToArray();
        }

        public static string GetImgUrl(string imgId)
        {
            //return "api/img/" + imgId;
            return "http://yimeiyiyou.oss-cn-shenzhen.aliyuncs.com/pics/" + imgId + ".jpg";
        }
    }
}
