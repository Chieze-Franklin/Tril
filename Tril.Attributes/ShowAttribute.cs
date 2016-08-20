using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// Used to signify that the target type or member should be included in the translation 
    /// if the translation is to one of the specified platforms.
    /// </summary>
    [AttributeUsage(AttributeTargets.All,
        AllowMultiple = false, Inherited = true)]
    public sealed class ShowAttribute : TrilAttribute
    {
        /// <summary>
        /// Creates a new instance of Tril.TargetAttribute
        /// </summary>
        public ShowAttribute() : this("*") { }
        /// <summary>
        /// Creates a new instance of Tril.TargetAttribute
        /// </summary>
        /// <param name="targetPlats"></param>
        public ShowAttribute(params string[] targetPlats) : base(targetPlats) { }
    }
}
