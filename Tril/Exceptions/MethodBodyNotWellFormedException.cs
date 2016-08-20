using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Exceptions
{
    /// <summary>
    /// An exception thrown when the CIL body of a method is not well formed.
    /// </summary>
    public class MethodBodyNotWellFormedException : ApplicationException
    {
        /// <summary>
        /// 
        /// </summary>
        public MethodBodyNotWellFormedException() : base() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public MethodBodyNotWellFormedException(string message) : base(message) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public MethodBodyNotWellFormedException(string message, Exception inner) : base(message, inner) { }
    }
}
