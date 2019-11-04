using System;
using System.Collections.Generic;

namespace ScheduleBotAPI.Models.Post
{
	public class PostScheduleDay
	{
        public DateTime Date { get; set; }
		public int Day { get; set; }
		public ICollection<PostLesson> Lessons { get; set; }

		public PostScheduleDay()
		{
			Lessons = new List<PostLesson>();
		}
	}
}
