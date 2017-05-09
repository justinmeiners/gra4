using System.Collections.Generic;
using System.Threading.Tasks;

namespace GRA.Domain.Repository
{
    public interface ITeamRepository : IRepository<Model.Team>
    {
        Task<IEnumerable<Model.Team>> GetAllAsync(int siteId);
    }
}
