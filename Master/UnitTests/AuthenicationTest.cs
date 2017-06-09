using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Turbine.Web.Test
{
    [TestClass]
    public class AuthenicationTest
    {
        [TestMethod]
        public void TestToken1()
        {
            string password = "testing123";
            string token = "pbkdf2_sha256$12000$deWtjbf5JDhB$pt5q2UOZ+JeGmmxicrfSuCZ/Q4th1sPiUT62+MqYohs=";
            bool check = Turbine.Security.AuthenticateCredentials.CheckToken(password, token);
            Assert.IsTrue(check, "Failed to confirm token");
        }
        [TestMethod]
        public void TestToken2()
        {
            string password = "testing123";
            string token = "pbkdf_sha256$12000$deWtjbf5JDhB$pt5q2UOZ+JeGmmxicrfSuCZ/Q4th1sPiUT62+MqYohs=";
            bool check = Turbine.Security.AuthenticateCredentials.CheckToken(password, token);
            Assert.IsFalse(check, "Bad algorithm");
        }
        [TestMethod]
        public void TestToken3()
        {
            string password = "testing124";
            string token = "pbkdf2_sha256$12000$deWtjbf5JDhB$pt5q2UOZ+JeGmmxicrfSuCZ/Q4th1sPiUT62+MqYohs=";
            bool check = Turbine.Security.AuthenticateCredentials.CheckToken(password, token);
            Assert.IsFalse(check, "Bad password");
        }
        [TestMethod]
        public void TestToken4()
        {
            string password = "testing123";
            string token = "pbkdf2_sha256$12000$deWtjbf5JDhBpt5q2UOZ+JeGmmxicrfSuCZ/Q4th1sPiUT62+MqYohs=";
            bool check = Turbine.Security.AuthenticateCredentials.CheckToken(password, token);
            Assert.IsFalse(check, "Bad token format");
        }
        [TestMethod]
        public void TestToken5()
        {
            string password = "testing123";
            string token = "pbkdf2_sha256$12000$deWtjbf5JDhB$pt5q2UOZ+JeGmmxicrfSuCZ/Q4th1sPiUT62+MqYohs=";
            string token2 = Turbine.Security.AuthenticateCredentials.
                CreateFormattedToken("pbkdf2_sha256", 12000, "deWtjbf5JDhB", password);
            Assert.AreEqual<string>(token, token2);
        }
        [TestMethod]
        public void testToken6()
        {
            int saltSize = 12;
            byte[] data = new byte[saltSize];
            var provider = new System.Security.Cryptography.RNGCryptoServiceProvider();
            provider.GetBytes(data);
            string salt = System.Text.UTF8Encoding.UTF8.GetString(data);
            Console.WriteLine("SALT: " + salt);

        }
        [TestMethod]
        public void testToken7()
        {
            string salt = null;
            for (int i = 0; i < 10; i++)
            {
                salt = Turbine.Security.AuthenticateCredentials.GetRandomString(12);
                Console.WriteLine(salt);
            }
        }
    }
}
