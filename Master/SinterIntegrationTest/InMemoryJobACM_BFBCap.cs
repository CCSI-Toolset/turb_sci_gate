using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Data.Contract.Behaviors;


namespace SinterIntegrationTest
{
    class InMemoryJobACM_BFBCap : InMemoryJob
    {
        protected override void SetupTestParamters()
        {
            applicationName = "ACM";
            simulationName = "Hybrid";
            jobId = 1;
            process = new InMemoryProcess();
            using (var fstream = File.Open(@"models\BFB_OUU_COE_opt.json", FileMode.Open))
            {
                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    bytes = ms.ToArray();
                    var f = new SimpleFile() { name = "configuration", content = ms.ToArray() };
                    inputFiles.Add(f);
                }
            }
            using (var fstream = File.Open(@"models\BFB_OUU_COE.acmf", FileMode.Open))
            {
                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    bytes = ms.ToArray();
                    var f = new SimpleFile() { name = "aspenfile", content = ms.ToArray() };
                    inputFiles.Add(f);
                }
            }
        }
    }
}
