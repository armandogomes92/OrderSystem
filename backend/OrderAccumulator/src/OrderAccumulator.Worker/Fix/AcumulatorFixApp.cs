using OrderAccumulator.Core.Exposure;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using Message = QuickFix.Message;

namespace OrderAccumulator.Worker.Fix;

public sealed class AccumulatorFixApp(ExposureCalculator calculator, ILogger<AccumulatorFixApp> logger)
    : MessageCracker, IApplication
{
    public void OnMessage(NewOrderSingle message, SessionID sessionID)
    {
        if (!FixOrderTranslator.TryToOrder(message, out var order, out var rejectReason))
        {
            Session.SendToTarget(FixOrderTranslator.ToInvalidOrderReport(message, rejectReason), sessionID);
            logger.LogWarning("Ordem {ClOrdID} inválida: {Reason}", message.ClOrdID.Value, rejectReason);
            return;
        }

        var decision = calculator.Apply(order);
        var report = FixOrderTranslator.ToExecutionReport(message, decision);

        Session.SendToTarget(report, sessionID);

        logger.LogInformation(
            "Ordem {ClOrdID} {Side} {Quantity} {Symbol} @ {Price}: {Result}. Exposição {Symbol}: {Exposure}",
            message.ClOrdID.Value, order.Side, order.Quantity, order.Symbol, order.Price,
            decision.Accepted ? "aceita" : "rejeitada", order.Symbol, decision.Exposure);
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

    public void OnLogon(SessionID sessionID) => logger.LogInformation("Sessão conectada: {SessionID}", sessionID);
    public void OnLogout(SessionID sessionID) => logger.LogInformation("Sessão desconectada: {SessionID}", sessionID);

    public void OnCreate(SessionID sessionID) { }
    public void ToAdmin(Message message, SessionID sessionID) { }
    public void FromAdmin(Message message, SessionID sessionID) { }
    public void ToApp(Message message, SessionID sessionID) { }
}
