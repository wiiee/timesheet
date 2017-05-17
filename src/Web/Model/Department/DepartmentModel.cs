namespace Web.Model.Department
{
    using System.Collections.Generic;
    using Entity.User;

    public class DepartmentModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public List<User> Owners { get; set; }
        public List<UserGroupModel> UserGroupModels { get; set; }

        public bool IsEditOrDelete { get; set; }

        public DepartmentModel(string id, string name, int number, List<User> owners, List<UserGroupModel> userGroupModels,
            bool isAdd, bool isEditOrDelete, bool isAddUserGroup)
        {
            this.Id = id;
            this.Name = name;
            this.Owners = owners;
            this.Number = number;
            this.UserGroupModels = userGroupModels;
            this.IsEditOrDelete = isEditOrDelete;
        }
    }
}
