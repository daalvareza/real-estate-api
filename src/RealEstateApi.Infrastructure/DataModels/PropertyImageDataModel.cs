using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RealEstateApi.Infrastructure.DataModels
{
    public class PropertyImageDataModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string IdProperty { get; set; } = null!;

        public byte[] File { get; set; } = null!;
        public bool Enabled { get; set; }
    }
}
