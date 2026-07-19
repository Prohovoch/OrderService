
using OrderService.Infrastructure.Entities.Catalog;

namespace OrderService.Infrastructure.Entities.Admin
{
    public class AdminModel
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public long TgId { get; set; }
        public AdminProfileModel? Profile { get; set; }
        public List<ProductModel> Products { get; } = new List<ProductModel>();
    }
}
