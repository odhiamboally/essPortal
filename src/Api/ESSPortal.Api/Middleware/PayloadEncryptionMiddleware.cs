using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Common;
using Microsoft.Extensions.Options;

using System.Text;

namespace ESSPortal.Api.Middleware;

public class PayloadEncryptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IPayloadEncryptionService _encryptionService;
    private readonly ILogger<PayloadEncryptionMiddleware> _logger;
    private readonly SecuritySettings _securitySettings;
    private readonly string[] _excludedPaths =
    [
        "/health", 
        "/swagger", 
        "/api/auth/login"
    ];

    public PayloadEncryptionMiddleware(
        RequestDelegate next,
        IPayloadEncryptionService encryptionService,
        ILogger<PayloadEncryptionMiddleware> logger,
        IOptions<SecuritySettings> securityOptions)
    {
        _next = next;
        _encryptionService = encryptionService;
        _logger = logger;
        _securitySettings = securityOptions.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip encryption for excluded paths
       
        if (_excludedPaths.Any(path => context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        if (string.IsNullOrWhiteSpace(_securitySettings.PayloadEncryption.Key))
        {
            _logger.LogWarning("Payload encryption key not configured, skipping encryption");
            await _next(context);
            return;
        }

        // Decrypt incoming request
        await DecryptRequestAsync(context);

        // Capture and encrypt outgoing response
        var originalResponseBody = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        await _next(context);

        await EncryptResponseAsync(context, originalResponseBody, responseBodyStream);
    }

    private async Task DecryptRequestAsync(HttpContext context)
    {
        if (context.Request.ContentLength > 0 &&
            (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "PATCH"))
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(requestBody))
            {
                try
                {
                    if (_encryptionService.IsEncrypted(requestBody))
                    {
                        var decryptedBody = _encryptionService.Decrypt(requestBody);
                        var decryptedBytes = Encoding.UTF8.GetBytes(decryptedBody);

                        var decryptedStream = new MemoryStream(decryptedBytes);
                        decryptedStream.Position = 0;

                        context.Request.Body = decryptedStream;

                        context.Request.ContentLength = decryptedBytes.Length;

                        _logger.LogDebug("Successfully decrypted request payload. New length: {Length}", decryptedBytes.Length);
                    }
                    else
                    {
                        _logger.LogDebug("Request payload is not encrypted, proceeding with original");
                        context.Request.Body.Position = 0;
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to decrypt request body, proceeding with original");

                    // Reset stream position for original processing
                    context.Request.Body.Position = 0;
                    throw;
                }
            }
        }
    }

    private async Task EncryptResponseAsync(HttpContext context, Stream originalResponseBody, MemoryStream responseBodyStream)
    {
        try
        {
            responseBodyStream.Seek(0, SeekOrigin.Begin);

            var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();

            if (!string.IsNullOrWhiteSpace(responseBody) &&
                context.Response.ContentType?.Contains("application/json") == true &&
                context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                try
                {
                    var encryptedResponse = _encryptionService.Encrypt(responseBody);
                    var encryptedBytes = Encoding.UTF8.GetBytes(encryptedResponse);

                    context.Response.ContentLength = encryptedBytes.Length;
                    await originalResponseBody.WriteAsync(encryptedBytes);

                    _logger.LogDebug("Successfully encrypted response payload");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to encrypt response, sending original");
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    await responseBodyStream.CopyToAsync(originalResponseBody);
                }
            }
            else
            {
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalResponseBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in EncryptResponseAsync");
            throw;
        }
    }
}
