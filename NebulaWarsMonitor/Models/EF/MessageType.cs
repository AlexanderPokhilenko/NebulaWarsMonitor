using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace NebulaWarsMonitor.Models.EF
{
    [Table("MessageTypes"), ReadOnlyTable]
    public class MessageType
    {
        private readonly ILazyLoader _lazyLoader;

        public MessageType() { }

        public MessageType(MessageTypeEnum type)
        {
            Id = (int) type;
            Name = type.ToString();
        }

        public MessageType(ILazyLoader lazyLoader)
        {
            _lazyLoader = lazyLoader;
        }

        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        private ICollection<LogRecord> _records;

        public virtual ICollection<LogRecord> Records
        {
            get => _lazyLoader.Load(this, ref _records);
            set => _records = value;
        }

        public override string ToString() => Name;
    }
}
