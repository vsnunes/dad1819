using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIDA_LIBRARY
{
    public interface IPCS
    {
        void Server(string server_id, int min_delay, int max_delay);
        void Client(string client_id, string script_file);
        string Status();
        void Crash(string processName);
        void Freeze(string processName);
        void Unfreeze(string processName);

    }
}
