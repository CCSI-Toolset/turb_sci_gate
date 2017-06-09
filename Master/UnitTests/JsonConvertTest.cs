using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Turbine.Web.Test
{
    [TestClass]
    public class JsonConvertTest
    {
        [TestMethod]
        public void TestJsonToDictionary2()
        {
            var json = @"{""Simulation"":""testSim1"", ""Inputs"":{""var1"":""100""}}";
            Dictionary<string, Object> d = JsonConvert.DeserializeObject<Dictionary<string, Object>>(json);
            Console.WriteLine("KEYS: " + d.Keys.Count());
            Console.WriteLine("INPUTS: " + d["Inputs"]);
            JObject obj = (JObject)d["Inputs"];
            Console.WriteLine("VALUES: " + obj);
            Dictionary<String, Object> dd = new Dictionary<string, object>();
            foreach (var v in obj)
            {
                Console.WriteLine("VALUE: " + v);
                dd[v.Key] = v.Value;
            }
        }    

        [TestMethod]
        public void TestJsonToDictionary1()
        {
            //""abs_ht_wash"":5.0, 
            var json = @"{""P_abs_top"":16.0,""abs_ht_mid"":10.7,""abs_ic_dT"":-11.3,""P_sol_pump"":30.0,""lean_load"":0.178,""P_regen_top"":21.2,""cond_T_regen"":121.0,""ht_regen"":13.0,""slv_cool_01"":130.0,""lr_rich_T"":207.0,""reg_dia_in"":17.37,""input_s[0,0]"":134.4,""input_s[0,1]"":16.0,""input_s[0,2]"":959553.0,""input_s[0,3]"":0.071,""input_s[0,4]"":0.0,""input_s[0,5]"":0.0,""input_s[0,6]"":0.211,""input_s[0,7]"":0.0,""input_s[0,8]"":0.0,""input_s[0,9]"":0.0,""input_s[0,10]"":0.0,""input_s[0,11]"":0.0,""input_s[0,12]"":0.0,""input_s[0,13]"":0.0,""input_s[0,14]"":0.0,""input_s[0,15]"":0.718,""input_s[1,0]"":126.0,""input_s[1,1]"":30.0,""input_s[1,2]"":4319500.4116,""input_s[1,3]"":0.66207726067,""input_s[1,4]"":0.28374739743,""input_s[1,5]"":0.0,""input_s[1,6]"":0.0541753419,""input_s[1,7]"":0.0,""input_s[1,8]"":0.0,""input_s[1,9]"":0.0,""input_s[1,10]"":0.0,""input_s[1,11]"":0.0,""input_s[1,12]"":0.0,""input_s[1,13]"":0.0,""input_s[1,14]"":0.0,""input_s[1,15]"":0.0,""input_s[2,0]"":120.0,""input_s[2,1]"":30.0,""input_s[2,2]"":10000.0,""input_s[2,3]"":1.0,""input_s[2,4]"":0.0,""input_s[2,5]"":0.0,""input_s[2,6]"":0.0,""input_s[2,7]"":0.0,""input_s[2,8]"":0.0,""input_s[2,9]"":0.0,""input_s[2,10]"":0.0,""input_s[2,11]"":0.0,""input_s[2,12]"":0.0,""input_s[2,13]"":0.0,""input_s[2,14]"":0.0,""input_s[2,15]"":0.0,""eq_par[0,0]"":0.7996,""eq_par[0,1]"":-8094.81,""eq_par[0,2]"":0.0,""eq_par[0,3]"":-0.007484,""eq_par[1,0]"":98.566,""eq_par[1,1]"":1353.8,""eq_par[1,2]"":-14.3043,""eq_par[1,3]"":0.0,""eq_par[2,0]"":216.049,""eq_par[2,1]"":-12431.7,""eq_par[2,2]"":-35.4819,""eq_par[2,3]"":0.0,""eq_par[3,0]"":1.282562,""eq_par[3,1]"":-3456.179,""eq_par[3,2]"":0.0,""eq_par[3,3]"":0.0,""eq_par[4,0]"":132.899,""eq_par[4,1]"":-13445.9,""eq_par[4,2]"":-22.4773,""eq_par[4,3]"":0.0}";
            Dictionary<string, Object> d = JsonConvert.DeserializeObject<Dictionary<string, Object>>(json);
            Console.WriteLine("KEYS: " + d.Keys.Count());
        }        

        [TestMethod]
        public void TestJsonToDictionary()
        {
            var json = @"{""var1"":1, ""var2"":""value2"", ""var3"":""value3""}";
            Dictionary<string, Object> d = JsonConvert.DeserializeObject<Dictionary<string, Object>>(json);
            Assert.AreEqual((long)1, d["var1"]);
            Assert.AreEqual("value2", d["var2"]);
            Assert.AreEqual("value3", d["var3"]);
        }

        [TestMethod]
        public void TestDictionaryToJson()
        {
            var obj = new Dictionary<string, Object>();
            obj["var1"] = 1;
            obj["var2"] = "value2";
            obj["var3"] = new Object[] { 1, 2, 3, "whatever" };
            string json = JsonConvert.SerializeObject(obj);
            Console.WriteLine(json);
            var s = @"{""var1"":1,""var2"":""value2"",""var3"":[1,2,3,""whatever""]}";
            Assert.AreEqual(s, json);
        }


    }
}
