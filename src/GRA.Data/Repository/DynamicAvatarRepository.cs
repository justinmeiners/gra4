using Microsoft.Extensions.Logging;
using GRA.Domain.Repository;
using GRA.Domain.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using System.Linq;
using System;

namespace GRA.Data.Repository
{
    public class DynamicAvatarRepository
        : AuditingRepository<Model.DynamicAvatar, DynamicAvatar>,
        IDynamicAvatarRepository
    {
        public DynamicAvatarRepository(ServiceFacade.Repository repositoryFacade,
            ILogger<DynamicAvatarRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task AddUserAvatar(int userId, int avatarId)
        {
            if (!await UserHasAvatar(userId, avatarId))
            {
                _context.UserAvatars.Add(new Model.UserAvatar
                {
                    UserId = userId,
                    DynamicAvatarId = avatarId,
                    CreatedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task<bool> UserHasAvatar(int userId, int avatarId)
        {
            return null != await _context.UserAvatars
                .Where(_ => _.UserId == userId && _.DynamicAvatarId == avatarId)
                .SingleOrDefaultAsync();
        }
         public async Task<ICollection<DynamicAvatar>> GetPaginatedAvatarListAsync(
                    int siteId,
                    int skip,
                    int take,
                    string search = default(string))
        {
            var avatars = DbSet.AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                avatars = avatars.Where(_ => _.Name.Contains(search));
            }

            return await avatars.OrderBy(_ => _.Position)
                    .ThenBy(_ => _.Name)
                    .Skip(skip)
                    .Take(take)
                    .ProjectTo<DynamicAvatar>()
                    .ToListAsync();
        }

        public async Task<ICollection<DynamicAvatar>> GetAvatarListAsync()
        {
            var avatars = DbSet.AsNoTracking();
            
            return await avatars.OrderBy(_ => _.Position)
                    .ThenBy(_ => _.Name)
                    .ProjectTo<DynamicAvatar>()
                    .ToListAsync();
        }

        new public async Task<DynamicAvatar> GetByIdAsync(int id)
        {
            var avatar = await DbSet
               .AsNoTracking()
               .Include(_ => _.Elements)
               .Where(_ => _.Id == id)
               .ProjectTo<DynamicAvatar>()
               .SingleOrDefaultAsync();

            return avatar;
        }
        public override async Task RemoveSaveAsync(int userId, int id)
        {
            _context.DynamicAvatarElements
                               .AsNoTracking()
                               .Where(_ => _.DynamicAvatarId == id)
                               .ToList().ForEach(x => _context.DynamicAvatarElements.Remove(x));

            _context.DynamicAvatarElements.RemoveRange();
            await base.RemoveSaveAsync(userId, id);
        }

        public async Task AddElement(int avatarId, int layerId)
        {
            var exists = await _context.DynamicAvatarElements
                                .Where(_ => _.DynamicAvatarId == avatarId && _.DynamicAvatarLayerId == layerId)
                                .AnyAsync();

            if (!exists)
            {
                var newElement =  new Model.DynamicAvatarElement
                {
                    DynamicAvatarId = avatarId,
                    DynamicAvatarLayerId = layerId,
                };

                _context.DynamicAvatarElements.Add(newElement);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<bool> ElementIsAvailable(int layerId, int avatarId, int userId)
        {
            return await _context.UserAvatars
                                .AsNoTracking()
                                .Where(_ => _.DynamicAvatarId == avatarId)
                                .Join(_context.DynamicAvatarElements,
                                u => u.DynamicAvatarId,
                                e => e.DynamicAvatarId,
                                (u, e) => e)
                                .Where(_ => _.DynamicAvatarLayerId == layerId)
                                .AnyAsync();
        }

        public async Task<DynamicAvatarElement> GetElementAsync(int avatarId, int dynamicAvatarLayerId)
        {
            var entity = await _context.DynamicAvatarElements
                .AsNoTracking()
                .Where(_ => _.DynamicAvatarId == avatarId && _.DynamicAvatarLayerId == dynamicAvatarLayerId)
                .SingleOrDefaultAsync();
            
            if (entity == null)
            {
                throw new Exception($"{nameof(DynamicAvatarElement)} id {avatarId} with layer id {dynamicAvatarLayerId} could not be found.");
            }
            return _mapper.Map<DynamicAvatarElement>(entity);
        }

        public async Task<int> GetFirstElement(int layerId, int userId)
        {
            var composite = await _context.UserAvatars
                                .AsNoTracking()
                                .Where(_ => _.UserId == userId)
                                .Join(_context.DynamicAvatarElements, 
                                    u => u.DynamicAvatarId, 
                                    e => e.DynamicAvatarId, 
                                    (u, e) => ( e ))
                                .Where(_ => _.DynamicAvatarLayerId == layerId)
                                .Join(_context.DynamicAvatars,
                                        e => e.DynamicAvatarId,
                                        a => a.Id,
                                        (e, a) => new { Element = e, Position = a.Position })
                                .OrderBy(_ => _.Position)
                                .FirstOrDefaultAsync();

            if (composite == null || composite.Element == null)
            {
                throw new Exception($"Couldn't find first element for layer {layerId}");
            }

            return composite.Element.DynamicAvatarId;
        }
        public async Task<int> GetLastElement(int layerId, int userId)
        {
            var composite = await _context.UserAvatars
                            .AsNoTracking()
                            .Where(_ => _.UserId == userId)
                            .Join(_context.DynamicAvatarElements, 
                                u => u.DynamicAvatarId, 
                                e => e.DynamicAvatarId, 
                                (u, e) => ( e ))
                            .Where(_ => _.DynamicAvatarLayerId == layerId)
                            .Join(_context.DynamicAvatars,
                                    e => e.DynamicAvatarId,
                                    a => a.Id,
                                    (e, a) => new { Element = e, Position = a.Position })
                            .OrderByDescending(_ => _.Position)
                            .FirstOrDefaultAsync();

            if (composite == null || composite.Element == null) 
            {
                throw new Exception($"Couldn't find first element for layer {layerId}");
            }

            return composite.Element.DynamicAvatarId;
        }

        public async Task<int?> GetNextElement(int layerId, int avatarId, int userId)
        {
            var avatar = await _context.DynamicAvatars.AsNoTracking()
                                    .Where(_ => _.Id == avatarId)
                                    .SingleOrDefaultAsync();

            if (avatar == null)
            {
                throw new Exception($"Couldn't find avatar {avatarId}");
            }

            var nextComposite = await _context.UserAvatars
                                    .AsNoTracking()
                                    .Where(_ => _.UserId == userId)
                                    .Join(_context.DynamicAvatarElements, 
                                        u => u.DynamicAvatarId, 
                                        e => e.DynamicAvatarId, 
                                        (u, e) => ( e ))
                                    .Where(_ => _.DynamicAvatarLayerId == layerId)
                                    .Join(_context.DynamicAvatars,
                                            e => e.DynamicAvatarId,
                                            a => a.Id,
                                            (e, a) => new { Element = e, Position = a.Position })
                                    .OrderBy(_ => _.Position)
                                    .Where(_ => _.Position > avatar.Position)
                                    .FirstOrDefaultAsync();

            if (nextComposite != null && nextComposite.Element != null)
            {
                return nextComposite.Element.DynamicAvatarId;
            }

            return await GetFirstElement(layerId, userId);
        }

        public async Task<int?> GetPreviousElement(int layerId, int avatarId, int userId)
        {

            var avatar = await _context.DynamicAvatars.AsNoTracking()
                                                      .Where(_ => _.Id == avatarId)
                                                      .SingleOrDefaultAsync();

            if (avatar == null)
            {
                throw new Exception($"Couldn't find  avatar {avatarId}");
            }

            var nextComposite = await _context.UserAvatars
                        .AsNoTracking()
                        .Where(_ => _.UserId == userId)
                        .Join(_context.DynamicAvatarElements, 
                            u => u.DynamicAvatarId, 
                            e => e.DynamicAvatarId, 
                            (u, e) => ( e ))
                        .Where(_ => _.DynamicAvatarLayerId == layerId)
                        .Join(_context.DynamicAvatars,
                                e => e.DynamicAvatarId,
                                a => a.Id,
                                (e, a) => new { Element = e, Position = a.Position })
                        .OrderByDescending(_ => _.Position)
                        .Where(_ => _.Position < avatar.Position)
                        .FirstOrDefaultAsync();

            if (nextComposite != null && nextComposite.Element != null)
            {
                return nextComposite.Element.DynamicAvatarId;
            }

            return await GetLastElement(layerId, userId);
        }
    }
}
