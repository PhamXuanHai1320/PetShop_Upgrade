using PetShop_Upgrade.Exceptions;
using System.Text.Json;

namespace PetShop_Upgrade.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            int statusCode;
            string message;

            switch (ex)
            {
                case NotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    message = ex.Message;
                    _logger.LogWarning(ex, "Not found: {Message}", ex.Message);
                    break;

                case BadRequestException:
                case ArgumentException:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = ex.Message;
                    _logger.LogWarning(ex, "Bad request: {Message}", ex.Message);
                    break;

                case UnauthorizedException:
                case UnauthorizedAccessException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    message = ex.Message;
                    _logger.LogWarning(ex, "Unauthorized: {Message}", ex.Message);
                    break;

                case ForbiddenException:
                    statusCode = StatusCodes.Status403Forbidden;
                    message = ex.Message;
                    _logger.LogWarning(ex, "Forbidden: {Message}", ex.Message);
                    break;

                case KeyNotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    message = ex.Message;
                    _logger.LogWarning(ex, "Not found: {Message}", ex.Message);
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "Đã xảy ra lỗi hệ thống, vui lòng thử lại sau.";
                    _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                    break;
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                Success = false,
                Message = message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
