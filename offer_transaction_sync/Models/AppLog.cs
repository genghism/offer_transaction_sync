using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offer_transaction_sync.Models
{
    [Table("AppLog")]
    public class AppLog(
        string _AppName,
        string _LogLevel,
        string _Logger,
        string _Message,
        string _Exception,
        string _StackTrace,
        string _MachineName,
        long _RequestId,
        DateTime _CreatedAt)
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(30)]
        public string AppName { get; init; } = _AppName;

        [Required]
        [StringLength(30)]
        public string LogLevel { get; init; } = _LogLevel;

        [Required]
        [StringLength(30)]
        public string Logger { get; init; } = _Logger;

        [Required]
        public string Message { get; init; } = _Message;

        [Required]
        public string Exception { get; init; } = _Exception;

        [Required]
        public string StackTrace { get; init; } = _StackTrace;

        [Required]
        [StringLength(100)]
        public string MachineName { get; init; } = _MachineName;

        [Required]
        public long RequestId { get; init; } = _RequestId;

        [Required]
        public DateTime CreatedAt { get; init; } = _CreatedAt;
    }
}
