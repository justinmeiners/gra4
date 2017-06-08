﻿using GRA.Controllers.ViewModel.Home;
using GRA.Domain.Model;
using GRA.Domain.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using GRA.Controllers.Attributes;

namespace GRA.Controllers
{
    public class HomeController : Base.UserController
    {
        private const string ActivityErrorMessage = "ActivityErrorMessage";
        private const string MissingTitle = "MissingTitle";
        private const string MissingAuthor = "MissingAuthor";

        private const string MissingReview = "MissingReview";

        private const string ModelData = "ModelData";
        private const string SecretCodeMessage = "SecretCodeMessage";
        private const int BadgesToDisplay = 6;

        private readonly ILogger<HomeController> _logger;
        private readonly ActivityService _activityService;
        private readonly DynamicAvatarService _dynamicAvatarService;
        private readonly EmailReminderService _emailReminderService;
        private readonly SiteService _siteService;
        private readonly StaticAvatarService _staticAvatarService;
        private readonly UserService _userService;

        private readonly TeamService _teamService;
        public HomeController(ILogger<HomeController> logger,
            ServiceFacade.Controller context,
            ActivityService activityService,
            DynamicAvatarService dynamicAvatarService,
            EmailReminderService emailReminderService,
            SiteService siteService,
            StaticAvatarService staticAvatarService,
            UserService userService,
            TeamService teamService)
            : base(context)
        {
            _logger = Require.IsNotNull(logger, nameof(logger));
            _activityService = Require.IsNotNull(activityService, nameof(activityService));
            _dynamicAvatarService = Require.IsNotNull(dynamicAvatarService,
                nameof(dynamicAvatarService));
            _emailReminderService = Require.IsNotNull(emailReminderService,
                nameof(emailReminderService));
            _staticAvatarService = Require.IsNotNull(staticAvatarService,
                nameof(staticAvatarService));
            _siteService = Require.IsNotNull(siteService, nameof(siteService));
            _userService = Require.IsNotNull(userService, nameof(userService));
            _teamService = Require.IsNotNull(teamService, nameof(teamService));
        }

