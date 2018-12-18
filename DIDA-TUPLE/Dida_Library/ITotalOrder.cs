namespace DIDA_LIBRARY
{
    /// <summary>
    /// Interface for describing Total Order operations
    /// </summary>
    public interface ITotalOrder
    {
        /// <summary>
        /// Commits an operation on a machine. It is done atomically
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <param name="request">The Request TYPE</param>
        /// <param name="tuple">The tuple</param>
        void commit(int id, Request.OperationType request, Tuple tuple);

        /// <summary>
        /// Asks a replic if it is the master
        /// </summary>
        /// <param name="serverPath">The path of who is asking</param>
        /// <returns>True if the machine is the master or false otherwise</returns>
        bool areYouTheMaster(string serverPath);

        /// <summary>
        /// Sets the path of a new master
        /// </summary>
        /// <param name="pathNewMaster">The URL of the new master</param>
        void setNewMaster(string pathNewMaster);

        /// <summary>
        /// Heartbeat function. Complete useless just to test machine liveliness.
        /// </summary>
        void imAlive();

        /// <summary>
        /// Sets the new backup machine to another master.
        /// </summary>
        /// <param name="masterPath">The new master path.</param>
        void setBackup(string masterPath);

        /// <summary>
        /// Get the my current log
        /// </summary>
        /// <returns>A Log structure containing all operation executed so far.</returns>
        Log fetchLog();


    }
}
