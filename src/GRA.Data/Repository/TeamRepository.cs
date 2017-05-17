using System.Collections.Generic;
using System.Threading.Tasks;
using GRA.Domain.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AutoMapper.QueryableExtensions;

namespace GRA.Data.Repository
{
    public class TeamRepository
        : AuditingRepository<Model.Team, Domain.Model.Team>, ITeamRepository
    {
        public TeamRepository(ServiceFacade.Repository repositoryFacade,
            ILogger<TeamRepository> logger) : base(repositoryFacade, logger)
        {
        }
        public async Task<IEnumerable<Domain.Model.Team>> GetAllAsync(int siteId)
        {
            return await _context.Teams
                .AsNoTracking()
                .Where(_ => _.SiteId == siteId)
                .OrderBy(_ => _.Name)
                .ProjectTo<Domain.Model.Team>()
                .ToListAsync();
        }


    }
}
