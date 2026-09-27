using OrderGenerator.Api.Contracts;
using OrderGenerator.Core.Orders;

namespace OrderGenerator.Api.Fix;

public interface IOrderSender
{
    Task<OrderResponse> SendAsync(Order order, CancellationToken cancellationToken);
}