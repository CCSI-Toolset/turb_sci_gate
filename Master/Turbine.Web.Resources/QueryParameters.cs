using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Web;
using Turbine.Data.Serialize;
using System.Diagnostics;

namespace Turbine.Web.Resources
{
    public class QueryParameters
    {
        internal static bool GetVerbose(bool defaultValue)
        {
            bool val = defaultValue;
            string param = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["verbose"];
            if (string.IsNullOrEmpty(param))
                return val;

            if (!Boolean.TryParse(param, out val))
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.QUERYPARAM_ERROR,
                    "verbose failed to parse as boolean"
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.BadRequest);
            }

            return val;
        }
        public static int GetPage(int defaultValue)
        {
            int val = defaultValue;
            string param = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["page"];
            if (string.IsNullOrEmpty(param))
                return val;

            if (!Int32.TryParse(param, out val))
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.QUERYPARAM_ERROR,
                    "page failed to parse as int"
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.BadRequest);
            }

            return val;
        }
        public static int GetPageSize(int val)
        {
            string param = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["rpp"];
            if (string.IsNullOrEmpty(param))
                return val;

            if (!Int32.TryParse(param, out val))
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.QUERYPARAM_ERROR,
                    "page size failed to parse as int"
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.BadRequest);
            }

            if (val == -1) val = 2000;
            if (val > 2000)
            {
                Debug.WriteLine("Exceed page size", "QueryParameters");
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATABOUNDS_ERROR,
                    String.Format("Exceeded page size maximum: {0} > 2000", val)
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }
            return val;
        }

        internal static bool GetMetaData(bool defaultValue)
        {
            bool val = defaultValue;
            string param = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["meta"];
            if (string.IsNullOrEmpty(param))
                return val;

            if (!Boolean.TryParse(param, out val))
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.QUERYPARAM_ERROR,
                    "meta failed to parse as boolean"
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.BadRequest);
            }

            return val;
        }
    }

}