namespace RCC.Client
{
    /// <summary>
    ///     Implement this class if you want to wait for
    ///     string to be delivered.
    /// </summary>
    public abstract class StringWaitCallback
    {
        /// Overide this method to receive callback for string.
        public abstract void OnStringAvailable(uint stringId, string value);

        /// Overide this method to receive callback for dynamic string.
        public abstract void OnDynStringAvailable(uint stringId, string value);
    };
}