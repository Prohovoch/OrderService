
using OrderService.Infrastructure.Entities.Catalog;

namespace OrderService.Infrastructure.Entities.Admin
{
    public class Admin
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public long TgId { get; set; }
        public AdminProfile? Profile { get; set; }
        public List<Product> Products { get; } = new List<Product>();
    }
}
