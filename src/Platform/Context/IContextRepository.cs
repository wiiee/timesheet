namespace Platform.Context
{
    public interface IContextRepository
    {
        IContext GetCurrent();
    }
}
