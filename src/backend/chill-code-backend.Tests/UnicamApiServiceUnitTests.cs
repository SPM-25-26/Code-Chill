using System.Net.Http;
using ChillCodeBackend.Data.Api;
using ChillCodeBackend.Models.Unicam;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace ChillCodeBackend.Tests
{
    [TestFixture]
    public class UnicamApiServiceUnitTests
    {
        private IUnicamApiProxy _proxy = null!;
        private ILogger<UnicamApiService> _logger = null!;
        private UnicamApiService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _proxy = Substitute.For<IUnicamApiProxy>();
            _logger = Substitute.For<ILogger<UnicamApiService>>();
            _service = new UnicamApiService(_proxy, _logger);
        }

        [Test]
        public async Task GetPoiCardsAsync_ComposesQueryAndReturnsData()
        {
            var expected = new List<UnicamPoiCard>
            {
                new() { EntityId = "id-1", EntityName = "Name" },
            };

            _proxy
                .FetchAsAsync<List<UnicamPoiCard>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(expected);

            var result = await _service.GetPoiCardsAsync(
                UnicamPoiCategory.ArtCulture,
                "Gradara",
                "en",
                CancellationToken.None
            );

            await _proxy.Received(1).FetchAsAsync<List<UnicamPoiCard>>(
                Arg.Is<string>(path =>
                    path.StartsWith("/api/art-culture/card-list", StringComparison.Ordinal)
                    && path.Contains("municipality=Gradara")
                    && path.Contains("language=en")
                ),
                Arg.Any<CancellationToken>()
            );

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public async Task GetEventsAsync_FormatsDateAndOptionalParams()
        {
            _proxy
                .FetchAsAsync<List<UnicamEventCard>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new List<UnicamEventCard>());

            var date = new DateOnly(2024, 1, 2);

            await _service.GetEventsAsync(
                "Gradara",
                "My Title",
                date,
                "de",
                CancellationToken.None
            );

            await _proxy.Received(1).FetchAsAsync<List<UnicamEventCard>>(
                Arg.Is<string>(path =>
                    path.Contains("municipality=Gradara")
                    && path.Contains("language=de")
                    && path.Contains("title=My+Title")
                    && path.Contains("date=2024-01-02")
                ),
                Arg.Any<CancellationToken>()
            );
        }

        [Test]
        public void GetRoutesAsync_WhenMunicipalityMissing_Throws()
        {
            Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetRoutesAsync(" ", cancellationToken: CancellationToken.None)
            );
        }

        [Test]
        public void GetPoiCardsAsync_WhenProxyThrowsHttpRequestException_Wraps()
        {
            _proxy
                .FetchAsAsync<List<UnicamPoiCard>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(
                    Task.FromException<List<UnicamPoiCard>?>(new HttpRequestException("fail"))
                );

            Assert.ThrowsAsync<UnicamApiException>(() =>
                _service.GetPoiCardsAsync(UnicamPoiCategory.ArtCulture, "Gradara")
            );
        }
    }
}
