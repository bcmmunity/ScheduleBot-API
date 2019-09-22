using System.Linq;
using Microsoft.EntityFrameworkCore;
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

        public void AddUniversity(string name)
        {
            if (!IsUniversityExist(name))
            {
                University un = new University { Name = name };

                _db.Universities.Add(un);
                _db.SaveChanges();
            }
        }

        public void AddFacility(string university, string name)
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

        public void AddCourse(string university, string facility, string name)
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

        public void AddGroup(string university, string facility, string course, string name, byte type)
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



        public void AddScheduleWeek(string university, string facility, string course, string group, ScheduleWeek week)
        {
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
                oldScheduleWeek.Day.Clear();


                foreach (var day in week.Day)
                {
                    oldScheduleWeek.Day.Add(day);
                }
            }

            else
                _db.ScheduleWeeks.Add(week);

            _db.SaveChanges();
        }

        public void AddTeacher(string name, long phoneNumber)
        {
            if (_db.Teachers.FirstOrDefault(t => t.Name == name) == null)
            {
                Teacher teacher = new Teacher
                {
                    Name = name,
                    PhoneNumber = phoneNumber
                };


                _db.Teachers.Add(teacher);
                _db.SaveChanges();
            }
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