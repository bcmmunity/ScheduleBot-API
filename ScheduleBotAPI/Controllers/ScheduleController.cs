using System;
using Microsoft.AspNetCore.Mvc;
using ScheduleBotAPI.DB;
using ScheduleBotAPI.Models;

namespace ScheduleBotAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        readonly ScheduleDB _scheduleDb = new ScheduleDB();

        [HttpGet]
        public ActionResult<string> Get()
        {
            PostResponse p = new PostResponse
            {
                University = "НИТУ МИСиС",
                Facility = "ИТАСУ",
                Course = "1",
                Group = "БИСТ-17-1 1 подгруппа"
            };
            return Ok(p);
        }

        // POST api/values
        [HttpPost]
        public IActionResult Add([FromBody] PostResponse postResponse)
        {
            if (!string.IsNullOrEmpty(postResponse.University) && !string.IsNullOrEmpty(postResponse.Facility) &&
                !string.IsNullOrEmpty(postResponse.Course) && !string.IsNullOrEmpty(postResponse.Group) &&
                postResponse.Week != null)
            {
                _scheduleDb.AddScheduleWeek(postResponse.University, postResponse.Facility, postResponse.Course,
                    postResponse.Group, postResponse.Week);
                return Ok("week");
            }
            if (!string.IsNullOrEmpty(postResponse.University) && !string.IsNullOrEmpty(postResponse.Facility) &&
                !string.IsNullOrEmpty(postResponse.Course) && !string.IsNullOrEmpty(postResponse.Group))
            {
                _scheduleDb.AddGroup(postResponse.University, postResponse.Facility, postResponse.Course,
                    postResponse.Group, postResponse.Type);
                return Ok("group");
            }
            if (!string.IsNullOrEmpty(postResponse.University) && !string.IsNullOrEmpty(postResponse.Facility) &&
                !string.IsNullOrEmpty(postResponse.Course))
            {
                _scheduleDb.AddCourse(postResponse.University, postResponse.Facility, postResponse.Course);
                return Ok("course");
            }
            if (!string.IsNullOrEmpty(postResponse.University) && !string.IsNullOrEmpty(postResponse.Facility))
            {
                _scheduleDb.AddFacility(postResponse.University, postResponse.Facility);
                return Ok("facility");
            }
            if (!string.IsNullOrEmpty(postResponse.University))
            {
                _scheduleDb.AddUniversity(postResponse.University);
                return Ok("university");
            }


            return Ok("Wrong request");
        }
    
    }
}