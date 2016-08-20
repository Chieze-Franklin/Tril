using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// Represents the user-defined parameter section (not list) of a member
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor,
        AllowMultiple = true, Inherited = true)]
    public sealed class ParamsSecAttribute : TrilAttribute
    {
        string _paramsSec = "";

        /// <summary>
        /// Creates a new instance of the Tril.Attributes.ParamsSectionAttribute class
        /// </summary>
        /// <param name="section"></param>
        public ParamsSecAttribute(string section)
            : this(section, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.Attributes.ParamsSectionAttribute class
        /// </summary>
        /// <param name="section"></param>
        /// <param name="targetPlats"></param>
        public ParamsSecAttribute(string section, params string[] targetPlats)
            :base(targetPlats)
        {
            _paramsSec = section == null ? "" : section.Trim();
        }

        /// <summary>
        /// Gets the parameters section
        /// </summary>
        public string ParametersSection
        {
            get { return _paramsSec; }
        }
    }
}
