using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// Base class for the pre-body attributes
    /// </summary>
    public abstract class PreBodyAttribute : TrilAttribute
    {
        string _preBdySec = "";

        /// <summary>
        /// Creates a new instance of the Tril.PreBodyAttribute class
        /// </summary>
        /// <param name="section"></param>
        public PreBodyAttribute(string section)
            : this(section, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.PreBodyAttribute class
        /// </summary>
        /// <param name="section"></param>
        /// <param name="targetPlats"></param>
        public PreBodyAttribute(string section, params string[] targetPlats)
            :base(targetPlats)
        {
            _preBdySec = section == null ? "" : section.Trim();
        }

        /// <summary>
        /// Gets the pre-body section
        /// </summary>
        public string PreBodySection
        {
            get { return _preBdySec; }
        }
    }

    /// <summary>
    /// Represents the user-defined piece of code to be printed
    /// before the body of a member.
    /// Two applications of this are:
    /// 1. To represent the explicit call to a constructor from another constructor using "this" or "base" in C#.
    /// 2. The "throws" part of a method signature in Java.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor,// | AttributeTargets.Property,
        AllowMultiple = true, Inherited = true)]
    public sealed class MemberPreBodyAttribute : PreBodyAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.MemberPreBodyAttribute class
        /// </summary>
        /// <param name="section"></param>
        public MemberPreBodyAttribute(string section)
            : this(section, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.MemberPreBodyAttribute class
        /// </summary>
        /// <param name="section"></param>
        /// <param name="targetPlats"></param>
        public MemberPreBodyAttribute(string section, params string[] targetPlats)
            : base(section, targetPlats) { }
    }

    /// <summary>
    /// Represents the user-defined piece of code to be printed
    /// before the body of a type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum
        | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Delegate,
        AllowMultiple = true, Inherited = false)]
    public sealed class TypePreBodyAttribute : PreBodyAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.TypePreBodyAttribute class
        /// </summary>
        /// <param name="section"></param>
        public TypePreBodyAttribute(string section)
            : this(section, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.TypePreBodyAttribute class
        /// </summary>
        /// <param name="section"></param>
        /// <param name="targetPlats"></param>
        public TypePreBodyAttribute(string section, params string[] targetPlats)
            : base(section, targetPlats) { }
    }
}
