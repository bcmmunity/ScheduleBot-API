using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using ScheduleBotAPI.Models;

namespace ScheduleBotAPI.DB
{
    public class ScheduleDB
    {
        private readonly MyContext _db;

        public ScheduleDB()
        {
            _db = new DB().Connect();
        }

        public void 
            AddScheduleWeek(string university, string facility, string course, string group, byte type, ScheduleWeek week)
        {
            if (!IsUniversityExist(university))
                AddUniversity(university);
            if (!IsFacilityExist(university,facility))
                AddFacility(university,facility);
            if (!IsCourseExist(university,facility,course))
                AddCourse(university,facility,course);
            if (!IsGroupExist(university,facility,course,group))
                AddGroup(university,facility,course,group,type);
            
            week.Group = _db.Groups.Where(c => c.Course == _db.Courses
                                                   .Where(ll => ll.Facility == _db.Facilities
                                                                    .Where(n => n.University == _db.Universities
                                                                                    .FirstOrDefault(m =>
                                                                                        m.Name == university))
                                                                    .FirstOrDefault(x => x.Name == facility))
                                                   .FirstOrDefault(x => x.Name == course))
                .FirstOrDefault(v => v.Name == group);


            University universitym = _db.Universities.FirstOrDefault(m => m.Name == university);

            Facility facultym = _db.Facilities.Where(l => l.University == universitym)
                .FirstOrDefault(t => t.Name == facility);

            Course coursem = _db.Courses.Where(o => o.Facility == facultym)
                .FirstOrDefault(t => t.Name == course);

            Group groupm = _db.Groups.Where(n => n.Course == coursem)
                .FirstOrDefault(t => t.Name == group);

            ScheduleWeek oldScheduleWeek = _db.ScheduleWeeks
                .Include(v => v.Day)
                .Where(n => n.Group == groupm)
                .FirstOrDefault(m => m.Week == week.Week);

            if (oldScheduleWeek != null)
            {
                _db.ScheduleWeeks.Remove(oldScheduleWeek);
            }

          
            AddWeek(week);

            _db.SaveChanges();
        }


        private void AddUniversity(string name)
        {
            if (!IsUniversityExist(name))
            {
                University un = new University { Name = name };

                _db.Universities.Add(un);
                _db.SaveChanges();
            }
        }

        private void AddFacility(string university, string name)
        {
            if (!IsFacilityExist(university, name))
            {
                Facility facility = new Facility
                {
                    Name = name,
                    University = _db.Universities.FirstOrDefault(n => n.Name == university)
                };

                _db.Facilities.Add(facility);
                _db.SaveChanges();
            }
        }

        private void AddCourse(string university, string facility, string name)
        {
            if (!IsCourseExist(university, facility, name))
            {
                Course co = new Course
                {
                    Name = name,
                    Facility = _db.Facilities
                        .Where(n => n.University == _db.Universities.FirstOrDefault(m => m.Name == university))
                        .FirstOrDefault(x => x.Name == facility)
                };

                _db.Courses.Add(co);
                _db.SaveChanges();
            }
        }

        private void AddGroup(string university, string facility, string course, string name, byte type)
        {
            if (!IsGroupExist(university, facility, course, name))
            {
                Group gr = new Group
                {
                    Name = name,
                    ScheduleType = type,
                    Course = _db.Courses.Where(l => l.Facility == _db.Facilities
                                                        .Where(n => n.University == _db.Universities
                                                                        .FirstOrDefault(m => m.Name == university))
                                                        .FirstOrDefault(x => x.Name == facility))
                        .FirstOrDefault(x => x.Name == course)
                };

                _db.Groups.Add(gr);
                _db.SaveChanges();
            }
        }

        private void AddWeek(ScheduleWeek week)
        {
            List<ScheduleDay> days = new List<ScheduleDay>();
            foreach (var day in week.Day)
            {
                days.Add(AddDay(day));
            }

            _db.ScheduleWeeks.Add(new ScheduleWeek
            {
                Group = week.Group,
                Week = week.Week,
                Day = days
            });
            _db.SaveChanges();
        }

