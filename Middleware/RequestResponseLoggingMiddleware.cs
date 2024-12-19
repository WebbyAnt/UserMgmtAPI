using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log request
        context.Request.EnableBuffering();
        var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;
        Console.WriteLine($"Incoming request: {context.Request.Method} {context.Request.Path} {requestBody}");

        // Capture the response body
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Log response
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        Console.WriteLine($"Outgoing response: {context.Response.StatusCode} {responseBodyText}");

        await responseBody.CopyToAsync(originalBodyStream);
    }
}