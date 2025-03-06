using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VisitorTracker.Helpers;
using VisitorTracker.Models;
using VisitorTracker.Services;

public class FunctionApp
{
    private readonly VisitorService _visitorService;
    private readonly ILogger<FunctionApp> _logger;

    public FunctionApp(VisitorService visitorService, ILogger<FunctionApp> logger)
    {
        _visitorService = visitorService ?? throw new ArgumentNullException(nameof(visitorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Function("LogVisit")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("Received a new visitor.");

        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"Received request body: {requestBody}");

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Request body is empty.");
            }

            try
            {
                JObject.Parse(requestBody);
            }
            catch (JsonReaderException)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid JSON format.");
            }

            var visit = JsonConvert.DeserializeObject<VisitorLog>(requestBody);
            if (visit == null || string.IsNullOrWhiteSpace(visit.PageVisited))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid or missing 'pageVisited' field.");
            }

            visit.Date = DateTime.UtcNow;
            visit.IpAddress = ExtractHeaderValue(req, "X-Forwarded-For") ?? visit.IpAddress ?? "Unknown";
            visit.Browser = ExtractHeaderValue(req, "User-Agent") ?? "Unknown";
            visit.Referrer = ExtractHeaderValue(req, "Referer") ?? "Unknown";
            visit.Device = DeviceHelper.GetDeviceType(visit.Browser);

            _logger.LogInformation("Logging visit for page {PageVisited} from IP {IpAddress}.", visit.PageVisited, visit.IpAddress);

            var result = await _visitorService.LogVisitAsync(visit);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { success = true, message = result });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing LogVisit function.");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal Server Error.");
        }
    }

    private static string ExtractHeaderValue(HttpRequestData req, string headerName)
    {
        return req.Headers.TryGetValues(headerName, out var values) ? values.FirstOrDefault()?.Split(',')[0]?.Trim() : null;
    }

    private static async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string errorMessage)
    {
        var errorResponse = req.CreateResponse(statusCode);
        await errorResponse.WriteAsJsonAsync(new { success = false, error = errorMessage });
        return errorResponse;
    }
}
