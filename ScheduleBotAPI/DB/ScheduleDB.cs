using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using ScheduleBotAPI.Models;
using ScheduleBotAPI.Models.Post;

namespace ScheduleBotAPI.DB
{
    public class ScheduleDB
    {
      //  private readonly MyContext _db;

        string connectionString;
        public ScheduleDB()
        {
            connectionString = DB.GetConnectionString();
        //_db = new DB().Connect();
        }

        public void DBResize()
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string dbName = connectionString.Split(';')[1].Split('=')[1];
                db.Execute(
                    "ALTER DATABASE "+dbName+" SET RECOVERY SIMPLE;DBCC SHRINKFILE ("+dbName+"_Log, 5);ALTER DATABASE "+dbName+" SET RECOVERY FULL;");
            }
        }

        public void FixingTeacherLesson()
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                List<Lesson> alLessons = db.Query<Lesson>("SELECT * FROM Lessons").ToList();
                foreach (var lesson in alLessons)
                {
                    int lessonId = lesson.LessonId;
                    if (String.IsNullOrEmpty(lesson.TeachersNames))
                        continue;
                    if (lesson.TeachersNames.Contains('|'))
                    {
                        foreach (var teacher in lesson.TeachersNames.Split('|'))
                        {
                            int teacherId =
                                db.QueryFirstOrDefault<int>("SELECT TeacherId FROM Teachers WHERE Name = @teacher",
                                    new {teacher});
                            if (teacherId == 0)
                            {
                              db.Execute( "INSERT INTO Teachers (Name, PhoneNumber) Values (@teacher,0)", new { teacher });
                               teacherId =
                                   db.QueryFirstOrDefault<int>("SELECT TeacherId FROM Teachers WHERE Name = @teacher",
                                       new { teacher });
                            }
                            int isRowExists =
                                db.QueryFirstOrDefault<int>(
                                    "SELECT Count(*) FROM TeacherLesson Where TeacherId = @teacherId and LessonId = @lessonId",
                                    new {teacherId, lessonId});
                            if (isRowExists == 0)
                            {
                                db.Execute(
                                    "INSERT INTO TeacherLesson (TeacherId, LessonId, TeacherLessonId) Values (@teacherId, @lessonId,0)",
                                    new {teacherId, lessonId});
                            }
                        }

                    }
                    else
                    {
                        int teacherId =
                            db.QueryFirstOrDefault<int>("SELECT TeacherId FROM Teachers WHERE Name = @teacher",
                                new { teacher = lesson.TeachersNames });
                        if (teacherId == 0)
                        {
                            db.Execute("INSERT INTO Teachers (Name, PhoneNumber) Values (@teacher,0)", new { teacher = lesson.TeachersNames });
                            teacherId =
                                db.QueryFirstOrDefault<int>("SELECT TeacherId FROM Teachers WHERE Name = @teacher",
                                    new { teacher = lesson.TeachersNames });
                        }
                        int isRowExists =
                            db.QueryFirstOrDefault<int>(
                                "SELECT Count(*) FROM TeacherLesson Where TeacherId = @teacherId and LessonId = @lessonId",
                                new { teacherId, lessonId });
                        if (isRowExists == 0)
                        {
                            db.Execute(
                                "INSERT INTO TeacherLesson (TeacherId, LessonId, TeacherLessonId) Values (@teacherId, @lessonId,0)",
                                new { teacherId, lessonId });
                        }
                    }
                }
            }
           
        }

        public void 
            AddScheduleWeek(string university, string facility, string course, string group, byte type, PostScheduleWeek week)
        {
            if (!IsUniversityExist(university))
                AddUniversity(university);
            if (!IsFacilityExist(university,facility))
                AddFacility(university,facility);
            if (!IsCourseExist(university,facility,course))
                AddCourse(university,facility,course);
            if (!IsGroupExist(university,facility,course,group))
                AddGroup(university,facility,course,group,type);

            using (IDbConnection db = new SqlConnection(connectionString))
            {
                int universityId =
                    db.QueryFirstOrDefault<int>("SELECT UniversityId FROM Universities WHERE Name = @university",
                        new { university });
                int facilityId = db.QueryFirstOrDefault<int>(
                    "SELECT FacilityId FROM Facilities WHERE UniversityId = @universityId and Name = @facility",
                    new { universityId, facility });
                int courseId = db.QueryFirstOrDefault<int>(
                    "SELECT CourseId FROM Courses WHERE FacilityId = @facilityId and Name = @course",
                    new { facilityId, course });
                int groupId = db.QueryFirstOrDefault<int>(
                    "SELECT GroupId FROM Groups WHERE CourseId = @courseId and Name = @group",
                    new { courseId, group });

                int scheduleWeekId = GetScheduleWeekId(groupId, week.Week);
                if (scheduleWeekId != 0)
                {
                    DeleteWeek(scheduleWeekId);
                }

                //AddWeek
                int insertedScheduleWeekId = db.QueryFirstOrDefault<int>("INSERT INTO ScheduleWeeks (Week, GroupId) Values (@weekNum, @groupId); SELECT SCOPE_IDENTITY()", new { weekNum = week.Week, groupId });
                foreach (var day in week.Days)
                {
                    int insertedScheduleDayId = db.QueryFirstOrDefault<int>("INSERT INTO ScheduleDays (ScheduleWeekId, Day, Date) Values (@insertedScheduleWeekId, @dayNum, '0001-01-01 00:00:00.0000000'); SELECT SCOPE_IDENTITY()", new { insertedScheduleWeekId, dayNum = day.Day });
                    foreach (var lesson in day.Lessons)
                    {
                        string names = String.Empty;
                        if (lesson.Teachers.Count != 0)
                        {
                            int i = 0;

                            foreach (var teacher in lesson.Teachers)
                            {
                                if (!String.IsNullOrEmpty(teacher.Name))
                                {
                                    if (i == 0)
                                        names += teacher.Name;
                                    else
                                        names += " | " + teacher.Name ;
                                    i++;
                                }
                            }
                        }
                        int insertedLessonId = db.QueryFirstOrDefault<int>("INSERT INTO Lessons (Number, Name, Type, Time, Room, TeachersNames, ScheduleDayId) Values (@number,@name,@type,@time,@room,@names,@insertedScheduleDayId); SELECT SCOPE_IDENTITY()", new { number = lesson.Number, name = lesson.Name, type = lesson.Type, time = lesson.Time, room = lesson.Room, names, insertedScheduleDayId });

                        if (lesson.Teachers.Count == 0)
                            continue;
                        foreach (var teacher in lesson.Teachers)
                        {
                            if (!String.IsNullOrEmpty(teacher.Name))
                            {
                                int teacherId = db.QueryFirstOrDefault<int>("SELECT TeacherId FROM Teachers WHERE Name = @teacherName", new {teacherName = teacher.Name });
                                if (teacherId == 0)
                                {
                                    teacherId = db.QueryFirstOrDefault<int>("INSERT INTO Teachers (Name, PhoneNumber) Values (@teacherName,0); SELECT SCOPE_IDENTITY()", new { teacherName = teacher.Name });
                                }

                                db.Execute(
                                    "INSERT INTO TeacherLesson (TeacherId, LessonId, TeacherLessonId) Values (@teacherId, @insertedLessonId,0)",
                                    new {teacherId, insertedLessonId});
                            }

                        }
                    }
                }
                
            }
        }


        private void AddUniversity(string name)
        {
            if (!IsUniversityExist(name))
            {
                using (IDbConnection db = new SqlConnection(connectionString))
                {
                    db.Execute("INSERT INTO Universities (Name) Values (@name)", new {name});
                }
            }
        }

        private void AddFacility(string university, string name)
        {
            if (!IsFacilityExist(university, name))
            {
                using (IDbConnection db = new SqlConnection(connectionString))
                {
                    int universityId =
                        db.QueryFirstOrDefault<int>("SELECT UniversityId FROM Universities WHERE Name = @university",
                            new {university});
                    db.Execute("INSERT INTO Facilities (Name, UniversityId) Values (@name, @universityId)",
                        new {name, universityId});
                }
            }
        }

        private void AddCourse(string university, string facility, string name)
        {
            if (!IsCourseExist(university, facility, name))
            {
                using (IDbConnection db = new SqlConnection(connectionString))
                {
                    int universityId =
                    db.QueryFirstOrDefault<int>("SELECT UniversityId FROM Universities WHERE Name = @university",
                        new { university });
                    int facilityId = db.QueryFirstOrDefault<int>("SELECT FacilityId FROM Facilities WHERE UniversityId = @universityId and Name = @facility",
                        new { universityId, facility });
                    db.Execute("INSERT INTO Courses(Name, FacilityId) Values (@name, @facilityId)",
                        new { name, facilityId });

                }
            }
        }

        private void AddGroup(string university, string facility, string course, string name, byte type)
        {
            if (!IsGroupExist(university, facility, course, name))
            {
                using (IDbConnection db = new SqlConnection(connectionString))
                {
                    int universityId =
                        db.QueryFirstOrDefault<int>("SELECT UniversityId FROM Universities WHERE Name = @university",
                            new {university});
                    int facilityId = db.QueryFirstOrDefault<int>(
                        "SELECT FacilityId FROM Facilities WHERE UniversityId = @universityId and Name = @facility",
                        new {universityId, facility});
                    int courseId = db.QueryFirstOrDefault<int>(
                        "SELECT CourseId FROM Courses WHERE FacilityId = @facilityId and Name = @course",
                        new {facilityId, course});
                    db.Execute("INSERT INTO Groups (Name, CourseId, ScheduleType) Values (@name, @courseId, @type)",
                        new { name, courseId, type });
                }
            }
        }

        private void DeleteWeek(int scheduleWeekId)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                List<ScheduleDay> days = db
                    .Query<ScheduleDay>("SELECT * FROM ScheduleDays WHERE ScheduleWeekId = @scheduleWeekId",
                        new {scheduleWeekId}).ToList();
                foreach (var day in days)
                {
                    int scheduleDayId = day.ScheduleDayId;
                    List<Lesson> lessons = db
                        .Query<Lesson>("SELECT * FROM Lessons WHERE ScheduleDayId = @scheduleDayId", 
                            new { scheduleDayId }).ToList();
                    foreach (var lesson in lessons)
                    {
                        int lessonId = lesson.LessonId;
                        List<TeacherLesson> teacherLessons = db
                            .Query<TeacherLesson>("SELECT * FROM TeacherLesson WHERE LessonId = @lessonId",
                                new { lessonId }).ToList();
                        foreach (var teacherLesson in teacherLessons)
                        {
                            int teacherLessonId = teacherLesson.TeacherLessonId;
                            db.Execute("DELETE FROM TeacherLesson where TeacherLessonId = @teacherLessonId",
                                new {teacherLessonId});
                        }
                        db.Execute("DELETE FROM Lessons where LessonId = @lessonId",
                            new { lessonId });
                    }
                    db.Execute("DELETE FROM ScheduleDays where ScheduleDayId = @scheduleDayId",
                        new { scheduleDayId });
                }
                db.Execute("DELETE FROM ScheduleWeeks where ScheduleWeekId = @scheduleWeekId",
                    new { scheduleWeekId });

            } 
        }

        private int GetScheduleWeekId(int groupId, int weekNumber)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                return db.QueryFirstOrDefault<int>(
                    "SELECT ScheduleWeekId FROM ScheduleWeeks WHERE GroupId = @groupId and Week = @weekNumber",
                    new {groupId, weekNumber});
            }
        }

        public bool IsUniversityExist(string university)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                University universitym = db.QueryFirstOrDefault<University>("SELECT * FROM Universities WHERE Name = @university", new { university });
                bool result = universitym != null;

                return result;
            }
        }

        public bool IsFacilityExist(string university, string facility)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                Facility facultym = db.QueryFirstOrDefault<Facility>(
                        "SELECT f.FacilityId, f.Name, f.UniversityId FROM Facilities as f JOIN Universities as u on f.UniversityId = u.UniversityId where u.Name = @university and f.Name = @facility",
                        new { university, facility });
                bool result = facultym != null;

                return result;
            }
        }

        public bool IsCourseExist(string university, string facility, string course)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                Course coursem = db.QueryFirstOrDefault<Course>(
                    "SELECT c.CourseId, c.Name, c.FacilityId FROM Courses as c JOIN Facilities as f on c.FacilityId = f.FacilityId JOIN Universities as u on f.UniversityId = u.UniversityId where u.Name = @university and f.Name = @facility and c.Name = @course",
                    new { university, facility, course });
                bool result = coursem != null;

                return result;
            }
        }

        public bool IsGroupExist(string university, string facility, string course, string group)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                Group groupm = db.QueryFirstOrDefault<Group>(
                    "SELECT g.GroupId, g.Name, g.ScheduleType, g.CourseId FROM Groups as g JOIN Courses as c on c.CourseId = g.CourseId JOIN Facilities as f on c.FacilityId = f.FacilityId JOIN Universities as u on f.UniversityId = u.UniversityId where u.Name = @university and f.Name = @facility and c.Name = @course and g.Name = @group",
                    new { university, facility, course, group });
                bool result = groupm != null;

                return result;
            }
        }
    }
}