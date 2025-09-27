using System;
using System.Net;
using System.Text;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace pk.api.Monitoring;

public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private const string BasicScheme = "Basic";
    private readonly ILogger<HangfireDashboardAuthorizationFilter> _logger;
    private readonly HangfireDashboardOptions _options;

    public HangfireDashboardAuthorizationFilter(IOptions<HangfireDashboardOptions> options, ILogger<HangfireDashboardAuthorizationFilter> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var remoteAddress = httpContext.Connection.RemoteIpAddress;

        if (_options.AllowAnonymous)
        {
            return true;
        }

        if (_options.AllowLocalRequestsWithoutAuth && IsLocalAddress(remoteAddress))
        {
            return true;
        }

        if (!_options.HasCredentials)
        {
            _logger.LogWarning("Hangfire dashboard credentials are not configured; remote access is denied.");
            Challenge(httpContext);
            return false;
        }

        var header = httpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(header) || !header.StartsWith(BasicScheme, StringComparison.OrdinalIgnoreCase))
        {
            Challenge(httpContext);
            return false;
        }

        var encodedCredentials = header.Substring(BasicScheme.Length).Trim();
        if (string.IsNullOrEmpty(encodedCredentials))
        {
            Challenge(httpContext);
            return false;
        }

        string decoded;
        try
        {
            var credentialBytes = Convert.FromBase64String(encodedCredentials);
            decoded = Encoding.UTF8.GetString(credentialBytes);
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Failed to decode Hangfire dashboard credentials.");
            Challenge(httpContext);
            return false;
        }

        var separatorIndex = decoded.IndexOf(':');
        if (separatorIndex < 0)
        {
            Challenge(httpContext);
            return false;
        }

        var username = decoded[..separatorIndex];
        var password = decoded[(separatorIndex + 1)..];

        if (string.Equals(username, _options.Username, StringComparison.Ordinal) && string.Equals(password, _options.Password, StringComparison.Ordinal))
        {
            return true;
        }

        Challenge(httpContext);
        return false;
    }

    private static bool IsLocalAddress(IPAddress? remoteAddress)
    {
        if (remoteAddress is null)
        {
            return false;
        }

        if (IPAddress.IsLoopback(remoteAddress))
        {
            return true;
        }

        if (remoteAddress.Equals(IPAddress.IPv6Loopback))
        {
            return true;
        }

        return false;
    }

    private static void Challenge(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
    }
}
