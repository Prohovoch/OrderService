using System.Reflection.Metadata;

namespace OrderService.Infrastructure.Entities.Auth
{
    public enum TokenType
    {
       Admin, Worker
    }
    public class TokenModel
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public string TokenHash { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public TokenType Type { get; set; }
    }
}
