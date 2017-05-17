namespace Platform.Enum
{
    public enum UserType
    {
        //超级管理员才能管理用户
        Admin = 0,
        //只有Leader才能创建用户组
        Leader = 1,
        User,
        Manager
    }
}
