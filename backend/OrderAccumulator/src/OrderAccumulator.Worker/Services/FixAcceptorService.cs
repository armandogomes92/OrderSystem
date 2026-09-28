using OrderAccumulator.Worker.Fix;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;

namespace OrderAccumulator.Worker.Services;

public sealed class FixAcceptorService(AccumulatorFixApp app, ILogger<FixAcceptorService> logger) : IHostedService
{
    private ThreadedSocketAcceptor? _acceptor;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = new SessionSettings(Path.Combine(AppContext.BaseDirectory, "SessionConfig", "fix-acceptor.cfg"));

        _acceptor = new ThreadedSocketAcceptor(app, new FileStoreFactory(settings), settings, new FileLogFactory(settings));
        _acceptor.Start();

        logger.LogInformation("FIX Acceptor iniciado.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _acceptor?.Stop();
        _acceptor?.Dispose();

        logger.LogInformation("FIX Acceptor encerrado.");
        return Task.CompletedTask;
    }
}