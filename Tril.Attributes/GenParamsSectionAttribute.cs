using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// Represents the user-defined generic parameters section (not list) of a type or member
    /// </summary>
    public abstract class GenParamsSectionAttribute : TrilAttribute
    {
        string _genParamsSec = "";

        /// <summary>
        /// Creates a new instance of the Tril.GenParamsSectionAttribute class
        /// </summary>
        /// <param name="section"></param>
        public GenParamsSectionAttribute(string section)
            : this(section, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.GenParamsSectionAttribute class
        /// </summary>
        /// <param name="section"></param>
        /// <param name="targetPlats"></param>
        public GenParamsSectionAttribute(string section, params string[] targetPlats)
            :base(targetPlats)
        {
            _genParamsSec = section == null ? "" : section.Trim();
        }

        /// <summary>
        /// Gets the generic parameters section
        /// </summary>
        public string GenericParametersSection 
        {
            get { return _genParamsSec; }
        }
    }

    /// <summary>
    /// Represents the user-defined generic parameters section (not list) of a type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Delegate,
        AllowMultiple = true, Inherited = false)]
    public sealed class TypeGenParamsSecAttribute : GenParamsSectionAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.TypeGenParamsSecAttribute class
        /// </summary>
        /// <param name="section"></param>
        public TypeGenParamsSecAttribute(string section)
            : this(section, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.TypeGenParamsSecAttribute class
        /// </summary>
        /// <param name="section"></param>
        /// <param name="targetPlats"></param>
        public TypeGenParamsSecAttribute(string section, params string[] targetPlats)
            : base(section, targetPlats) { }
    }

    /// <summary>
    /// Represents the user-defined generic parameters section (not list) of a member
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor,
        AllowMultiple = true, Inherited = true)]
    public sealed class MemberGenParamsSecAttribute : GenParamsSectionAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.MemberGenParamsSecAttribute class
        /// </summary>
        /// <param name="section"></param>
        public MemberGenParamsSecAttribute(string section)
            : this(section, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.MemberGenParamsSecAttribute class
        /// </summary>
        /// <param name="section"></param>
        /// <param name="targetPlats"></param>
        public MemberGenParamsSecAttribute(string section, params string[] targetPlats)
            : base(section, targetPlats) { }
    }
}
