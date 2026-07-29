using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace TicketReserve.Api.Tests
{
    [Collection("Integration")]
    public class PurchaseConcurrencyTests
    {
        private readonly IntegrationTestFactory _factory;

        public PurchaseConcurrencyTests(IntegrationTestFactory factory)
        {
            _factory = factory;
        }

        private async Task<HttpClient> RegisterAndLoginAsync(string email, string password = "Passw0rd!") 
        {
            var client = _factory.CreateClient();

            await client.PostAsJsonAsync("/api/auth/register",
                new { email, password, fullName = "Test User" });

            var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
                new { email, password });

            var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

            client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login!.AccessToken);
            return client;
        }

        private async Task<(Guid EventId, Guid SeatId)> AdminCreateEventWithSeatAsync()
        {
            var admin = _factory.CreateClient();

            var loginResponse = await admin.PostAsJsonAsync("/api/auth/login",
                new { email = "admin@test.local", password = "Admin123!" });
            var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
            admin.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", login!.AccessToken);

            var eventResponse = await admin.PostAsJsonAsync("/api/events",
                new { name = "Concurrency Test", description = "test", venue = "Hall", date = DateTime.UtcNow.AddDays(30) });
            var createdEvent = await eventResponse.Content.ReadFromJsonAsync<EventCreated>();

            var seatsResponse = await admin.PostAsJsonAsync($"/api/events/{createdEvent!.Id}/seats",
                new { startRow = "A", endRow = "A", seatsPerRow = 1, price = 100 });
            var seats = await seatsResponse.Content.ReadFromJsonAsync<List<SeatCreated>>();

            return (createdEvent.Id, seats![0].Id);
        }

        private static async Task<HttpResponseMessage> LockAndPurchaseAsync(HttpClient client, Guid eventId, Guid seatId)
        {
            await client.PostAsJsonAsync($"/api/events/{eventId}/seats/lock",
                new { seatIds = new[] { seatId } });

            return await client.PostAsJsonAsync($"/api/events/{eventId}/seats/purchase",
                new { seatIds = new[] { seatId } });
        }

        [Fact]
        public async Task Two_users_buying_same_seat_only_one_succeeds()
        {
            // Kurulum: bir event + tek koltuk, iki farklı kullanıcı.
            var (eventId, seatId) = await AdminCreateEventWithSeatAsync();
            var userA = await RegisterAndLoginAsync($"a-{Guid.NewGuid():N}@test.local");
            var userB = await RegisterAndLoginAsync($"b-{Guid.NewGuid():N}@test.local");

            // Yarış: ikisi AYNI ANDA aynı koltuğa lock + purchase.
            var results = await Task.WhenAll(
                LockAndPurchaseAsync(userA, eventId, seatId),
                LockAndPurchaseAsync(userB, eventId, seatId));

            // Tam olarak biri başarılı olmalı.
            results.Count(r => r.IsSuccessStatusCode).Should().Be(1);
        }



        private sealed record LoginResponse(string AccessToken);
        private sealed record EventCreated(Guid Id);
        private sealed record SeatCreated(Guid Id);

    }
}
