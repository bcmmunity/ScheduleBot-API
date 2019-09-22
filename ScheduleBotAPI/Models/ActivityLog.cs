using System;

namespace ScheduleBotAPI.Models
{
    public class ActivityLog
    {
        public int ActivityLogId { get; set; }
        public SnUser SnUser { get; set; }
        public string MessageText { get; set; }
        public DateTime MessageDateTime { get; set; }
    }
}
