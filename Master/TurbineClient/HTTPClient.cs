using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;


namespace Turbine.Client
{

    interface IAuthHandler
    {
        void SetUpRequest(WebRequest req);
    }

    public class BasicAuthHandler : IAuthHandler
    {
        private string userName = null;
        private string password = null;

        public BasicAuthHandler(string userName, string password)
        {
            this.userName = userName;
            this.password = password;
        }
        public void SetUpRequest(WebRequest req)
        {
            string token = userName + ":" + password;
            token = Convert.ToBase64String(Encoding.Default.GetBytes(token));
            req.Headers["Authorization"] = "Basic " + token;
        }
    }

    class HTTPClient
    {


        IAuthHandler authHandler = null;

        public IAuthHandler AuthHandler
        {
            get { return authHandler; }
            set { authHandler = value; }
        }

        internal static string doPut(Uri url, string postData)
        {
            Console.WriteLine("PUT {0}", url);
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] byteData = encoding.GetBytes(postData);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.ContentLength = byteData.Length;
            //request.Credentials = new NetworkCredential("xx", "xx");
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(byteData, 0, byteData.Length);
            requestStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseStatus = response.StatusDescription;
            Console.Out.WriteLine("status: {0}", responseStatus);
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string data = reader.ReadToEnd();
            Console.Out.WriteLine(data);
            reader.Close();
            response.Close();
            dataStream.Close();
            return data;
        }
        public string doPOST(Uri url, string postData)
        {
            Console.WriteLine("POST {0}", url);
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] byteData = encoding.GetBytes(postData);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.ContentLength = byteData.Length;
            if (authHandler != null) authHandler.SetUpRequest(request);

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(byteData, 0, byteData.Length);
            requestStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseStatus = response.StatusDescription;
            Console.Out.WriteLine("status: {0}", responseStatus);
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string data = reader.ReadToEnd();
            Console.Out.WriteLine(data);
            reader.Close();
            response.Close();
            dataStream.Close();
            return data;
        }

        public string doGet(Uri url)
        {
            Console.WriteLine("GET {0}", url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json";
            request.Accept = "application/json";
            if (authHandler != null) authHandler.SetUpRequest(request);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseStatus = response.StatusDescription;
            Console.Out.WriteLine("status: {0}", responseStatus);
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string data = reader.ReadToEnd();
            Console.Out.WriteLine(data);
            reader.Close();
            response.Close();
            dataStream.Close();
            return data;
        }
    }
}
