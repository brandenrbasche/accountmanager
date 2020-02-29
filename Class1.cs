using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace accountmanager
{
    public class user_Accounts
    {
        //this is just a container for all info related
        //to an account.  We'll simply create public class-level
        //variables representing each piece of information!
        public int user_ID;
        public string University_name;
        public string Fname;
        public string Lname;
        public string Password;
        public string Major;
        public int AdminStatus;
    }
}