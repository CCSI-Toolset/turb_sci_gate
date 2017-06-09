using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;
using Turbine.DataEF6;

namespace Turbine.Lite.Web.Resources.Contracts
{
    class ProducerProcessContract : IProcess
    {
        private Guid id;

        public ProducerProcessContract(Guid id)
        {
            // TODO: Complete member initialization
            this.id = id;
        }
        public void AddStdout(string data)
        {
            throw new NotImplementedException();
        }

        public void AddStderr(string data)
        {
            throw new NotImplementedException();
        }

        public void SetStatus(int status)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> Input
        {
            get
            {
                Dictionary<String, Object> dict = null;
                using (var db = new ProducerContext())
                {
                    Turbine.Data.Entities.Process entity =
                        db.Processes.Single<Turbine.Data.Entities.Process>(s => s.Id == id);
                    String data = entity.Input;
                    if (data != null)
                    {
                        dict = JsonConvert.DeserializeObject<Dictionary<string, Object>>(data);
                    }
                    return dict;
                }
            }
            set
            {
                String data = JsonConvert.SerializeObject(value);
                //SetInput(data);
                using (var db = new ProducerContext())
                {
                    Turbine.Data.Entities.Process entity =
                        db.Processes.Single<Turbine.Data.Entities.Process>(s => s.Id == id);
                    entity.Input = data;
                    db.SaveChanges();
                }
            }
        }

        public IDictionary<string, object> Output
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string WorkingDirectory
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void AddConvergenceError(Dictionary<string, List<string>> dictionary)
        {
            throw new NotImplementedException();
        }

        public void AddBlockError(Dictionary<string, List<string>> dictionary)
        {
            throw new NotImplementedException();
        }

        public void AddStreamError(Dictionary<string, List<string>> dictionary)
        {
            throw new NotImplementedException();
        }

        public void AddConvergenceWarning(Dictionary<string, List<string>> dictionary)
        {
            throw new NotImplementedException();
        }

        public void AddBlockWarning(Dictionary<string, List<string>> dictionary)
        {
            throw new NotImplementedException();
        }

        public void AddStreamWarning(Dictionary<string, List<string>> dictionary)
        {
            throw new NotImplementedException();
        }
    }
}
