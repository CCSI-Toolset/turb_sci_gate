using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Data.Contract.Behaviors;


namespace Sinter_Aspen73_IntegrationTest
{
    class InMemoryJobACM : InMemoryJob
    {
        protected override void SetupTestParamters()
        {
            applicationName = "ACM";
            simulationName = "Hybrid";
            jobId = 1;
            process = new InMemoryProcess();
            using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809_sinter.json", FileMode.Open))
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
            using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809.acmf", FileMode.Open))
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
