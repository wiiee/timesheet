namespace Web.Model
{
    using System.Collections.Generic;

    public class ReviewTaskModel
    {
        public string ProjectId { get; set; }
        public int TaskId { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public string Description { get; set; }
        public string CodeReview { get; set; }
        public bool IsReviewed { get; set; }
        public int Value { get; set; }
        public Dictionary<string, int> Values { get; set; }

        public ReviewTaskModel()
        {

        }

        //User
        public ReviewTaskModel(string projectId, int taskId, string name, string userId, string description, string codeReview, bool isReviewed, int value)
        {
            this.ProjectId = projectId;
            this.TaskId = taskId;
            this.Name = name;
            this.UserId = userId;
            this.Description = description;
            this.CodeReview = codeReview;
            this.IsReviewed = isReviewed;
            this.Value = value;
        }

        //Leader
        public ReviewTaskModel(string projectId, int taskId, string name, string userId, string description, string codeReview, bool isReviewed, Dictionary<string, int> values)
        {
            this.ProjectId = projectId;
            this.TaskId = taskId;
            this.Name = name;
            this.UserId = userId;
            this.Description = description;
            this.CodeReview = codeReview;
            this.IsReviewed = isReviewed;
            this.Values = values;
        }
    }
}
