﻿namespace DIDA_LIBRARY
{
    public interface IPCS
    {
        string Server(string url, int min_delay, int max_delay, string nameService);
        string Client(string url, string script_file, int counter);
    }
}
