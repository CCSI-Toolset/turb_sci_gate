using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Turbine.Data.Serialize;

namespace TurbineSimulationList
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.WriteLine("get all simulations", "TurbineSimuluationList");
            Console.WriteLine(DataMarshal.GetSimulations());
        }
    }
}
