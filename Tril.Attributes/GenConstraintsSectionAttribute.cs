using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// Represents the user-defined generic constraints section (not list) of a type or member
    /// </summary>
    public abstract class GenConstraintsSectionAttribute : TrilAttribute
    {
        string _genConsSec = "";

        /// <summary>
        /// Creates a new instance of the Tril.GenConstraintsSectionAttribute class
        /// </summary>
        /// <param name="section"></param>
        public GenConstraintsSectionAttribute(string section)
            : this(section, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.GenConstraintsSectionAttribute class
        /// </summary>
        /// <param name="section"></param>
        /// <param name="targetPlats"></param>
        public GenConstraintsSectionAttribute(string section, params string[] targetPlats)
            : base(targetPlats)
        {
            _genConsSec = section == null ? "" : section.Trim();
        }

        /// <summary>
        /// Gets the generic constraints section
        /// </summary>
        public string GenericConstraintsSection
        {
            get { return _genConsSec; }
        }
    }

    /// <summary>
    /// Represents the user-defined generic constraints section (not list) of a type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Delegate,
        AllowMultiple = true, Inherited = false)]
    public sealed class TypeGenConstraintsSecAttribute : GenConstraintsSectionAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.TypeGenConstraintsSecAttribute class
        /// </summary>
        /// <param name="section"></param>
        public TypeGenConstraintsSecAttribute(string section)
            : this(section, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.TypeGenConstraintsSecAttribute class
        /// </summary>
        /// <param name="section"></param>
        /// <param name="targetPlats"></param>
        public TypeGenConstraintsSecAttribute(string section, params string[] targetPlats)
            : base(section, targetPlats) { }
    }

    /// <summary>
    /// Represents the user-defined generic constraints section (not list) of a member
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor,
        AllowMultiple = true, Inherited = true)]
    public sealed class MemberGenConstraintsSecAttribute : GenConstraintsSectionAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.MemberGenConstraintsSecAttribute class
        /// </summary>
        /// <param name="section"></param>
        public MemberGenConstraintsSecAttribute(string section)
            : this(section, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.MemberGenConstraintsSecAttribute class
        /// </summary>
        /// <param name="section"></param>
        /// <param name="targetPlats"></param>
        public MemberGenConstraintsSecAttribute(string section, params string[] targetPlats)
            : base(section, targetPlats) { }
    }
}
