using System;
using System.Collections.Generic;
using System.Text;

namespace TestApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreationTime { get; set; }

        public long? CanNullVal { get; set; }
    }
}
