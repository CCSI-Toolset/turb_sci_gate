using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;
using Turbine.Data;
using Newtonsoft.Json;

namespace Turbine.Data.Contract
{
    public class SinterProcessContract : IProcess
    {
        private Guid id;

        public static IProcess Get(Guid id) 
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                SinterProcess entity = container.SinterProcesses.Single<SinterProcess>(s => s.Id == id);
                return new SinterProcessContract() { id = id };
            }
        }

        public static IProcess New(int jobid)
        {
            Guid id = Guid.Empty;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                SinterProcess entity = new SinterProcess() { };
                entity.Job = container.Jobs.Single<Job>(s => s.Id == jobid);
                container.SinterProcesses.AddObject(entity);
                id = entity.Id;
                container.SaveChanges();
            }
            return new SinterProcessContract() { id = id };
        }

        private SinterProcessContract() { }


        public void AddStdout(string data)
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                SinterProcess entity = container.SinterProcesses.Single<SinterProcess>(s => s.Id == id);
                byte[] stdout = entity.Stdout;
                if (stdout == null)
                    entity.Stdout = System.Text.Encoding.ASCII.GetBytes(data);
                else
                {
                    List<byte> list = new List<byte>(entity.Stdout);
                    list.AddRange(System.Text.Encoding.ASCII.GetBytes(data));
                    entity.Stdout = list.ToArray<byte>();
                }
                container.SaveChanges();
            }
        }

        public void AddStderr(string data)
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
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

        void SetInput(string data)
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                SinterProcess entity = container.SinterProcesses.Single<SinterProcess>(s => s.Id == id);
                entity.Input = data;
                container.SaveChanges();
            }
        }

        void SetOutput(string data)
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                SinterProcess entity = container.SinterProcesses.Single<SinterProcess>(s => s.Id == id);
                entity.Output = data;
                container.SaveChanges();
            }
        }

        public void SetStatus(int status)
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                SinterProcess entity = container.SinterProcesses.Single<SinterProcess>(s => s.Id == id);
                entity.Status = status;
                container.SaveChanges();
            }
        }
        //Sets the dir in the function to MyDocs, but doesn't actually chdir
        void SetWorkingDirectory(string path)
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                SinterProcess entity = container.SinterProcesses.Single<SinterProcess>(s => s.Id == id);
                entity.WorkingDir = path;
                container.SaveChanges();
            }
        }

        public IDictionary<String, Object> Input
        {
            get
            {
                Dictionary<String, Object> dict = null;
                using (TurbineModelContainer container = new TurbineModelContainer())
                {
                    SinterProcess entity = container.SinterProcesses.Single<SinterProcess>(s => s.Id == id);
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
                SetInput(data);
            }
            //get
            //{
            //    var list = new List<string>();
            //    using (TurbineModelContainer container = new TurbineModelContainer())
            //    {
            //        SinterProcess entity = container.SinterProcesses.Single<SinterProcess>(s => s.Id == id);
            //        String data = System.Text.Encoding.Unicode.GetString(entity.Input);
            //        if (data != null)
            //        {
            //            foreach (String s in data.Split(new string[]{Environment.NewLine}, 
            //                StringSplitOptions.RemoveEmptyEntries)) 
            //            {
            //                list.Add(s);
            //            }
            //        }
            //        return list.ToArray<string>();
            //    }   
            //}
            //set
            //{
            //    String data = String.Join(Environment.NewLine, value);
            //    SetInput(System.Text.Encoding.Unicode.GetBytes(data));
            //}
        }


        public IDictionary<String, Object> Output
        {
            get
            {
                IDictionary<String, Object> dict = null;
                using (TurbineModelContainer container = new TurbineModelContainer())
                {
                    SinterProcess entity = container.SinterProcesses.Single<SinterProcess>(s => s.Id == id);
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
                using (TurbineModelContainer container = new TurbineModelContainer())
                {
                    SinterProcess entity = container.SinterProcesses.Single<SinterProcess>(s => s.Id == id);
                    return entity.WorkingDir;
                }
            }
            set
            {
                SetWorkingDirectory(value);
            }
        }


        public void AddConvergenceError(Dictionary<string, List<string>> dictionary)
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
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
            using (TurbineModelContainer container = new TurbineModelContainer())
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
            using (TurbineModelContainer container = new TurbineModelContainer())
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

    }
}