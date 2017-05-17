namespace Service
{
    using Platform.Context;

    public interface IService
    {
        IContext GetContext();
    }
}