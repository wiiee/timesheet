namespace Entity.User
{
    using Platform.Enum;
    using System.Collections.Generic;

    public class User : BaseEntity
    {
        //Id为工号

        public string NickName { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string Password { get; set; }
        public List<string> Pics { get; set; }
        public Gender Gender { get; set; }

        public UserType UserType { get; set; }

        public AccountType AccountType { get; set; }

        public User(string id, string password, string name, string nickName, string mobileNo, Gender gender, List<string> pics, UserType userType, AccountType accountType)
        {
            this.Id = id;
            this.Password = password;
            this.Name = name;
            this.NickName = nickName;
            this.MobileNo = mobileNo;
            this.Gender = gender;
            this.Pics = pics;
            this.UserType = userType;
            this.AccountType = accountType;
        }

        public User()
        {

        }
    }
}
