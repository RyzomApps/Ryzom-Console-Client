using System;
using System.Runtime.Serialization;

namespace API.Exceptions
{
    [Serializable]
    public class UnknownDependencyException : Exception
    {
        public UnknownDependencyException() { }

        public UnknownDependencyException(string message) : base(message) { }

        public UnknownDependencyException(string message, Exception innerException) : base(message, innerException) { }

        protected UnknownDependencyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}