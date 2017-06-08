﻿using AutoMapper.QueryableExtensions;
using GRA.Domain.Model;
using GRA.Domain.Model.Filters;
using GRA.Domain.Repository;
using GRA.Domain.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GRA.Data.Repository
{
    public class EventRepository
        : AuditingRepository<Model.Event, Domain.Model.Event>, IEventRepository
    {
        public EventRepository(ServiceFacade.Repository repositoryFacade,
            ILogger<EventRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<int> CountAsync(EventFilter filter)
        {
            return await ApplyFilters(filter)
                .CountAsync();
        }

        public async Task<ICollection<Event>> PageAsync(EventFilter filter)
        {
            var events = await ApplyFilters(filter)
                .OrderBy(_ => _.StartDate)
                .ApplyPagination(filter)
                .ProjectTo<Event>()
                .ToListAsync();
            await AddLocationData(events);
            return events;
        }

        public override async Task<Event> GetByIdAsync(int id)
        {
            var evt = await base.GetByIdAsync(id);
            await AddLocationData(evt);
            return evt;
        }

        private async Task AddLocationData(ICollection<Event> events)
        {
            foreach (var evt in events)
            {
                await AddLocationData(evt);
            }
        }

        private async Task AddLocationData(Event evt)
        {
            if (evt.AtLocationId.HasValue)
            {
                try {
                    var location = await _context.Locations
                        .AsNoTracking()
                        .Where(_ => _.Id == evt.AtLocationId)
                        .SingleOrDefaultAsync();

                                        if (location != null)
                    {
                        evt.EventLocationAddress = location.Address;
                        evt.EventLocationLink = location.Url;
                        evt.EventLocationName = location.Name;
                        evt.EventLocationTelephone = location.Telephone;
                    }
                } catch (System.Exception e) {
                     _logger.LogWarning($"Add location data error: {e.Message}");
                }

            }
            else if (evt.AtBranchId.HasValue)
            {
                try {
                    var branch = await _context.Branches
                        .AsNoTracking()
                        .Where(_ => _.Id == evt.AtBranchId)
                        .SingleOrDefaultAsync();

                    if (branch != null)
                    {
                        evt.EventLocationAddress = branch.Address;
                        evt.EventLocationLink = branch.Url;
                        evt.EventLocationName = branch.Name;
                        evt.EventLocationTelephone = branch.Telephone;
                    }
                } catch (System.Exception e) {
                    _logger.LogWarning($"Add location data error: {e.Message}");
                }
            }
        }

        private IQueryable<Model.Event> ApplyFilters(EventFilter filter)
        {
            // site id filter
            var events = DbSet
                .AsNoTracking()
                .Where(_ => _.SiteId == filter.SiteId);

            // active-only filter
            if (filter.IsActive.HasValue)
            {
                events = events.Where(_ => _.IsActive == filter.IsActive);
            }

            if (filter.EventType.HasValue)
            {
                switch (filter.EventType.Value)
                {
                    case 0:
                        events = events.Where(_ => _.IsCommunityExperience == false);
                        break;

                    case 1:
                        events = events.Where(_ => _.IsCommunityExperience == true);
                        break;

                    default:
                        break;
                }
            }

            // apply filter
            // collect branch ids
            var branchIds = new HashSet<int?>();
            if (filter.BranchIds != null)
            {
                foreach (var branchId in filter.BranchIds)
                {
                    if (!branchIds.Contains(branchId))
                    {
                        branchIds.Add(branchId);
                    }
                }
            }

            if (filter.SystemIds != null)
            {
                var branchIdsFromSystem = _context.Branches
                    .AsNoTracking()
                    .Where(_ => filter.SystemIds.Contains(_.SystemId))
                    .Select(_ => _.Id);

                foreach (var branchId in branchIdsFromSystem)
                {
                    if (!branchIds.Contains(branchId))
                    {
                        branchIds.Add(branchId);
                    }
                }
            }

            // filter by branch ids
            if (branchIds.Count > 0)
            {
                events = events.Where(_ => branchIds.Contains(_.AtBranchId));
            }

            // filter by location ids
            if (filter.LocationIds != null)
            {
                events = events.Where(_ => filter.LocationIds.Contains(_.AtLocationId));
            }

            // filter by program ids
            if (filter.ProgramIds != null)
            {
                events = events.Where(_ => filter.ProgramIds.Contains(_.ProgramId));
            }

            // filter by user ids
            if (filter.UserIds != null)
            {
                events = events.Where(_ => filter.UserIds.Contains(_.CreatedBy));
            }

            // filter by dates
            if (filter.StartDate != null)
            {
                events = events.Where(_ => _.StartDate.Date >= filter.StartDate.Value.Date);
            }
            if (filter.EndDate != null)
            {
                events = events.Where(_ => _.StartDate.Date <= filter.EndDate.Value.Date);
            }

            // apply search
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var lowerSearch = filter.Search.ToLower();

                events = events.Where(_ => _.Name.ToLower().Contains(lowerSearch)
                    || _.Description.ToLower().Contains(lowerSearch));
            }

            return events;
        }

        public async Task<bool> LocationInUse(int siteId, int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SiteId == siteId && _.AtLocationId == locationId)
                .AnyAsync();
        }

        public async Task DetachRelatedTrigger(int triggerId)
        {
            var Events = await DbSet.Where(_ => _.RelatedTriggerId == triggerId).ToListAsync();
            Events.Select(_ => { _.RelatedTriggerId = null; return _; }).ToList();
            _context.UpdateRange(Events);
            await _context.SaveChangesAsync();
        }

        public async Task<ICollection<Event>> GetRelatedEventsForTriggerAsync(int triggerId)
        {
            return await DbSet.AsNoTracking()
                .Where(_ => _.RelatedTriggerId == triggerId)
                .ProjectTo<Event>()
                .ToListAsync();
        }
    }
}
