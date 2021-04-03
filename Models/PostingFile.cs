using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace bsatovidal1_Job_Bank.Models
{
    public class PostingFile : UploadedFile
    {
        [Display(Name ="Posting")]
        public int PostingID { get; set; }

        public Posting Posting { get; set; }
        [StringLength(255, ErrorMessage ="Please enter a description with less than 255 characters.")]
        public string Description { get; set; }
    }

    public class ApplicantFile : UploadedFile
    {
        [Display(Name = "Applicant")]
        public int ApplicantID { get; set; }

        public Posting Applicant { get; set; }
        [StringLength(255, ErrorMessage = "Please enter a description with less than 255 characters.")]
        public string Description { get; set; }
    }
}
