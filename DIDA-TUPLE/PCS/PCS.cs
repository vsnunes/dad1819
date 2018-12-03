using System;
using System.Diagnostics;
using System.IO;
using DIDA_LIBRARY;

namespace PCS
{
    class PCS : MarshalByRefObject, IPCS
    {
        public enum ServerType { XL, SMR };
        private ServerType _type;

        public PCS()
        {
            _type = ServerType.XL;
        }

        public PCS(string Type)
        {
            if (Type == "XL")
                _type = ServerType.XL;
            else if (Type == "SMR")
                _type = ServerType.SMR;
        }

        public string Server(string url, int min_delay, int max_delay)
        {
            try
            {
                string args = url + " " + min_delay + " " + max_delay;
                if (_type == ServerType.SMR)
                {
                    ProcessStartInfo info = new ProcessStartInfo(Path.Combine(Directory.GetCurrentDirectory(), "../../../DIDA-TUPLE-SMR/bin/Debug/DIDA-TUPLE-SMR.exe"), args);
                    info.CreateNoWindow = false;
                    info.UseShellExecute = true;
                    Process processChild = Process.Start(info);

                    return "SMR";
                }
                else if (_type == ServerType.XL)
                {
                    ProcessStartInfo info = new ProcessStartInfo(Path.Combine(Directory.GetCurrentDirectory(), "../../../DIDA-TUPLE-XL/bin/Debug/DIDA-TUPLE-XL.exe"), args);
                    info.CreateNoWindow = false;
                    info.UseShellExecute = true;
                    Process processChild = Process.Start(info);
                    return "XL";
                }
                return null;
            }
            catch(Exception)
            {
                Console.WriteLine("Erro a criar Servidor");
                return null;
            }
            
        }

        public string Client(string url, string script_file, int counter)
        {
            try
            {
                string port = url.Split(':')[2].Split('/')[0];

                if (_type == ServerType.SMR)
                {
                    string args = " SMR " + " 0 " + script_file;
                    ProcessStartInfo info = new ProcessStartInfo(Path.Combine(Directory.GetCurrentDirectory(), "../../../DIDA-CLIENT/bin/Debug/DIDA-CLIENT.exe"), args);
                    info.CreateNoWindow = false;
                    info.UseShellExecute = true;
                    Process processChild = Process.Start(info);
                    return "SMR";

                }
                else if (_type == ServerType.XL)
                {
                    string args = " XL " + counter + " " + script_file;
                    Console.WriteLine(args);
                    ProcessStartInfo info = new ProcessStartInfo(Path.Combine(Directory.GetCurrentDirectory(), "../../../DIDA-CLIENT/bin/Debug/DIDA-CLIENT.exe"), args);
                    info.CreateNoWindow = false;
                    info.UseShellExecute = true;
                    Process processChild = Process.Start(info);
                    return "XL";
                }
                return null;
            }
            catch(Exception)
            {
                Console.WriteLine("Erro a criar cliente");
                return null;
            }
           
        }

    }
}
