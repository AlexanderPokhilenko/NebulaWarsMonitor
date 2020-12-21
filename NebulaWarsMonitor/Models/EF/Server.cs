using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NebulaWarsMonitor.Models.EF
{
    [Table("Servers")]
    public class Server
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Address { get; set; }

        public override string ToString() => Address;
    }
}
