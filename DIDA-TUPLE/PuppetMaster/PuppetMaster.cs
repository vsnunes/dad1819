using System;
using System.Collections.Generic;

namespace PUPPETMASTER
{
    public class PuppetMaster : MarshalByRefObject
    {
        public string MyPath = null;
        public List<string> serverList = new List<string>();

        public PuppetMaster()
        {

        }

        public void Server(string server_id, string URL, int min_delay, int max_delay) {
 
        }

        public void Client(string client_id, string URL, string script_file) {
            System.Console.WriteLine("client_id " + client_id);
            System.Console.WriteLine("URL " + URL);
            System.Console.WriteLine("script " + script_file);

        }

        public void Status() {
            System.Console.WriteLine("STATUUUUUUUUUUUS");
        }

        public void Crash(string processName) {
            System.Console.WriteLine("processName" + processName);
        }

        public void Freeze(string processName) {
            System.Console.WriteLine("processName" + processName);
        }

        public void Unfreeze(string processName) {
            System.Console.WriteLine("processName" + processName);
        }
    }
}
