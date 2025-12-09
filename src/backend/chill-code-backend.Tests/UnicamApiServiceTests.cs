using ChillCodeBackend.Data.Api;
using ChillCodeBackend.Models.Unicam;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace ChillCodeBackend.Tests
{
    [TestFixture]
    public class UnicamApiServiceIntegrationTests
    {
        private static HttpClient CreateHttpClient()
        {
            return new HttpClient
            {
                BaseAddress = new Uri("https://apispm.eppoi.io"),
                Timeout = TimeSpan.FromSeconds(40),
            };
        }

        private static UnicamApiService CreateService(HttpClient httpClient)
        {
            return new UnicamApiService(
                new UnicamApiProxy(httpClient),
                NullLogger<UnicamApiService>.Instance
            );
        }

        [Test]
        public async Task GetPoiCardsAsync_ReturnsData_ForGradara()
        {
            using var httpClient = CreateHttpClient();
            var service = CreateService(httpClient);

            var cards = await service.GetPoiCardsAsync(
                UnicamPoiCategory.ArtCulture,
                "Gradara",
                cancellationToken: CancellationToken.None
            );

            Assert.That(cards, Is.Not.Empty);
            Assert.That(cards.All(card => !string.IsNullOrWhiteSpace(card.EntityId)), Is.True);
        }

        [Test]
        public async Task GetRoutesAsync_ReturnsData_ForGradara()
        {
            using var httpClient = CreateHttpClient();
            var service = CreateService(httpClient);

            var routes = await service.GetRoutesAsync("Gradara", cancellationToken: CancellationToken.None);

            Assert.That(routes, Is.Not.Empty);
            Assert.That(routes.All(route => !string.IsNullOrWhiteSpace(route.EntityId)), Is.True);
        }

        [Test]
        public async Task GetOrganizationsAsync_ReturnsData_ForGradara()
        {
            using var httpClient = CreateHttpClient();
            var service = CreateService(httpClient);

            var organizations = await service.GetOrganizationsAsync(
                "Gradara",
                cancellationToken: CancellationToken.None
            );

            Assert.That(organizations, Is.Not.Empty);
            Assert.That(
                organizations.All(org => !string.IsNullOrWhiteSpace(org.EntityId)),
                Is.True
            );
        }

        [Test]
        [Explicit("Events endpoint is occasionally slow; run explicitly when needed.")]
        public async Task GetEventsAsync_Completes_ForGradara()
        {
            using var httpClient = CreateHttpClient();
            var service = CreateService(httpClient);

            var eventsResult = await service.GetEventsAsync(
                "Gradara",
                cancellationToken: CancellationToken.None
            );

            Assert.That(eventsResult, Is.Not.Null);
        }
    }
}
