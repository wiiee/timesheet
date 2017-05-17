namespace Web.Model
{
    using System;

    public class CommentInfo
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public DateTime Time { get; set; }

        public CommentInfo(string name, string comment, DateTime time)
        {
            this.Name = name;
            this.Comment = comment;
            this.Time = time;
        }
    }
}
