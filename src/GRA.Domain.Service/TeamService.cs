using GRA.Domain.Model;
using GRA.Domain.Repository;
using GRA.Domain.Service.Abstract;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GRA.Domain.Service
{
    public class TeamService : Abstract.BaseUserService<TeamService>
    {
        private readonly ITeamRepository _teamRepository;
        public TeamService(ILogger<TeamService> logger,
            ITeamRepository teamRepository,
            IUserContextProvider userContextProvider) : base(logger, userContextProvider)
        {
            _teamRepository = Require.IsNotNull(teamRepository,
                nameof(teamRepository));
        }

        public async Task<Team> GetByIdAsync(int id)
        {
            return await _teamRepository.GetByIdAsync(id);
        }
    }
}
