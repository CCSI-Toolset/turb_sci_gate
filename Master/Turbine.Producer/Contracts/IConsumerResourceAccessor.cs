using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Producer.Contracts
{
    public interface IConsumerResourceAccessor
    {
        //Dictionary<string, Object> GetConsumerConfig();

        //void SetConsumerConfigFloor(int num);

        void GetConsumerLog(string instanceID, StringBuilder builder);

        //Dictionary<string, Object> SetConsumerConfigurationOptions(Dictionary<string, object> inputDict);
    }
}
