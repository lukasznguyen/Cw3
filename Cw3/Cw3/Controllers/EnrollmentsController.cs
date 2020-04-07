using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Cw3.DTOs.Requests;
using Cw3.DTOs.Responses;
using Cw3.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cw3.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        String connection = "Data Source=db-mssql;Initial Catalog=s18964;Integrated Security=True";
        [HttpPost]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            var st = new Student();
            st.FirstName = request.FirstName;
            st.LastName = request.LastName;
            st.BirthDate = request.BirthDate;
            st.Studies = request.Studies;
            st.IndexNumber = request.IndexNumber;

            using (var client = new SqlConnection(connection))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = client;
                    client.Open();
                    var tran = client.BeginTransaction();
                    //CZY STUDIA ISTNIEJA
                    command.CommandText = "SELECT idstudies FROM studies where name=@name";
                    command.Parameters.AddWithValue("name", request.Studies);
                    var dr = command.ExecuteReader();
                    if (!dr.Read())
                    {
                        tran.Rollback();
                        return BadRequest("Nie ma takiego kierunku");
                    }
                    int idstudies = (int)dr["IdStudy"];
                    //ZAPISANIE STUDENTA
                    command.CommandText = "SELECT IndexNumber FROM student s, enrollment e where s.IdEnrollment = e.IdEnrollment AND semester = 1 AND indexnumber = @indexnumber ";
                    command.Parameters.AddWithValue("indexnumber", st.IndexNumber);
                    dr = command.ExecuteReader();
                    if (!dr.Read())
                    {
                        command.CommandText = "SELECT idenrollment FROM enrollment ORDER BY idenrollment DESC";
                        dr = command.ExecuteReader();
                        dr.Read();
                        int index = (int)dr["idenrollment"]+1;

                        command.CommandText = "INSERT INTO student(indexnumber, firstname, lastname, birthdate, idenrollment) VALUES (@indexnumber,@firstname,@lastname,@birthdate, @idenrollment)";
                        command.Parameters.AddWithValue("indexnumber", st.IndexNumber);
                        command.Parameters.AddWithValue("firstnamer", st.FirstName);
                        command.Parameters.AddWithValue("lastname", st.LastName);
                        command.Parameters.AddWithValue("birthdate", st.BirthDate);
                        command.Parameters.AddWithValue("idenrollment", index);
                        dr = command.ExecuteReader();

                        command.CommandText = "SELECT idstudy FROM studies WHERE name = @name";
                        command.Parameters.AddWithValue("name", st.Studies);
                        dr = command.ExecuteReader();
                        dr.Read();
                        int id = (int)dr["idstudy"];

                        command.CommandText = "INSERT INTO enrollment (idenrollment, semester, idstudy, startdate) VALUES (@idenrollment, @semester, @idstudy, @startdate)";
                        command.Parameters.AddWithValue("idenrollment", index);
                        command.Parameters.AddWithValue("semester", 1);
                        command.Parameters.AddWithValue("idstudy", id);
                        command.Parameters.AddWithValue("startdate", DateTime.Now);
                        dr = command.ExecuteReader();
                    }
                    else
                    {
                        var indexnumber = dr["indexnumber"];
                        if (indexnumber.Equals(st.IndexNumber)){
                            tran.Rollback();
                            return BadRequest("Indeks nie jest unikalny");
                        }
                    }
                    tran.Commit();
                }
            }
            var response = new EnrollStudentResponse();
            response.LastName = st.LastName;
            response.Semester = st.Semester;
            response.StartDate = DateTime.Now;
            return Ok(response);
        }
    }
}