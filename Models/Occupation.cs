using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace bsatovidal1_Job_Bank.Models
{
    public class Occupation : Auditable
    {
        public Occupation()
        {
            this.Positions = new HashSet<Position>();
        }
        public int ID { get; set; }
        [Required(ErrorMessage = "Please enter an Occupation Title.")]
        [StringLength(50,ErrorMessage ="Plese enter an Occupation with less than 50 char.")]
        public string Title { get; set; }

        public ICollection<Position> Positions { get; set; }
    }
}
