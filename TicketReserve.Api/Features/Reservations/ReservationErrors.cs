using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Reservations
{
    public static class ReservationErrors
    {
        public static readonly Error NotFound = new(
            "Reservation.NotFound",
            "Rezervasyon bulunamadı.",
            ErrorType.NotFound);

        // NotOwned da bilerek NotFound (404): "yok" ile "senin değil" aynı yanıtı verir,
        // saldırgan id deneyerek geçerli rezervasyonları ayırt edemez (varlığı gizle).
        public static readonly Error NotOwned = new(
            "Reservation.NotOwned",
            "Rezervasyon bulunamadı.",
            ErrorType.NotFound);
    }
}
