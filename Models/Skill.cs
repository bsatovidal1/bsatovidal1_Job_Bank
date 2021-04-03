using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace bsatovidal1_Job_Bank.Models
{
    public class Skill
    {
        public Skill()
        {
            ApplicantSkills = new HashSet<ApplicantSkill>();
            PositionSkills = new HashSet<PositionSkill>();
        }
        public int ID { get; set; }
        [Required(ErrorMessage = "Please enter a Skill.")]
        [StringLength(50, ErrorMessage = "Please enter a skill with less than 50 characters.")]
        [Display(Name = "Skill")]
        public string Name { get; set; }
        public ICollection<ApplicantSkill> ApplicantSkills { get; set; } 
        public ICollection<PositionSkill> PositionSkills { get; set; }
    }
}
