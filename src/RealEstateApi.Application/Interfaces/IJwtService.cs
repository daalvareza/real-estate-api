namespace RealEstateApi.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(string ownerId);
    }
}
