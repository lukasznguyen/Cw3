using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cw3.DAL;
using Cw3.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cw3.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbservice;

        public StudentsController(IDbService dbService)
        {
            _dbservice = dbService;
        }

        [HttpGet]
        public IActionResult GetStudents(string orderBy)
        {
            return Ok(_dbservice.GetStudents());
        }

        [HttpGet("{id}")]
        public IActionResult GetStudent(int id)
        {
            if (id == 1)
            {
                return Ok("Kowalski");
            } else if (id == 2)
            {
                return Ok("Malewski");
            }
            return NotFound("Nie znaleziono studenta");
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            // add to database
            // generating index number
            student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateStudent(int id)
        {
            return Ok("Aktualizacja dokonczona");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            return Ok("Usuwanie ukonczone");
        }

    }
}