namespace RCC.NetworkAction
{
    /// <summary>
    ///     base for actions with impusions
    /// </summary>
    public abstract class ActionImpulsion : Action
    {
        /// <summary>
        ///     allow exceeding the maximum size of the message
        /// </summary>
        public bool AllowExceedingMaxSize;

        /// <summary>
        ///     reset the action
        /// </summary>
        public override void Reset()
        {
            AllowExceedingMaxSize = false;
        }
    }
}