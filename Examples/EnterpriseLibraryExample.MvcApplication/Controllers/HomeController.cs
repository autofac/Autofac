using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnterpriseLibraryExample.MvcApplication.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Displays the main view, allowing the user to select an exception type to be thrown.
        /// </summary>
        public ActionResult Index()
        {
            var exceptionTypeArray = new string[]
			{
				typeof(NotImplementedException).AssemblyQualifiedName,
				typeof(NotSupportedException).AssemblyQualifiedName,
                typeof(DivideByZeroException).AssemblyQualifiedName
			};
            this.ViewData["ExceptionTypes"] = exceptionTypeArray;
            return View();
        }

        /// <summary>
        /// Throws an exception of a specified type.
        /// </summary>
        /// <param name="exceptionTypeName">
        /// The assembly-qualified type name of the exception type to throw.
        /// </param>
        public ActionResult ThrowControllerException(string exceptionTypeName)
        {
            if (String.IsNullOrEmpty(exceptionTypeName))
            {
                return this.HttpNotFound();
            }

            var exceptionType = Type.GetType(exceptionTypeName);
            if (exceptionType == null)
            {
                return this.HttpNotFound();
            }

            var instance = Activator.CreateInstance(exceptionType) as Exception;
            throw instance;
        }
    }
}
