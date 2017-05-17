namespace Web.Extension
{
    using Entity.User;
    using Entity.ValueType;
    using Model.Department;
    using Platform.Context;
    using Platform.Enum;
    using Service;
    using Service.User;
    using System.Collections.Generic;
    using System.Linq;

    public static class DepartmentExtension
    {
        public static DepartmentModel Convert(this Department department, User user)
        {
            var isAdd = false;
            var isEditOrDelete = false;
            var isAddUserGroup = false;

            if(user.UserType == UserType.Admin)
            {
                isAdd = true;
                isEditOrDelete = true;
                isAddUserGroup = true;
            }
            else if(user.UserType == UserType.Manager)
            {
                isAddUserGroup = true;
                isEditOrDelete = true;
            }

            var owners = ServiceFactory.Instance.GetService<UserService>().GetByIds(department.OwnerIds);

            var userGroupModels = new List<UserGroupModel>();

            foreach(var item in department.UserGroups)
            {
                userGroupModels.Add(Convert(item.Value, user));
            }

            var userIds = department.UserGroups.Values.SelectMany(o => o.UserIds).ToList();

            userIds.AddRange(owners.Where(o => o.AccountType == AccountType.Public).Select(o => o.Id).ToList());

            userIds = userIds.Distinct().ToList();
            
            return new DepartmentModel(department.Id, department.Id, userIds.Count, owners, userGroupModels, isAdd, isEditOrDelete, isAddUserGroup);
        }

        private static UserGroupModel Convert(UserGroup userGroup, User user)
        {
            bool isEdit = false;
            bool isDelete = false;

            if(user.UserType == UserType.Admin || user.UserType == UserType.Manager)
            {
                isEdit = true;
                isDelete = true;
            }
            else if(user.UserType == UserType.Leader && userGroup.OwnerIds.Contains(user.Id))
            {
                isEdit = true;
            }

            var userService = ServiceFactory.Instance.GetService<UserService>();

            var owners = userService.GetByIds(userGroup.OwnerIds);
            var users = userService.GetByIds(userGroup.UserIds);

            return new UserGroupModel(userGroup.Id, userGroup.Name, owners, users, userGroup.IsTest, isEdit, isDelete);
        }
    }
}
