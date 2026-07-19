
using OrderService.Infrastructure.Entities.Catalog;

namespace OrderService.Infrastructure.Entities.Admin
{
    public class Admin
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public long TgId { get; set; }
        public AdminProfile? Profile { get; set; }
        public List<CatalogItem> Products { get; } = new List<CatalogItem>();
    }
}
