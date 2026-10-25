using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryManagement.Api.Filters;

public class LogActionFilter : IActionFilter
{
    private readonly ILogger<LogActionFilter> _logger;
    
    public LogActionFilter(ILogger<LogActionFilter> logger)
    {
        _logger = logger;
    }
    
    // executed before the action method
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var actionName = context.ActionDescriptor.DisplayName;
        _logger.LogInformation("Executing action: {ActionName}", actionName);

        foreach (var argument in context.ActionArguments)
        {
            _logger.LogInformation("Argument: {Name} = {Value}", argument.Key, argument.Value);
        }
    }

    // executed after the action method
    public void OnActionExecuted(ActionExecutedContext context)
    {
        var actionName = context.ActionDescriptor.DisplayName;
        var statusCode = context.HttpContext.Response.StatusCode;
        _logger.LogInformation("Executed Action: {ActionName}, Status: {StatusCode}", actionName, statusCode);
    }
}