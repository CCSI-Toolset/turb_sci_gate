using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Turbine.Security
{
    public class AuthenticateCredentials
    {
        static private Dictionary<String,DateTime> userCache = new Dictionary<String,DateTime>();
        static private int cacheRefreshSec = 60;

        static public string CreateToken_pbkdf2_sha256(int iterations, string salt, string password)
        {

            using (var hmac = new System.Security.Cryptography.HMACSHA256())
            {
                var df = new Pbkdf2(hmac, password, salt, iterations);
                var d = System.Convert.ToBase64String(df.GetBytes(32));
                return d;
            }
        }

        /// <summary>
        /// Creates a string with seperator '$', of four fields:
        ///     algorithm$iterations$salt$hash
        /// This is stored in the database.
        /// </summary>
        /// <param name="iterations"></param>
        /// <param name="salt"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        static public string CreateFormattedToken(string algorithm, int iterations, string salt, string password)
        {
            if (!"pbkdf2_sha256".Equals(algorithm))
            {
                throw new Exception("Unsupported authentication Algorithm: " + algorithm);
            }
            string uhash = CreateToken_pbkdf2_sha256(iterations, salt, password);
            return String.Format("{0}${1}${2}${3}", algorithm, iterations, salt, uhash);
        }

        static public bool CheckToken(string password, string token)
        {
            //bool authorized = false;
            //var salt = "KETAXaZ4MOzl";
            //var iterations = 12000;
            //var X = "/ivlO11uxHut2s1MOwiewhWAg4bjmO2YXgwE8G6a0tM=";
            string algorithm = null;
            int iterations = 0;
            string salt = null;
            string uhash = null;

            string[] toks = token.Split('$');
            if (toks == null || toks.Length != 4)
            {
                Debug.WriteLine("Bad Token Split", "AuthenticateCredentials");
                return false;
            }

            algorithm = toks[0];
            if (!algorithm.Equals("pbkdf2_sha256"))
            {
                Debug.WriteLine("Unsupported Algorithm: " + algorithm, "AuthenticateCredentials");
                return false;
            }
            if (!int.TryParse(toks[1], out iterations))
            {
                Debug.WriteLine("Iterations must be an integer", "AuthenticateCredentials");
                return false;
            }
            salt = toks[2];
            uhash = toks[3];
            var d = CreateToken_pbkdf2_sha256(iterations, salt, password);
            Debug.WriteLine("Generated Token:  " + d, "AuthenticateCredentials");
            return (d == uhash);
        }

        static private System.Random random = new System.Random((int)System.DateTime.Now.Ticks);
        static public string GetRandomString(int length)
        {
            string[] array = new string[54]
	        {
		        "0","2","3","4","5","6","8","9",
		        "a","b","c","d","e","f","g","h","j","k","m","n","p","q","r","s","t","u","v","w","x","y","z",
		        "A","B","C","D","E","F","G","H","J","K","L","M","N","P","R","S","T","U","V","W","X","Y","Z"
	        };
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < length; i++) sb.Append(array[GetRandomInteger(53)]);
            return sb.ToString();
        }

        static private int GetRandomInteger(int maxValue)
        {
            return random.Next(1, maxValue);
        }
    }
}
