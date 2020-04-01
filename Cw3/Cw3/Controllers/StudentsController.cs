using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        String connection = "Data Source=db-mssql;Initial Catalog=s18964;Integrated Security=True";
        private readonly IDbService _dbservice;

        public StudentsController(IDbService dbService)
        {
            _dbservice = dbService;
        }

        [HttpGet]
        public IActionResult GetStudents(string orderBy)
        {
            List<Student> list = new List<Student>();
            using (var client = new SqlConnection(connection))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = client;
                    command.CommandText = "SELECT firstname , lastname , birthdate , name , semester " +
                                          " FROM Enrollment e, Student s, Studies ss" +
                                          " WHERE s.IdEnrollment = e.IdEnrollment AND e.IdStudy = ss.IdStudy;";
                    client.Open();
                    var dr = command.ExecuteReader();
                    while (dr.Read())
                    {
                        var st = new Student();
                        st.FirstName = dr["FirstName"].ToString();
                        st.LastName = dr["LastName"].ToString();
                        st.BirthDate = (DateTime)dr["BirthDate"];
                        st.Studies = dr["Name"].ToString();
                        st.Semester = int.Parse(dr["SEMESTER"].ToString());
                        list.Add(st);
                    }
                }
            }
            //return Ok(_dbservice.GetStudents());
            return Ok(list);
        }

        [HttpGet("{idstudent}")]
        public IActionResult GetStudent(int idstudent)
        {
            string semester;
            using (var client = new SqlConnection(connection))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = client;
                    command.CommandText = "SELECT indexnumber, firstname, lastname, semester " +
                                          " FROM Enrollment e, Student s, Studies ss" +
                                          " WHERE s.IdEnrollment = e.IdEnrollment AND e.IdStudy = ss.IdStudy AND IndexNumber = @idstudent;";
                    command.Parameters.AddWithValue("idstudent", idstudent);
                    client.Open();
                    var dr = command.ExecuteReader();
                    dr.Read();
                    semester = dr["indexnumber"].ToString();
                }
            }
            return Ok(semester);
            //if (id == 1)
            //{
            //    return Ok("Kowalski");
            //} else if (id == 2)
            //{
            //    return Ok("Malewski");
            //}
            //return NotFound("Nie znaleziono studenta");
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            // add to database
            // generating index number
            //student.IndexNumber = $"s{new Random().Next(1, 20000)}";
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