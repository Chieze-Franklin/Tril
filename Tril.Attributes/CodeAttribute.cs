using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// Represents a user-defined piece of code
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Event | AttributeTargets.Method,
        AllowMultiple = true, Inherited = false)]
    public sealed class CodeAttribute : TrilAttribute
    {
        string _code = "";

        /// <summary>
        /// Creates a new instance of the Tril.CodeLineAttribute class
        /// </summary>
        /// <param name="code"></param>
        public CodeAttribute(string code)
            : this(code, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.CodeLineAttribute class
        /// </summary>
        /// <param name="code"></param>
        /// <param name="targetPlats"></param>
        public CodeAttribute(string code, params string[] targetPlats)
            : base(targetPlats)
        {
            _code = code == null ? "" : code;
        }

        /// <summary>
        /// Gets the user-defined line of code
        /// </summary>
        public string Code
        {
            get { return _code; }
        }
    }
}
