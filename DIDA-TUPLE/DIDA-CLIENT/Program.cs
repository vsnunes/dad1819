﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DIDA_LIBRARY;
using Tuple = DIDA_LIBRARY.Tuple;

namespace DIDA_CLIENT
{
    class Program
    {
        static void Main(string[] args)
        {
            ITupleSpace server = (ITupleSpace)Activator.GetObject(typeof(ITupleSpace), "tcp://localhost:8088/DIDA-TUPLE-SMR");
            List<Object> fields = new List<Object>();
            fields.Add("cat");
            fields.Add("white");
            Tuple t = new Tuple(fields);

            if (args.Count() == 0)
            {
                Console.WriteLine("Client will read now ...");
                Tuple r = server.read(t);
                Console.WriteLine("Client read returned: " + r);
            }
            else if (args.Count() == 1)
            {
                Console.WriteLine("Client will take now ...");
                Tuple r = server.take(t);
                Console.WriteLine("Client take returned: " + r);
            }

            else
            {
                Console.WriteLine("Client will write now ...");
                server.write(t);
                Console.WriteLine("Client write returned!");
            }
            Console.ReadLine();
        }
    }
}
