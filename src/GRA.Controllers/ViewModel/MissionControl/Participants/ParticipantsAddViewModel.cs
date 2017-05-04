﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GRA.Controllers.ViewModel.MissionControl.Participants
{
    public class ParticipantsAddViewModel
    {
        [Required]
        [MaxLength(36)]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [DisplayName("First Name")]
        [MaxLength(255)]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        [MaxLength(255)]
        public string LastName { get; set; }
        
        [DisplayName("Library Card")]
        [MaxLength(64)]
        public string Card { get; set; }

        [DisplayName("Zip Code")]
        [MaxLength(32)]
        public string PostalCode { get; set; }

        [Required]
        [DisplayName("System")]
        [Range(0, int.MaxValue, ErrorMessage = "The System field is required.")]
        public int? SystemId { get; set; }

        [Required]
        [DisplayName("Branch")]
        [Range(0, int.MaxValue, ErrorMessage = "The Branch field is required.")]
        public int? BranchId { get; set; }

        [Required]
        [DisplayName("Program")]
        [Range(0, int.MaxValue, ErrorMessage = "The Program field is required.")]
        public int? ProgramId { get; set; }

        public int? Age { get; set; }
        [DisplayName("School")]
        public int? SchoolId { get; set; }
        [DisplayName("School Name")]
        [MaxLength(255)]
        public string EnteredSchoolName { get; set; }

        [DisplayName("Email Address")]
        [EmailAddress]
        [MaxLength(254)]
        public string Email { get; set; }

        [DisplayName("Phone Number")]
        [Phone]
        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        public bool RequirePostalCode { get; set; }
        public bool ShowEmail {get; set; }
        public bool ShowCard {get; set; }
        public bool ShowPhoneNumber {get; set; }
        public bool ShowAge { get; set; }
        public bool ShowSchool { get; set; }
        public bool NewEnteredSchool { get; set; }
        public int? SchoolDistrictId { get; set; }
        public int? SchoolTypeId { get; set; }
        public string ProgramJson { get; set; }

        public SelectList SystemList { get; set; }
        public SelectList BranchList { get; set; }
        public SelectList ProgramList { get; set; }
        public SelectList SchoolList { get; set; }
        public SelectList SchoolDistrictList { get; set; }
        public SelectList SchoolTypeList { get; set; }
    }
}