        public async Task<IActionResult> Index()
        {
            var site = await GetCurrentSiteAsync();
            if (site != null)
            {
                PageTitle = site.Name;
            }
            if (AuthUser.Identity.IsAuthenticated)
            {
                // signed-in users can view the dashboard
                var user = await _userService.GetDetails(GetActiveUserId());
                string staticAvatarPath = null;
                DynamicAvatarDetails dynamicAvatarDetails = null;

                if (site.UseDynamicAvatars)
                {
                    dynamicAvatarDetails = await GetDynamicAvatarDetailsAsync(user.DynamicAvatar,
                        _dynamicAvatarService);
                }
                else
                {
                    var avatar = new StaticAvatar();
                    if (user.AvatarId != null)
                    {
                        avatar = await _staticAvatarService.GetByIdAsync(user.AvatarId.Value);
                        staticAvatarPath = _pathResolver.ResolveContentPath(avatar.Filename);
                    }
                }

                var badges = await _userService.GetPaginatedBadges(user.Id, 0, BadgesToDisplay);
                foreach (var badge in badges.Data)
                {
                    badge.Filename = _pathResolver.ResolveContentPath(badge.Filename);
                }

                var pointTranslation = await _activityService.GetUserPointTranslationAsync();

                string teamName = null;
                if (user.TeamId.HasValue)
                {
                    var team = await _teamService.GetByIdAsync(user.TeamId.Value);
                    if (team != null)
                    {
                        teamName = team.Name;
                    }
                }

                var viewModel = new DashboardViewModel()
                {
                    FirstName = user.FirstName,
                    TeamName = teamName,
                    CurrentPointTotal = user.PointsEarned,
                    SingleEvent = pointTranslation.IsSingleEvent,
                    ActivityDescription = pointTranslation.ActivityDescription,
                    ActivityDescriptionPlural = pointTranslation.ActivityDescriptionPlural,
                    TranslationDescriptionPresentTense = pointTranslation.TranslationDescriptionPresentTense,
                    TranslationDescriptionPastTense = pointTranslation.TranslationDescriptionPastTense,
                    Badges = badges.Data,
                    AskTitle = pointTranslation.AskTitle,
                    AskAuthor = pointTranslation.AskAuthor,
                    AskReview = pointTranslation.AskReview,
                };

                var program = await _siteService.GetProgramByIdAsync(user.ProgramId);

                int? achieveAmount = null;

                if (program.AchieverGoalMultiplier.HasValue && user.Goal.HasValue)
                {
                    achieveAmount = user.Goal * program.AchieverGoalMultiplier.Value;
                }
                else if (program.AchieverTotal.HasValue)
                {
                    achieveAmount = program.AchieverTotal.Value;
                }

                if (achieveAmount.HasValue)
                {
                    float dec = user.PointsEarned / (float)achieveAmount.Value;
                    viewModel.AchieverProgressPercent = (int)(dec * 100.0f);
                }

                if (!string.IsNullOrEmpty(staticAvatarPath))
                {
                    viewModel.AvatarPath = staticAvatarPath;
                }
                if (dynamicAvatarDetails != null
                    && dynamicAvatarDetails.Paths.Count > 0)
                {
                    viewModel.DynamicAvatarPaths = dynamicAvatarDetails.Paths;
                }

                if (TempData.ContainsKey(ModelData))
                {
                    var model = Newtonsoft.Json.JsonConvert
                        .DeserializeObject<DashboardViewModel>((string)TempData[ModelData]);
                    viewModel.ActivityAmount = model.ActivityAmount;
                    viewModel.Title = model.Title;
                    viewModel.Author = model.Author;
                }
                if (TempData.ContainsKey(ActivityErrorMessage))
                {
                    ModelState.AddModelError("ActivityAmount", (string)TempData[ActivityErrorMessage]);
                    viewModel.ActivityAmount = null;
                }
                if (TempData.ContainsKey(MissingTitle))
                {
                    ModelState.AddModelError("Title", (string)TempData[MissingTitle]);
                }

                if (TempData.ContainsKey(MissingAuthor))
                {
                    ModelState.AddModelError("Author", (string)TempData[MissingAuthor]);
                }

                if (TempData.ContainsKey(MissingReview))
                {
                    ModelState.AddModelError("Review", (string)TempData[MissingReview]);
                }

                if (TempData.ContainsKey(SecretCodeMessage))
                {
                    viewModel.SecretCodeMessage = (string)TempData[SecretCodeMessage];
                }

                return View("Dashboard", viewModel);
            }
            else
            {
                // TODO handle pages if they are assigned in lieu of views
                switch (GetSiteStage())
                {
                    case SiteStage.BeforeRegistration:
                        var viewModel = new BeforeRegistrationViewModel
                        {
                            Site = await GetCurrentSiteAsync(),
                            SignUpSource = "BeforeRegistration"
                        };
                        if (viewModel.Site != null && viewModel.Site.RegistrationOpens != null)
                        {
                            viewModel.RegistrationOpens
                                = ((DateTime)viewModel.Site.RegistrationOpens).ToString("D");
                        }
                        return View("IndexBeforeRegistration", viewModel);
                    case SiteStage.RegistrationOpen:
                        return View("IndexRegistrationOpen");
                    case SiteStage.ProgramEnded:
                        return View("IndexProgramEnded");
                    case SiteStage.AccessClosed:
                        return View("IndexAccessClosed");
                    case SiteStage.ProgramOpen:
                    default:
                        return View("IndexProgramOpen");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddReminder(BeforeRegistrationViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.Email))
            {
                await _emailReminderService
                    .AddEmailReminderAsync(viewModel.Email, viewModel.SignUpSource);
                ShowAlertInfo("Thanks! We'll let you know when you can join the program.", "envelope");
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> GetIcs()
        {
            var site = await GetCurrentSiteAsync();
            var filename = new string(site.Name.Where(_ => char.IsLetterOrDigit(_)).ToArray());
            var siteUrl = await _siteService.GetBaseUrl(Request.Scheme, Request.Host.Value);
            var calendarBytes = await _siteService.GetIcsFile(siteUrl);
            return File(calendarBytes, "text/calendar", $"{filename}.ics");
        }

        [PreventQuestionnaireRedirect]
        public async Task<IActionResult> Signout()
        {
            if (AuthUser.Identity.IsAuthenticated)
            {
                await LogoutUserAsync();
            }
            return RedirectToAction("Index");
        }

        public IActionResult LogActivity()
        {
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> LogActivity(DashboardViewModel viewModel)
        {
            var pointTranslation = await _activityService.GetUserPointTranslationAsync();

            bool valid = true;
            if (!viewModel.SingleEvent
                && (viewModel.ActivityAmount == null || viewModel.ActivityAmount <= 0))
            {
                valid = false;
                TempData[ActivityErrorMessage] = "Please enter a whole number greater than 0.";
            }

            if (pointTranslation.AskTitle && string.IsNullOrWhiteSpace(viewModel.Title))
            {
                valid = false;
                TempData[MissingTitle] = "Please include the Title of the book";
            }

            if (pointTranslation.AskAuthor && string.IsNullOrWhiteSpace(viewModel.Author))
            {
                valid = false;
                TempData[MissingAuthor] = "Please include the Author of the book";
            }       

            if (pointTranslation.AskReview && string.IsNullOrWhiteSpace(viewModel.Review))
            {
                valid = false;
                TempData[MissingReview] = "Please include the Review of the book";
            }             

            if (!valid)
            {
                TempData[ModelData] = Newtonsoft.Json.JsonConvert.SerializeObject(viewModel);
            }
            else
            {
                // the service ignores the book if title is empty
                var book = new Domain.Model.Book
                {
                    Author = viewModel.Author,
                    Title = viewModel.Title,
                    Review = viewModel.Review
                };
                await _activityService
                    .LogActivityAsync(GetActiveUserId(), viewModel.ActivityAmount ?? 1, book);
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> LogSecretCode(DashboardViewModel viewModel)
        {
            try
            {
                await _activityService.LogSecretCodeAsync(GetActiveUserId(), viewModel.SecretCode);
            }
            catch (GraException gex)
            {
                TempData[SecretCodeMessage] = gex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}