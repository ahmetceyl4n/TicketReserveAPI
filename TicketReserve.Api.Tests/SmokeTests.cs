using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace TicketReserve.Api.Tests
{
    [Collection("Integration")]
    public class SmokeTests
    {
        private readonly IntegrationTestFactory _factory;

        public SmokeTests(IntegrationTestFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Register_returns_success()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                email = $"smoke-{Guid.NewGuid():N}@test.local",
                password = "Passw0rd!",
                fullName = "Smoke Test"
            });
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
