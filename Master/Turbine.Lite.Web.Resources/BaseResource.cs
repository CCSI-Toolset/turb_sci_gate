using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;

namespace Turbine.Lite.Web.Resources
{
    /// <summary>
    /// Wraps Accesses to Global WebOperationContext in order to Allow for Mock
    /// Without changing all accessibility in Assemblies
    /// </summary>
    abstract public class BaseResource
    {
        virtual protected string OutgoingWebResponseContext_ContentType
        {
            get 
            {
                return WebOperationContext.Current.OutgoingResponse.ContentType; 
            }
            set
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = value;
            }
        }
        virtual protected System.Net.HttpStatusCode OutgoingWebResponseContext_StatusCode
        {
            get
            {
                return WebOperationContext.Current.OutgoingResponse.StatusCode;
            }
            set
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = value;
            }
        }
        virtual protected int QueryParameters_GetPage(int p)
        {
            return QueryParameters.GetPage(p);
        }

        virtual protected int QueryParameters_GetPageSize(int p)
        {
            return QueryParameters.GetPageSize(p);
        }

        virtual protected bool QueryParameters_GetVerbose(bool p)
        {
            return QueryParameters.GetVerbose(p);
        }

        virtual protected bool QueryParameters_GetMetaData(bool p)
        {
            return QueryParameters.GetMetaData(p);
        }

        virtual protected string QueryParameters_GetData(string defaultvalue, string dataname)
        {
            return QueryParameters.GetData(defaultvalue, dataname);
        }
    }
}
