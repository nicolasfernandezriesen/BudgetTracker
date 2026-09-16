using System.Net;
using Hangfire.AspNetCore;
using Hangfire.Dashboard;

namespace BudgetTracker.Hangfire;

public sealed class LocalhostDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        if (context is not AspNetCoreDashboardContext aspNetContext)
            return false;

        var remoteIp = aspNetContext.HttpContext.Connection.RemoteIpAddress;
        if (remoteIp is null)
            return false;

        if (remoteIp.IsIPv4MappedToIPv6)
            remoteIp = remoteIp.MapToIPv4();

        return IPAddress.IsLoopback(remoteIp);
    }
}
