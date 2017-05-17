namespace Platform.Model.BootstrapFileInput
{
    public class InitialPreview
    {
        private string imgId;
        private string className;
        private string alt;
        private string title;

        public InitialPreview(string imgId, string className, string alt, string title)
        {
            this.imgId = imgId;
            this.className = className;
            this.alt = alt;
            this.title = title;
        }

        public string GetPreview()
        {
            return string.Format("<img src='../api/img/{0}' class='{1}' alt='{2}' title='{3}'>", imgId, className, alt, title);
        }
    }
}