namespace DIDA_LIBRARY
{
    public interface IPCS
    {
        string Server(string url, int min_delay, int max_delay);
        string Client(string url, string script_file, int counter);
        string Status();
        void Crash(string processName);
        void Freeze(string processName);
        void Unfreeze(string processName);

    }
}
