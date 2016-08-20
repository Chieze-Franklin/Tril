using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// Marks a method whose implementation should be hidden.
    /// When applied to a Type, this attribute affects every method in that type
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property
        | AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Delegate,
        AllowMultiple = true, Inherited = false)]
    public sealed class HideImplementationAttribute : TrilAttribute
    {
        public HideImplementationAttribute()
            : this("*") { }
        public HideImplementationAttribute(params string[] targetPlats)
            : base(targetPlats) { }
    }
}
