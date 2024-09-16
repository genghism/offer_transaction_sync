using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offer_transaction_sync.Models
{
    public class Job (
        byte _Id,
        string _Method,
        string _Definition,
        byte _Status,
        int _Period,
        DateTime _Last_dt,
        DateTime _Next_dt,
        string _Log_path)
    {
        public byte Id { get; init; } = _Id;
        public string Method { get; init; } = _Method;
        public string Definition { get; init; } = _Definition;
        public byte Status { get; init; } = _Status;
        public int Period { get; init; } = _Period;
        public DateTime Last_dt { get; init; } = _Last_dt;
        public DateTime Next_dt { get; init; } = _Next_dt;
        public string Log_path { get; init; } = _Log_path;
    }
}
