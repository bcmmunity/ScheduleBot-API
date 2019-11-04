using System.Collections.Generic;

namespace ScheduleBotAPI.Models.Post
{
    public class PostResponse
    {
        public string Key { get; set; }
        public string University { get; set; }
        public string Facility { get; set; }
        public string Course { get; set; }
        public string Group { get; set; }
        public byte Type { get; set; }
        public ICollection<PostScheduleWeek> Weeks { get; set; }

        public PostResponse()
        {
            Weeks = new List<PostScheduleWeek>();
        }
    }
}
