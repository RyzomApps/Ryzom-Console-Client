///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Apache Commons Lang'
// https://github.com/apache/commons-lang
// which is released under Apache License 2.0.
// https://www.apache.org/licenses/LICENSE-2.0
// Copyright 2021 Apache Commons
///////////////////////////////////////////////////////////////////

namespace API.Helper
{
    /// <summary>
    /// This class assists in validating arguments. 
    /// </summary>
    public class Validate
    {
        /// <summary>
        /// Validate that the argument condition is true; otherwise throwing an exception with the specified message.
        /// </summary>
        public static void IsTrue(bool expression, string message)
        {
            if (!expression)
                throw new System.Exception(message);
        }

        /// <summary>
        /// Validate that the specified argument is not null; otherwise throwing an exception with the specified message.
        /// </summary>
        public static void NotNull(object expression, string message)
        {
            if (expression == null)
                throw new System.Exception(message);
        }

        /// <summary>
        /// Validate that the specified argument array is neither null nor a length of zero (no elements); otherwise throwing an exception with the specified message.
        /// </summary>
        public static void NotEmpty(string expression, string message)
        {
            if (string.IsNullOrEmpty(expression))
                throw new System.Exception(message);
        }
    }
}