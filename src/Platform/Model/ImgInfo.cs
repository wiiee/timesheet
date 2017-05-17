namespace Platform.Model
{
    public class ImgInfo
    {
        public string url { get; set; }
        public string thumbnailUrl { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int size { get; set; }
        public string deleteUrl { get; set; }
        public string deleteType { get; set; }

        public ImgInfo()
        {

        }

        public ImgInfo(string url, string thumbnailUrl, string name, string type, int size, string deleteUrl, string deleteType)
        {
            this.url = url;
            this.thumbnailUrl = thumbnailUrl;
            this.name = name;
            this.type = type;
            this.size = size;
            this.deleteUrl = deleteUrl;
            this.deleteType = deleteType;
        }
    }
}
