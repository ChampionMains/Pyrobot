using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;

namespace ChampionMains.Pyrobot.WebAPI
{
    /// <summary>
    ///     A WebAPI message handler which transforms response messages in to a common format.
    /// </summary>
    public class ApiMessageHandler : DelegatingHandler
    {
        private static readonly MediaTypeFormatter JsonFormatter = new JsonMediaTypeFormatter
        {
            SerializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}
        };

        /// <summary>
        ///     Data transfer object for WebAPI response messages.
        /// </summary>
        private class ResponseDto
        {
            [JsonProperty("result")]
            public object Content { get; set; }

            [JsonProperty("error")]
            public string Error { get; set; }

            [JsonProperty("exception")]
            public object Exception { get; set; }

            [JsonProperty("validationErrors")]
            public string[] ValidationErrors { get; set; }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return GetResponseMessage(request, await base.SendAsync(request, cancellationToken));
        }

        private HttpResponseMessage GetResponseMessage(HttpRequestMessage request, HttpResponseMessage response)
        {
            object content;

            if (response.TryGetContentValue(out content))
            {
                var error = content as HttpError;

                if (content is string && !response.IsSuccessStatusCode)
                {
                    content = new {error = content};
                }
                else if (error != null)
                {
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        error.Message = "Internal server error.";
                    }
                    content = GetErrorResult(error);
                }
                else
                {
                    content = new {result = content};
                }
            }

            if (content != null)
                response.Content = new ObjectContent(content.GetType(), content, JsonFormatter);
            return response;
        }

        private object GetErrorResult(HttpError error)
        {
            dynamic r = new ExpandoObject();
            r.error = error.Message;
            r.errorDetail = error.MessageDetail;
            if (error.ModelState != null)
            {
                r.errors = error.ModelState.SelectMany(pair => (IEnumerable<string>) pair.Value).ToArray();
            }
#if DEBUG
            if (!string.IsNullOrEmpty(error.ExceptionMessage))
            {
                r.exception = ResolveExceptionDto(error);
            }
#endif
            return r;
        }

        private object ResolveExceptionDto(HttpError error)
        {
            if (error == null) return null;
            dynamic r = new ExpandoObject();
            r.exceptionType = error.ExceptionType;
            r.exceptionMessage = error.ExceptionMessage;
            r.stackTrace = error.StackTrace;
            r.innerException = ResolveExceptionDto(error.InnerException);
            return r;
        }
    }
}