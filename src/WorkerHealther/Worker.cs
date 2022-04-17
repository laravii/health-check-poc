using System.Net;
using Polly;
using Polly.CircuitBreaker;

namespace WorkerHealth;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }
    public CancellationToken GetCancellationToken(bool isHealthy)
    {
        var cancellationToken = new CancellationTokenSource();

        if (!isHealthy)
        {
            cancellationToken.Cancel();
        }

        return cancellationToken.Token;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var httpClient = new HttpClient();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var circuit = CreatePolicy();
                var httpResponse = await circuit.ExecuteAsync(async () =>
                {
                    var healthyDepedency = await httpClient.GetAsync("http://localhost:5001/Radom");

                    return healthyDepedency;
                });

                _logger.LogInformation($"{DateTimeOffset.Now.ToString()} Circuito = {circuit.CircuitState}");

                var cancellation = GetCancellationToken(httpResponse.IsSuccessStatusCode);

                if (!cancellation.IsCancellationRequested)
                {
                    _logger.LogInformation("The application was executed with success", DateTimeOffset.Now);
                }

                await Task.Delay(1000, stoppingToken);
            }

        }
        catch (System.Exception ex)
        {
            _logger.LogCritical("Was not executed with success", ex.Message, ex.StackTrace);
        }
    }

    private AsyncCircuitBreakerPolicy<HttpResponseMessage> CreatePolicy()
    {
        return Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.InternalServerError)
            .CircuitBreakerAsync(1, TimeSpan.FromSeconds(3),
                onBreak: (_, _) => _logger.LogInformation("Open (onBreak)"),
                onReset: () => _logger.LogInformation("Closed (onReset)"),
                onHalfOpen: () => _logger.LogInformation("Half Open (onHalfOpen)"));
    }
}
