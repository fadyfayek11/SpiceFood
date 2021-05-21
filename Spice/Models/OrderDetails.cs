using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Models
{
    public class OrderDetails
    {
        [key]
        public int Id { get; set; }

        [Required]
        public int OrederId { get; set; }

        [ForeignKey("OrederId")]
        public virtual OrderHeader OrderHeader { get; set; }


        [Required]
        public int MenuItemId { get; set; }

        [ForeignKey("MenuItemId")]
        public virtual MenuItem MenuItem { get; set; }

        public int Count { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        
        [Required]
        public double Price { get; set; }
    }
}
