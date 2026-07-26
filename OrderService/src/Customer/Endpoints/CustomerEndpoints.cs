namespace OrderService.src.Customer.Endpoints
{
    public static class CustomerEndpoints
    {
        public static void MapCustomerEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/customer").WithTags("Customer");
            group.MapPost("/{tgId}", CustomerCreateHandler);
            group.MapPost("/{id}/profile", CreateProfileHandler);
            group.MapPatch("/{id}/profile", UpdateProfileHandler);
            group.MapGet("/{id}/profile", GetProfileHandler);
            group.MapDelete("/{id}", DeleteCustomerHandler);
        }

    }
}
