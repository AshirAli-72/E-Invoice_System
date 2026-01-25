using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Invoice_system.Models
{
    [Table("sellers")]
    public class Sellers
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Column("name")]
        public string? name { get; set; }

        [Column("contact")]
        public string? contact { get; set; }

        [Column("address")]
        public string? address { get; set; }

        [Column("email")]
        public string? email { get; set; }

        [Column("status")]
        public string? status { get; set; }
    }
}
