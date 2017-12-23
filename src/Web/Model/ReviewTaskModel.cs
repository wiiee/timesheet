namespace Web.Model
{
    using System;
    using System.Collections.Generic;

    public class ReviewTaskModel
    {
        public string ProjectId { get; set; }
        public DateTime LastUpdate { get; set; }
        public List<ReviewTaskItem> Items { get; set; }

        public ReviewTaskModel()
        {

        }

        public ReviewTaskModel(string projectId, DateTime lastUpdate, List<ReviewTaskItem> items)
        {
            this.ProjectId = projectId;
            this.LastUpdate = lastUpdate;
            this.Items = items;
        }
    }
}
