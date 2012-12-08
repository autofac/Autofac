using System;
using System.Reflection;
using System.Web.Mvc;

namespace Autofac.Tests.Integration.Mvc
{
    public class TestActionInvoker : IActionInvoker
    {
        public bool InvokeAction(ControllerContext controllerContext, string actionName)
        {
            return true;
        }
    }

    public class TestController : Controller
    {
        public object Dependency;

        public virtual ActionResult Action1(string value)
        {
            return new EmptyResult();
        }

        public virtual ActionResult Action2(int value)
        {
            return new EmptyResult();
        }

        public static MethodInfo GetAction1MethodInfo<T>() where T : TestController
        {
            return typeof(T).GetMethod("Action1");
        }
    }

    public class TestControllerA : TestController
    {
        public override ActionResult Action1(string value)
        {
            return new EmptyResult();
        }

        public override ActionResult Action2(int value)
        {
            return new EmptyResult();
        }
    }

    public class TestControllerB : TestControllerA
    {
        public override ActionResult Action1(string value)
        {
            return new EmptyResult();
        }

        public override ActionResult Action2(int value)
        {
            return new EmptyResult();
        }
    }

    public class IsAControllerNot : Controller
    {
    }

    public class TestModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            throw new NotImplementedException();
        }
    }

    public class TestModel1
    {
    }

    public class TestModel2
    {
    }

    public class TestActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }

    public class TestActionFilter2 : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }

    public class TestAuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
        }
    }

    public class TestAuthorizationFilter2 : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
        }
    }

    public class TestExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
        }
    }

    public class TestExceptionFilter2 : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
        }
    }

    public class TestResultFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }

    public class TestResultFilter2 : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }

    public class TestCombinationFilter : IActionFilter, IAuthorizationFilter, IExceptionFilter, IResultFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
        }

        public void OnException(ExceptionContext filterContext)
        {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}