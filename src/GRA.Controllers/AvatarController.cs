﻿using GRA.Controllers.ViewModel.Avatar;
using GRA.Domain.Repository.Extensions;
using GRA.Domain.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GRA.Controllers
{
    [Authorize]
    public class AvatarController : Base.UserController
    {
        private readonly ILogger<AvatarController> _logger;
        private readonly DynamicAvatarService _dynamicAvatarService;
        private readonly StaticAvatarService _staticAvatarService;
        private readonly UserService _userService;

        public AvatarController(ILogger<AvatarController> logger,
            ServiceFacade.Controller context,
            DynamicAvatarService dynamicAvatarService,
            StaticAvatarService staticAvatarService,
            UserService userService)
            : base(context)
        {
            _logger = Require.IsNotNull(logger, nameof(logger));
            _dynamicAvatarService = Require.IsNotNull(dynamicAvatarService,
                nameof(dynamicAvatarService));
            _staticAvatarService = Require.IsNotNull(staticAvatarService,
                nameof(staticAvatarService));
            _userService = Require.IsNotNull(userService, nameof(userService));
            PageTitle = "Avatar";
        }

        public async Task<IActionResult> Index(string id)
        {
            var currentSite = await GetCurrentSiteAsync();
            if (currentSite.UseDynamicAvatars)
            {
                if (string.IsNullOrEmpty(id))
                {
                    var currentUser = await _userService.GetDetails(GetActiveUserId());
                    if (!string.IsNullOrEmpty(currentUser.DynamicAvatar))
                    {
                        return RedirectToRoute(new
                        {
                            controller = "Avatar",
                            action = "Index",
                            id = currentUser.DynamicAvatar
                        });
                    }
                }
                return await DynamicIndex(id);
            }
            else
            {
                int? numericId = id == null ? null : (int?)Convert.ToInt32(id);
                return await StaticIndex(numericId);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(AvatarSelectionViewModel model)
        {
            try
            {
                var avatar = await _staticAvatarService.GetByIdAsync(model.Avatar.Id);
                var user = await _userService.GetDetails(GetActiveUserId());
                user.AvatarId = avatar.Id;
                await _userService.Update(user);
                AlertSuccess = "Your avatar has been updated";
                return RedirectToAction("Index", "Home");
            }
            catch (GraException gex)
            {
                AlertInfo = gex.Message;
                return RedirectToAction("Index");
            }
        }

        private async Task<IActionResult> StaticIndex(int? id)
        {
            var avatarList = (await _staticAvatarService.GetAvatarListAsync()).ToList();

            if (avatarList.Count() > 0)
            {
                if (id.HasValue)
                {
                    if (!avatarList.Any(_ => _.Id == id.Value))
                    {
                        id = null;
                    }
                }

                var user = await _userService.GetDetails(GetActiveUserId());
                var viewingAvatarId = id ?? user.AvatarId ?? avatarList.FirstOrDefault().Id;
                var avatar = avatarList.FirstOrDefault(_ => _.Id == viewingAvatarId);
                avatar.Filename = _pathResolver.ResolveContentPath(avatar.Filename);

                var currentIndex = avatarList.FindIndex(_ => _.Id == viewingAvatarId);
                int previousAvatarId;
                int nextAvatarId;
                if (currentIndex == 0)
                {
                    previousAvatarId = avatarList.Last().Id;
                }
                else
                {
                    previousAvatarId = avatarList[currentIndex - 1].Id;
                }

                if (currentIndex == avatarList.Count - 1)
                {
                    nextAvatarId = avatarList.First().Id;
                }
                else
                {
                    nextAvatarId = avatarList[currentIndex + 1].Id;
                }

                var viewModel = new AvatarSelectionViewModel()
                {
                    Avatar = avatarList.FirstOrDefault(_ => _.Id == viewingAvatarId),
                    PreviousAvatarId = previousAvatarId,
                    NextAvatarId = nextAvatarId
                };

                return View(viewModel);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> DynamicIndex(string id = default(string))
        {
            if (!string.IsNullOrEmpty(id) && id.Length % 2 != 0)
            {
                return RedirectToAction("Index");
            }

            var details = await GetDynamicAvatarDetailsAsync(id, _dynamicAvatarService);

            if (details.DynamicAvatarPaths.Count == 0)
            {
                // the requested avatar was not found, show the user's avatar
                int userId = GetActiveUserId();
                var currentUser = await _userService.GetDetails(userId);
                if (!string.IsNullOrEmpty(currentUser.DynamicAvatar))
                {
                    // validate avatar
                    details = await GetDynamicAvatarDetailsAsync(currentUser.DynamicAvatar,
                        _dynamicAvatarService);
                    if (details.DynamicAvatarPaths != null && details.DynamicAvatarPaths.Count() > 0)
                    {
                        // user has a valid avatar, redirect to that
                        return RedirectToRoute(new
                        {
                            controller = "Avatar",
                            action = "Index",
                            id = details.DynamicAvatarString
                        });
                    }
                }

                // check for a default avatar

                Dictionary<int, int> defaultDynamic = null;

                try
                {
                    defaultDynamic = await _dynamicAvatarService.GetDefaultAvatarAsync(userId);
                } catch {
                    defaultDynamic = null;
                }

                if (defaultDynamic == null || defaultDynamic.Count() == 0)
                {
                    // there is no default avatar, redirect to home page, avatars not configured
                    return RedirectToRoute(new
                    {
                        controller = "Home",
                        action = "Index",
                        id = string.Empty
                    });
                }
                else
                {
                    // build a url with the default avatar
                    var defaultId = new StringBuilder();

                    foreach (var key in defaultDynamic.Keys)
                    {
                        defaultId.Append(defaultDynamic[key].ToString("x2"));
                    }
                    return RedirectToRoute(new
                    {
                        controller = "Avatar",
                        action = "Index",
                        id = defaultId.ToString()
                    });
                }
            }

            return View("DynamicIndex", details);
        }

        public async Task<IActionResult> Increase(int id, DynamicAvatarDetails details)
        {
            return await IncreaseOrDecrease(id, details, true);
        }
        public async Task<IActionResult> Decrease(int id, DynamicAvatarDetails details)
        {
            return await IncreaseOrDecrease(id, details, false);
        }

        private async Task<IActionResult> IncreaseOrDecrease(int layerNumber, DynamicAvatarDetails details, bool increase)
        {
            var newValue = new StringBuilder();
            int counter = 0;
            int userId = GetActiveUserId();

            foreach (string elementIdHex in details.DynamicAvatarString.SplitInParts(2))
            {
                counter++;
                if (counter == layerNumber)
                {
                    int elementIdInt = Convert.ToInt32(elementIdHex, 16);
                    if (increase)
                    {
                        elementIdInt
                            = await _dynamicAvatarService.GetNextElement(layerNumber, elementIdInt, userId);
                    }
                    else
                    {
                        elementIdInt
                            = await _dynamicAvatarService.GetPreviousElement(layerNumber, elementIdInt, userId);
                    }
                    newValue.Append(elementIdInt.ToString("x2"));
                }
                else
                {
                    newValue.Append(elementIdHex);
                }
            }
            return RedirectToRoute(new
            {
                controller = "Avatar",
                action = "Index",
                id = newValue.ToString()
            });
        }

        [HttpPost]
        public async Task<IActionResult> DynamicIndex(DynamicAvatarDetails details)
        {
            var currentUserId = GetActiveUserId();
            var currentUser = await _userService.GetDetails(currentUserId);
            currentUser.DynamicAvatar = details.DynamicAvatarString.Trim();
            await _userService.Update(currentUser);
            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }
    }
}
