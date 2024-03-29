﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ScheduleBotAPI.DB;

namespace ScheduleBotAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return
                new string[]
                {
                    "id = 1 - Get current DB connection, id = 2 - Fixing TeacherLesson, id = 3 - Change DB to Main, id = 4 - Change DB to Beta, id = 5 - Shrink DB"
                };
        }
        
        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            if (id == 1)
            {
                return DB.DB.GetConnectionString().Split(';').First();
            }
            else if (id == 2)
            {
                try
                {
                    new ScheduleDB().FixingTeacherLesson();
                    return "Result=Success";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return "Result="+e.Message;
                }
            }

            else if (id == 3)
            {
                DB.DB.IsDefault = true;
                return "Result=Changed to Main";
            }
            else if (id == 4)
            {
                DB.DB.IsDefault = false;
                return "Result=Changed to Beta";
            }
            else if (id == 5)
            {
                try
                {
                    new ScheduleDB().DBResize();
                    return "Result=Success";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return "Result=" + e.Message;
                }
            }

            return "Result=Wrong Path";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
