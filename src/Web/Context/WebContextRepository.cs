namespace Web.Context
{
    using Platform.Context;
    using System.Threading;

    public class WebContextRepository : IContextRepository
    {
        public IContext GetCurrent()
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            return WebContextUtil.Instance.GetContext(threadId);
        }
    }
}
