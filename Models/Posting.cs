using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bsatovidal1_Job_Bank.Models
{
    public class Posting : Auditable, IValidatableObject
    {
        public Posting()
        {
            this.Applications = new HashSet<Application>();
            this.PostingFiles = new HashSet<PostingFile>();
        }
        public int ID { get; set; }

        [Display(Name = "Posting")]
        public string PostingSummary
        {
            get
            {
                return (string.IsNullOrEmpty(Position?.Name) ? "" : Position?.Name + " - ") +
                    "Closing: " + ClosingDate.ToString("yyyy-MM-dd");
            }
        }
        [Display(Name ="Openings")]
        public string OpeningsSummary
        {
            get
            {
                return "(" + NumberOpen.ToString() + (NumberOpen == 1 ? " opening)" : " openings)");
            }
        }

        [Required(ErrorMessage ="Please enter the Number of Job Openings.")]
        [Range(0,20,ErrorMessage ="Please enter a valid Number.")]
        [Display(Name ="Number of Openings")]
        public int NumberOpen { get; set; }

        [Required(ErrorMessage ="Please enter the Closing Date for this Opening.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString ="{0:yyyy-MM-dd}",ApplyFormatInEditMode =true)]
        [Display(Name = "Opening due date")]
        public DateTime ClosingDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Opening starts")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage ="Please enter a Position.")]
        [Display(Name ="Position")]
        public int PositionID { get; set; }

        [Display(Name = "Position")]
        public Position Position { get; set; }
        
        public ICollection<Application> Applications { get; set; }

        [Display(Name ="File Name")]
        public ICollection<PostingFile> PostingFiles { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ClosingDate < DateTime.Today)
            {
                yield return new ValidationResult("The Closing date for the position cannot be before today.", new[] { "ClosingDate" });
            }
        }

        public IEnumerable<ValidationResult> ValidateStart(ValidationContext validationContext)
        {
            if (ClosingDate > StartDate.GetValueOrDefault())
            {
                yield return new ValidationResult("The Start date for the position cannot be after the Closing date.", new[] { "StartDate" });
            }
        }
    }
}
