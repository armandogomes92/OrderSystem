using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using Message = QuickFix.Message;

namespace OrderGenerator.Api.Fix;

public sealed class GeneratorFixApp(PendingOrderRegistry registry, ILogger<GeneratorFixApp> logger)
    : MessageCracker, IApplication
{
    private volatile SessionID? _activeSession;

    public SessionID? ActiveSession => _activeSession;

    public void OnMessage(ExecutionReport report, SessionID sessionID)
    {
        var response = FixOrderTranslator.ToOrderResponse(report);

        if (!registry.TryComplete(response.ClOrdId, response))
            logger.LogWarning("ExecutionReport sem ordem aguardando: {ClOrdID}", response.ClOrdId);
    }

    public void FromApp(Message message, SessionID sessionID)
    {
        try
        {
            Crack(message, sessionID);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar mensagem {MsgType}", message.Header.GetString(Tags.MsgType));
            throw;
        }
    }

    public void OnLogon(SessionID sessionID)
    {
        _activeSession = sessionID;
        logger.LogInformation("Sessão conectada: {SessionID}", sessionID);
    }

    public void OnLogout(SessionID sessionID)
    {
        _activeSession = null;
        logger.LogInformation("Sessão desconectada: {SessionID}", sessionID);
    }

    public void OnCreate(SessionID sessionID) { }
    public void ToAdmin(Message message, SessionID sessionID) { }
    public void FromAdmin(Message message, SessionID sessionID) { }
    public void ToApp(Message message, SessionID sessionID) { }
}