        private ScheduleDay AddDay(ScheduleDay day)
        {
            List<Lesson> lessons = new List<Lesson>();
            foreach (var lesson in day.Lesson)
            {
                string names = String.Empty;
                if (lesson.TeacherLessons != null)
                {
                    

                    foreach (var teacher in lesson.TeacherLessons)
                    {
                        if (teacher.Teacher != null)
                            names += teacher.Teacher.Name + " | ";
                    }
                }

                Lesson a = new Lesson
                {
                    Name = lesson.Name,
                    Number = lesson.Number,
                    Room = lesson.Room,
                    Time = lesson.Time,
                    Type = lesson.Type,
                    TeachersNames = names
                };

                _db.Lessons.Add(a);
                _db.SaveChanges();
                lessons.Add(a);

                if (lesson.TeacherLessons == null)
                    continue;
                int lessonId = a.LessonId;
                foreach (var teacher in lesson.TeacherLessons)
                {
                    if (teacher.Teacher != null)
                    {
                        var teacherId = 0;
                        if (_db.Teachers.FirstOrDefault(t => t.Name == teacher.Teacher.Name) == null)
                        {
                            _db.Teachers.Add(teacher.Teacher);
                            _db.SaveChanges();
                            teacherId = teacher.Teacher.TeacherId;

                        }
                        else
                        {
                            teacherId = _db.Teachers.FirstOrDefault(t => t.Name == teacher.Teacher.Name).TeacherId;
                        }
                        
                        _db.Lessons.Find(lessonId).TeacherLessons.Add(new TeacherLesson
                        {
                            LessonId = lessonId,
                            TeacherId = teacherId
                        });
                        _db.Lessons.Update(lesson);
                        _db.SaveChanges();
                    }

                }
            }

            ScheduleDay dayx = new ScheduleDay
            {
                Day = day.Day,
                Date = day.Date,
                Lesson = lessons
            };
            _db.ScheduleDays.Add(dayx);
            _db.SaveChanges();
            return dayx;
        }

        private bool IsUniversityExist(string university)
        {
            University universitym = _db.Universities.FirstOrDefault(m => m.Name == university);

            bool result = universitym != null;

            return result;
        }

        private bool IsFacilityExist(string university, string facility)
        {
            University universitym = _db.Universities.FirstOrDefault(m => m.Name == university);
            if (universitym == null)
                return false;
            Facility facultym = _db.Facilities.Where(l => l.University == universitym)
                .FirstOrDefault(t => t.Name == facility);

            bool result = facultym != null;

            return result;
        }

        private bool IsCourseExist(string university, string facility, string course)
        {
            University universitym = _db.Universities.FirstOrDefault(m => m.Name == university);
            if (universitym == null)
                return false;
            Facility facultym = _db.Facilities.Where(l => l.University == universitym)
                .FirstOrDefault(t => t.Name == facility);
            if (facultym == null)
                return false;
            Course coursem = _db.Courses.Where(o => o.Facility == facultym).FirstOrDefault(t => t.Name == course);

            bool result = coursem != null;

            return result;
        }

        private bool IsGroupExist(string university, string facility, string course, string group)
        {
            University universitym = _db.Universities.FirstOrDefault(m => m.Name == university);
            if (universitym == null)
                return false;
            Facility facultym = _db.Facilities.Where(l => l.University == universitym)
                .FirstOrDefault(t => t.Name == facility);
            if (facultym == null)
                return false;
            Course coursem = _db.Courses.Where(o => o.Facility == facultym)
                .FirstOrDefault(t => t.Name == course);
            if (course == null)
                return false;
            Group groupm = _db.Groups.Where(n => n.Course == coursem)
                .FirstOrDefault(t => t.Name == group);

            bool result = groupm != null;

            return result;
        }


    }
}