using GRA.Domain.Model;
using GRA.Domain.Repository;
using GRA.Domain.Service.Abstract;
using GRA.Abstract;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;


namespace GRA.Domain.Service
{
    public class DynamicAvatarService : BaseUserService<DynamicAvatarService>
    {
        private readonly IDynamicAvatarRepository _dynamicAvatarRepository;
        private readonly IDynamicAvatarLayerRepository _dynamicAvatarLayerRepository;
        private readonly IPathResolver _pathResolver;
        public DynamicAvatarService(ILogger<DynamicAvatarService> logger,
            IUserContextProvider userContextProvider,
            IDynamicAvatarRepository dynamicAvatarRepository,
            IDynamicAvatarLayerRepository dynamicAvatarLayerRepository,
            IPathResolver pathResolver)
            : base(logger, userContextProvider)
        {
            _dynamicAvatarRepository = Require.IsNotNull(dynamicAvatarRepository,
                nameof(dynamicAvatarRepository));
    
            _dynamicAvatarLayerRepository = Require.IsNotNull(dynamicAvatarLayerRepository,
                nameof(dynamicAvatarLayerRepository));
            _pathResolver = Require.IsNotNull(pathResolver, nameof(pathResolver));

            SetManagementPermission(Permission.ManageAvatars);
        }

        public async Task<IEnumerable<DynamicAvatar>> GetPaginatedAvatarListAsync(int skip,
            int take,
            string Search)
        {
            VerifyManagementPermission();
            int siteId = GetClaimId(ClaimType.SiteId);
            return await _dynamicAvatarRepository.GetPaginatedAvatarListAsync(siteId, skip, take, Search);
        }

        public async Task<DynamicAvatar> GetAvatarDetailsAsync(int avatarId)
        {
            VerifyManagementPermission();
            return await _dynamicAvatarRepository.GetByIdAsync(avatarId);
        }

        public async Task<DynamicAvatar> EditAvatarAsync(DynamicAvatar graAvatar)
        {
            VerifyManagementPermission();
            return await _dynamicAvatarRepository.UpdateSaveAsync(GetClaimId(ClaimType.UserId), graAvatar);
        }

        public async Task<Dictionary<int, int>> GetDefaultAvatarAsync(int userId)
        {
            var layerIds = await _dynamicAvatarLayerRepository.GetLayerIdsAsync();

            var avatarParts = new Dictionary<int, int>();

            foreach (var layerId in layerIds)
            {
                var elementId = await _dynamicAvatarRepository.GetFirstElement(layerId, userId);
                avatarParts.Add(layerId, elementId);
            }

            return avatarParts;
        }

        public async Task<Dictionary<int, int>> ReturnValidated(IEnumerable<int> elementIds, int userId)
        {
            var avatarParts = new Dictionary<int, int>();
            var layerIds = await _dynamicAvatarLayerRepository.GetLayerIdsAsync();

            if (layerIds.Count() != elementIds.Count())
            {
                return null;
            }

            var layerElements = layerIds
                .Zip(elementIds, (LayerId, ElementId) => new { LayerId, ElementId })
                .ToDictionary(_ => _.LayerId, _ => _.ElementId);

            foreach (var layerId in layerElements.Keys)
            {
                if (!await _dynamicAvatarRepository.ElementIsAvailable(layerId, layerElements[layerId], userId))
                {
                    return null;
                }
            }
            return layerElements;
        }

        public async Task<int> GetNextElement(int layerNumber, int elementId, int userId)
        {
            var layerIds = await _dynamicAvatarLayerRepository.GetLayerIdsAsync();
            int layerId = layerIds.ElementAt(layerNumber - 1);
            var nextId = await _dynamicAvatarRepository.GetNextElement(layerId, elementId, userId);

            if (nextId == null)
            {
                nextId = await _dynamicAvatarRepository.GetFirstElement(layerId, userId);
            }

            return (int)nextId;
        }

        public async Task<int> GetPreviousElement(int layerNumber, int elementId, int userId)
        {
            var layerIds = await _dynamicAvatarLayerRepository.GetLayerIdsAsync();
            int layerId = layerIds.ElementAt(layerNumber - 1);
            var prevId = await _dynamicAvatarRepository.GetPreviousElement(layerId, elementId, userId);

            if (prevId == null)
            {
                prevId = await _dynamicAvatarRepository.GetLastElement(layerId, userId);
            }

            return (int)prevId;
        }
        public async Task<DynamicAvatarLayer> GetLayerByIdAsync(int id)
        {
            return await _dynamicAvatarLayerRepository.GetByIdAsync(id);
        }

        public async Task<ICollection<DynamicAvatarLayer>> GetAllLayersAsync()
        {
            return await _dynamicAvatarLayerRepository.GetAllAsync();
        }

        public async Task<DynamicAvatar> AddAvatarAsync(DynamicAvatar dynamicAvatar)
        {
            VerifyManagementPermission();
            return await _dynamicAvatarRepository.AddSaveAsync(GetClaimId(ClaimType.UserId),
                dynamicAvatar);
        }

        public async Task<DynamicAvatarLayer> AddLayerAsync(DynamicAvatarLayer dynamicAvatarLayer)
        {
            VerifyManagementPermission();
            return await _dynamicAvatarLayerRepository.AddSaveAsync(GetClaimId(ClaimType.UserId),
                dynamicAvatarLayer);
        }

        public async Task AddElementAsync(int avatarId, int layerId)
        {
            VerifyManagementPermission();
            await _dynamicAvatarRepository.AddElement(avatarId, layerId);
        }

        public async Task RemoveAvatarAsync(int avatarId)
        {
            VerifyManagementPermission();
            await _dynamicAvatarRepository.RemoveSaveAsync(GetClaimId(ClaimType.UserId), avatarId);
        }

        public void DeleteElementFile(int avatarId, int layerId)
        {
            VerifyManagementPermission();
            var destinationRoot = Path.Combine($"site{GetCurrentSiteId()}", "dynamicavatars", $"layer{layerId}");
            var destinationPath = _pathResolver.ResolveContentFilePath(destinationRoot);

            var fullFilePath = Path.Combine(destinationPath, $"{avatarId}.png");

            if (File.Exists(fullFilePath))
            {
                try
                {
                    File.Delete(fullFilePath);
                }
                catch (IOException)
                {
                    _logger.LogWarning($"Failed to delete element: {avatarId}");
                }
            }
        }
        public void WriteElementFile(int avatarId, int layerId, byte[] imageFile)
        {
            VerifyManagementPermission();
            var destinationRoot = Path.Combine($"site{GetCurrentSiteId()}", "dynamicavatars", $"layer{layerId}");
            var destinationPath = _pathResolver.ResolveContentFilePath(destinationRoot);

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            var fullFilePath = Path.Combine(destinationPath, $"{avatarId}.png");

            _logger.LogInformation($"Writing out avatar file {fullFilePath}...");
            File.WriteAllBytes(fullFilePath, imageFile);
        }
    }
}
