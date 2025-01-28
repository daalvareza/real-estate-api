namespace RealStateApi.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(string ownerId);
    }
}
