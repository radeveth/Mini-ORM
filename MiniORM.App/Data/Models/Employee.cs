using System.Collections.Generic;

namespace SoftUni.App.Data.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{this.Id}. {this.Name}";
        }
    }
}
