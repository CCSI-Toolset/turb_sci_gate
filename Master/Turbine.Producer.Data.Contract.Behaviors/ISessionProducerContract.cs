using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Producer.Data.Contract.Behaviors
{
    public interface ISessionProducerContract
    {
        /* Create
         *   Returns GUID of new session resource.
         */
        Guid Create();

        /* Submit:
         *   Returns number of jobs moved to submit state.
         */
        int Submit(Guid session);

        /* Cancel:
         *   Returns number of jobs moved to cancel.
         */
        int Cancel(Guid session);

        /* Terminate:
         *   Returns number of jobs moved to Terminate.
         */
        int Terminate(Guid session);

        /* Pause:
         *   Returns number of jobs moved from submit to pause.
         */
        int Pause(Guid session);

        /* Pause:
         *   Returns number of jobs moved from submit to pause.
         */
        int Unpause(Guid session);


        /* Status:
         *   Returns a dictionary, each state and a count
         *   {"finished":100, "submit":10, etc}
         */
        Dictionary<string,int> Status(Guid guid);

        int Delete(Guid session);

        /* Copy:
         *   Copy Session to a new session with the desired job state.
         */
        Dictionary<string, string> Copy(Guid sessionId, string state);

        /* Create
         *   Returns GUID of new session Generator.
         */
        Guid CreateGenerator(Guid session);

        /* Create
         *   Returns new pagenum for Generators.
         */
        int CreateResultPage(Guid session, Guid generator);
    }
}
