namespace DIDA_LIBRARY
{
    /// <summary>
    /// An interface for describing basic Tuple Spaces operations.
    /// </summary>
    public interface ITupleSpace
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
        /// <param name="tuple">A tupple to be written.</param>
        void write(Tuple tuple);

        /// <summary>
        /// Reads and takes a tuple from the tuple space.
        /// </summary>
        /// <param name="tuple">A tuple to take.</param>
        /// <returns>A taple matching the param.</returns>
        Tuple take(Tuple tuple);

        /// <summary>
        /// Gets the number of itens in the tupple space.
        /// </summary>
        /// <returns></returns>
        int ItemCount();

        int GetID();

        void Status();

        void Freeze();

        void Freeze(int seconds);

        void SoftFreeze();

        void SoftUnFreeze();

        void Unfreeze();

        void Unfreeze(int seconds);

        void Crash();

        Log fetchLog();
    }
}
