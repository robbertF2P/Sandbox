using System;

namespace ContextTests.Dal.Model
{
    public class Student
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public DateTime EnrolledOn { get; set; }
        public bool Graduated { get; set; }
    }
}
