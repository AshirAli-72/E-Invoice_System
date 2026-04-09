using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Invoice_system.Models
{
    [Table("app_settings")]
    public class AppSetting
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Column("key")]
        public string? key { get; set; }

        [Column("value")]
        public string? value { get; set; }
    }
}
