namespace Web.Context
{
    using Platform.Context;
    using System.Threading;

    public class WebContextRepository : IContextRepository
    {
        private ThreadLocal<IContext> contexts;

        public WebContextRepository()
        {
            this.contexts = new ThreadLocal<IContext>();
        }

        public void SetContext(IContext context)
        {
            contexts.Value = context;
        }

        public IContext GetCurrent()
        {
            return this.contexts.Value;
        }
    }
}
