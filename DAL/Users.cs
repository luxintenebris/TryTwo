using System;
using System.Collections.Generic;

#nullable disable

namespace DAL
{
    public partial class Users
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Password { get; set; }
    }
}
