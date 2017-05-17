namespace Platform.Model.BootstrapFileInput
{
    public class InitialPreviewConfig
    {
        public string caption { get; set; }
        public string width { get; set; }
        public string url { get; set; }
        public string key { get; set; }
        public object extra { get; set; }

        public InitialPreviewConfig(string caption, string width, string url, string key, object extra)
        {
            this.caption = caption;
            this.width = width;
            this.url = url;
            this.key = key;
            this.extra = extra;
        }
    }
}
