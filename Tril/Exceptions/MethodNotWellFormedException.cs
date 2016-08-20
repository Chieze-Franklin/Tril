using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Exceptions
{
    /// <summary>
    /// Thrown when a method marked with the [CodeInside] attribute does not 
    /// follow the expected rules for such methods.
    /// </summary>
    public class MethodNotWellFormedException : ApplicationException
    {
        /// <summary>
        /// 
        /// </summary>
        public MethodNotWellFormedException() : base() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public MethodNotWellFormedException(string message) : base(message) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public MethodNotWellFormedException(string message, Exception inner) : base(message, inner) { }
    }
}
