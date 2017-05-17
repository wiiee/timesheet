namespace Web.Model
{
    using Platform.Enum;

    public class UserModel
    {
        public string Id { get; set; }
        public string NickName { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string Password { get; set; }
        public UserType UserType { get; set; }
        public AccountType AccountType { get; set; }
        public Gender Gender { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public bool IsResetPassword { get; set; }

        public UserModel(string id, string nickName, string name, string mobileNo, Gender gender, UserType userType, AccountType accountType, bool isEdit, bool isDelete, bool isResetPassword)
        {
            this.Id = id;
            this.NickName = nickName;
            this.Name = name;
            this.MobileNo = mobileNo;
            this.Gender = gender;
            this.UserType = userType;
            this.AccountType = accountType;
            this.IsEdit = isEdit;
            this.IsDelete = isDelete;
            this.IsResetPassword = isResetPassword;
        }
    }
}
