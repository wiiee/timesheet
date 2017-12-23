namespace Web.Model
{
    using System.Collections.Generic;

    public class ReviewTaskItem
    {
        public int TaskId { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public string Description { get; set; }
        public string CodeReview { get; set; }
        public bool IsReviewed { get; set; }
        public int Value { get; set; }
        public Dictionary<string, int> Values { get; set; }

        public ReviewTaskItem()
        {

        }

        //User
        public ReviewTaskItem(int taskId, string name, string userId, string description, string codeReview, bool isReviewed, int value)
        {
            this.TaskId = taskId;
            this.Name = name;
            this.UserId = userId;
            this.Description = description;
            this.CodeReview = codeReview;
            this.IsReviewed = isReviewed;
            this.Value = value;
        }

        //Leader
        public ReviewTaskItem(int taskId, string name, string userId, string description, string codeReview, bool isReviewed, Dictionary<string, int> values)
        {
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
