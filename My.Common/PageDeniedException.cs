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
    public class PageDeniedException : MyException
    {
        public PageDeniedException(string message) : base(message) {}
        public PageDeniedException(string message, Exception inner) : base(message, inner) {}


        [StringFormatMethod("message")]
        public PageDeniedException(string message, params object[] args)
                : this(string.Format(message, args)) {}


        [StringFormatMethod("message")]
        [TerminatesProgram]
        public new static void Throw(string message, params object[] args)
        {
            throw new PageDeniedException(message, args);
        }


        protected PageDeniedException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}