﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GRA.Domain.Model
{
    public class DrawingCriterion : Abstract.BaseDomainEntity
    {
        public int SiteId { get; set; }
        [MaxLength(255)]
        [Required]
        public string Name { get; set; }
        [DisplayName("Program")]
        public int? ProgramId { get; set; }
        [DisplayName("System")]
        public int? SystemId { get; set; }
        [DisplayName("Branch")]
        public int? BranchId { get; set; }
        [DisplayName("Minimum Points")]
        public int? PointsMinimum { get; set; }
        [DisplayName("Maximum Points")]
        public int? PointsMaximum { get; set; }
        [DisplayName("Selection Start")]
        public DateTime? StartOfPeriod { get; set; }
        [DisplayName("Selection End")]
        public DateTime? EndOfPeriod { get; set; }
        public int? ActivityAmount { get; set; }
        public int? PointTranslationId { get; set; }
        [DisplayName("Include Admin")]
        public bool IncludeAdmin { get; set; }

        public string BranchName { get; set; }
        [DisplayName("Eligible Count")]
        public int EligibleCount { get; set; }
    }
}
