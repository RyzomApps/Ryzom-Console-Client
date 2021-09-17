using System;
using System.Threading;

namespace RCC
{
    /// <summary>
    ///     Holds an asynchronous task with return value
    /// </summary>
    /// <typeparam name="T">Type of the return value</typeparam>
    public class TaskWithResult<T>
    {
        private readonly AutoResetEvent _resultEvent = new AutoResetEvent(false);
        private readonly Func<T> _task;
        private readonly object _taskRunLock = new object();
        private T _result;

        /// <summary>
        ///     Create a new asynchronous task with return value
        /// </summary>
        /// <param name="task">Delegate with return value</param>
        public TaskWithResult(Func<T> task)
        {
            _task = task;
        }

        /// <summary>
        ///     Check whether the task has finished running
        /// </summary>
        public bool HasRun { get; private set; }

        /// <summary>
        ///     Get the task result (return value of the inner delegate)
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the task is not finished yet</exception>
        public T Result
        {
            get
            {
                if (HasRun)
                {
                    return _result;
                }

                throw new InvalidOperationException("Attempting to retrieve the result of an unfinished task");
            }
        }

        /// <summary>
        ///     Get the exception thrown by the inner delegate, if any
        /// </summary>
        public Exception Exception { get; private set; } = null;

        /// <summary>
        ///     Execute the task in the current thread and set the <see cref="Result" /> property or to the returned
        ///     value
        /// </summary>
        public void ExecuteSynchronously()
        {
            // Make sur the task will not run twice
            lock (_taskRunLock)
            {
                if (HasRun)
                {
                    throw new InvalidOperationException("Attempting to run a task twice");
                }
            }

            // Run the task
            try
            {
                _result = _task();
            }
            catch (Exception e)
            {
                Exception = e;
            }

            // Mark task as complete and release wait event
            lock (_taskRunLock)
            {
                HasRun = true;
            }

            _resultEvent.Set();
        }

        /// <summary>
        ///     Wait until the task has run from another thread and get the returned value or exception thrown by the task
        /// </summary>
        /// <returns>Task result once available</returns>
        /// <exception cref="System.Exception">Any exception thrown by the task</exception>
        public T WaitGetResult()
        {
            // Wait only if the result is not available yet
            bool mustWait;
            lock (_taskRunLock)
            {
                mustWait = !HasRun;
            }

            if (mustWait)
            {
                _resultEvent.WaitOne();
            }

            // Receive exception from task
            if (Exception != null)
                throw Exception;

            return _result;
        }
    }
}