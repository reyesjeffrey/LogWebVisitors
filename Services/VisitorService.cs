using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VisitorTracker.Models;
using System;
using System.Threading.Tasks;

namespace VisitorTracker.Services
{
    public class VisitorService : IDisposable
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;
        private readonly ILogger<VisitorService> _logger;

        public VisitorService(IConfiguration config, ILogger<VisitorService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            try
            {
                var connectionString = config["CosmosDB"];
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ArgumentException("CosmosDB connection string is missing from configuration.");
                }

                _cosmosClient = new CosmosClient(connectionString);
                _container = _cosmosClient.GetContainer("reyesjeffrey", "SiteVisits");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing CosmosDB client.");
                throw;
            }
        }

        public async Task<string> LogVisitAsync(VisitorLog visit)
        {
            try
            {
                if (visit == null || string.IsNullOrWhiteSpace(visit.PageVisited))
                {
                    throw new ArgumentException("Invalid visitor log data.");
                }

                visit.Date = DateTime.UtcNow;

                _logger.LogInformation("Inserting visit for {PageVisited} from IP {IpAddress}.", visit.PageVisited, visit.IpAddress);

                // ✅ Use CreateItemAsync instead of CreateItemStreamAsync
                await _container.CreateItemAsync(visit, new PartitionKey(visit.IpAddress));

                _logger.LogInformation("Visit logged successfully.");
                return "Visit logged successfully.";
            }
            catch (CosmosException cosmosEx)
            {
                _logger.LogError(cosmosEx, "CosmosDB error: {StatusCode}", cosmosEx.StatusCode);
                return $"CosmosDB Error: {cosmosEx.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log visit.");
                return $"Error: {ex.Message}";
            }
        }

        public void Dispose()
        {
            _cosmosClient?.Dispose();
        }
    }
}
