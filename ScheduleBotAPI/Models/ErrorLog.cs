using System;

namespace ScheduleBotAPI.Models
{
    public class ErrorLog
    {
        public int ErrorLogId { get; set; }
        public SnUser SnUser { get; set; }
        public string UpdateType { get; set; }
        public string MessageText { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime ErrorDateTime { get; set; }
    }
}
