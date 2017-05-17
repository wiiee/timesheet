using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Model
{
    public class GroupModel
    {
        public string Id { get; set; }
        public string Name { get;  set; }
        public HashSet<string> OwnerIds { get; set; }
        public List<UserInfo> Users { get; set; }

        public GroupModel(string id, string name, HashSet<string> ownerIds, List<UserInfo> users)
        {
            this.Id = id;
            this.Name = name;
            this.OwnerIds = ownerIds;
            this.Users = users;
        }

        public GroupModel() { }
    }

    public class UserInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public UserInfo(string id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public UserInfo() { }
    }
}
