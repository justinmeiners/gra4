﻿using GRA.Controllers.Filter;
using GRA.Controllers.ViewModel.Join;
using GRA.Controllers.ViewModel.Shared;
using GRA.Domain.Model;
using GRA.Domain.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GRA.Controllers
{
    [UnauthenticatedFilter]
    public class JoinController : Base.UserController
    {
        private const string TempStep1 = "TempStep1";
        private const string TempStep2 = "TempStep2";

        private readonly ILogger<JoinController> _logger;
        private readonly AutoMapper.IMapper _mapper;
        private readonly AuthenticationService _authenticationService;
        private readonly SchoolService _schoolService;
        private readonly SiteService _siteService;
        private readonly QuestionnaireService _questionnaireService;
        private readonly UserService _userService;
        public JoinController(ILogger<JoinController> logger,
            ServiceFacade.Controller context,
            AuthenticationService authenticationService,
            SchoolService schoolService,
            SiteService siteService,
            QuestionnaireService questionnaireService,
            UserService userService)
                : base(context)
        {
            _logger = Require.IsNotNull(logger, nameof(logger));
            _mapper = context.Mapper;
            _authenticationService = Require.IsNotNull(authenticationService,
                nameof(authenticationService));
            _schoolService = Require.IsNotNull(schoolService, nameof(schoolService));
            _siteService = Require.IsNotNull(siteService, nameof(siteService));
            _questionnaireService = Require.IsNotNull(questionnaireService,
                nameof(questionnaireService));
            _userService = Require.IsNotNull(userService, nameof(userService));
            PageTitle = "Join";
        }

        public async Task<IActionResult> Index()
        {
            var site = await GetCurrentSiteAsync();
            if (!site.SinglePageSignUp)
            {
                return RedirectToAction("Step1");
            }
            var siteStage = GetSiteStage();
            if (siteStage == SiteStage.BeforeRegistration)
            {
                if (site.RegistrationOpens.HasValue)
                {
                    AlertInfo = $"You can join {site.Name} on {site.RegistrationOpens.Value.ToString("d")}";
                }
                else
                {
                    AlertInfo = $"Registration for {site.Name} has not opened yet";
                }
                return RedirectToAction("Index", "Home");
            }
            else if (siteStage >= SiteStage.ProgramEnded)
            {
                AlertInfo = $"{site.Name} has ended, please join us next time!";
                return RedirectToAction("Index", "Home");
            }

            PageTitle = $"{site.Name} - Join Now!";

            var systemList = await _siteService.GetSystemList();
            var programList = await _siteService.GetProgramList();
            var programViewObject = _mapper.Map<List<ProgramViewModel>>(programList);
            var districtList = await _schoolService.GetDistrictsAsync();

            var viewModel = new SinglePageViewModel()
            {
                RequirePostalCode = site.RequirePostalCode,
                ProgramJson = Newtonsoft.Json.JsonConvert.SerializeObject(programViewObject),
                SystemList = new SelectList(systemList.ToList(), "Id", "Name"),
                ProgramList = new SelectList(programList.ToList(), "Id", "Name"),
                SchoolDistrictList = new SelectList(districtList.ToList(), "Id", "Name")
            };

            if (systemList.Count() == 1)
            {
                var systemId = systemList.SingleOrDefault().Id;
                var branchList = await _siteService.GetBranches(systemId);
                if (branchList.Count() > 1)
                {
                    branchList = branchList.Prepend(new Branch() { Id = -1 });
                }
                else
                {
                    viewModel.BranchId = branchList.SingleOrDefault().Id;
                }
                viewModel.BranchList = new SelectList(branchList.ToList(), "Id", "Name");
                viewModel.SystemId = systemId;
            }

            if (programList.Count() == 1)
            {
                var programId = programList.SingleOrDefault().Id;
                var program = await _siteService.GetProgramByIdAsync(programId);
                viewModel.ProgramId = programList.SingleOrDefault().Id;
                viewModel.ShowAge = program.AskAge;
                viewModel.ShowSchool = program.AskSchool;
                viewModel.ShowEmail = program.AskEmail;
                viewModel.ShowPhoneNumber = program.AskPhoneNumber;
                viewModel.ShowCard = program.AskCard;
                viewModel.ShowGoal = program.AskGoal;
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(SinglePageViewModel model)
        {
            var site = await GetCurrentSiteAsync();
            if (!site.SinglePageSignUp)
            {
                return RedirectToAction("Step1");
            }
            if (site.RequirePostalCode && string.IsNullOrWhiteSpace(model.PostalCode))
            {
                ModelState.AddModelError("PostalCode", "The Zip Code field is required.");
            }

            bool askAge = false;
            bool askSchool = false;
            bool askEmail = false;
            bool askCard = false;
            bool askGoal = false;
            bool askPhoneNumber = false;
            if (model.ProgramId.HasValue)
            {
                var program = await _siteService.GetProgramByIdAsync(model.ProgramId.Value);
                askAge = program.AskAge;
                askSchool = program.AskSchool;
                askEmail = program.AskEmail;
                askCard = program.AskCard;
                askGoal = program.AskGoal;
                askPhoneNumber = program.AskPhoneNumber;

                if (program.AgeRequired && !model.Age.HasValue)
                {
                    ModelState.AddModelError("Age", "The Age field is required.");
                }
                if (program.SchoolRequired)
                {
                    if (!model.NewEnteredSchool && !model.SchoolId.HasValue)
                    {
                        ModelState.AddModelError("SchoolId", "The School field is required.");
                    }
                    else if (model.NewEnteredSchool
                        && string.IsNullOrWhiteSpace(model.EnteredSchoolName))
                    {
                        ModelState.AddModelError("EnteredSchoolName", "The School Name field is required.");
                    }
                }
                if (model.NewEnteredSchool && !model.SchoolDistrictId.HasValue
                    && ((program.AskSchool && !string.IsNullOrWhiteSpace(model.EnteredSchoolName))
                        || program.SchoolRequired))
                {
                    ModelState.AddModelError("SchoolDistrictId", "The School District field is required.");
                }
                if (program.CardRequired && string.IsNullOrWhiteSpace(model.CardNumber))
                {
                    ModelState.AddModelError("CardNumber", "The library card field is required.");
                }
                if (program.GoalRequired && !model.Goal.HasValue)
                {
                    ModelState.AddModelError("Goal", "The goal is required.");
                }
                if (program.GoalMaximum.HasValue && model.Goal > program.GoalMaximum.Value)
                {
                    ModelState.AddModelError("Goal", string.Format("The goal maximum is {0}.", program.GoalMaximum.Value));
                }
                if (program.GoalMinimum.HasValue && model.Goal < program.GoalMinimum.Value)
                {
                    ModelState.AddModelError("Goal", string.Format("The goal minimum is {0}.", program.GoalMinimum.Value));
                }
                if (program.EmailRequired && string.IsNullOrWhiteSpace(model.Email))
                {
                    ModelState.AddModelError("Email", "The email field is required.");
                }
                if (program.PhoneNumberRequired && string.IsNullOrWhiteSpace(model.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "The phone number field is required.");
                }
            }

            if (ModelState.IsValid)
            {
                if (!askAge)
                {
                    model.Age = null;
                }
                if (askSchool)
                {
                    if (model.NewEnteredSchool)
                    {
                        model.SchoolId = null;
                    }
                    else
                    {
                        model.EnteredSchoolName = null;
                    }
                }
                else
                {
                    model.SchoolId = null;
                    model.EnteredSchoolName = null;
                }
                if (!askCard)
                {
                    model.CardNumber = null;
                }
                if (!askGoal)
                {
                    model.Goal = null;
                }
                if (!askEmail)
                {
                    model.Email = null;
                }
                if (!askPhoneNumber)
                {
                    model.PhoneNumber = null;
                }

                var user = _mapper.Map<User>(model);
                user.SiteId = site.Id;
                try
                {
                    await _userService.RegisterUserAsync(user, model.Password,
                        model.SchoolDistrictId);
                    var loginAttempt = await _authenticationService
                        .AuthenticateUserAsync(user.Username, model.Password);
                    await LoginUserAsync(loginAttempt);
                    var questionnaireId = await _questionnaireService
                            .GetRequiredQuestionnaire(loginAttempt.User.Id, loginAttempt.User.Age);
                    if (questionnaireId.HasValue)
                    {
                        HttpContext.Session.SetInt32(SessionKey.PendingQuestionnaire,
                            questionnaireId.Value);
                    }

                    return RedirectToAction("Index", "Home");
                }
                catch (GraException gex)
                {
                    ShowAlertDanger("Could not create your account: ", gex);
                    if (gex.Message.Contains("password"))
                    {
                        ModelState.AddModelError("Password", "Please correct the issues with your password.");
                    }
                }
            }

            PageTitle = $"{site.Name} - Join Now!";

            if (model.SystemId.HasValue)
            {
                var branchList = await _siteService.GetBranches(model.SystemId.Value);
                if (model.BranchId < 1)
                {
                    branchList = branchList.Prepend(new Branch() { Id = -1 });
                }
                model.BranchList = new SelectList(branchList.ToList(), "Id", "Name");
            }
            var systemList = await _siteService.GetSystemList();
            var programList = await _siteService.GetProgramList();
            var programViewObject = _mapper.Map<List<ProgramViewModel>>(programList);
            model.SystemList = new SelectList(systemList.ToList(), "Id", "Name");
            model.ProgramList = new SelectList(programList.ToList(), "Id", "Name");
            model.ProgramJson = Newtonsoft.Json.JsonConvert.SerializeObject(programViewObject);
            model.RequirePostalCode = site.RequirePostalCode;
            model.ShowAge = askAge;
            model.ShowSchool = askSchool;
            model.ShowEmail = askEmail;
            model.ShowPhoneNumber = askPhoneNumber;
            model.ShowCard = askCard;
            model.ShowGoal = askGoal;

            var districtList = await _schoolService.GetDistrictsAsync();
            if (model.SchoolId.HasValue)
            {
                var schoolDetails = await _schoolService.GetSchoolDetailsAsync(model.SchoolId.Value);
                var typeList = await _schoolService.GetTypesAsync(schoolDetails.SchoolDisctrictId);
                model.SchoolDistrictList = new SelectList(districtList.ToList(), "Id", "Name",
                    schoolDetails.SchoolDisctrictId);
                model.SchoolTypeList = new SelectList(typeList.ToList(), "Id", "Name",
                    schoolDetails.SchoolTypeId);
                model.SchoolList = new SelectList(schoolDetails.Schools.ToList(), "Id", "Name");
            }
            else
            {
                model.SchoolDistrictList = new SelectList(districtList.ToList(), "Id", "Name");
                if (model.SchoolDistrictId.HasValue)
                {
                    var typeList = await _schoolService.GetTypesAsync(model.SchoolDistrictId);
                    model.SchoolTypeList = new SelectList(typeList.ToList(), "Id", "Name",
                        model.SchoolTypeId);
                    var schoolList = await _schoolService.GetSchoolsAsync(model.SchoolDistrictId,
                        model.SchoolTypeId);
                    model.SchoolList = new SelectList(schoolList.ToList(), "Id", "Name");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Step1()
        {
            var site = await GetCurrentSiteAsync();
            if (site.SinglePageSignUp)
            {
                return RedirectToAction("Index");
            }
            var siteStage = GetSiteStage();
            if (siteStage == SiteStage.BeforeRegistration)
            {
                if (site.RegistrationOpens.HasValue)
                {
                    AlertInfo = $"You can join {site.Name} on {site.RegistrationOpens.Value.ToString("d")}";
                }
                else
                {
                    AlertInfo = $"Registration for {site.Name} has not opened yet";
                }
                return RedirectToAction("Index", "Home");
            }
            else if (siteStage >= SiteStage.ProgramEnded)
            {
                AlertInfo = $"{site.Name} has ended, please join us next time!";
                return RedirectToAction("Index", "Home");
            }

            PageTitle = $"{site.Name} - Join Now!";

            var systemList = await _siteService.GetSystemList();
            Step1ViewModel viewModel = new Step1ViewModel()
            {
                RequirePostalCode = site.RequirePostalCode,
                SystemList = new SelectList(systemList.ToList(), "Id", "Name")
            };

            if (systemList.Count() == 1)
            {
                var systemId = systemList.SingleOrDefault().Id;
                var branchList = await _siteService.GetBranches(systemId);
                if (branchList.Count() > 1)
                {
                    branchList = branchList.Prepend(new Branch() { Id = -1 });
                }
                else
                {
                    viewModel.BranchId = branchList.SingleOrDefault().Id;
                }
                viewModel.BranchList = new SelectList(branchList.ToList(), "Id", "Name");
                viewModel.SystemId = systemId;
            }
            else
            {
                viewModel.BranchList = new SelectList(await _siteService.GetAllBranches(true), 
                    "Id", "Name");
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Step1(Step1ViewModel model)
        {
            var site = await GetCurrentSiteAsync();
            if (site.SinglePageSignUp)
            {
                return RedirectToAction("Index");
            }
            if (site.RequirePostalCode && string.IsNullOrWhiteSpace(model.PostalCode))
            {
                ModelState.AddModelError("PostalCode", "The Zip Code field is required.");
            }

            if (ModelState.IsValid)
            {
                TempData[TempStep1] = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                return RedirectToAction("Step2");
            }

            PageTitle = $"{site.Name} - Join Now!";
            var systemList = await _siteService.GetSystemList();
            model.SystemList = new SelectList(systemList.ToList(), "Id", "Name");
            if (model.SystemId.HasValue)
            {
                var branchList = await _siteService.GetBranches(model.SystemId.Value);
                if (model.BranchId < 1)
                {
                    branchList = branchList.Prepend(new Branch() { Id = -1 });
                }
                model.BranchList = new SelectList(branchList.ToList(), "Id", "Name");
            }
            else if (systemList.Count() > 1)
            {
                model.BranchList = new SelectList(await _siteService.GetAllBranches(true),
                    "Id", "Name");
            }
            model.RequirePostalCode = site.RequirePostalCode;

            return View(model);
        }

        public async Task<IActionResult> Step2()
        {
            var site = await GetCurrentSiteAsync();
            if (site.SinglePageSignUp)
            {
                return RedirectToAction("Index");
            }
            if (!TempData.ContainsKey(TempStep1))
            {
                return RedirectToAction("Step1");
            }

            PageTitle = $"{site.Name} - Join Now!";

            var programList = await _siteService.GetProgramList();
            var programViewObject = _mapper.Map<List<ProgramViewModel>>(programList);
            var districtList = await _schoolService.GetDistrictsAsync();

            if (programList.Count() > 1)
            {
                programList = programList.Prepend(new Program());
            }

            var viewModel = new Step2ViewModel()
            {
                ProgramJson = Newtonsoft.Json.JsonConvert.SerializeObject(programViewObject),
                ProgramList = new SelectList(programList.ToList(), "Id", "Name"),
                SchoolDistrictList = new SelectList(districtList.ToList(), "Id", "Name")
            };

            if (programList.Count() == 1)
            {
                var programId = programList.SingleOrDefault().Id;
                var program = await _siteService.GetProgramByIdAsync(programId);
                viewModel.ProgramId = programList.SingleOrDefault().Id;
                viewModel.ShowAge = program.AskAge;
                viewModel.ShowSchool = program.AskSchool;
                viewModel.ShowCard = program.AskCard;
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Step2(Step2ViewModel model)
        {
            var site = await GetCurrentSiteAsync();
            if (site.SinglePageSignUp)
            {
                return RedirectToAction("Index");
            }
            if (!TempData.ContainsKey(TempStep1))
            {
                return RedirectToAction("Step1");
            }

            bool askAge = false;
            bool askSchool = false;
            bool askCard = false;
            bool askGoal = false;

            if (model.ProgramId.HasValue)
            {
                var program = await _siteService.GetProgramByIdAsync(model.ProgramId.Value);
                askAge = program.AskAge;
                askSchool = program.AskSchool;
                askCard = program.AskCard;
                askGoal = program.AskGoal;

                if (program.AgeRequired && !model.Age.HasValue)
                {
                    ModelState.AddModelError("Age", "The Age field is required.");
                }
                if (program.SchoolRequired)
                {
                    if (!model.NewEnteredSchool && !model.SchoolId.HasValue)
                    {
                        ModelState.AddModelError("SchoolId", "The School field is required.");
                    }
                    else if (model.NewEnteredSchool
                        && string.IsNullOrWhiteSpace(model.EnteredSchoolName))
                    {
                        ModelState.AddModelError("EnteredSchoolName", "The School Name field is required.");
                    }
                }
                if (model.NewEnteredSchool && !model.SchoolDistrictId.HasValue
                    && ((program.AskSchool && !string.IsNullOrWhiteSpace(model.EnteredSchoolName))
                        || program.SchoolRequired))
                {
                    ModelState.AddModelError("SchoolDistrictId", "The School District field is required.");
                }

                if (program.CardRequired && string.IsNullOrWhiteSpace(model.CardNumber))
                {
                    ModelState.AddModelError("CardNumber", "The Library Card field is required");
                }

                if (program.GoalRequired && !model.Goal.HasValue)
                {
                    ModelState.AddModelError("Goal", "The Goal field is required");
                }

                if (program.GoalMaximum.HasValue && model.Goal > program.GoalMaximum.Value)
                {
                    ModelState.AddModelError("Goal", string.Format("The goal maximum is {0}.", program.GoalMaximum.Value));
                }

                if (program.GoalMinimum.HasValue && model.Goal < program.GoalMinimum.Value)
                {
                    ModelState.AddModelError("Goal", string.Format("The goal minimum is {0}.", program.GoalMinimum.Value));
                }
            }

            if (ModelState.IsValid)
            {
                if (!askAge)
                {
                    model.Age = null;
                }
                if (askSchool)
                {
                    if (model.NewEnteredSchool)
                    {
                        model.SchoolId = null;
                    }
                    else
                    {
                        model.EnteredSchoolName = null;
                    }
                }
                else
                {
                    model.SchoolId = null;
                    model.EnteredSchoolName = null;
                }

                if (!askAge)
                {
                    model.CardNumber = null;
                }

                TempData[TempStep2] = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                return RedirectToAction("Step3");
            }

            PageTitle = $"{site.Name} - Join Now!";

            var programList = await _siteService.GetProgramList();
            var programViewObject = _mapper.Map<List<ProgramViewModel>>(programList);
            model.ProgramList = new SelectList(programList.ToList(), "Id", "Name");
            model.ProgramJson = Newtonsoft.Json.JsonConvert.SerializeObject(programViewObject);
            model.ShowAge = askAge;
            model.ShowSchool = askSchool;
            model.ShowCard = askCard;
            model.ShowGoal = askGoal;

            var districtList = await _schoolService.GetDistrictsAsync();
            if (model.SchoolId.HasValue)
            {
                var schoolDetails = await _schoolService.GetSchoolDetailsAsync(model.SchoolId.Value);
                var typeList = await _schoolService.GetTypesAsync(schoolDetails.SchoolDisctrictId);
                model.SchoolDistrictList = new SelectList(districtList.ToList(), "Id", "Name",
                    schoolDetails.SchoolDisctrictId);
                model.SchoolTypeList = new SelectList(typeList.ToList(), "Id", "Name",
                    schoolDetails.SchoolTypeId);
                model.SchoolList = new SelectList(schoolDetails.Schools.ToList(), "Id", "Name");
            }
            else
            {
                model.SchoolDistrictList = new SelectList(districtList.ToList(), "Id", "Name");
                if (model.SchoolDistrictId.HasValue)
                {
                    var typeList = await _schoolService.GetTypesAsync(model.SchoolDistrictId);
                    model.SchoolTypeList = new SelectList(typeList.ToList(), "Id", "Name",
                        model.SchoolTypeId);
                    var schoolList = await _schoolService.GetSchoolsAsync(model.SchoolDistrictId,
                        model.SchoolTypeId);
                    model.SchoolList = new SelectList(schoolList.ToList(), "Id", "Name");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Step3()
        {
            var site = await GetCurrentSiteAsync();

            if (site.SinglePageSignUp)
            {
                return RedirectToAction("Index");
            }
            if (!TempData.ContainsKey(TempStep1))
            {
                return RedirectToAction("Step1");
            }
            else if (!TempData.ContainsKey(TempStep2))
            {
                return RedirectToAction("Step2");
            }

            string step2Json = (string)TempData.Peek(TempStep2);
            var step2 = Newtonsoft.Json.JsonConvert.DeserializeObject<Step2ViewModel>(step2Json);

            bool askEmail = false;
            bool askPhone = false;
            if (step2.ProgramId.HasValue)
            {
                var program = await _siteService.GetProgramByIdAsync(step2.ProgramId.Value);
                askEmail = program.AskEmail;
                askPhone = program.AskPhoneNumber;
            }

            var model = new Step3ViewModel
            {
                ShowEmail = askEmail,
                ShowPhoneNumber = askPhone,
            };

            PageTitle = $"{site.Name} - Join Now!";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Step3(Step3ViewModel model)
        {
            var site = await GetCurrentSiteAsync();
            if (site.SinglePageSignUp)
            {
                return RedirectToAction("Index");
            }
            if (!TempData.ContainsKey(TempStep1))
            {
                return RedirectToAction("Step1");
            }
            else if (!TempData.ContainsKey(TempStep2))
            {
                return RedirectToAction("Step2");
            }

            string step1Json = (string)TempData.Peek(TempStep1);
            string step2Json = (string)TempData.Peek(TempStep2);

            var step1 = Newtonsoft.Json.JsonConvert.DeserializeObject<Step1ViewModel>(step1Json);
            var step2 = Newtonsoft.Json.JsonConvert.DeserializeObject<Step2ViewModel>(step2Json);

            bool askEmail = false;
            bool askPhone = false;
            if (step2.ProgramId.HasValue)
            {
                var program = await _siteService.GetProgramByIdAsync(step2.ProgramId.Value);
                askEmail = program.AskEmail;
                askPhone = program.AskPhoneNumber;

                if (program.EmailRequired && string.IsNullOrWhiteSpace(model.Email))
                {
                    ModelState.AddModelError("Email", "The email field is required.");
                }
                if (program.PhoneNumberRequired && string.IsNullOrWhiteSpace(model.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "The phone number field is required.");
                }
            }

            model.ShowEmail = askEmail;
            model.ShowPhoneNumber = askPhone;

            if (ModelState.IsValid)
            {
                User user = new User();
                _mapper.Map<Step1ViewModel, User>(step1, user);
                _mapper.Map<Step2ViewModel, User>(step2, user);
                _mapper.Map<Step3ViewModel, User>(model, user);
                user.SiteId = site.Id;
                try
                {
                    await _userService.RegisterUserAsync(user, model.Password,
                        step2.SchoolDistrictId);
                    TempData.Remove(TempStep1);
                    TempData.Remove(TempStep2);
                    var loginAttempt = await _authenticationService
                        .AuthenticateUserAsync(user.Username, model.Password);
                    await LoginUserAsync(loginAttempt);
                    var questionnaireId = await _questionnaireService
                            .GetRequiredQuestionnaire(loginAttempt.User.Id, loginAttempt.User.Age);
                    if (questionnaireId.HasValue)
                    {
                        HttpContext.Session.SetInt32(SessionKey.PendingQuestionnaire,
                            questionnaireId.Value);
                    }

                    return RedirectToAction("Index", "Home");
                }
                catch (GraException gex)
                {
                    ShowAlertDanger("Could not create your account: ", gex);
                    if (gex.Message.Contains("password"))
                    {
                        ModelState.AddModelError("Password", "Please correct the issues with your password.");
                    }
                }
            }

            PageTitle = $"{site.Name} - Join Now!";

            return View(model);
        }
    }
}
