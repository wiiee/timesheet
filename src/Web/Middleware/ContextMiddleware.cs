namespace Web.Middleware
{
    using System.Threading.Tasks;
    using Context;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Platform.Context;

    public class ContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private IContextRepository _contextRepository;

        public ContextMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IContextRepository contextRepository)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ContextMiddleware>();
            _contextRepository = contextRepository;
        }

        public async Task Invoke(HttpContext context)
        {
            _contextRepository.SetContext(new WebContext(context));

            await _next.Invoke(context);
        }
    }
}
