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
                Group = "БИСТ-17-1 1 подгруппа",
                Type = 2
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
                    postResponse.Group,postResponse.Type, postResponse.Week);
                return Ok("week");
            }


            return Ok("Wrong request");
        }
    
    }
}