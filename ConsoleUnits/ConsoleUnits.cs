using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UC2Lib;

namespace ConsoleUnits
{
    class ConsoleUnits
    {
        static void Main(string[] args)
        {

            if (args.Length != 3)
            {
                Console.WriteLine("USAGE: ConsoleUnits takes 3 arguments: Source Value, Source Units, Target Units");
                Console.WriteLine("PRESS ANY KEY TO END PROGRAM");
                Console.ReadLine();
                return;
            }

            UC2Lib.IUnitCon uc = new UC2Lib.UnitCon();


            string sourceUnits = args[1];
            string targetUnits = args[2];
            double sourceValue = Convert.ToDouble(args[0]);

            bool unitsOK = false;

            unitsOK = uc.CheckUnits(sourceUnits, targetUnits);
            if(!unitsOK) {
                Console.WriteLine(String.Format("{0} and {1} are incompatable", sourceUnits, targetUnits));
                Console.WriteLine("PRESS ANY KEY TO END PROGRAM");
                Console.ReadLine();
                return;
            }

            double targetValue = uc.ConvertUnits(sourceValue, sourceUnits, targetUnits);

            Console.WriteLine("RESULT: {0}{1}", targetValue, targetUnits);
            Console.WriteLine("PRESS ANY KEY TO END PROGRAM");
            Console.ReadLine();

        }
    }
}
