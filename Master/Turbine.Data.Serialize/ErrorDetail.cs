using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Turbine.Data.Serialize
{
    [DataContract]
    public class ErrorDetail
    {
        public enum Codes
        {
            DATAFORMAT_ERROR = 101, DATABOUNDS_ERROR = 102,
            JOB_STATE_ERROR = 103, QUERYPARAM_ERROR = 104,
            AUTHORIZATION_ERROR = 105,
            SERVER_ERROR = 106
        }
        Dictionary<Codes, String> Desc = new Dictionary<Codes,String>()
        {
            { Codes.DATAFORMAT_ERROR , "Bad Data Format" },
            { Codes.DATABOUNDS_ERROR , "Data Bounds Error" },
            { Codes.JOB_STATE_ERROR , "Job State Error" },
            { Codes.QUERYPARAM_ERROR , "Query Paramater Error" },
            { Codes.AUTHORIZATION_ERROR , "Authorization Error" },
            { Codes.SERVER_ERROR , "Server Error" }
        };

        public ErrorDetail(Codes code, string detail)
        {
            Code = (int)code;
            Description = Desc[code];
            Detail = detail;
        }

        [DataMember]
        public string Description { get; private set; }

        [DataMember]
        public string Detail { get; private set; }

        [DataMember]
        public int Code { get; private set; }
    }
}
