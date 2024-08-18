using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoWelcomeTwoApp
{
    internal class Author
    {
        //public int Id { get; set; }
        public string? FirstName { get; set; }
        public string LastName { get; set; } = null!;
        public string? Country { get; set; }
    }
}
