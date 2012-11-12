#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using JetBrains.Annotations;


#endregion



namespace My.Common
{
    [Serializable]
    public class MyException : Exception
    {
        public MyException(string message)
                : base(message) {}


        [StringFormatMethod("message")]
        public MyException(string message, params object[] args)
                : this(string.Format(message, args)) {}


        public MyException(string message, Exception innerException)
                : base(message, innerException) {}


        protected MyException(SerializationInfo info, StreamingContext context)
                : base(info, context) {}


        [StringFormatMethod("message")]
        [TerminatesProgram]
        public static void Throw(string message, params object[] args)
        {
            throw new MyException(message, args);
        }
    }
}