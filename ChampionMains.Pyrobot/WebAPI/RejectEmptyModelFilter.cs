using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace ChampionMains.Pyrobot.WebAPI
{
    /// <summary>
    ///     An action filter which rejects WebApi requests where the model is empty.
    /// </summary>
    public class RejectEmptyModelFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var parameters = actionContext.ActionDescriptor.GetParameters();

            foreach (var parameter in parameters)
            {
                object value = null;

                if (actionContext.ActionArguments.ContainsKey(parameter.ParameterName))
                    value = actionContext.ActionArguments[parameter.ParameterName];

                if (value != null)
                    continue;

                value = Activator.CreateInstance(parameter.ParameterType);
                actionContext.ActionArguments[parameter.ParameterName] = value;

                var bodyModelValidator = actionContext.ControllerContext.Configuration.Services.GetBodyModelValidator();
                var metadataProvider = actionContext.ControllerContext.Configuration.Services.GetModelMetadataProvider();

                bodyModelValidator.Validate(value, value.GetType(), metadataProvider, actionContext, string.Empty);
            }

            base.OnActionExecuting(actionContext);
        }
    }
}