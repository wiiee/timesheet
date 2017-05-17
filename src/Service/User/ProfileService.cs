namespace Service.User
{
    using Entity.User;
    using Platform.Context;
    using System.Collections.Generic;
    using System;

    public class ProfileService : BaseService<Profile>
    {
        public ProfileService(IContextRepository contextRepository) : base(contextRepository) { }

        public void AddComment(string userId, string comment, string commentUserId)
        {
            var profile = Get(userId);

            if(profile == null)
            {
                profile = new Profile();
                profile.Id = userId;
                profile.Comments = new Dictionary<string, List<KeyValuePair<string, DateTime>>>();
                var comments = new List<KeyValuePair<string, DateTime>>();
                comments.Add(new KeyValuePair<string, DateTime>(comment, DateTime.Now));
                profile.Comments.Add(commentUserId, comments);

                Create(profile);
            }
            else
            {
                if(profile.Comments == null)
                {
                    profile.Comments = new Dictionary<string, List<KeyValuePair<string, DateTime>>>();
                }

                if(profile.Comments.ContainsKey(commentUserId))
                {
                    profile.Comments[commentUserId].Add(new KeyValuePair<string, DateTime>(comment, DateTime.Now));
                }
                else
                {
                    var comments = new List<KeyValuePair<string, DateTime>>();
                    comments.Add(new KeyValuePair<string, DateTime>(comment, DateTime.Now));
                    profile.Comments.Add(commentUserId, comments);
                }

                Update(userId, "Comments", profile.Comments);
            }
        }

        public void ThumbsUp(string userId, string thumbsUpId)
        {
            var profile = Get(userId);

            if(profile == null)
            {
                profile = new Profile();
                profile.Id = userId;
                profile.ThumbsUpIds = new HashSet<string>(new string[] { thumbsUpId });
                Create(profile);
            }
            else
            {
                if(profile.ThumbsUpIds == null)
                {
                    profile.ThumbsUpIds = new HashSet<string>(new string[] { thumbsUpId });
                }
                else
                {
                    profile.ThumbsUpIds.Add(thumbsUpId);
                }

                Update(userId, "ThumbsUpIds", profile.ThumbsUpIds);
            }
        }

        public void ThumbsDown(string userId, string thumbsDownId)
        {
            var profile = Get(userId);

            if (profile == null)
            {
                profile = new Profile();
                profile.Id = userId;
                profile.ThumbsDownIds = new HashSet<string>(new string[] { thumbsDownId });
                Create(profile);
            }
            else
            {
                if (profile.ThumbsDownIds == null)
                {
                    profile.ThumbsDownIds = new HashSet<string>(new string[] { thumbsDownId });
                }
                else
                {
                    profile.ThumbsDownIds.Add(thumbsDownId);
                }

                Update(userId, "ThumbsDownIds", profile.ThumbsDownIds);
            }
        }
    }
}
