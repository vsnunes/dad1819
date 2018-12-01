using System;
using DIDA_LIBRARY;

namespace PCS
{
    class PCS : MarshalByRefObject, IPCS
    {
        public PCS()
        {

        }

        public void Server(string server_id, int min_delay, int max_delay)
        {
            System.Console.WriteLine("Criei Server");
        }

        public void Client(string client_id, string script_file)
        {
            System.Console.WriteLine("Criei Cliente");
        }

        public string Status()
        {
            return "STAAAAAAAAAAAAAAAAAAAAAAAAAAAATUS";
        }

        public void Crash(string processName) { }

        public void Freeze(string processName) { }

        public void Unfreeze(string processName) { }

    }
}
