namespace Platform.Model.BootstrapFileInput
{
    using System.Collections.Generic;

    public class SendingData
    {
        public List<InitialPreview> initialPreviews { get; set; }
        public List<InitialPreviewConfig> initialPreviewConfigs { get; set; }

        public SendingData(List<InitialPreview> initialPreviews, List<InitialPreviewConfig> initialPreviewConfigs)
        {
            this.initialPreviewConfigs = initialPreviewConfigs;
            this.initialPreviews = initialPreviews;
        }
    }
}
