using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace bsatovidal1_Job_Bank.Models
{
    public class Position : Auditable
    {
        public Position()
        {
            this.Postings = new HashSet<Posting>();
            this.PositionSkills = new HashSet<PositionSkill>();
        }
        public int ID { get; set; }

        [Required(ErrorMessage="Please enter a Name for the Position.")]
        [StringLength(255, ErrorMessage ="Please enter a Valid Name for the Position.")]
        public string Name { get; set; }

        [Required(ErrorMessage ="Please enter a Description for the Position.")]
        [StringLength(511, MinimumLength =50,ErrorMessage ="Please enter a Description within 50 to 511 char.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please enter a Salary for the Position.")]
        [DataType(DataType.Currency)]
        [Column(TypeName ="decimal(9,2)")]
        [Range(0,9999999.99,ErrorMessage ="Please enter a valid Salary.")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Please select an Occupation.")]
        [Display(Name = "Occupation")]
        public int OccupationID { get; set; }

        [Display(Name = "Occupation")]
        public Occupation Occupation { get; set; }

        public ICollection<Posting> Postings { get; set; }
        [Display(Name ="Skills")]
        public ICollection<PositionSkill> PositionSkills { get; set; }
    }
}
