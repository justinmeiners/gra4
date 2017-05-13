using GRA.Domain.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GRA.Domain.Repository
{
    public interface IDynamicAvatarRepository : IRepository<DynamicAvatar>
    {
        
        Task AddElement(int avatarId, int layerId);
        Task<bool> ElementIsAvailable(int layerId, int avatarId, int userId);
        Task<int> GetFirstElement(int layerId, int userId);
        Task<int> GetLastElement(int layerId, int userId);
        Task<int?> GetNextElement(int layerId, int avatarId, int userId);
        Task<int?> GetPreviousElement(int layerId, int avatarId, int userId);
        Task<ICollection<DynamicAvatar>> GetPaginatedAvatarListAsync(
            int siteId,
            int skip,
            int take,
            string search = default(string));

        Task<ICollection<DynamicAvatar>> GetAvatarListAsync();

        Task AddUserAvatar(int userId, int avatarId);
    }
}
