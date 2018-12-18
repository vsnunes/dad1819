namespace DIDA_LIBRARY
{
    /// <summary>
    /// An interface for describing basic PCS operations
    /// </summary>
    public interface IPCS
    {
        string Server(string url, int min_delay, int max_delay);
        string Client(string url, string script_file, int counter);
    }
}
