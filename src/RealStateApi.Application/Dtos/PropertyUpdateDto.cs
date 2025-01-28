using Microsoft.AspNetCore.Http;

namespace RealStateApi.Application.Dtos
{
    public class PropertyUpdateDto
    {
        public string IdProperty { get; set; } = null!;
        public string? IdOwner { get; set; } = null;
        public string? Name { get; set; }
        public string? Address { get; set; }
        public decimal? Price { get; set; }
        public string? CodeInternal { get; set; }
        public int? Year { get; set; }
        public IFormFile? Image { get; set; } = null;
    }
}