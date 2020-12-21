using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NebulaWarsMonitor.Models.EF
{
    [Table("LogRecords"), ReadOnlyTable]
    public class LogRecord
    {
        public LogRecord()
        {
            DateTime = DateTime.UtcNow;
        }

        public LogRecord(MessageTypeEnum type, int messageId, int serverId) : this()
        {
            MessageTypeId = (int) type;
            LogMessageId = messageId;
            ServerId = serverId;
        }

        public LogRecord(MessageTypeEnum type, LogMessage message, Server server) : this()
        {
            MessageTypeId = (int)type;
            LogMessage = message;
            Server = server;
        }

        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        [Required, Column("Id_Server"), ForeignKey("Server")]
        public int ServerId { get; set; }

        [ForeignKey("ServerId")]
        public virtual Server Server { get; set; }

        [Required, Column("Id_MessageType"), ForeignKey("MessageType")]
        public int MessageTypeId { get; set; }

        [ForeignKey("MessageTypeId")]
        public virtual MessageType MessageType { get; set; }

        [Required, Column("Id_LogMessage"), ForeignKey("LogMessage")]
        public int LogMessageId { get; set; }

        [ForeignKey("LogMessageId")]
        public virtual LogMessage LogMessage { get; set; }

        public override string ToString() => $"{DateTime:MM.dd.yyyy HH:mm:ss} {MessageType} from {Server} : {LogMessage}";
    }
}
