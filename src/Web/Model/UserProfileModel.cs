namespace Web.Model
{
    using System.Collections.Generic;

    public class UserProfileModel
    {
        public string Id { get; set; }
        public List<string> ThumbsUpNames { get; set; }
        public List<string> ThumbsDownNames { get; set; }
        
        public List<CommentInfo> Comments { get; set; }

        public UserProfileModel(string id, List<string> thumbsUpNames, List<string> thumbsDownNames, List<CommentInfo> comments)
        {
            this.Id = id;
            this.ThumbsUpNames = thumbsUpNames;
            this.ThumbsDownNames = thumbsDownNames;
            this.Comments = comments;
        }
    }
}
