namespace Web.Model.Other
{
    using Entity.Other;

    public class FeedbackModel
    {
        public Feedback Feedback { get; set; }
        public string SuggesterName { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }

        public FeedbackModel(Feedback feedback, string suggesterName, bool isEdit, bool isDelete)
        {
            this.Feedback = feedback;
            this.SuggesterName = suggesterName;
            this.IsEdit = isEdit;
            this.IsDelete = isDelete;
        }
    }
}
