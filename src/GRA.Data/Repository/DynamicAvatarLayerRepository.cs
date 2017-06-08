using Microsoft.Extensions.Logging;
using GRA.Domain.Repository;
using GRA.Domain.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using System.Linq;


namespace GRA.Data.Repository
{
    public class DynamicAvatarLayerRepository
        : AuditingRepository<Model.DynamicAvatarLayer, DynamicAvatarLayer>,
        IDynamicAvatarLayerRepository
    {
        public DynamicAvatarLayerRepository(ServiceFacade.Repository repositoryFacade,
            ILogger<DynamicAvatarLayerRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<DynamicAvatarLayer>> GetAllAsync()
        {
            // order by position
            return await DbSet
               .AsNoTracking()
               .OrderBy(_ => _.Id)
               .ProjectTo<DynamicAvatarLayer>()
               .ToListAsync();
        }

        public async Task<ICollection<int>> GetLayerIdsAsync()
        {
            // order by id
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Id)
                .Select(_ => _.Id)
                .ToListAsync();
        }
    }
}
