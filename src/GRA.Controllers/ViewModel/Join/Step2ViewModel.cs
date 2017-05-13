﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GRA.Controllers.ViewModel.Join
{
    public class Step2ViewModel
    {
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

        [DisplayName("Library Card")]
        [RegularExpression(@"^\d+\s*$", ErrorMessage="This does not appear to be a valid library card.")]
        [MaxLength(64)]
        public string Card { get; set; }
        public bool ShowAge { get; set; }
        public bool ShowSchool { get; set; }
        public bool ShowCard { get; set; }
        public bool NewEnteredSchool { get; set; }
        public int? SchoolDistrictId { get; set; }
        public int? SchoolTypeId { get; set; }
        
        public string ProgramJson { get; set; }

        public SelectList ProgramList { get; set; }
        public SelectList SchoolList { get; set; }
        public SelectList SchoolDistrictList { get; set; }
        public SelectList SchoolTypeList { get; set; }
    }
}
