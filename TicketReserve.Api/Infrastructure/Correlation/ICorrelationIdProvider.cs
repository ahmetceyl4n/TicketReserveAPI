namespace TicketReserve.Api.Infrastructure.Correlation;

public interface ICorrelationIdProvider
{
    string CorrelationId { get; set; }
}
