using Cw3.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Cw3.DAL
{
    public class MockDbService : IDbService
    {
        private static IEnumerable<Student> _students;
        String connection = "Data Source=db-mssql;Initial Catalog=s18964;Integrated Security=True";

        static MockDbService()
        {
            _students = new List<Student>
            {
                //new Student{IdStudent=1, FirstName="Jan", LastName="Kowalski"},
                //new Student{IdStudent=2, FirstName="Anna", LastName="Malewska"},
                //new Student{IdStudent=3, FirstName="Andrzej", LastName="Andrzejewicz"}
            };
        }

        public Student CheckIndex(string index)
        {
            var student = new Student();
            using (var client = new SqlConnection(connection))
            using (var command = new SqlCommand())
            {
                command.Connection = client;
                client.Open();
                command.CommandText = "SELECT * FROM student WHERE indexnumber=@indexnumber";
                command.Parameters.AddWithValue("indexnumber", index);
                var dr = command.ExecuteReader();
                if (dr.Read())
                {
                    student.IndexNumber = dr["IndexNumber"].ToString();
                    student.FirstName = dr["FirstName"].ToString();
                    student.LastName = dr["LastName"].ToString();
                    return student;
                }

            }
            return null;
        }

        public IEnumerable<Student> GetStudents()
        {
            return _students;
        }
    }
}
