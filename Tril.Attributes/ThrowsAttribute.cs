using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;

namespace Tril.Attributes
{
    /// <summary>
    /// Represents the "throws" section of a method signature in Java.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor,
        AllowMultiple = true, Inherited = true)]
    public sealed class ThrowsAttribute : TrilAttribute
    {
        TypeReference _typ2Thrw;

        public ThrowsAttribute(Type typeToThrow)
            : this(typeToThrow, "*") { }

        public ThrowsAttribute(Type typeToThrow, params string[] targetPlats)
            : base(targetPlats) 
        {
            if (typeToThrow == null)
                throw new NullReferenceException("The type to throw cannot be null");

            _typ2Thrw = typeToThrow.ToTypeDefinition(true);
        }

        /// <summary>
        /// Gets the type to throw
        /// </summary>
        public TypeReference TypeToThrow
        {
            get { return _typ2Thrw; }
        }
    }
}
