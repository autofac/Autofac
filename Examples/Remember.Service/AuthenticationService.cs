using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Remember.Service
{
    public class AuthenticationService:IAuthenticationService
    {


        public bool IsValid(string emailAddress, string password)
        {
            if (emailAddress == "test@test.com" && password == "test")
            {
                return true;
            }
            return false;
        }

        
    }
}
