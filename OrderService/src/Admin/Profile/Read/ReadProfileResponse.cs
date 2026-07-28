using FastEndpoints;


namespace OrderService.src.Admin.Profile.Read
{
    public enum Gender { Male, Female }
    public sealed record ReadProfileResponse
    {
        public string Name { get; init; } = null!;
        public string Surname { get; init; } = null!;
        public int Age { get; init; }
        public Gender? Gender { get; init; }
    }
}
