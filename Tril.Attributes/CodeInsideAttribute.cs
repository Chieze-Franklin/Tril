using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// Marks a method whose user-defined implementation is inside its body.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Event | AttributeTargets.Method,
        AllowMultiple = true, Inherited = false)]
    public sealed class CodeInsideAttribute : TrilAttribute
    {
        public CodeInsideAttribute()
            : this("*") { }
        public CodeInsideAttribute(params string[] targetPlats)
            : base(targetPlats) { }
    }
}
