using Microsoft.AspNetCore.Http;

namespace RealEstateApi.Application.Dtos
{
    public class PropertyCreateDto
    {
        public string? IdOwner { get; set; } = null;
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public decimal Price { get; set; }
        public string? CodeInternal { get; set; } = null;
        public int Year { get; set; }
        public IFormFile Image { get; set; } = null!;
    }
}