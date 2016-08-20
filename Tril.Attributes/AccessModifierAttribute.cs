using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// Represents an access modifier to be applied to a type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Event|
        AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Property |
        AttributeTargets.Struct | AttributeTargets.Delegate,
        AllowMultiple = true, Inherited = false)]
    public sealed class AccessModifierAttribute : TrilAttribute
    {
        string _modifier = "";

        /// <summary>
        /// Creates a new instance of the Tril.AccessModifierAttribute class
        /// </summary>
        /// <param name="AccessModifier"></param>
        public AccessModifierAttribute(string AccessModifier)
            : this(AccessModifier, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.AccessModifierAttribute class
        /// </summary>
        /// <param name="AccessModifier"></param>
        /// <param name="targetPlats"></param>
        public AccessModifierAttribute(string AccessModifier, params string[] targetPlats)
            : base(targetPlats)
        {
            _modifier = AccessModifier == null ? "" : AccessModifier.Trim();
        }
        /// <summary>
        /// Creates a new instance of the Tril.AccessModifierAttribute class
        /// </summary>
        /// <param name="AccessModifier"></param>
        public AccessModifierAttribute(AccessModifiers AccessModifier)
            : this(AccessModifier, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.AccessModifierAttribute class
        /// </summary>
        /// <param name="AccessModifier"></param>
        /// <param name="targetPlats"></param>
        public AccessModifierAttribute(AccessModifiers AccessModifier, params string[] targetPlats)
            : base(targetPlats)
        {
            if (AccessModifier == AccessModifiers.None)
                _modifier = "";
            else if (AccessModifier == AccessModifiers.Private)
                _modifier = "private";
            else if (AccessModifier == AccessModifiers.Protected)
                _modifier = "protected";
            else if (AccessModifier == AccessModifiers.Public)
                _modifier = "public";
            else if (AccessModifier == AccessModifiers.ProtectedInternal)
                _modifier = "protected internal";
            else if (AccessModifier == AccessModifiers.Internal)
                _modifier = "internal";
            else
                _modifier = "";
        }

        /// <summary>
        /// Gets the access modifier applied to a type
        /// </summary>
        public string AccessModifier
        {
            get { return _modifier; }
        }
    }

    /// <summary>
    /// Lists the recognised access modifiers
    /// </summary>
    public enum AccessModifiers
    {
        /// <summary>
        /// none specified
        /// </summary>
        None,
        /// <summary>
        /// public
        /// </summary>
        Public,
        /// <summary>
        /// private
        /// </summary>
        Private,
        /// <summary>
        /// protected
        /// </summary>
        Protected,
        /// <summary>
        /// internal
        /// </summary>
        Internal,
        /// <summary>
        /// protected internal
        /// </summary>
        ProtectedInternal
    }
}
