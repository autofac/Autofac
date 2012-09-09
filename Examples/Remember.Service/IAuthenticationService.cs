using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Remember.Service
{
    public interface IAuthenticationService
    {
        bool IsValid(string emailAddress, string password);
    }
}
