namespace RealEstateApi.Application.Dtos
{
    public class PropertyListDto
    {
        public string IdProperty { get; set; } = null!;
        public string IdOwner { get; set; } = null!;
        public string OwnerName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public decimal Price { get; set; }
        public string? FirstImage { get; set; }
    }
}
