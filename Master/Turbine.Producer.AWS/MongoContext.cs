using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Producer.AWS.Contracts;

namespace Turbine.Producer.AWS
{
    class MongoContext : IMongoContext
    {
        private string connectionString;
        private string databaseName;

        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }

        public string DatabaseName
        {
            get { return databaseName; }
            set { databaseName = value; }
        }
    }
}
