using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace bsatovidal1_Job_Bank.Models
{
    public class Applicant : Auditable
    {        
        public Applicant()
        {
            this.Applications = new HashSet<Application>();
            this.ApplicantSkills = new HashSet<ApplicantSkill>();
        }

        public int ID { get; set; }

        [Required(ErrorMessage ="Please enter a first name.")]
        [StringLength(50, ErrorMessage ="Please enter a first name with less than 50 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [StringLength(30, ErrorMessage = "Please enter a middle name with less than 30 characters.")]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Please enter a last name.")]
        [StringLength(50, ErrorMessage = "Please enter a last name with less than 50 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage ="Please enter a Social Insurance Number.")]
        [RegularExpression("^\\d{9}$", ErrorMessage = "The SIN number must be exactly 9 digits.")]
        [StringLength(9)]
        public string SIN { get; set; }

        [Required(ErrorMessage = "Please enter a phone number")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number without spaces.")]
        [DataType(DataType.PhoneNumber)]
        [DisplayFormat(DataFormatString = "{0:(###) ###-####}", ApplyFormatInEditMode = false)]
        [Display(Name = "Phone Number")]
        public Int64 Phone { get; set; }

        [Required(ErrorMessage = "Please enter an e-mail address.")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        [Display(Name ="Email Address")]
        public string Email { get; set; }

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[] RowVersion { get; set; }//Added for concurrency

        [Display(Name ="Applicant")]
        public string FullName
        {
            get
            {
                return FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                        (" " + (char?)MiddleName[0]).ToUpper() + MiddleName.Substring(1) + " ")
                    + LastName;
            }
        }
        [Display(Name = "Applicant")]
        public string FormalName
        {
            get
            {
                return LastName + ", "
                    +FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                        (" " + (char?)MiddleName[0] + ". ").ToUpper());
            }
        }
        [Display(Name = "Retraining Program")]
        public string HasProg
        {
            get
            {
                return (RetrainingProgramID.HasValue ? "Yes" : "No");
            }
        }
        public ICollection<Application> Applications { get; set; }
        [Display(Name ="Applicant Skills")]
        public ICollection<ApplicantSkill> ApplicantSkills { get; set; }
        [Display(Name ="Retraining Program")]
        public int? RetrainingProgramID { get; set; }
        [Display(Name = "Retraining Program")]
        public RetrainingProgram RetrainingPrograms { get; set; }
        public ApplicantPhoto ApplicantPhoto { get; set; }

    }
}
