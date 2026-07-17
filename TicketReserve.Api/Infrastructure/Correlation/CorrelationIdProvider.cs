namespace TicketReserve.Api.Infrastructure.Correlation;

public class CorrelationIdProvider : ICorrelationIdProvider
{
    public string CorrelationId { get; set; } = string.Empty;
}
