using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ScheduleBotAPI.DB;
using ScheduleBotAPI.Models;
using ScheduleBotAPI.Models.Post;

namespace ScheduleBotAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        readonly ScheduleDB _scheduleDb = new ScheduleDB();
        private readonly IConfiguration _configuration;

        public ScheduleController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            PostResponse p = new PostResponse
            {
                University = "НИТУ МИСиС",
                Facility = "ИТАСУ",
                Course = "1",
                Group = "БИСТ-17-1 1 подгруппа",
                Type = 2
            };
            return Ok(p);
        }

        // POST api/values
        [HttpPost]
        public IActionResult Add([FromBody] PostResponse postResponse)
        {
            string result = IsPostCorrect(postResponse);
            if (result == "Ok")
            {
                foreach (var week in postResponse.Weeks)
                {
                    _scheduleDb.AddScheduleWeek(postResponse.University, postResponse.Facility, postResponse.Course,
                        postResponse.Group, postResponse.Type, week);
                }
                return Ok("weeks");
            }


            return StatusCode(400, result);
        }


        private string IsPostCorrect(PostResponse postResponse)
        {
            //0 step - key
            if (postResponse.Key != _configuration["Root:Key"])
                return "Wrong request: Api key is invalid";
            //1 step - not empty group, course, uni, facility, type and week
            if (string.IsNullOrEmpty(postResponse.University) || string.IsNullOrEmpty(postResponse.Facility) ||
                string.IsNullOrEmpty(postResponse.Course) || string.IsNullOrEmpty(postResponse.Group) ||
                postResponse.Weeks == null)
                return "Wrong request: PostRequest contains null values";
            else if (postResponse.Type > 11)
                return "Wrong request: ScheduleType is bigger than 11";

            else if (postResponse.University.Length > 40 || postResponse.Facility.Length > 40 ||
                     postResponse.Course.Length > 40 || postResponse.Group.Length > 40)
                return "Wrong request: String values should be not more than 40 letters length";
            //2 step - week correct
            foreach (var week in postResponse.Weeks)
            {
                if (week.Days == null)
                    return "Wrong request: Week contains null values";
                else if ((week.Week > postResponse.Type || postResponse.Weeks.Count > postResponse.Type) && postResponse.Type != 0)
                    return "Wrong request: Week number is bigger than group's ScheduleType";

                //3 step - day correct
                if (week.Days.Count != 6)
                    return "Wrong request: It should be 6 days in a week!";
                else
                {
                    foreach (var day in week.Days)
                    {
                        if (day.Day < 0 || day.Day > 6)
                            return "Wrong request: Day number should be from 0 to 6";
                        foreach (var lesson in day.Lessons)
                        {
                            if (Int32.TryParse(lesson.Number, out var number) == false)
                                return "Wrong request: Lesson number should not be NaN";
                            else
                            {
                                if (number <= 0)
                                    return "Wrong request: Lesson number should be >0";
                            }

                            if (String.IsNullOrEmpty(lesson.Name))
                                return "Wrong request: Lesson name should not be empty";
                            if (String.IsNullOrEmpty(lesson.Time))
                                return "Wrong request: Lesson time should not be empty";

                            if (!Regex.IsMatch(lesson.Time,
                                @"^(([01]\d)|(2[0-3])):[0-5]\d:00 - (([01]\d)|(2[0-3])):[0-5]\d:00$"))
                                return
                                    "Wrong request: Lesson time has an incorrect format (Correct example: \"12:40:00 - 14:15:00\"";


                        }

                    }
                }

            }
            return "Ok";
        }
    }
}