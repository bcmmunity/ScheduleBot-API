﻿namespace ScheduleBotAPI.Models
{
    public class TeacherLesson
    {
        public int TeacherLessonId { get; set; }


        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }


        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
    }
}
