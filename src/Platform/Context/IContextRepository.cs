namespace Platform.Context
{
    public interface IContextRepository
    {
        IContext GetCurrent();
        void SetContext(IContext context);
    }
}
