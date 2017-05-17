namespace Service.User
{
    using Entity.Project;
    using Entity.User;
    using Platform.Context;
    using Platform.Enum;
    using Platform.Extension;
    using Result;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Project;

    public class UserService : BaseService<User>
    {
        private const string INVALID_USERNAME = "User name is invalid, please check it.";
        private const string INVALID_PWD = "Password is invalid, please check it.";
        private const string USER_ALREADY_EXIST = "User is already exist.";

        public UserService(IContextRepository contextRepository) : base(contextRepository) { }

        public UserServiceResult LogIn(string id, string pwd)
        {
            UserServiceResult result = new UserServiceResult();

            User user = Get(id);

            if (user == null)
            {
                result.Status = Result.Failure;
                result.Message = INVALID_USERNAME;
            }
            else
            {
                if (user.Password.Equals(pwd))
                {
                    result.Status = Result.Success;
                    result.Type = (int)user.UserType;
                    result.User = user;
                }
                else
                {
                    result.Status = Result.Failure;
                    result.Message = INVALID_PWD;
                }
            }

            return result;
        }

        public UserServiceResult SignUp(User user)
        {
            UserServiceResult result = new UserServiceResult();

            User dbUser = Get(user.Id);

            if (dbUser == null)
            {
                Create(user);
                result.User = user;
                result.Status = Result.Success;
            }
            else
            {
                result.Status = Result.Failure;
                result.Message = USER_ALREADY_EXIST;
            }

            return result;
        }

        public bool IsUserExist(string id)
        {
            return Get(id) != null;
        }

        public override void Delete(string id)
        {
            //删除用户
            base.Delete(id);

            //Department里面删除用户
            ServiceFactory.Instance.GetService<DepartmentService>().DeleteUser(id);

            //删除UserTimeSheetStatus
            ServiceFactory.Instance.GetService<UserTimeSheetStatusService>().Delete(id);

            //删除TimeSheet
            ServiceFactory.Instance.GetService<TimeSheetService>().Delete(o => o.UserId == id);
        }

        public override void Delete(List<string> ids)
        {
            foreach (var id in ids)
            {
                Delete(id);
            }
        }

        public override void Delete(Expression<Func<User, bool>> selector)
        {
            var ids = Get().Where(selector.Compile()).Select(o => o.Id).ToList();
            Delete(ids);
        }
    }
}
