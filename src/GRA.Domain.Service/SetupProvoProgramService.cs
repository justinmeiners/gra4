using GRA.Domain.Repository;
using GRA.Domain.Service.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GRA.Domain.Service
{
    public class SetupProvoProgramService
        : BaseService<SetupProvoProgramService>, IInitialSetupService
    {
        private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IChallengeTaskRepository _challengeTaskRepository;
        private readonly IProgramRepository _programRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ISystemRepository _systemRepository;
        private readonly IPointTranslationRepository _pointTranslationRepository;

        private readonly ITeamRepository _teamRepository;

        public SetupProvoProgramService(ILogger<SetupProvoProgramService> logger,
            IAuthorizationCodeRepository authorizationCodeRepository,
            IBranchRepository branchRepository,
            IChallengeTaskRepository challengeTaskRepository,
            IProgramRepository programRepository,
            IRoleRepository roleRepository,
            ISystemRepository systemRepository,
            IPointTranslationRepository pointTranslationRepository,
            ITeamRepository teamRepository) : base(logger)
        {
            _authorizationCodeRepository = Require.IsNotNull(authorizationCodeRepository,
                nameof(authorizationCodeRepository));
            _branchRepository = Require.IsNotNull(branchRepository, nameof(branchRepository));
            _challengeTaskRepository = Require.IsNotNull(challengeTaskRepository,
                nameof(challengeTaskRepository));
            _programRepository = Require.IsNotNull(programRepository,
                nameof(programRepository));
            _roleRepository = Require.IsNotNull(roleRepository, nameof(roleRepository));
            _systemRepository = Require.IsNotNull(systemRepository, nameof(systemRepository));
            _pointTranslationRepository = Require.IsNotNull(pointTranslationRepository,
                nameof(pointTranslationRepository));
            _teamRepository = Require.IsNotNull(teamRepository, nameof(teamRepository));
        }

        public async Task InsertAsync(int siteId, string initialAuthorizationCode, int userId = -1)
        {
            //_config[ConfigurationKey.InitialAuthorizationCode]
            // this is the data required for a user to register
            var system = new Model.System
            {
                SiteId = siteId,
                Name = "Provo"
            };
            system = await _systemRepository.AddSaveAsync(userId, system);

            var branch = new Model.Branch
            {
                SiteId = siteId,
                SystemId = system.Id,
                Name = "Provo City Library",
            };
            branch = await _branchRepository.AddSaveAsync(userId, branch);

            var playPointTranslation = new Model.PointTranslation
            {
                ActivityAmount = 1,
                ActivityDescription = "minute",
                ActivityDescriptionPlural = "minutes",
                IsSingleEvent = false,
                AskBook = false,
                PointsEarned = 1,
                TranslationName = "Playing (minutes)",
                TranslationDescriptionPastTense = "played",
                TranslationDescriptionPresentTense = "playing"
            };

            var readMinutesPointTranslation = new Model.PointTranslation
            {
                ActivityAmount = 1,
                ActivityDescription = "minute",
                ActivityDescriptionPlural = "minutes",
                IsSingleEvent = false,
                AskBook = false,
                PointsEarned = 1,
                TranslationName = "Reading (minutes)",
                TranslationDescriptionPastTense = "read",
                TranslationDescriptionPresentTense = "reading"
            };

            var booksPointTranslation = new Model.PointTranslation
            {
                ActivityAmount = 1,
                ActivityDescription = "book",
                ActivityDescriptionPlural = "books",
                IsSingleEvent = false,
                AskBook = true,
                PointsEarned = 200,
                TranslationName = "Reading (books)",
                TranslationDescriptionPastTense = "read",
                TranslationDescriptionPresentTense = "reading"
            };

            int programCount = 0;
            var program = new Model.Program
            {
                SiteId = siteId,
                AchieverPointAmount = 1000,
                Name = "Apprentices (ages 3 and below)",
                Position = programCount++,
                AgeRequired = true,
                AskAge = true,
                EditAge = false,
                SchoolRequired = false,
                AskSchool = false,
                AskCard = true,
                CardRequired = true,
                AskEmail = false,
                EditEmail = false,
                EmailRequired = false,
                AskPhoneNumber = false,
                EditPhoneNumber = false,
                PhoneNumberRequired = false,
                AgeMaximum = 3
            };
            program = await _programRepository.AddSaveAsync(userId, program);

            playPointTranslation.ProgramId = program.Id;
            await _pointTranslationRepository.AddSaveAsync(userId, playPointTranslation);

            program = new Model.Program
            {
                SiteId = siteId,
                AchieverPointAmount = 1000,
                Name = "Journeymen (ages 4 to 8)",
                Position = programCount++,
                AgeRequired = true,
                AskAge = true,
                EditAge = true,
                SchoolRequired = false,
                AskSchool = false,
                AskCard = true,
                CardRequired = true,
                AskEmail = false,
                EditEmail = false,
                EmailRequired = false,
                AskPhoneNumber = false,
                EditPhoneNumber = false,
                PhoneNumberRequired = false,
                AgeMaximum = 8,
                AgeMinimum = 4
            };
            program = await _programRepository.AddSaveAsync(userId, program);

            readMinutesPointTranslation.ProgramId = program.Id;
            await _pointTranslationRepository.AddSaveAsync(userId, readMinutesPointTranslation);

            program = new Model.Program
            {
                SiteId = siteId,
                AchieverPointAmount = 1000,
                Name = "Master Craftsmen (ages 9 to 12)",
                Position = programCount++,
                AgeRequired = true,
                AskAge = true,
                EditAge = true,
                SchoolRequired = false,
                AskSchool = false,
                AskCard = true,
                CardRequired = true,
                AskEmail = false,
                EditEmail = false,
                EmailRequired = false,
                AskPhoneNumber = false,
                EditPhoneNumber = false,
                PhoneNumberRequired = false,
                AgeMaximum = 12,
                AgeMinimum = 9
            };
            program = await _programRepository.AddSaveAsync(userId, program);

            booksPointTranslation.ProgramId = program.Id;
            await _pointTranslationRepository.AddSaveAsync(userId, booksPointTranslation);

            program = new Model.Program
            {
                SiteId = siteId,
                AchieverPointAmount = 1000,
                Name = "Teen (ages 12 to 18)",
                Position = programCount,
                AgeRequired = true,
                AskAge = true,
                SchoolRequired = false,
                AskSchool = false,
                AskCard = true,
                CardRequired = true,
                AskEmail = true,
                EditEmail = true,
                EmailRequired = true,
                AskPhoneNumber = true,
                EditPhoneNumber = true,
                PhoneNumberRequired = true,
                AgeMaximum = 18,
                AgeMinimum = 12
            };
            program = await _programRepository.AddSaveAsync(userId, program);

            booksPointTranslation.ProgramId = program.Id;
            await _pointTranslationRepository.AddSaveAsync(userId, booksPointTranslation);

            program = new Model.Program
            {
                SiteId = siteId,
                AchieverPointAmount = 1000,
                Name = "Adults (ages 18 and up)",
                Position = programCount,
                AgeRequired = true,
                AskAge = true,
                SchoolRequired = false,
                AskSchool = false,
                AskCard = true,
                CardRequired = true,
                AskEmail = true,
                EditEmail = true,
                EmailRequired = true,
                AskPhoneNumber = true,
                EditPhoneNumber = true,
                PhoneNumberRequired = true,
                AgeMinimum = 18
            };
            program = await _programRepository.AddSaveAsync(userId, program);

            booksPointTranslation.ProgramId = program.Id;
            await _pointTranslationRepository.AddSaveAsync(userId, booksPointTranslation);

            // required for a user to be an administrator
            var adminRole = await _roleRepository.AddSaveAsync(userId, new Model.Role
            {
                Name = "System Administrator"
            });

            // add code to make first user system administrator
            await _authorizationCodeRepository.AddSaveAsync(userId, new Model.AuthorizationCode
            {
                Code = initialAuthorizationCode.Trim().ToLower(),
                Description = "Initial code to grant system administrator status.",
                IsSingleUse = false,
                RoleId = adminRole.Id,
                SiteId = siteId
            });

            var team = new Model.Team 
            {
                SiteId = siteId,
                Name = "Green"
            };
            team = await _teamRepository.AddSaveAsync(userId, team);

            team = new Model.Team 
            {
                SiteId = siteId,
                Name = "Yellow"
            };
            team = await _teamRepository.AddSaveAsync(userId, team);

            team = new Model.Team 
            {
                SiteId = siteId,
                Name = "Orange"
            };
            team = await _teamRepository.AddSaveAsync(userId, team);

            // system permissions
            foreach (var value in Enum.GetValues(typeof(Model.Permission)))
            {
                await _roleRepository.AddPermissionAsync(userId, value.ToString());
            }
            await _roleRepository.SaveAsync();

            // add permissions to the admin role
            foreach (var value in Enum.GetValues(typeof(Model.Permission)))
            {
                await _roleRepository.AddPermissionToRoleAsync(userId,
                    adminRole.Id,
                    value.ToString());
            }
            await _roleRepository.SaveAsync();

            foreach (var value in Enum.GetValues(typeof(Model.ChallengeTaskType)))
            {
                await _challengeTaskRepository.AddChallengeTaskTypeAsync(userId,
                    value.ToString());
            }
            await _challengeTaskRepository.SaveAsync();
        }
    }
}