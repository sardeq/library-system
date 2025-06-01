using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibrarySystem_Main
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? ClientID { get; set; }
        public string UserType { get; set; }
    }
}