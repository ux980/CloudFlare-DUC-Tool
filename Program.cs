using CF_DUC_Tool.src.Core;
using CF_DUC_Tool.src.Infrastructure;
using CF_DUC_Tool.src.Application;

class Program
{
    static async Task Main(string[] args)
    {
        var logger = new CompositeLogger(new ConsoleLogger(), new FileLogger());
        
        logger.LogInfo("=== CloudFlare-DUC-Tool ===");
        logger.LogInfo($"OS: {Environment.OSVersion} | .NET: {Environment.Version}");

        try
        {
            var config = new IniConfigService(logger).Load();
            logger.LogInfo($"Perfis carregados: {config.Records.Count}");

            var service = new DnsUpdaterService(logger, new FailoverIpProvider(), new CloudflareDnsProvider(), config);

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); logger.LogWarning("Parando serviço..."); };

            var interval = TimeSpan.FromMinutes(config.IntervalMinutes > 0 ? config.IntervalMinutes : 5);
            var timer = new PeriodicTimer(interval);

            await service.ExecuteAsync();

            while (await timer.WaitForNextTickAsync(cts.Token))
            {
                await service.ExecuteAsync();
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInfo("Serviço finalizado corretamente.");
        }
        catch (Exception ex)
        {
            logger.LogException(ex);
            Environment.Exit(1);
        }
    }
}