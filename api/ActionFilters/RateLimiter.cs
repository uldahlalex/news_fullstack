namespace api.ActionFilters;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class RateLimitAttribute(int requestsPerMinute) : ActionFilterAttribute
{
    private static readonly Dictionary<string, DateTime> Timestamps = new(); //IP and when they requested
    private static readonly Dictionary<string, int> RequestCounts = new(); //IP and number of requests

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var ip = context.HttpContext.Connection.RemoteIpAddress!.ToString();

        if (!Timestamps.ContainsKey(ip))
        {
            InitializeRequestCountAndTimeStamp(ip);
            return;
        }

        if ((DateTime.Now - Timestamps[ip]).TotalMinutes >= 1)
        {
            ResetRequestCountAndTimeStamp(ip);
            return;
        }

        IncrementRequestCount(ip);

        if (RequestCounts[ip] > requestsPerMinute)
        {
            TooManyRequests(context);
        }
    }

    private void TooManyRequests(ActionExecutingContext context)
    {
        context.Result = new StatusCodeResult((int)HttpStatusCode.TooManyRequests);
    }

    private void InitializeRequestCountAndTimeStamp(string ip)
    {
        Timestamps[ip] = DateTime.Now;
        RequestCounts[ip] = 1;
    }

    private void ResetRequestCountAndTimeStamp(string ip)
    {
        Timestamps[ip] = DateTime.Now;
        RequestCounts[ip] = 1;
    }

    private void IncrementRequestCount(string ip)
    {
        RequestCounts[ip] += 1;
    }
}