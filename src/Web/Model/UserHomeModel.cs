namespace Web.Model
{
    public class UserHomeModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }

        public bool IsThumbsUpActive { get; set; }
        public bool IsThumbsDownActive { get; set; }
        public bool IsCommentActive { get; set; }

        public string Logo { get; set; }
        public string Thumbnail { get; set; }

        public int ThumbsUp { get; set; }
        public int ThumbsDown { get; set; }
        public int Comment { get; set; }

        public UserHomeModel(string id, string name, string nickName, bool isThumbsUpActive, bool isThumbsDownActive, bool isCommentActive,
            string logo, string thumbnail, int thumbsUp, int thumbsDown, int comment)
        {
            this.Id = id;
            this.Name = name;
            this.NickName = nickName;
            this.IsThumbsDownActive = isThumbsDownActive;
            this.IsThumbsUpActive = isThumbsUpActive;
            this.IsCommentActive = isCommentActive;
            this.Logo = logo;
            this.Thumbnail = thumbnail;
            this.ThumbsUp = thumbsUp;
            this.ThumbsDown = thumbsDown;
            this.Comment = comment;
        }
    }
}
