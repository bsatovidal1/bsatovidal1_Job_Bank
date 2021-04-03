using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace bsatovidal1_Job_Bank.Models
{
    public class ApplicantSkill
    {
        [Required(ErrorMessage ="Please select a Skill.")]
        [Display(Name ="Skill")]
        public int SkillID { get; set; }
        public Skill Skill { get; set; }
        [Required(ErrorMessage = "Please select an Applicant.")]
        [Display(Name ="Applicant")]
        public int ApplicantID { get; set; }
        public Applicant Applicant { get; set; }
    }
}
