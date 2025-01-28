namespace RealStateApi.Domain.Entities
{
    public class PropertyImage
    {
        public string Id { get; set; } = null!;
        public string IdProperty { get; set; } = null!;
        public byte[] File { get; set; } = null!;
        public bool Enabled { get; set; }
    }
}
