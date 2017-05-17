namespace Web.Extension.Middleware
{
    using Microsoft.AspNetCore.Builder;
    using Web.Middleware;

    public static class ContextExtension
    {
        public static IApplicationBuilder UseContext(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ContextMiddleware>();
        }
    }
}
