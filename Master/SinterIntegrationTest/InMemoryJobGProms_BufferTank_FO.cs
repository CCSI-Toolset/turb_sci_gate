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
    class InMemoryJobGProms_BufferTank_FO : InMemoryJob
    {
        protected override void SetupTestParamters()
        {
            applicationName = "GProms";
            simulationName = "BufferTank_FO";
            jobId = 1;
            process = new InMemoryProcess();
            using (var fstream = File.Open(@"models\BufferTank_FO.json", FileMode.Open))
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
            using (var fstream = File.Open(@"models\BufferTank_FO.gPJ", FileMode.Open))
            {
                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    bytes = ms.ToArray();
                    var f = new SimpleFile() { name = "simulationDescriptionFile", content = ms.ToArray() };
                    inputFiles.Add(f);
                }
            }
            using (var fstream = File.Open(@"models\BufferTank_FO.gENCRYPT", FileMode.Open))
            {
                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    bytes = ms.ToArray();
                    var f = new SimpleFile() { name = "model", content = ms.ToArray() };
                    inputFiles.Add(f);
                }
            }
        }
    }
}
