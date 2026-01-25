using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Invoice_system.Models
{
    [Table("users")]
    public class users
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Column("email")]
        public string? email { get; set; }

        [Column("password")]
        public string? password { get; set; }

        [Column("status")]
        public string? status { get; set; }
    }
}
