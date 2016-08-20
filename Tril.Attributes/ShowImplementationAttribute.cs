using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// Marks a member or type whose implementation should be shown.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property |
        AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Delegate,
        AllowMultiple = true, Inherited = false)]
    public sealed class ShowImplementationAttribute : TrilAttribute
    {
        public ShowImplementationAttribute()
            : this("*") { }
        public ShowImplementationAttribute(params string[] targetPlats)
            : base(targetPlats) { }
    }
}
