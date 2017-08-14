namespace Service.User
{
    using Entity.User;
    using Platform.Context;
    using System.Collections.Generic;
    using System.Linq;
    using Platform.Enum;
    using System.Linq.Expressions;
    using System;
    using Entity.ValueType;
    using Platform.Extension;
    using Model;
    public class DepartmentService : BaseService<Department>
    {
        public DepartmentService(IContextRepository contextRepository) : base(contextRepository) { }

        private object lockObj = new object();
        private HashSet<string> testerIds;

        public bool IsTester(string userId)
        {
            if(testerIds == null)
            {
                lock (lockObj)
                {
                    testerIds = new HashSet<string>();
                    var userGroups = GetUserGroupsWithList().Where(o => o.IsTest).ToList();
                    
                    foreach(var item in userGroups)
                    {
                        foreach(var id in item.UserIds)
                        {
                            testerIds.Add(id);
                        } 
                    }
                }
            }

            return testerIds.Contains(userId);
        }

        public List<string> GetLeaderIdsByUserId(string userId)
        {
            var user = ServiceFactory.Instance.GetService<UserService>().Get(userId);

            if(user == null)
            {
                return new List<string>();
            }
            else if(user.UserType == UserType.Admin)
            {
                return GetUserGroupsWithList().SelectMany(o => o.OwnerIds).ToList();
            }
            else if(user.UserType == UserType.Manager)
            {
                return Get().Where(o => o.OwnerIds.Contains(user.Id)).SelectMany(o => o.UserGroups.SelectMany(p => p.Value.OwnerIds)).Distinct().ToList();
            }
            else
            {
                return GetUserGroupsWithList().Where(o => o.UserIds.Contains(userId)).SelectMany(o => o.OwnerIds).Distinct().ToList();
            }
        }

        //得到所在部门的所有测试人员
        public List<string> GetTestUserIdsByUserId(string userId)
        {
            return Get().Where(o => o.UserGroups.SelectMany(p => p.Value.UserIds).Contains(userId))
                .SelectMany(o => o.UserGroups.Where(p => p.Value.IsTest).SelectMany(q => q.Value.UserIds))
                .ToList();
        }

        //得到所在部门的测试Leader
        public List<string> GetTestLeaderIdsByUserId(string userId)
        {
            return Get().Where(o => o.UserGroups.SelectMany(p => p.Value.UserIds).Contains(userId))
                .SelectMany(o => o.UserGroups.Where(p => p.Value.IsTest).SelectMany(q => q.Value.OwnerIds))
                .ToList();
        }

        public List<string> GetUserIdsByManagerId(string userId)
        {
            return Get().Where(o => o.OwnerIds.Contains(userId))
                .SelectMany(o => o.UserGroups.SelectMany(p => p.Value.UserIds))
                .ToList();
        }

        public List<string> GetUserIdsByDepartmentId(string departmentId)
        {
            return Get(departmentId).UserGroups.SelectMany(o => o.Value.UserIds).ToList();
        }

        public List<UserGroup> GetTestGroupsByDepartmentId(string departmentId)
        {
            return Get(departmentId).UserGroups.Where(o => o.Value.IsTest).Select(p => p.Value).ToList();
        }

        //得到下属成员
        public List<string> GetSubordinatesByUserId(string userId)
        {
            var user = ServiceFactory.Instance.GetService<UserService>().Get(userId);

            switch (user.UserType)
            {
                case UserType.Admin:
                    return ServiceFactory.Instance.GetService<UserService>().GetIds();
                case UserType.Manager:
                    var userIds = Get().Where(o => o.OwnerIds.Contains(userId))
                        .SelectMany(o => o.UserGroups.SelectMany(p => p.Value.UserIds)).Distinct().ToList();
                    userIds.Add(userId);
                    userIds = userIds.Distinct().ToList();
                    return userIds;
                case UserType.Leader:
                    return GetUserGroupsWithList()
                        .Where(o => o.OwnerIds.Contains(userId))
                        .SelectMany(o => o.UserIds)
                        .Distinct().ToList();
                default:
                    return new List<string>(new string[] { userId });
            }
        }

        //得到下属成员
        public List<string> GetSubordinatesByUserIds(IEnumerable<string> userIds)
        {
            var result = new List<string>();

            foreach (var item in userIds)
            {
                result.AddRange(GetSubordinatesByUserId(item));
            }

            return result.Distinct().ToList();
        }

        //得到用户所在的部门
        public List<Department> GetDepartmentsByUserId(string userId)
        {
            var user = ServiceFactory.Instance.GetService<UserService>().Get(userId);

            if (user != null)
            {
                if (user.UserType == UserType.Admin)
                {
                    return Get();
                }
                else if (user.UserType == UserType.Manager)
                {
                    return Get().Where(o => o.OwnerIds.Contains(userId)).ToList();
                }
            }

            return Get().Where(o => o.UserGroups.SelectMany(p => p.Value.UserIds).Contains(userId)).ToList();
        }

        //得到用户所在的部门
        public List<Department> GetDepartmentsByUserIds(List<string> userIds)
        {
            var result = new List<Department>();

            foreach (var item in userIds)
            {
                result.AddRange(GetDepartmentsByUserId(item));
            }

            return result.Distinct().ToList();
        }

        public bool IsBoss(string bossId, string memberId)
        {
            return GetSubordinatesByUserId(bossId).Contains(memberId);
        }

        public bool IsBoss(string bossId, IEnumerable<string> memberIds)
        {
            if (string.IsNullOrEmpty(bossId))
            {
                return false;
            }

            if (memberIds.IsEmpty())
            {
                return true;
            }

            return GetSubordinatesByUserId(bossId).Intersect(memberIds).Count() > 0;
        }

        public Dictionary<string, UserGroup> GetUserGroupsWithDictionary()
        {
            return Get().SelectMany(o => o.UserGroups).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public List<UserGroup> GetUserGroupsWithList()
        {
            return Get().SelectMany(o => o.UserGroups).Select(o => o.Value).ToList();
        }

        public List<UserGroup> GetUserGroupsByOwnerId(string ownerId)
        {
            return GetUserGroupsWithList().Where(o => o.OwnerIds.Contains(ownerId)).ToList();
        }

        public List<UserGroup> GetUserGroupsByOwnerIds(List<string> ownerIds)
        {
            return GetUserGroupsWithList().OrderBy(o => o.IsTest).Where(o => o.OwnerIds.Intersect(ownerIds).Count() > 0).ToList();
        }

        public UserGroup GetUserGroupById(string userGroupId)
        {
            var userGroups = GetUserGroupsWithDictionary();

            var userGroup = default(UserGroup);

            if (userGroups.ContainsKey(userGroupId))
            {
                userGroup = userGroups[userGroupId];
            }

            return userGroup;
        }

        //得到该用户所属的用户组
        public List<UserGroup> GetUserGroupsByUserId(string userId)
        {
            var user = ServiceFactory.Instance.GetService<UserService>().Get(userId);

            if(user == null)
            {
                return new List<UserGroup>();
            }

            if (user.UserType == UserType.Admin)
            {
                return GetUserGroupsWithList();
            }
            else if(user.UserType == UserType.Manager)
            {
                return Get().Where(o => o.OwnerIds.Contains(user.Id)).SelectMany(o => o.UserGroups.Values).ToList();
            }
            else
            {
                return GetUserGroupsWithList().Where(o => o.UserIds.Contains(userId)).ToList();
            }
        }

        public void UpdateUserGroup(UserGroup userGroup)
        {
            var departments = Get();

            foreach (var item in departments)
            {
                if (item.UserGroups.ContainsKey(userGroup.Id))
                {
                    item.UserGroups[userGroup.Id] = userGroup;
                    Update(item.Id, "UserGroups", item.UserGroups);
                    return;
                }
            }
        }

        public void AddUserGroup(UserGroup userGroup)
        {
            var department = Get(userGroup.DepartmentId);

            if (!department.UserGroups.ContainsKey(userGroup.Id))
            {
                department.UserGroups.Add(userGroup.Id, userGroup);
                Update(userGroup.DepartmentId, "UserGroups", department.UserGroups);
            }
        }

        public void DeleteUserGroup(string userGroupId)
        {
            var departments = Get();

            foreach (var item in departments)
            {
                if (item.UserGroups.ContainsKey(userGroupId))
                {
                    item.UserGroups.Remove(userGroupId);
                    Update(item.Id, "UserGroups", item.UserGroups);
                }
            }
        }

        public void DeleteUser(string userId)
        {
            var departments = Get();

            foreach (var item in departments)
            {
                bool isFind = false;

                foreach (var entry in item.UserGroups)
                {
                    if (entry.Value.OwnerIds.Contains(userId))
                    {
                        isFind = true;
                        entry.Value.OwnerIds.Remove(userId);
                    }

                    if (entry.Value.UserIds.Contains(userId))
                    {
                        isFind = true;
                        entry.Value.UserIds.Remove(userId);
                    }
                }

                if (isFind)
                {
                    Update(item.Id, "UserGroups", item.UserGroups);
                }
            }
        }

        public void DeleteUserGroups(List<string> userGroupIds)
        {
            foreach (var item in userGroupIds)
            {
                DeleteUserGroup(item);
            }
        }

        public void DeleteUserGroups(Expression<Func<UserGroup, bool>> selector)
        {
            var userGroupIds = GetUserGroupsWithList().Where(selector.Compile()).Select(o => o.Id).ToList();
            DeleteUserGroups(userGroupIds);
        }

        //获得当前用户项目相关组，开发只能添加本组人员+部门所有测试组；测试可以添加部门所有组
        public List<GroupModel> GetUserGroupModelsByOwnerIds(List<string> ownerIds)
        {
            var userGroups = GetUserGroupsWithList().OrderBy(o => o.IsTest).Where(o => o.OwnerIds.Intersect(ownerIds).Count() > 0).ToList();
            foreach (var userId in ownerIds)
            {
                var user = (User)ServiceFactory.Instance.GetService<UserService>().Get(userId);
                if (user.UserType == UserType.Manager || user.UserType == UserType.Admin)
                {
                    List<UserGroup> groups = GetUserGroupsByUserId(userId);
                    userGroups.AddRange(groups);
                }
            }
            userGroups = userGroups.Distinct().ToList();

            List<GroupModel> groupModels = new List<GroupModel>();
            foreach (var item in userGroups)
            {
                var userInfos = getSubordinatesByLeaderId(item.OwnerIds.FirstOrDefault());
                groupModels.Add(new GroupModel(item.Id, item.Name.Replace("深圳", ""), item.OwnerIds, userInfos));
            }

            return groupModels;
        }

        //获得当前用户项目相关组，开发只能添加本组人员+部门所有测试组；测试可以添加部门所有组
        public List<GroupModel> GetUserGroupModelsByUserId(string userId)
        {
            var departments = GetDepartmentsByUserId(userId);
            List<UserGroup> allUserGroups = new List<UserGroup>();
            foreach (var item in departments)
            {
                allUserGroups.AddRange(item.UserGroups.Select(o => o.Value).OrderBy(o=>o.IsTest).ToList());
            }
            var user = ServiceFactory.Instance.GetService<UserService>().Get(userId);
            List<UserGroup> userGroups = GetUserGroupsByUserId(userId);
            if (IsTester(userId) || user.UserType == UserType.Manager || user.UserType == UserType.Admin)
            {
                userGroups.AddRange(allUserGroups);
            }
            else//dev user or dev leader
            {
                userGroups.AddRange(allUserGroups.Where(o => o.IsTest).ToList());
            }
            userGroups = userGroups.Distinct().ToList();

            List<GroupModel> groupModels = new List<GroupModel>();
            foreach (var item in userGroups)
            {
                var userInfos = getSubordinatesByLeaderId(item.OwnerIds.FirstOrDefault());
                groupModels.Add(new GroupModel(item.Id, item.Name.Replace("深圳",""), item.OwnerIds, userInfos));
            }

            return groupModels;
        }

        //根据各个组的lead获取组员信息
        private List<UserInfo> getSubordinatesByLeaderId(string userId)
        {
            var user = ServiceFactory.Instance.GetService<UserService>().Get(userId);
            var userIds = GetUserGroupsWithList()
                        .Where(o => o.OwnerIds.Contains(userId))
                        .SelectMany(o => o.UserIds)
                        .Distinct().ToList();

            List<UserInfo> userInfos = new List<UserInfo>();
            foreach (var id in userIds)
            {
                var tmp = ServiceFactory.Instance.GetService<UserService>().Get(id);
                userInfos.Add(new UserInfo(tmp.Id, tmp.Name));
            }
            return userInfos;
        }

        public List<User> GetOwnersByUserId(string userId)
        {
            var user = ServiceFactory.Instance.GetService<UserService>().Get(userId);

            //如果是测试人员可以添加全部门的人
            if(IsTester(userId))
            {
                var departments = GetDepartmentsByUserId(userId);
                var userIds = departments.SelectMany(o => o.GetLeaderIds()).ToList();
                var users = ServiceFactory.Instance.GetService<UserService>().GetByIds(userIds);
                return users.Where(o => o.AccountType == AccountType.Public).ToList();
            }
            else if(user.UserType == UserType.Manager || user.UserType == UserType.Admin)
            {
                var userIds = GetSubordinatesByUserId(userId);
                var users = ServiceFactory.Instance.GetService<UserService>().GetByIds(userIds);
                return users.Where(o => o.AccountType == AccountType.Public && o.UserType != UserType.User).ToList();
            }
            else
            {
                var groups = GetUserGroupsByUserId(userId);
                var userIds = groups.SelectMany(o => o.OwnerIds).ToList();
                userIds.AddRange(GetTestLeaderIdsByUserId(userId));
                userIds = userIds.Distinct().ToList();
                var users = ServiceFactory.Instance.GetService<UserService>().GetByIds(userIds);
                return users.Where(o => o.AccountType == AccountType.Public).ToList();
            }
        }

        public List<User> GetOwnersByUserIds(List<string> userIds)
        {
            var result = new List<User>();

            foreach(var item in userIds)
            {
                result.AddRange(GetOwnersByUserId(item));
            }

            return result.Distinct().ToList();
        }

        public List<User> GetUsersByUserIds(List<string> userIds)
        {
            var result = new List<User>();

            foreach (var item in userIds)
            {
                result.AddRange(GetUsersByUserId(item));
            }

            return result.Distinct().ToList();
        }

        public List<User> GetUsersByUserId(string userId)
        {
            var user = ServiceFactory.Instance.GetService<UserService>().Get(userId);

            //如果是测试人员可以添加全部门的人
            if (IsTester(userId))
            {
                var departments = GetDepartmentsByUserId(userId);
                var userIds = departments.SelectMany(o => o.GetUserIds()).ToList();
                var users = ServiceFactory.Instance.GetService<UserService>().GetByIds(userIds);
                return users.Where(o => o.AccountType == AccountType.Public).ToList();
            }
            else if (user.UserType == UserType.Manager || user.UserType == UserType.Admin)
            {
                var userIds = GetSubordinatesByUserId(userId);
                var users = ServiceFactory.Instance.GetService<UserService>().GetByIds(userIds);
                return users.Where(o => o.AccountType == AccountType.Public).ToList();
            }
            else
            {
                var groups = GetUserGroupsByUserId(userId);
                var userIds = groups.SelectMany(o => o.UserIds).ToList();
                userIds.AddRange(GetTestUserIdsByUserId(userId));
                userIds = userIds.Distinct().ToList();
                var users = ServiceFactory.Instance.GetService<UserService>().GetByIds(userIds);
                return users.Where(o => o.AccountType == AccountType.Public).ToList();
            }
        }
    }
}
