using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Net;
using System.Security.Cryptography;

namespace MiddlewareExample.Middlewares.Tests
{
    public class MaximumFileSizeMiddlewareTests
    {
        [Fact]
        public async Task OverMaximumSize_ShouldReturn_BadRequest()
        {
            var httpContext = GetHttpContext(4096);
            var configuration = GetConfiguration("1024");
            var logger = GetLogger();
            var middleware = new MaximumFileSizeMiddleware(logger.Object, configuration);

            await middleware.InvokeAsync(httpContext, RequestDelegate);

            logger.Verify(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            Assert.Equal((int)HttpStatusCode.BadRequest, httpContext.Response.StatusCode);

        }

        [Fact]
        public async Task UnderMaximumSize_ShouldReturn_Ok()
        {
            var httpContext = GetHttpContext(128);
            var configuration = GetConfiguration("1024");
            var logger = GetLogger();
            var middleware = new MaximumFileSizeMiddleware(logger.Object, configuration);

            await middleware.InvokeAsync(httpContext, RequestDelegate);

            logger.Verify(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
            Assert.Equal((int)HttpStatusCode.OK, httpContext.Response.StatusCode);
        }

        private HttpContext GetHttpContext(int fileLength)
        {
            byte[] fileBytes = RandomNumberGenerator.GetBytes(fileLength);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = "multipart/form-data";
            var fields = new Dictionary<string, StringValues>();
            var formFiles = new FormFileCollection();
            var formFile = new FormFile(
                new MemoryStream(fileBytes),
                0, fileBytes.Length, "source", "form-file.txt");
            formFiles.Add(formFile);
            httpContext.Request.Form = new FormCollection(fields, formFiles);

            return httpContext;
        }

        private IConfiguration GetConfiguration(string fileLength)
        {
            var configurationBuilder = new ConfigurationBuilder();
            var configurationCollection = new Dictionary<string, string>
            {
                { "MaximumFileBytes", fileLength }
            };
            configurationBuilder.AddInMemoryCollection(configurationCollection);
            configurationBuilder.Properties.Add("MaximumFileBytes", fileLength);

            return configurationBuilder.Build();
        }
        private Mock<ILogger<MaximumFileSizeMiddleware>> GetLogger()
        {
            return new Mock<ILogger<MaximumFileSizeMiddleware>>();
        }

        private async Task RequestDelegate(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await context.Response.WriteAsync("Başarılı!");
        }
    }
}
