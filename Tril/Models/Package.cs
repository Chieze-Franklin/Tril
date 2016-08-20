using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tril.Utilities;

namespace Tril.Models
{
    /// <summary>
    /// Represents a namespace in an assembly
    /// </summary>
    [Serializable]
    public class Package
    {
        string _namespace;
        List<Kind> _kinds = new List<Kind>();

        /// <summary>
        /// Creates a new instance of the Tril.Models.Package class
        /// </summary>
        /// <param name="Namespace"></param>
        public Package(string Namespace)
        {
            _namespace = Namespace;
        }

        /// <summary>
        /// Adds a Tril.Models.kind object to this Tril.Models.Package instance
        /// </summary>
        /// <param name="kind">the Tril.Models.kind object to add</param>
        /// <returns>returns true if the Kind was added</returns>
        public bool Add(Kind kind)
        {
            return Add(kind, "*");
        }
        /// <summary>
        /// Adds a Tril.Models.kind object to this Tril.Models.Package instance
        /// </summary>
        /// <param name="kind">the Tril.Models.kind object to add</param>
        /// <param name="useDefaultOnly"></param>
        /// <returns>returns true if the Kind was added</returns>
        public bool Add(Kind kind, bool useDefaultOnly)
        {
            return Add(kind, useDefaultOnly, "*");
        }
        /// <summary>
        /// Adds a Tril.Models.kind object to this Tril.Models.Package instance
        /// </summary>
        /// <param name="kind">the Tril.Models.kind object to add</param>
        /// <param name="targetLangs"></param>
        /// <returns>returns true if the Kind was added</returns>
        public bool Add(Kind kind, params string[] targetLangs)
        {
            return Add(kind, false, targetLangs);
        }
        /// <summary>
        /// Adds a Tril.Models.kind object to this Tril.Models.Package instance
        /// </summary>
        /// <param name="kind">the Tril.Models.kind object to add</param>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetLangs"></param>
        /// <returns>returns true if the kind was added</returns>
        public bool Add(Kind kind, bool useDefaultOnly, params string[] targetLangs)
        {
            targetLangs = Utility.UnNullifyArray(targetLangs);

            if (Namespace != kind.GetNamespace(useDefaultOnly, targetLangs))
                return false;

            if (kind.IsHidden(targetLangs) && !useDefaultOnly)
            {
                return false;
            }

            _kinds.Add(kind);
            return true;
        }
        /// <summary>
        /// Returns a collection of all Tril.Models.Kind objects added to this Tril.Models.Package instance
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Kind> GetKinds()
        {
            return _kinds.AsEnumerable<Kind>();
        }

        /// <summary>
        /// Gets the namespace represented by this Tril.Package instance
        /// </summary>
        public string Namespace
        {
            get { return _namespace; }
        }
    }
}
