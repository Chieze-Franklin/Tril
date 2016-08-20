using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Exceptions
{
    /// <summary>
    /// An exception thrown when a push operation on the code stack fails
    /// </summary>
    public class StackPushException : MethodBodyNotWellFormedException
    {
        /// <summary>
        /// 
        /// </summary>
        public StackPushException() : base() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public StackPushException(string message) : base(message) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public StackPushException(string message, Exception inner) : base(message, inner) { }
    }
}
