using System;
using System.Web.Mvc;
using Remember.Web.Models;
using Remember.Service;

namespace Remember.Web.Binders
{
    [Autofac.Integration.Web.Mvc.ModelBinderType(typeof(LoginForm))]
    public class LoginFormBinder: IModelBinder
    {
        private readonly IAuthenticationService _authService;

        public LoginFormBinder(IAuthenticationService authService)
        {
            _authService = authService;
        }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {

            var model = new LoginForm();

            try
            {

                // do the bind

                model.EmailAddress = bindingContext.ValueProvider.GetValue("Email").AttemptedValue;
                model.Password = bindingContext.ValueProvider.GetValue("Password").AttemptedValue;


                // validate

                if (!_authService.IsValid(model.EmailAddress,model.Password ))
                {
                    bindingContext.ModelState.AddModelError("", "Invalid credentials (so says the injected LoginFormBinder)");
                }
            }
            catch (Exception ex)
            {
                bindingContext.ModelState.AddModelError("", ex);

            }

            return model;


        }

    }
}