using System.Diagnostics;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Middlewares
{
    public class TransactionLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public TransactionLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITransactionLogService logService)
        {
            var sw = Stopwatch.StartNew();

            // Enable đọc body nhiều lần
            context.Request.EnableBuffering();

            // Đọc body TRƯỚC khi vào Controller
            string requestBody = string.Empty;
            if (context.Request.ContentLength > 0)
            {
                using var reader = new StreamReader(
                    context.Request.Body,
                    leaveOpen: true  // không đóng stream
                );
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // reset để Controller vẫn đọc được
            }

            var queryString = context.Request.QueryString.Value ?? string.Empty;

            await _next(context);

            sw.Stop();

            if (context.Request.Path.StartsWithSegments("/api"))
            {
                var statusCode = context.Response.StatusCode;
                var isError = statusCode >= 400;

                var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

                // Nếu là các API liên quan đến tài khoản, mật khẩu thì ẩn body đi để bảo mật thông tin khách hàng
                if (path.Contains("login") || path.Contains("register") || path.Contains("password"))
                {
                    requestBody = "[DATA REDACTED FOR SECURITY]";
                }

                // Gộp queryString + body vào Data
                var data = string.IsNullOrEmpty(queryString)
                    ? requestBody
                    : $"{queryString} | body: {requestBody}";

                var log = new TransactionLog
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    API = $"{context.Request.Method} {context.Request.Path}",
                    Status = statusCode.ToString(),
                    Data = data,                    // ← giờ có data
                    ErrorCode = isError ? statusCode.ToString() : string.Empty,
                    Message = isError ? GetErrorMessage(statusCode) : $"OK - {sw.ElapsedMilliseconds}ms",
                    CreatedAt = DateTime.UtcNow
                };
                try
                {
                    await logService.LogAsync(log);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ghi log lỗi: {ex.Message}");
                }
            }
        }

        private static string GetErrorMessage(int statusCode) => statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            500 => "Internal Server Error",
            _ => "Error"
        };
    }
}