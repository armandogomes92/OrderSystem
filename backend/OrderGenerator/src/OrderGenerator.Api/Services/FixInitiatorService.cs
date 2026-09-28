using OrderGenerator.Api.Fix;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

namespace OrderGenerator.Api.Services;

public sealed class FixInitiatorService(
    GeneratorFixApp app, IConfiguration configuration, ILogger<FixInitiatorService> logger) : IHostedService
{
    private SocketInitiator? _initiator;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = new SessionSettings(Path.Combine(AppContext.BaseDirectory, "SessionConfig", "fix-initiator.cfg"));

        // Permite apontar para outro host sem alterar o .cfg (ex.: Fix__ConnectHost=accumulator no Docker).
        var connectHost = configuration["Fix:ConnectHost"];
        if (!string.IsNullOrWhiteSpace(connectHost))
        {
            foreach (var sessionId in settings.GetSessions())
                settings.Get(sessionId).SetString("SocketConnectHost", connectHost);
        }

        _initiator = new SocketInitiator(app, new FileStoreFactory(settings), settings, new FileLogFactory(settings));
        _initiator.Start();

        logger.LogInformation("FIX Initiator iniciado.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _initiator?.Stop();
        _initiator?.Dispose();

        logger.LogInformation("FIX Initiator encerrado.");
        return Task.CompletedTask;
    }
}