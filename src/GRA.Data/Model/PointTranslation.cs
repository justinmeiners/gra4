﻿using System.ComponentModel.DataAnnotations;

namespace GRA.Data.Model
{
    public class PointTranslation : Abstract.BaseDbEntity
    {
        [Required]
        public int ProgramId { get; set; }
        [Required]
        [MaxLength(255)]
        public string TranslationName { get; set; }
        [Required]
        [MaxLength(255)]
        public string ActivityDescription { get; set; }
        [Required]
        [MaxLength(255)]
        public string ActivityDescriptionPlural { get; set; }
        [Required]
        public int ActivityAmount { get; set; }
        [Required]
        public int PointsEarned { get; set; }
        [Required]
        public bool IsSingleEvent { get; set; }

        [Required]
        public bool AskBook { get; set; }

        [Required]
        public string TranslationDescriptionPresentTense { get; set; }
        [Required]
        public string TranslationDescriptionPastTense { get; set; }
    }
}
