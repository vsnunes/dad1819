using System;
using DIDA_LIBRARY;
using System.Collections.Generic;
using System.IO;

namespace PUPPETMASTER
{
    public class PuppetMaster : MarshalByRefObject
    {
        public string MyPath = null;

        string[] file = File.ReadAllLines("../../pcsList.txt");
        List<string> urls = new List<string>();

        public PuppetMaster()
        {
            foreach (string i in file)
            {
                urls.Add(i);
            }

        }

        public void Server(string server_id, string URL, int min_delay, int max_delay) {
            IPCS pcs = null;
            pcs = (IPCS)Activator.GetObject(typeof(IPCS), URL);
            pcs.Server(server_id, min_delay, max_delay);
        }

        public void Client(string client_id, string URL, string script_file) {
            IPCS pcs = null;
            pcs = (IPCS)Activator.GetObject(typeof(IPCS), URL);
            pcs.Client(client_id, script_file);
        }

        public void Status() {
            IPCS pcs = null;

            foreach (string s in urls)
            {
                pcs = (IPCS)Activator.GetObject(typeof(IPCS), s);
                Console.WriteLine("Status of " + s + ":\n" + pcs.Status());

            }

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
