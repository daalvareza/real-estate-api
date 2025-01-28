using RealEstateApi.Domain.Entities;

namespace RealEstateApi.Application.Interfaces
{
    public interface IOwnerRepository
    {
        Task<Owner?> GetOwnerByIdAsync(string idOwner);
        Task<Owner?> GetOwnerByEmailAsync(string email);
        Task CreateOwnerAsync(Owner owner);
    }
}
