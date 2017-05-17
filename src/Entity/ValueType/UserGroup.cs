namespace Entity.ValueType
{
    using System.Collections.Generic;

    public class UserGroup
    {
        private string _id;

        //Id为[DepartmentId]_[UserGroupName]
        public string Id
        {
            get
            {
                if(string.IsNullOrEmpty(_id))
                {
                    _id = DepartmentId + "_" + Name;
                }

                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public string DepartmentId { get; set; }

        public string Name { get; set; }

        public HashSet<string> OwnerIds { get; set; }

        public HashSet<string> UserIds { get; set; }

        public bool IsTest { get; set; }

        public UserGroup() { }

        public UserGroup(string departmentId, string name, HashSet<string> ownerIds, HashSet<string> userIds, bool isTest)
        {
            this.Id = departmentId + "_" + name;
            this.DepartmentId = departmentId;
            this.Name = name;
            this.OwnerIds = ownerIds;
            this.UserIds = userIds;
            this.IsTest = isTest;
        }
    }
}
