﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GRA.Domain.Model
{
    public class User : Abstract.BaseDomainEntity
    {
        [Required]
        public int SiteId { get; set; }
        [EmailAddress]
        [MaxLength(254)]
        public string Email { get; set; }
        public bool IsDeleted { get; set; }
        public bool CanBeDeleted { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime LockedOutAt { get; set; }
        public string LockedOutFor { get; set; }
        public bool IsAchiever { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastActivityDate { get; set; }
        [MaxLength(36)]
        public string Username { get; set; }

        [DisplayName("First Name")]
        [Required]
        [MaxLength(255)]
        public string FirstName { get; set; }
        [DisplayName("Last Name")]
        [MaxLength(255)]
        public string LastName { get; set; }
        [DisplayName("Phone Number")]
        [Phone]
        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        [DisplayName("Zip Code")]
        [MaxLength(32)]
        public string PostalCode { get; set; }

        [DisplayName("Library Card")]
        [RegularExpression(@"^(21391|23005)\d{9}\s*$", ErrorMessage="This does not appear to be a valid library card.")]
        [MaxLength(64)]
        public string CardNumber { get; set; }
        public DateTime? LastAccess { get; set; }
        [DisplayName("Branch")]
        [Range(0, int.MaxValue, ErrorMessage = "The Branch field is required.")]
        public int BranchId { get; set; }
        [DisplayName("System")]
        [Range(0, int.MaxValue, ErrorMessage = "The System field is required.")]
        public int SystemId { get; set; }
        public int PointsEarned { get; set; }
        [DisplayName("Program")]
        [Range(0, int.MaxValue, ErrorMessage = "The Program field is required.")]
        public int ProgramId { get; set; }
        public string StaticAvatarFilename { get; set; }
        public int? AvatarId { get; set; }
        public int? HouseholdHeadUserId { get; set; }
        public string BranchName { get; set; }
        public string SystemName { get; set; }
        public string ProgramName { get; set; }
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
        public int? Age { get; set; }
        [DisplayName("School")]
        public int? SchoolId { get; set; }
        public int? EnteredSchoolId { get; set; }
        [DisplayName("School Name")]
        public string EnteredSchoolName { get; set; }
        [MaxLength(16)]
        public string DynamicAvatar { get; set; }

        public int? TeamId { get; set; }
        [DisplayName("Goal")]
        public int? Goal { get; set; }
        public bool HasNewMail { get; set; }
        public bool HasUnclaimedPrize { get; set; }
        public bool HasPendingQuestionnaire { get; set; }
        public string VendorCode { get; set; }
    }
}
