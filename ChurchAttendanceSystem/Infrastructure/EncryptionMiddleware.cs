using System.Text;
using ChurchAttendanceSystem.Application.Interfaces;

namespace ChurchAttendanceSystem.Infrastructure;

public class EncryptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;

    public EncryptionMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check for bypass header
        var bypassEncryption = context.Request.Headers.ContainsKey("X-Bypass-Encryption") && 
                              context.Request.Headers["X-Bypass-Encryption"] == "true";
        
        if (ShouldSkipEncryption(context.Request.Path) || bypassEncryption)
        {
            await _next(context);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var encryption = scope.ServiceProvider.GetRequiredService<IEncryptionService>();

        // Decrypt request
        if (context.Request.ContentLength > 0)
        {
            context.Request.EnableBuffering();
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrEmpty(body))
            {
                // Check if payload looks like plain JSON (not encrypted)
                if ((body.TrimStart().StartsWith('{') || body.TrimStart().StartsWith('[')) && !bypassEncryption)
                {
                    context.Response.StatusCode = 500;
                    return;
                }

                try
                {
                    var decrypted = encryption.Decrypt(body);
                    var newBody = new MemoryStream(Encoding.UTF8.GetBytes(decrypted));
                    context.Request.Body = newBody;
                }
                catch (Exception)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Decryption failed");
                    return;
                }
            }
        }

        // Capture response
        var originalBody = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Encrypt response
        responseBody.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(responseBody).ReadToEndAsync();
        
        if (!string.IsNullOrEmpty(response))
        {
            var encrypted = encryption.Encrypt(response);
            var encryptedBytes = Encoding.UTF8.GetBytes(encrypted);
            
            context.Response.ContentLength = encryptedBytes.Length;
            await originalBody.WriteAsync(encryptedBytes);
        }
    }

    private static bool ShouldSkipEncryption(string path)
    {
        return path.StartsWith("/health") || path.StartsWith("/swagger");
    }
}