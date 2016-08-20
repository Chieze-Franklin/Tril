using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// Used to signify that the target type or member should be excluded in the translation 
    /// if the translation is to one of the specified platforms.
    /// </summary>
    [AttributeUsage(AttributeTargets.All,
        AllowMultiple = false, Inherited = true)]
    public sealed class HideAttribute : TrilAttribute
    {
        /// <summary>
        /// Creates a new instance of Tril.HideAttribute
        /// </summary>
        public HideAttribute() : this("*") { }
        /// <summary>
        /// Creates a new instance of Tril.HideAttribute
        /// </summary>
        /// <param name="forbiddenPlats"></param>
        public HideAttribute(params string[] forbiddenPlats) : base(forbiddenPlats) { }
    }
}
