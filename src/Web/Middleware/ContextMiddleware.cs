namespace Web.Middleware
{
    using Context;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using System.Threading;
    using System.Threading.Tasks;

    public class ContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ContextMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ContextMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            WebContextUtil.Instance.AddContext(threadId, new WebContext(context));

            await _next.Invoke(context);

            WebContextUtil.Instance.RemoveContext(threadId);
        }
    }
}
