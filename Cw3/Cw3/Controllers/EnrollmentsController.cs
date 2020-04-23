using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cw3.DTOs.Requests;
using Cw3.DTOs.Responses;
using Cw3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cw3.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        String connection = "Data Source=db-mssql;Initial Catalog=s18964;Integrated Security=True";
        public IConfiguration Configuration { get; set; }

        public EnrollmentsController(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        [HttpPost]
        [Route("enroll")]
        [Authorize(Roles = "employee")]
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

        [HttpPost]
        [Route("promote")]
        [Authorize(Roles = "employee")]
        public IActionResult PromoteStudent()
        {
            return Ok();
        }
        
        [HttpPost]
        [Route("login")]
        public IActionResult Login(LoginRequestDto request)
        {
            //
            var salt = EnrollmentsController.CreateSalt();
            var password = EnrollmentsController.Create("brokuly", salt);
            Console.WriteLine("Salt: " + salt);
            Console.WriteLine("Password: " + password);
            //
            string login;
            string imie;
            using (var client = new SqlConnection(connection))
            using (var command = new SqlCommand())
            {
                client.Open();
                command.Connection = client;
                command.CommandText = "SELECT * FROM student WHERE indexnumber = @indexnumber";
                command.Parameters.AddWithValue("indexnumber", request.Login);
                var dr = command.ExecuteReader();
                if (!dr.Read())
                {
                    return Unauthorized("Zly login lub haslo");
                }
                if (!Validate(request.Haslo, dr["salt"].ToString(), dr["password"].ToString()))
                {
                    return Unauthorized("Zly login lub haslo");
                }
                login = dr["IndexNumber"].ToString();
                imie = dr["FirstName"].ToString();
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, login),
                new Claim(ClaimTypes.Name, imie),
                new Claim(ClaimTypes.Role, "employee")
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
            (
                issuer: "Admin",
                audience: "Employees",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            var refreshtoken = Guid.NewGuid();
            setRefreshTokenInDB(refreshtoken.ToString(), login);
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshtoken
            });
        }

        [HttpPost]
        [Route("refreshtoken/{FToken}")]
        public IActionResult TokenFromRefreshToken(String FToken)
        {
            string login = "";
            string imie = "";
            using(var client = new SqlConnection(connection))
            using (var comm = new SqlCommand())
            {
                client.Open();
                comm.Connection = client;
                comm.CommandText = "SELECT indexnumber, firstname FROM student WHERE refreshtoken=@refreshtoken";
                comm.Parameters.AddWithValue("refreshtoken", FToken);
                var dr = comm.ExecuteReader();
                if (!dr.Read())
                {
                    return Unauthorized("Bledny Token");
                }
                login = dr["indexnumber"].ToString();
                imie = dr["firstname"].ToString();
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, login),
                new Claim(ClaimTypes.Name, imie),
                new Claim(ClaimTypes.Role, "employee")
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
            (
                issuer: "Admin",
                audience: "Employees",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            var refreshtoken = Guid.NewGuid();
            setRefreshTokenInDB(refreshtoken.ToString(), login);
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshtoken
            });
        }
        public void setRefreshTokenInDB(String token, string login)
        {
            using(var client = new SqlConnection(connection))
            using (var comm = new SqlCommand())
            {
                client.Open();
                comm.Connection = client;
                comm.CommandText = "UPDATE STUDENT SET Refreshtoken = @token WHERE indexnumber = @index";
                comm.Parameters.AddWithValue("index", login);
                comm.Parameters.AddWithValue("token", token);
                comm.ExecuteNonQuery();
            }
        }

        public static string Create(string value, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(
                password: value,
                salt: Encoding.UTF8.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 1000,
                numBytesRequested: 256 / 8);
            return Convert.ToBase64String(valueBytes);
        }

        public static bool Validate(string value, string salt, string hash) => Create(value, salt) == hash;

        public static string CreateSalt()
        {
            byte[] randomBytes = new byte[128 / 8];
            using(var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
    }
}