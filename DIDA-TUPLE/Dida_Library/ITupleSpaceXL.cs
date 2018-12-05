using System.Collections.Generic;

namespace DIDA_LIBRARY
{
    /// <summary>
    /// An interface for describing basic Tuple Spaces Xu and Liskov operations.
    /// </summary>
    public interface ITupleSpaceXL
    {
        /// <summary>
        /// Reads a tuple from the tuple space.
        /// </summary>
        /// <param name="tuple">A tuple to match the read.</param>
        /// <returns>A taple matching the param.</returns>
        Tuple read(Tuple tuple);

        /// <summary>
        /// Writes a new tuple to the tuple space.
        /// </summary>
        /// <param name="workerId">The client/worker identification.</param>
        /// <param name="requestId">The request identification of this operation.</param>
        /// <param name="tuple">A tupple to be written.</param>
        void write(int workerId, int requestId, Tuple tuple, bool writeOnLog=true);

        /// <summary>
        /// Reads and takes a tuple from the tuple space.
        /// </summary>
        /// <param name="workerId">The client/worker identification.</param>
        /// <param name="requestId">The request identification of this operation.</param>
        /// <param name="tuple">A tuple to take.</param>
        /// <returns>A list of taples matching the param.</returns>
        List<Tuple> take(int workerId, int requestId, Tuple tuple);

        /// <summary>
        /// Gets the number of itens in the tupple space.
        /// </summary>
        /// <returns></returns>
        int ItemCount();

        /// <summary>
        /// Executes second phase of take
        /// </summary>
        /// <param name="tuple"></param>
        void remove(Tuple tuple, int workerId, bool writeOnLog = true);

        View Remove(string url);

        View GetActualView();

        void SetView(View view);

        void Status();
        
        void Freeze();

        void Unfreeze();

        void Crash();

        Log fetchLog();

    }
}
