using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace bsatovidal1_Job_Bank.Models
{
    public class Application : Auditable
    {
        public int ID { get; set; }

        [Required(ErrorMessage ="Please enter a Comment")]
        [StringLength(2000, MinimumLength =20,ErrorMessage ="Please enter a Comment eith at least 20 characters.")]
        [DisplayFormat(NullDisplayText = "No Comment given for this posting.")]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set; }

        [Required(ErrorMessage = "Please select a Posting.")]
        [Display(Name = "Posting")]
        public int PostingID { get; set; }
        public virtual Posting Posting { get; set; }
        
        [Required(ErrorMessage = "Please select an Applicant.")]
        [Display(Name = "Applicant")]
        public int ApplicantID { get; set; }
        public Applicant Applicant { get; set; }
    }
}
