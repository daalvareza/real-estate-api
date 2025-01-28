using RealStateApi.Domain.Entities;

namespace RealStateApi.Application.Interfaces
{
    public interface IOwnerRepository
    {
        Task<Owner?> GetOwnerByIdAsync(string idOwner);
        Task<Owner?> GetOwnerByEmailAsync(string email);
        Task CreateOwnerAsync(Owner owner);
    }
}
