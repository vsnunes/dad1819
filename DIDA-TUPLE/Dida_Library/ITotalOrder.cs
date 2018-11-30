namespace DIDA_LIBRARY
{
    /// <summary>
    /// Interface for describing Total Order operations
    /// </summary>
    public interface ITotalOrder
    {
        
        void commit(int id, Request.OperationType request, Tuple tuple);

        bool areYouTheMaster(string serverPath);

        void setNewMaster(string pathNewMaster);

        void imAlive();

        void setBackup(string masterPath);

        Log fetchLog();

    }
}
