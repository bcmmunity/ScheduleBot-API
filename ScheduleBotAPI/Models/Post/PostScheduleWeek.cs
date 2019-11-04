using System.Collections.Generic;

namespace ScheduleBotAPI.Models.Post
{
	public class PostScheduleWeek
	{
        public int Week { get; set; }
        public ICollection<PostScheduleDay> Days { get; set; }

		public PostScheduleWeek()
		{
			Days = new List<PostScheduleDay>();
		}
	}
}