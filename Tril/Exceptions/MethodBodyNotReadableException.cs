using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Exceptions
{
    /// <summary>
    /// An exception thrown when the CIL body of a method is not readable
    /// </summary>
    public class MethodBodyNotReadableException : ApplicationException
    {
        /// <summary>
        /// 
        /// </summary>
        public MethodBodyNotReadableException() : base() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public MethodBodyNotReadableException(string message) : base(message) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public MethodBodyNotReadableException(string message, Exception inner) : base(message, inner) { }
    }
}
