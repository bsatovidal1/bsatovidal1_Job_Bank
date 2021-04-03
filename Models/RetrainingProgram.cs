using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace bsatovidal1_Job_Bank.Models
{
    public class RetrainingProgram
    {
        public RetrainingProgram()
        {
            Applicants = new HashSet<Applicant>();
        }
        public int ID { get; set; }
        [Required(ErrorMessage ="Please enter a Name for the Training Program.")]
        [StringLength(70,ErrorMessage ="Please enter a program with less than 70 characters.")]
        [DisplayFormat(NullDisplayText ="None")]
        [Display(Name="Training Program")]
        public string Name { get; set; }
        public ICollection<Applicant> Applicants { get; set; }
    }
}
