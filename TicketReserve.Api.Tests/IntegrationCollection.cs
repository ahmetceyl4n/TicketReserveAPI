namespace TicketReserve.Api.Tests;

// [Collection("Integration")] ile işaretli tüm test sınıfları TEK IntegrationTestFactory'yi paylaşır
// (tek konteyner seti). Bu sınıfın gövdesi boş — sadece xUnit'e "bu collection şu fixture'ı paylaşsın"
// diyen bir işaretleyici. Aynı collection'daki testler sıralı koşar → paylaşılan state yarışmaz.
[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<IntegrationTestFactory>;
