using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MiddlewareExample.Middlewares
{
    public class MaximumFileSizeMiddleware : IMiddleware
    {
        private readonly ILogger<MaximumFileSizeMiddleware> _logger;
        private readonly long _maximumBytes;

        public MaximumFileSizeMiddleware(ILogger<MaximumFileSizeMiddleware> logger, IConfiguration configuration)
        {
            _logger = logger;
            if (!long.TryParse(configuration["MaximumFileBytes"], out _maximumBytes))
            {
                throw new ArgumentException("\"MaximumFileBytes\" değeri ortam dosyasından okunamadı!");
            }
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Form.Files.Any(file => file.Length > _maximumBytes))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync($"Yüklemeye çalıştığınız dosya veya dosyaların boyutu sınırın üzerindedir. ( En fazla : {_maximumBytes}B )");

                string username = context.User.Identity.Name ?? "Anonymous";
                _logger.LogWarning($"{username} adlı kullanıcı boyutu büyük dosya yüklemeye çalıştı.");
                return;
            }

            await next(context);
        }
    }
}
