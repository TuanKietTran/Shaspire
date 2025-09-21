using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shaspire.ServiceDefaults.Models;
namespace Shaspire.ServiceDefaults.Webs;

internal class ExceptionInterceptorMiddleware(RequestDelegate next, ILogger<ExceptionInterceptorMiddleware> logger)
{
  private readonly RequestDelegate _next = next;
  private readonly ILogger<ExceptionInterceptorMiddleware> _logger = logger;

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context); // Call the next middleware
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Intercepted exception: {Message}", ex.Message);
      await HandleExceptionAsync(context, ex);
    }
  }

  private static Task HandleExceptionAsync(HttpContext context, Exception exception)
  {
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = exception switch
    {
      ArgumentException => StatusCodes.Status400BadRequest,
      BadRequestException => StatusCodes.Status400BadRequest,
      UnauthorizedException => StatusCodes.Status401Unauthorized,
      ForbiddenException => StatusCodes.Status403Forbidden,
      UnauthorizedAccessException => StatusCodes.Status403Forbidden,
      NotFoundException => StatusCodes.Status404NotFound,

      _ => StatusCodes.Status500InternalServerError
    };

    var response = new
    {
      error = exception.GetType().Name,
      message = exception.Message
    };

    return context.Response.WriteAsJsonAsync(response);
  }
}

// Extension method to register the middleware
public static class ExceptionInterceptorMiddlewareExtensions
{
  public static IApplicationBuilder UseExceptionInterceptor(this IApplicationBuilder builder)
  {
    return builder.UseMiddleware<ExceptionInterceptorMiddleware>();
  }
}