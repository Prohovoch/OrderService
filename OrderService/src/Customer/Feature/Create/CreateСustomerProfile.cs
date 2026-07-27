
using Microsoft.AspNetCore.Mvc;
using OrderService.src.Customer.DTO;
using OrderService.Infrastructure.Persistence;


namespace OrderService.src.Customer.Feature.Create
{

    public class CreateCustomerProfile(ApplicationDbContext db)
    {
        // This class can contain methods or properties related to creating a customer profile.
        private readonly ApplicationDbContext _db = db;

        public async Task CreateCustomerProfileAsync(CreateCustomerProfileRequest request)
        {
            // Validate the request
            
    
        }

    



    
}
