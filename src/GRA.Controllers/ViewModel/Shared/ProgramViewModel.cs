using System;

namespace GRA.Controllers.ViewModel.Shared
{
    public class ProgramViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool AskAge { get; set; }
        public bool EditAge { get; set; }
        public bool AskSchool { get; set; }
        public bool AskCard { get; set; }
        public bool EditCart { get; set; }
        public bool AskEmail { get; set; }
        public bool EditEmail { get; set; }
        public bool AskPhoneNumber { get; set; }
        public bool EditPhoneNumber { get; set; }
        public int? AgeMaximum { get; set; }
        public int? AgeMinimum { get; set; }
    }
}
