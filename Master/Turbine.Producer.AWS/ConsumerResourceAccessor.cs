using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using Microsoft.Practices.Unity;
using Turbine.Producer.AWS.Contracts;
using Turbine.Producer.Contracts;

namespace Turbine.Producer.AWS
{
    /// <summary>
    /// ConsumerResourceAccessor uses both the IAWSContext and the IMongoContext, these must be
    /// registered in the Turbine.Producer.Container
    /// </summary>
    public class ConsumerResourceAccessor : IConsumerResourceAccessor
    {

        public void GetConsumerLog(string instanceID, StringBuilder builder)
        {
            var container = Container.GetProducerContainer();
            var ctx = container.Resolve<IMongoContext>();
            string connectionString = ctx.ConnectionString;
            var server = MongoDB.Driver.MongoServer.Create(connectionString);
            var db = server.GetDatabase(ctx.DatabaseName);
            var col = db.GetCollection(instanceID);
            Debug.WriteLine("Log For " + instanceID + "(" + col.Count() + ")");
            foreach (MongoDB.Bson.BsonDocument bdoc in col.FindAll().SetSortOrder(
                MongoDB.Driver.Builders.SortBy.Ascending("timestamp")))
            {
                var msg = bdoc.GetElement("message").Value;
                builder.AppendLine(msg.ToString());
            }
        }

    }
}

