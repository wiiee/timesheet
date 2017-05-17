namespace Platform.Context
{
    public interface IContext
    {
        string UserId { get; }
        string RemoteIp { get; }
        bool IsUseCache { get; }
    }
}
