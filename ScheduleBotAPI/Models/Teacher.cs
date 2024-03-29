﻿using System.Collections.Generic;

namespace ScheduleBotAPI.Models
{
    public class Teacher
    {
        public int TeacherId { get; set; }
        public string Name { get; set; }
        public long PhoneNumber { get; set; }
        public List<TeacherLesson> TeacherLessons { get; set; } = new List<TeacherLesson>();
    }
}
