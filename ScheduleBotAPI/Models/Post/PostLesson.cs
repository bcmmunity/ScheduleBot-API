using System.Collections.Generic;

namespace ScheduleBotAPI.Models.Post
{
	public class PostLesson
	{
        public string Number { get; set; }
		public string Name { get; set; }
        public string Type { get; set; }
		public string Time { get; set; }
		public string Room { get; set; }
        public List<PostTeacher> Teachers { get; set; } = new List<PostTeacher>();
    }
}
