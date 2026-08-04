using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CarSalesManagementSystemAPI.Middleware;

/// <summary>
/// Middleware bắt mọi exception chưa xử lý: ghi log đầy đủ (console + file logs/errors-yyyyMMdd.log)
/// và trả về JSON lỗi thống nhất. Giúp chẩn đoán 500 khi không đọc được response ở client.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private static readonly object _fileLock = new();

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            var detail =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context.Request.Method} {context.Request.Path}{context.Request.QueryString}\n" +
                $"{ex}\n" +
                (ex.InnerException != null ? $"INNER: {ex.InnerException}\n" : string.Empty) +
                new string('-', 100) + "\n";

            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            WriteToFile(detail);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";
            var payload = JsonSerializer.Serialize(new
            {
                success = false,
                message = "Lỗi hệ thống. Chi tiết đã được ghi vào file log của server.",
                error = ex.InnerException?.Message ?? ex.Message
            });
            await context.Response.WriteAsync(payload);
        }
    }

    private static void WriteToFile(string content)
    {
        try
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, $"errors-{DateTime.Now:yyyyMMdd}.log");
            lock (_fileLock)
            {
                File.AppendAllText(file, content);
            }
        }
        catch
        {
            // Không để việc ghi log gây thêm lỗi.
        }
    }
}
