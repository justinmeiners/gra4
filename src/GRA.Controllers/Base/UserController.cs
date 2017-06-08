using GRA.Controllers.Filter;
using GRA.Domain.Repository.Extensions;
using GRA.Domain.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GRA.Controllers.Base
{
    [ServiceFilter(typeof(UserFilter), Order = 2)]
    [ServiceFilter(typeof(NotificationFilter))]
    public abstract class UserController : Controller
    {
        public UserController(ServiceFacade.Controller context) : base(context)
        {
        }

        protected async Task<DynamicAvatarDetails> GetDynamicAvatarDetailsAsync(string dynamicAvatar,
            DynamicAvatarService dynamicAvatarService)
        {
            int userId = GetActiveUserId();
            bool problem = false;

            // key is layer id, value is avatar id
            Dictionary<int, int> avatarLayerElements = null;
            if (!string.IsNullOrEmpty(dynamicAvatar))
            {
                // the string elements are ordered by layer Id
                var elementAvatarIds = new List<int>();
                foreach (string hexString in dynamicAvatar.SplitInParts(2))
                {
                    try
                    {
                        elementAvatarIds.Add(Convert.ToInt32(hexString, 16));
                    }
                    catch (Exception)
                    {
                        problem = true;
                        break;
                    }
                }
                if (!problem)
                {
                    avatarLayerElements = await dynamicAvatarService.ReturnValidated(elementAvatarIds, userId);
                    if (avatarLayerElements == null)
                    {
                        problem = true;
                    }
                }
            }

            var details = new DynamicAvatarDetails
            {
                Paths = new Dictionary<int, string>(),
                LayerOrders = new List<int>(),
            };


            var dynamicAvatarString = new StringBuilder();
            if (avatarLayerElements != null && !problem)
            {
                int siteId = GetCurrentSiteId();

                var layers = await dynamicAvatarService.GetAllLayersAsync();
                
                foreach (var layer in layers)
                {
                    var avatarId = avatarLayerElements[layer.Id];
                    string path = $"site{siteId}/dynamicavatars/layer{layer.Id}/{avatarId}.png";
                    details.Paths.Add(layer.Position, _pathResolver.ResolveContentPath(path));
                    dynamicAvatarString.Append(avatarLayerElements[layer.Id].ToString("x2"));
                }

                details.AvatarString = dynamicAvatarString.ToString();
            }
            return details;
        }
    }
}
