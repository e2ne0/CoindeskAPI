namespace CoindeskAPI.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate m_next;
        private readonly ILogger<RequestResponseLoggingMiddleware> m_logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            m_next = next;
            m_logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();
            var _requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;
            var _url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            m_logger.LogInformation($"Request url: {_url}");
            m_logger.LogInformation($"Request Body: {_requestBody}");

            var _originalResponseBody = context.Response.Body;
            using (var newResponseBody = new MemoryStream())
            {
                context.Response.Body = newResponseBody;

                await m_next(context);

                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var _responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                m_logger.LogInformation($"Response Body: {_responseBody}");
                await newResponseBody.CopyToAsync(_originalResponseBody);
            }
        }
    }
}
