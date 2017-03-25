using System.Collections.Generic;
using GRA.Domain.Model;

namespace GRA.Controllers.ViewModel.MissionControl.Challenges
{
    public class ChallengesApprovalListViewModel
    {
        public IEnumerable<ChallengeTask> Tasks { get; set; }
    }
}
