using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.IO;

namespace PUPPETMASTER
{
    class Program
    {
        private static int _counter = 0;
        static void Main(string[] args)
        {
            TcpChannel channel;

            
            channel = new TcpChannel(10001);
            ChannelServices.RegisterChannel(channel, false);


            PuppetMaster puppetMaster = new PuppetMaster();

            RemotingServices.Marshal(puppetMaster, "PuppetMaster", typeof(PuppetMaster));

            System.Console.WriteLine("PuppetMaster Service Started");
            System.Console.WriteLine("---------------");
            System.Console.WriteLine("Type Exit to exit...");

            //-----------------------------------
            string input = "";
            string operation = "";
            string[] lines;

            if (args.Count() > 0)
            {
                while (true)
                {
                    _counter = 0;
                    input = args[0];
                    

                    try
                    {
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "../../" + input + ".txt");
                        
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
                        operation = input.Split(' ')[0];
                        ExecuteOperation(puppetMaster, operation, lines[_counter]);
                    }

                   
                }
            }
            else
            {
                while (true)
                {
                    Console.Write("Insert your command > "); input = Console.ReadLine();
                    operation = input.Split(' ')[0];
                    ExecuteOperation(puppetMaster, operation, input);

                }
            }
        }

        private static void ExecuteOperation(PuppetMaster puppetMaster, string operation, string input)
        {
            try
            {
                switch (operation)
                {
                    case "Server":
                        try
                        {
                            puppetMaster.Server(input.Split(' ')[1], input.Split(' ')[2], Int32.Parse(input.Split(' ')[3]), Int32.Parse(input.Split(' ')[4]));
                            _counter++;
                            break;
                        }
                        catch (FormatException)
                        {
                            System.Console.WriteLine("Max_delay and Min_delay need to be integers");
                            break;
                        }
                        catch (ArgumentNullException)
                        {
                            System.Console.WriteLine("Max_delay and Min_delay cannot be null");
                            break;
                        }
                        
                    case "Client":
                        puppetMaster.Client(input.Split(' ')[1], input.Split(' ')[2], input.Split(' ')[3]);
                        _counter++;
                        break;
                    case "Status":
                        puppetMaster.Status();
                        _counter++;
                        break;
                    case "Crash":
                        puppetMaster.Crash(input.Split(' ')[1]);
                        _counter++;
                        break;
                    case "Freeze":
                        puppetMaster.Freeze(input.Split(' ')[1]);
                        _counter++;
                        break;
                    case "Unfreeze":
                        puppetMaster.Unfreeze(input.Split(' ')[1]);
                        _counter++;
                        break;
                    case "Wait":
                        try
                        {
                            Thread.Sleep(Int32.Parse(input.Split(' ')[1]));
                            break;
                        }
                        catch (FormatException)
                        {
                            System.Console.WriteLine("Wait argument must be an integer");
                            break;
                        }
                        catch (ArgumentNullException)
                        {
                            System.Console.WriteLine("wait time cannot be null");
                            break;
                        }
                    case "Load":
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "../../" + input.Split(' ')[1] + ".txt");
                        string[] lines = File.ReadAllLines(path);
                        foreach(string line in lines){
                            ExecuteOperation(puppetMaster, line.Split(' ')[0], line);
                        }
                        _counter++;
                        break;
                    case "Exit":
                        System.Environment.Exit(1);
                        break;
                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Missing Arguments");
            }
            catch (OverflowException)
            {
                System.Console.WriteLine("Number too big");
            }
        }
    }
}
