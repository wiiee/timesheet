namespace Entity.User
{
    using System;
    using System.Collections.Generic;

    public class Profile : BaseEntity
    {
        //Id为UserId

        public HashSet<string> ThumbsUpIds { get; set; }
        public HashSet<string> ThumbsDownIds { get; set; }

        //点评的用户
        public Dictionary<string, List<KeyValuePair<string, DateTime>>> Comments { get; set; }

        //
        public List<KeyValuePair<string, DateTime>> Feelings { get; set; }
    }
}
