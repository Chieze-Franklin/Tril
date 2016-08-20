using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Exceptions
{
    /// <summary>
    /// An exception thrown when a pop operation on the code stack fails
    /// </summary>
    public class StackPopException : MethodBodyNotWellFormedException
    {
        /// <summary>
        /// 
        /// </summary>
        public StackPopException() : base() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public StackPopException(string message) : base(message) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public StackPopException(string message, Exception inner) : base(message, inner) { }
    }
}
