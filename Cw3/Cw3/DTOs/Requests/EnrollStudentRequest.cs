using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cw3.DTOs.Requests
{
    public class EnrollStudentRequest
    {
        [Required(ErrorMessage ="Musisz podac index")]
        public string IndexNumber { get; set; }
        [Required(ErrorMessage = "Musisz podac imie")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Musisz podac nazwisko")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Musisz podac dateurodzenia")]
        public DateTime BirthDate { get; set; }
        [Required(ErrorMessage = "Musisz podac kierunek")]
        public string Studies { get; set; }
    }
}
