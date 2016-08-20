using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using Tril.Utilities;

namespace Tril.Models
{
    public partial class Kind
    {
        static Dictionary<string, Kind> cachedKinds = new Dictionary<string, Kind>();

        /// <summary>
        /// Creates a new instance of Tril.Models.Kind
        /// </summary>
        /// <param name="type"></param>
        private Kind(TypeReference type) : base(type) { }

        /// <summary>
        /// Returns either a cached Kind if the specified TypeReference has been used before, otherwise a new Kind
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Kind GetCachedKind(TypeReference type)
        {
            return new Kind(type);

            #region caching (produces undebuggable "An element with the same key already exists" errors sometimes)
            //if (type.FullName == null || type.FullName == "")
            //    return new Kind(type);

            //string key = type.FullName + "," + type.Module.FullyQualifiedName;
            //if (cachedKinds.Keys.Contains(key))//.ContainsKey(key))
            //    return cachedKinds[key];
            //else
            //{
            //    Kind kind = new Kind(type);
            //    cachedKinds.Add(key, kind);
            //    return kind;
            //}
            #endregion
        }
        /// <summary>
        /// Returns the appropriate type definition for the underlying type reference
        /// </summary>
        /// <returns></returns>
        public TypeDefinition GetTypeDefinition() 
        {
            return UnderlyingType.Resolve();
        }

        TypeDefinition namesaketype;
        /// <summary>
        /// Returns the equivalent object to this object from the NamesakeAssembly
        /// </summary>
        /// <returns></returns>
        public override dynamic GetNamesake() 
        {
            if (namesaketype != null)
                return namesaketype;

            if (Model.NamesakeAssembly != null) 
            {
                namesaketype = Utility.FindNamesakeInAssembly(UnderlyingType, Model.NamesakeAssembly);

                return namesaketype;
            }

            return null;
        }
    }
}
