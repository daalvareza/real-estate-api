using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RealEstateApi.Infrastructure.DataModels
{
    public class OwnerDataModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = null!;
        public string Photo { get; set; } = null!;
        public DateTime Birthday { get; set; }
    }
}
