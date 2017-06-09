using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System;
using System.Diagnostics;

namespace Turbine.Behaviors
{
    public class CustomUserNameValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            Debug.WriteLine("Custom Validator");

            if (userName == null || password == null)
            {
                throw new ArgumentNullException();
            }
            if (!(userName == "test1" && password == "test1"))
            {
                throw new SecurityTokenException("");
            }
        }
    }
}