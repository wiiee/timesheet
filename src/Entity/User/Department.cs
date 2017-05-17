namespace Entity.User
{
    using System.Collections.Generic;
    using System.Linq;
    using ValueType;

    public class Department : BaseEntity
    {
        //Id为部门名字

        public HashSet<string> OwnerIds { get; set; }

        //key为UserGroupId
        public Dictionary<string, UserGroup> UserGroups { get; set; }

        public Department(string id, HashSet<string> ownerIds, Dictionary<string, UserGroup> userGroups)
        {
            this.Id = id;
            this.OwnerIds = ownerIds;
            this.UserGroups = userGroups;
        }

        public Department() { }

        public List<string> GetLeaderIds()
        {
            List<string> result = new List<string>();
            result.AddRange(OwnerIds);
            result.AddRange(UserGroups.SelectMany(o => o.Value.OwnerIds).ToList());

            return result.Distinct().ToList();
        }

        public List<string> GetUserIds()
        {
            List<string> result = new List<string>();
            result.AddRange(OwnerIds);
            result.AddRange(UserGroups.SelectMany(o => o.Value.UserIds).ToList());

            return result.Distinct().ToList();
        }
    }
}