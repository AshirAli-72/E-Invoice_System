using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Invoice_system.Models
{
    [Table("employee")]
    public class Employee
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Column("date")]
        public DateTime date { get; set; }

        [Column("full_name")]
        public string? full_name { get; set; }

        [Column("emp_code")]
        public string? emp_code { get; set; }

        [Column("cnic")]
        public string? cnic { get; set; }

        [Column("email")]
        public string? email { get; set; }

        [Column("mobile_no")]
        public string? mobile_no { get; set; }

        [Column("address")]
        public string? address { get; set; }

        [Column("image_path")]
        public string? image_path { get; set; }

        [Column("salary")]
        public decimal salary { get; set; }
    }
}
