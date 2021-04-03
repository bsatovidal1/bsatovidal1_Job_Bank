using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace bsatovidal1_Job_Bank.Models
{
    public class ApplicantPhoto : UploadedPhoto
    {
        [Display(Name = "Patient")]
        public int ApplicantID { get; set; }
        public Applicant Applicant { get; set; }
    }
}
