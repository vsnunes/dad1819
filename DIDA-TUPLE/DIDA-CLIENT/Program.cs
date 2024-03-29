﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DIDA_LIBRARY;
using Tuple = DIDA_LIBRARY.Tuple;

namespace DIDA_CLIENT
{
    public class Program
    {
        private static int _counter = 0;
        private static string[] lines;
        private static List<string> servers = new List<string>();

        private static List<Tuple> responses = new List<Tuple>();

        private static Tuple checkTupleSyntax(Parser parser, string input)
        {
            ParseTree tree = parser.Parse(input);
            Tuple tuple = null;

            tuple = (Tuple)tree.Eval(null);

            if (tuple == null)
            {
                Console.WriteLine("### ERROR: Invalid tuple syntax representation");
            }
            return tuple;
        }

        static void Main(string[] args)
        {
            IFrontEnd frontEnd = null;
            // create the scanner to use
            Scanner scanner = new Scanner();

            // create the parser, and supply the scanner it should use
            Parser parser = new Parser(scanner);


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

            string input = "";
            string operation;
            string prompt = "[CLIENT " + args[0] + " " + args[1] + "]";

            while (true)
            {
                _counter = 0;
                Console.WriteLine(prompt); input = args[2];
                if (input == "exit")
                {
                    return;
                }
                else if (input == "help")
                {
                    Console.WriteLine("Available commands: ");
                    Console.WriteLine("add <\"A\",\"B\",\"C\">");
                    Console.WriteLine("read <\"A\",\"B\",\"C\">");
                    Console.WriteLine("take <\"A\",\"B\",\"C\">");
                    Console.WriteLine("exit");
                    Console.WriteLine();
                    break;
                }
                try
                {
                    
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "../../../scripts/" + input);
                    Console.WriteLine(path);
                    lines = File.ReadAllLines(path);
                }
                catch (Exception)
                {
                    Console.WriteLine("Fizeste asneira. Ou o ficheiro não esta na diretoria certa ou o nome não é o correto. Tenta outra vez.");
                    Console.ReadLine();
                    break;
                }
                while (_counter < lines.Count())
                {
                    operation = lines[_counter].Split(' ')[0];
                    ExecuteOperation(operation, lines[_counter], parser, frontEnd, prompt);
                }
                break;
            }
        }
        /// <summary>
        /// Execute an operation based on a parsed instruction.
        /// </summary>
        /// <param name="operation">Operation type (e.g. read, add, write, begin-repeat)</param>
        /// <param name="input">A complete string instruction</param>
        /// <param name="parser">An instance of the parser</param>
        /// <param name="frontEnd">An instance of the frontEnd (could be SMR ou XL).</param>
        /// <param name="prompt">A default string which are printed as prompt label.</param>
        private static void ExecuteOperation(string operation, string input, Parser parser, IFrontEnd frontEnd, string prompt)
        {
            Tuple tuple = null;
            switch (operation)
            {
                case "read":
                    tuple = checkTupleSyntax(parser, input);

                    Console.WriteLine("Tuple received: " + frontEnd.Read(tuple));
                    _counter++;
                    break;
                case "add":
                    tuple = checkTupleSyntax(parser, input);
                    frontEnd.Write(tuple);
                    _counter++;
                    break;
                case "take":
                    tuple = checkTupleSyntax(parser, input);

                    Console.WriteLine("Tuple received: " + frontEnd.Take(tuple));
                    _counter++;
                    break;

                case "begin-repeat":
                    try
                    {
                        int times = Int32.Parse(input.Split(' ')[1]);
                        if (times <= 0)
                        {
                            Console.WriteLine("### ERROR: Invalid begin-repeat arg: must be a positive integer!");
                        }
                        else
                        {
                            _counter++;
                            List<string> inputs = new List<string>();
                            while (true)
                            {
                                string innerInput = lines[_counter];
                                string innerOperation;

                                //Only when end is provided we execute the all body of begin-repeat
                                if (innerInput == "end-repeat")
                                {
                                    for (int i = 0; i < times; i++)
                                    {
                                        foreach (string storedInput in inputs)
                                        {
                                            innerOperation = storedInput.Split(' ')[0];
                                            ExecuteOperation(innerOperation, storedInput, parser, frontEnd, prompt);
                                        }

                                    }
                                    break;
                                }
                                else
                                {
                                    inputs.Add(innerInput);
                                    _counter++;
                                }



                            }

                        }




                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("### ERROR: Invalid begin-repeat arg");
                        Console.WriteLine(e.StackTrace);
                    }

                    break;

                case "wait":
                    try
                    {
                        int seconds = Int32.Parse(input.Split(' ')[1]);
                        if (seconds <= 0)
                        {
                            Console.WriteLine("### ERROR: Invalid wait arg: must be a positive number!");
                        }
                        else
                        {
                            Console.WriteLine("I'm waiting ...");
                            Thread.Sleep(seconds);
                            Console.WriteLine("Finished waiting!");
                            _counter++;
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("### ERROR: Invalid wait arg");
                    }
                    break;

                default:
                    Console.WriteLine("### ERROR: Invalid command");
                    break;
            }
        }
    }
}