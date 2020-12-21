using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NebulaWarsMonitor.Models.EF
{
    [Table("LogMessages"), ReadOnlyTable]
    public class LogMessage
    {
        public LogMessage() { }

        public LogMessage(string message)
        {
            Message = message;
        }

        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Message { get; set; }

        public override string ToString() => Message;
    }
}
