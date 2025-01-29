namespace RealEstateApi.Application.Dtos
{
    public class PaginatedPropertiesDto
    {
        public long TotalCount { get; set; }
        public IEnumerable<PropertyListDto> Properties { get; set; } = Enumerable.Empty<PropertyListDto>();
    }
}
