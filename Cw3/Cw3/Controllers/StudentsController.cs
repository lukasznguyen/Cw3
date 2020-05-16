using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Cw3.DAL;
using Cw3.DTOs.Requests;
using Cw3.Models2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cw3.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        //String connection = "Data Source=db-mssql;Initial Catalog=s18964;Integrated Security=True";
        private readonly IDbService _dbservice;
        
        public StudentsController(IDbService dbService)
        {
            _dbservice = dbService;
        }

        [HttpGet]
        public IActionResult GetStudents(string orderBy)
        {
            //List<Student> list = new List<Student>();
            //using (var client = new SqlConnection(connection))
            //{
            //    using (var command = new SqlCommand())
            //    {
            //        command.Connection = client;
            //        command.CommandText = "SELECT firstname , lastname , birthdate , name , semester " +
            //                              " FROM Enrollment e, Student s, Studies ss" +
            //                              " WHERE s.IdEnrollment = e.IdEnrollment AND e.IdStudy = ss.IdStudy;";
            //        client.Open();
            //        var dr = command.ExecuteReader();
            //        while (dr.Read())
            //        {
            //            var st = new Student();
            //            st.FirstName = dr["FirstName"].ToString();
            //            st.LastName = dr["LastName"].ToString();
            //            st.BirthDate = (DateTime)dr["BirthDate"];
            //            st.Studies = dr["Name"].ToString();
            //            st.Semester = int.Parse(dr["SEMESTER"].ToString());
            //            list.Add(st);
            //        }
            //    }
            //}
            ////return Ok(_dbservice.GetStudents());

            //SELECT * FROM student;
            var db = new s18964Context();
            var res = db.Student.ToList();
            return Ok(res);
        }

        [HttpGet("{idstudent}")]
        public IActionResult GetStudent(string idstudent)
        {
            //string semester;
            //using (var client = new SqlConnection(connection))
            //{
            //    using (var command = new SqlCommand())
            //    {
            //        command.Connection = client;
            //        command.CommandText = "SELECT indexnumber, firstname, lastname, semester " +
            //                              " FROM Enrollment e, Student s, Studies ss" +
            //                              " WHERE s.IdEnrollment = e.IdEnrollment AND e.IdStudy = ss.IdStudy AND IndexNumber = @idstudent;";
            //        command.Parameters.AddWithValue("idstudent", idstudent);
            //        client.Open();
            //        var dr = command.ExecuteReader();
            //        dr.Read();
            //        semester = dr["indexnumber"].ToString();
            //    }
            //}
            //return Ok(semester);
            //if (id == 1)
            //{
            //    return Ok("Kowalski");
            //} else if (id == 2)
            //{
            //    return Ok("Malewski");
            //}
            //return NotFound("Nie znaleziono studenta");
            var db = new s18964Context();
            var res = db.Student.Where(student => student.IndexNumber == idstudent).Select(student => new
            {
                Numer = student.IndexNumber,
                Imie = student.FirstName,
                Nazwisko = student.LastName
            });
            return Ok(res);
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
        public IActionResult UpdateStudent(string id)
        {
            var db = new s18964Context();
            var d1 = new Student
            {
                IndexNumber = id,
                LastName = "ZmienioneNazwisko"
            };
            db.Attach(d1);
            db.Entry(d1).Property("LastName").IsModified = true;
            db.SaveChanges();

            var res = db.Student.Where(student => student.IndexNumber == id).Select(student => new
            {
                Numer = student.IndexNumber,
                Nazwisko = student.LastName
            });
            return Ok(res);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(string id)
        {
            var db = new s18964Context();

            var d = new Student
            {
                IndexNumber = id
            };
            db.Attach(d);
            db.Remove(d);
            db.SaveChanges();
            return Ok("Usuwanie ukonczone");
        }

       

    }
}