using System;
using System.Collections.Generic;
using System.Linq;
using DIDA_LIBRARY;
using Tuple = DIDA_LIBRARY.Tuple;

namespace DIDA_CLIENT
{
    class Program
    {
        private static List<string> servers = new List<string>();

        private static List<Tuple> responses = new List<Tuple>();

        static void Main(string[] args)
        {
            IFrontEnd frontEnd = null;

            //Display Client Usage Help if no arguments are given
            if (args.Count() == 0)
            {
                Console.WriteLine("** Missing arguments");
                Console.WriteLine("DIDA-CLIENT Usage:");
                Console.WriteLine("DIDA-CLIENT.exe TS_TYPE ID PATH_TXT_SERVERS");
                Console.WriteLine("-----------------------------------");
                Console.WriteLine();
                Console.WriteLine("TS_TYPE should be SMR (State Machine Replication) or XL (Xu and Liskov)");
                Console.WriteLine("ID must be a positive integer corresponding to the Worker/Client ID");
                Console.WriteLine("PATH_TXT_SERVERS is the path of the file containing the full network address of the servers.");
                System.Environment.Exit(1);
            }

            switch (args[0])
            {
                case "SMR": frontEnd = new FrontEndSMR(); break;
                case "XL": frontEnd = new FrontEndXL(Int32.Parse(args[1])); break;
            }

            List<Object> fields = new List<Object>();
            fields.Add("cat");
            fields.Add("white");
            Tuple t = new Tuple(fields);

            switch (args[2])
            {
                case "read":
                    Console.WriteLine("Tuple received: " + frontEnd.Read(t));
                    break;
                case "write":
                    frontEnd.Write(t);
                    break;
                case "take":
                    Console.WriteLine("Tuple received: " + frontEnd.Take(t));
                    break;
            }
        }
    }
}