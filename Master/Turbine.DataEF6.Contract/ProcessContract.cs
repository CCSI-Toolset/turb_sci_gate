using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;
using Turbine.DataEF6;
using Newtonsoft.Json;


namespace Turbine.DataEF6.Contract
{
    public partial class ProcessContract : IProcess
    {
        private Guid id;

        private ProcessContract() { }


        public void AddStdout(string data)
        {
            using (ProducerContext db = new ProducerContext())
            {
                Turbine.Data.Entities.Process entity = db.Processes.Single(s => s.Id == id);
                if (entity.Stdout == null)
                    entity.Stdout = data;
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(entity.Stdout);
                    sb.Append(data);
                    entity.Stdout = sb.ToString();
                }
                db.SaveChanges();
            }
        }
        /*
        public void AddStderr(string data)
        {
            using (TurbineCompactDatabase container = new TurbineCompactDatabase())
            {
                SinterProcess entity = container.SinterProcesses.Single<SinterProcess>(s => s.Id == id);
                byte[] stdout = entity.Stderr;
                if (stdout == null)
                    entity.Stderr = System.Text.Encoding.ASCII.GetBytes(data);
                else
                {
                    List<byte> list = new List<byte>(entity.Stderr);
                    list.AddRange(System.Text.Encoding.ASCII.GetBytes(data));
                    entity.Stderr = list.ToArray<byte>();
                }
                container.SaveChanges();
            }
        }
        */
        private void SetInput(string data)
        {
            using (ProducerContext db = new ProducerContext())
            {
                var entity = db.Processes.Single(s => s.Id == id);
                entity.Input = data;
                db.SaveChanges();
            }
        }

        void SetOutput(string data)
        {
            using (ProducerContext db = new ProducerContext())
            {
                var entity = db.Processes.Single(s => s.Id == id);
                entity.Output = data;
                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
        }


        public void SetStatus(int status)
        {
            using (ProducerContext db = new ProducerContext())
            {
                var entity = db.Processes.Single(s => s.Id == id);
                entity.Status = status;
                db.SaveChanges();
            }
        }

        public IDictionary<String, Object> Input
        {
            get
            {
                Dictionary<String, Object> dict = null;
                using (ProducerContext db = new ProducerContext())
                {
                    Turbine.Data.Entities.Process entity = db.Processes.Single(p => p.Id == id);
                    if (entity.Input != null)
                        dict = JsonConvert.DeserializeObject<Dictionary<string, Object>>(entity.Input);
                }
                return dict;
            }
            set
            {
                String data = JsonConvert.SerializeObject(value);
                SetInput(data);
            }
        }


        public IDictionary<String, Object> Output
        {
            get
            {
                IDictionary<String, Object> dict = null;
                using (ProducerContext db = new ProducerContext())
                {
                    Turbine.Data.Entities.Process entity = db.Processes.Single(s => s.Id == id);
                    String data = entity.Output;
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
                SetOutput(data);
            }
        }

        
        public string WorkingDirectory
        {
            get
            {
                using (ProducerContext db = new ProducerContext())
                {
                    var entity = db.Processes.Single(s => s.Id == id);
                    return entity.WorkingDir;
                }
            }
            set
            {
                using (ProducerContext db = new ProducerContext())
                {
                    var entity = db.Processes.Single(s => s.Id == id);
                    entity.WorkingDir = value;
                    db.SaveChanges();
                }
            }
        }

        /*
        public void AddConvergenceError(Dictionary<string, List<string>> dictionary)
        {
            using (TurbineCompactDatabase container = new TurbineCompactDatabase())
            {
                SinterProcess entity = container.SinterProcesses.Single<SinterProcess>(s => s.Id == id);
                ProcessError error;
                foreach (KeyValuePair<String, List<String>> entry in dictionary)
                {
                    foreach (String msg in entry.Value)
                    {
                        error = new ProcessError();
                        error.Type = "convergence";
                        error.Name = entry.Key;
                        error.Error = msg;
                        entity.Errors.Add(error);
                    }
                }
                container.SaveChanges();
            }
        }

        public void AddBlockError(Dictionary<string, List<string>> dictionary)
        {
            using (TurbineCompactDatabase container = new TurbineCompactDatabase())
            {
                SinterProcess entity = container.SinterProcesses.Single<SinterProcess>(s => s.Id == id);
                ProcessError error;
                foreach (KeyValuePair<String, List<String>> entry in dictionary)
                {
                    foreach (String msg in entry.Value)
                    {
                        error = new ProcessError();
                        error.Type = "block";
                        error.Name = entry.Key;
                        error.Error = msg;
                        entity.Errors.Add(error);
                    }
                }
                container.SaveChanges();
            }
        }

        public void AddStreamError(Dictionary<string, List<string>> dictionary)
        {
            using (TurbineCompactDatabase container = new TurbineCompactDatabase())
            {
                SinterProcess entity = container.SinterProcesses.Single<SinterProcess>(s => s.Id == id);
                ProcessError error;
                foreach (KeyValuePair<String, List<String>> entry in dictionary)
                {
                    foreach (String msg in entry.Value)
                    {
                        error = new ProcessError();
                        error.Type = "stream";
                        error.Name = entry.Key;
                        error.Error = msg;
                        entity.Errors.Add(error);
                    }
                }
                container.SaveChanges();
            }
        }

        public void AddConvergenceWarning(Dictionary<string, List<string>> dictionary)
        {
            AddConvergenceError(dictionary);
        }

        public void AddBlockWarning(Dictionary<string, List<string>> dictionary)
        {
            AddBlockError(dictionary);
        }

        public void AddStreamWarning(Dictionary<string, List<string>> dictionary)
        {
            AddStreamError(dictionary);
        }
        */


        public void AddStderr(string data)
        {
            throw new NotImplementedException();
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
