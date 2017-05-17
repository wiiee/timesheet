namespace Web.Model.Chart.Bubble
{
    using System.Collections.Generic;

    public class BubbleModel
    {
        public List<BubbleItem> Bubbles { get; set; }

        public BubbleModel(List<BubbleItem> bubbles)
        {
            this.Bubbles = bubbles;
        }
    }
}
