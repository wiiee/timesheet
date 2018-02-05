namespace Web.Model
{
    using System;
    using System.Collections.Generic;

    public class ReviewTaskModel
    {
        public string ProjectId { get; set; }
        public DateTime LastUpdate { get; set; }
        //是否为用户模式
        public bool IsUserMode { get; set; }
        public List<ReviewTaskItem> Items { get; set; }

        public ReviewTaskModel()
        {

        }

        public ReviewTaskModel(string projectId, DateTime lastUpdate, bool isUserMode, List<ReviewTaskItem> items)
        {
            this.ProjectId = projectId;
            this.LastUpdate = lastUpdate;
            this.IsUserMode = isUserMode;
            this.Items = items;
        }
    }
}
