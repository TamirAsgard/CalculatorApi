using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace IO.Swagger.Attributes;

/// <summary>
/// Action filter that restricts endpoints to development environment only
/// 
/// Returns 404 Not Found when accessed outside development environment.
/// Used for testing and debugging endpoints that should not be exposed in production.
/// </summary>
public class DevelopmentOnlyAttribute: ActionFilterAttribute
{
    /// <summary>
    /// Checks if the current environment is development
    /// </summary>
    /// <param name="context">The action executing context</param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var env = context.HttpContext.RequestServices.GetService<IHostEnvironment>();
        if (env == null || !env.IsDevelopment())
        {
            context.Result = new NotFoundResult();
        }
    }
}