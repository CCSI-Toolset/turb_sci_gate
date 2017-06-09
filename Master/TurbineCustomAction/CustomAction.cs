using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using Turbine.Security;


namespace Turbine.Installation
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult SetSecurityToken(Session session)
        {
            session.Log("Begin CustomAction SetSecurityToken");
            foreach (var item in session.CustomActionData)
            {
                session.Log(item.Key + ": " + item.Value);
            }
            string password = session["PROP.TURBINE_PASSWORD"];
            session.Log("  password:  " + password);
            string salt = Turbine.Security.AuthenticateCredentials.GetRandomString(12);
            session.Log("  salt:  " + salt);
            string token = Turbine.Security.AuthenticateCredentials.CreateFormattedToken("pbkdf2_sha256", 1200, salt, password);
            session.Log("Done SetSecurityToken:  " + token);
            session["PROP.TURBINE_PASSWORD"] = token;
            
            return ActionResult.Success;
        }
    }
}
