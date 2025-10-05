using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace server_NET.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            // JWT validation logic here
            await _next(context);
        }
    }
}
