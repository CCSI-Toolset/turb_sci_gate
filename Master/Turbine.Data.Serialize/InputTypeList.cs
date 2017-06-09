using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Turbine.Data.Serialize
{
    [CollectionDataContract]
    public class InputTypeList : List<InputType>
    {
        public InputTypeList() { }
        public InputTypeList(IEnumerable<InputType> source) : base(source) { }
    }
}