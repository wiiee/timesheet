namespace Web.Context
{
    using Platform.Context;
    using System;
    using System.Collections.Concurrent;

    public class WebContextUtil
    {
        private static readonly Lazy<WebContextUtil> lazy = new Lazy<WebContextUtil>(() => new WebContextUtil());
        private ConcurrentDictionary<int, IContext> contexts;

        private WebContextUtil()
        {
            this.contexts = new ConcurrentDictionary<int, IContext>();
        }

        public static WebContextUtil Instance { get { return lazy.Value; } }


        public void AddContext(int key, IContext context)
        {
            this.contexts.TryAdd(key, context);
        }

        public void RemoveContext(int key)
        {
            IContext context;
            this.contexts.TryRemove(key, out context);
        }

        public IContext GetContext(int key)
        {
            IContext context;
            this.contexts.TryGetValue(key, out context);
            return context;
        }
    }
}
