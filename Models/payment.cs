using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Invoice_system.Models
{
    [Table("payment")]
    public class Payment
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Column("Payment_method")]
        public string? Payment_method { get; set; }

        [Column("status")]
        public string? status { get; set; }
    }
}
