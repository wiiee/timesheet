namespace Web.Model.Department
{
    using Entity.User;
    using System.Collections.Generic;

    public class UserGroupModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<User> Owners { get; set; }
        public bool IsTest { get; set; }
        public List<User> Users { get; set; }

        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }

        public UserGroupModel(string id, string name, List<User> owners, List<User> users, bool isTest, bool isEdit, bool isDelete)
        {
            this.Id = id;
            this.Name = name;
            this.Owners = owners;
            this.Users = users;
            this.IsTest = isTest;
            this.IsEdit = isEdit;
            this.IsDelete = isDelete;
        }
    }
}
