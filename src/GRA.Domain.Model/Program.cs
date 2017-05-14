﻿using System.ComponentModel.DataAnnotations;

namespace GRA.Domain.Model
{
    public class Program : Abstract.BaseDomainEntity
    {
        [Required]
        public int SiteId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        // AchieverGoalMultiplier and AchieverPointAmount are mutually exclusive
        // either a program has a fixed point amount, or uses a point multiplier
        public int? AchieverGoalMultiplier { get; set; }
        public int? AchieverTotal { get; set; }
        public int? AchieverBadgeId { get; set; }
        public string AchieverBadgeName { get; set; }
        public int? JoinBadgeId { get; set; }
        public string JoinBadgeName { get; set; }
        [Required]
        public bool AskAge { get; set; }
        [Required]
        public bool AgeRequired { get; set; }
        [Required]
        public bool EditAge { get; set; }
        
        [Required]
        public bool AskSchool { get; set; }
        [Required]
        public bool SchoolRequired { get; set; }

        [Required]
        public bool AskCard { get; set; }
        [Required]
        public bool CardRequired { get; set; }
        [Required]
        public bool EditCard { get; set; }
        [Required]
        public bool AskEmail { get; set; }
        [Required]
        public bool EmailRequired { get; set; }
        [Required]
        public bool EditEmail { get; set; }
        [Required]
        public bool AskPhoneNumber { get; set; }
        [Required]
        public bool PhoneNumberRequired { get; set; }
        [Required]
        public bool EditPhoneNumber { get; set; }

        [Required]
        public bool AskGoal { get; set; }
        [Required]
        public bool GoalRequired { get; set; }
        [Required]
        public bool EditGoal { get; set; }

        public int Position { get; set; }

        public int? AgeMaximum { get; set; }
        public int? AgeMinimum { get; set; }
    }
}
