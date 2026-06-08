using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using TwilightImperiumUltimate.Contracts.DTOs.Faction;
using TwilightImperiumUltimate.Contracts.DTOs.Galaxy;
using TwilightImperiumUltimate.Contracts.DTOs.Technology;
using TwilightImperiumUltimate.Web.Components.Library.Models;
using TwilightImperiumUltimate.Web.Components.Library.Services;
using TwilightImperiumUltimate.Web.Services.Search;
using Xunit;

namespace TwilightImperiumUltimate.Tests.Web;

public class GlobalSearchServiceTests
{
    [Fact]
    public async Task GetIndexAsyncAddsLocalResourcesAndCachesTheCatalog()
    {
        const string resourcesJson = """
            [
              {
                "id": "ffg-print-and-play",
                "type": "Print and Play",
                "title": "Official Card Sheets",
                "description": "Printable official cards.",
                "version": "Codex Vigil",
                "url": "https://example.com/cards.pdf",
                "icon": "/resources/images/shared/icons/pdfdownload.webp"
              }
            ]
            """;
        const string rulesJson = """
            {
              "schemaVersion": 1,
              "sourceRevision": "test",
              "sources": [],
              "rules": [
                {
                  "key": "breakthroughs",
                  "title": "Breakthroughs",
                  "contentHtml": "<p>Faction-specific cards.</p>",
                  "notesHtml": "",
                  "relatedKeys": [],
                  "sourceIds": ["tirules"],
                  "authoritySourceIds": ["ffg-thunders-edge"]
                }
              ]
            }
            """;
        var library = CreateEmptyLibrary();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        request.RequestUri?.AbsolutePath.EndsWith(
                            "rules-reference.json",
                            StringComparison.Ordinal) == true
                            ? rulesJson
                            : resourcesJson,
                        Encoding.UTF8,
                        "application/json"),
                });
        using var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://example.test/"),
        };
        var service = new GlobalSearchService(
            library.Object,
            httpClient,
            new TestNavigationManager("https://example.test/"));

        var firstIndex = await service.GetIndexAsync();
        var secondIndex = await service.GetIndexAsync();

        firstIndex.Should().BeSameAs(secondIndex);
        firstIndex.Should().HaveCount(2);
        firstIndex.Should().ContainEquivalentOf(
            new SearchDocument(
                "resource-ffg-print-and-play",
                "Resources",
                "Official Card Sheets",
                "Print and Play",
                "Printable official cards.",
                "/game/resources?type=print-and-play",
                "Printable official cards. Print and Play Codex Vigil"));
        firstIndex.Should().ContainEquivalentOf(
            new SearchDocument(
                "rule-breakthroughs",
                "Rules",
                "Breakthroughs",
                "Rules reference",
                "Faction-specific cards.",
                "/game/reference/breakthroughs",
                "Faction-specific cards."));
        handler.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    private static Mock<ILibraryDataService> CreateEmptyLibrary()
    {
        var library = new Mock<ILibraryDataService>(MockBehavior.Strict);
        library.Setup(service => service.GetFactionsAsync(false))
            .ReturnsAsync(Array.Empty<FactionDto>());
        library.Setup(service => service.GetTechnologiesAsync(false))
            .ReturnsAsync(Array.Empty<TechnologyDto>());
        library.Setup(service => service.GetPlanetsAsync(false))
            .ReturnsAsync(Array.Empty<PlanetDto>());
        library.Setup(service => service.GetSystemTilesAsync(false))
            .ReturnsAsync(Array.Empty<SystemTileDto>());
        library.Setup(service => service.GetRulesAsync(false))
            .ReturnsAsync(new RulesLibrarySnapshot([], [], []));
        library.Setup(service => service.GetCardsAsync(It.IsAny<string>(), false))
            .ReturnsAsync(Array.Empty<LibraryCardItem>());
        return library;
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager(string baseUri)
        {
            Initialize(baseUri, baseUri);
        }

        protected override void NavigateToCore(string uri, NavigationOptions options)
        {
        }
    }
}