using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// The base class for all Tril attributes.
    /// </summary>
    public abstract class TrilAttribute : Attribute
    {
        string[] _tgtPlats;

        /// <summary>
        /// Creates a new instance of Tril.TrilAttribute
        /// </summary>
        public TrilAttribute() : this("*") { }
        /// <summary>
        /// Creates a new instance of Tril.TrilAttribute
        /// </summary>
        /// <param name="targetPlatforms"></param>
        public TrilAttribute(params string[] targetPlatforms) 
        {
            //if (targetLangs == null || targetLangs.Any(l => l == null))
            //    throw new NullReferenceException(this.GetType().FullName + ": Array of target languages cannot be null or have a null element!");
            if (targetPlatforms == null)
                targetPlatforms = new string[1] { "*" };
            //targetLangs = targetLangs.Where(t => t != null).ToArray();
            targetPlatforms = targetPlatforms.Select<string, string>(t => { if (t == null || t.Trim() == "") return "*"; return t; }).ToArray();
            for (int index = 0; index < targetPlatforms.Length; index++)
                targetPlatforms[index] = targetPlatforms[index].Trim().ToLower();
            _tgtPlats = targetPlatforms;
        }

        /// <summary>
        /// Gets a list of all the platforms this Tril.TrilAttribute instance targets
        /// </summary>
        public string[] TargetPlatforms
        {
            get { return _tgtPlats; }
        }
    }
}
