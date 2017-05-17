namespace Service.Result
{
    using Entity.User;
    using Platform.Enum;

    public class UserServiceResult
    {
        public Result Status { get; set; }

        public string Message { get; set; }

        public int Type { get; set; }

        public User User { get; set; }
    }
}
