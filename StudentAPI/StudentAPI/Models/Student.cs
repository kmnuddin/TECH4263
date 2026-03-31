using System.ComponentModel.DataAnnotations.Schema;

namespace StudentAPI.Models
{
    public class Student
    {   
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }              // server assigns
        public string Name { get; set; } = "";   // required
        public int Age { get; set; }             // simple field
        public string Major { get; set; } = "";  // optional/simple

        public Student(string name, int age, string major) 
        {
       
            Name = name;
            Age = age;
            Major = major;
        }
    }
}
