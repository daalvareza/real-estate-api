namespace RealStateApi.Domain.Entities
{
    public class Property
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public decimal Price { get; set; }
        public string CodeInternal { get; set; } = null!;
        public int Year { get; set; }
        public string IdOwner { get; set; } = null!;
    }
}
