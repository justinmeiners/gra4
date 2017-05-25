using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GRA.Controllers.ViewModel.MissionControl.Mail
{
    public class MailReplyViewModel
    {
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Body { get; set; }

        public List<GRA.Domain.Model.Mail> Thread { get; set; }
        public int? InReplyToId { get; set; }
        public string InReplyToSubject { get; set; }

        public int? ParticipantId { get; set; }
        public string ParticipantName { get; set; }
        public string ParticipantLink { get; set; }
    }
}
