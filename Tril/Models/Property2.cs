using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using Tril.Utilities;

namespace Tril.Models
{
    public partial class Property
    {
        static Dictionary<string, Property> cachedPropertys = new Dictionary<string, Property>();

        /// <summary>
        /// Creates a new instance of Tril.Models.Property
        /// </summary>
        /// <param name="property"></param>
        private Property(PropertyReference property) : base(property) { }

        /// <summary>
        /// Returns either a cached Property if the specified PropertyReference has been used before, otherwise a new Property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static Property GetCachedProperty(PropertyReference property)
        {
            return new Property(property);

            #region caching (produces undebuggable "An element with the same key already exists" errors sometimes)
            //if (property.FullName == null || property.FullName == "")
            //    return new Property(property);

            //string key = property.FullName + "," + property.Module.FullyQualifiedName;
            //if (cachedPropertys.Keys.Contains(key))//.ContainsKey(key))
            //    return cachedPropertys[key];
            //else
            //{
            //    Property prop = new Property(property);
            //    cachedPropertys.Add(key, prop);
            //    return prop;
            //}
            #endregion
        }
        /// <summary>
        /// Returns the appropriate type definition for the declaring type reference
        /// </summary>
        /// <returns></returns>
        public TypeDefinition GetDeclaringTypeDefinition()
        {
            return UnderlyingProperty.DeclaringType.Resolve();
        }
        /// <summary>
        /// Returns the appropriate property definition for the underlying property reference
        /// </summary>
        /// <returns></returns>
        public PropertyDefinition GetPropertyDefinition()
        {
            return UnderlyingProperty.Resolve();
        }

        PropertyDefinition namesakeproperty;
        /// <summary>
        /// Returns the equivalent object to this object from the NamesakeAssembly
        /// </summary>
        /// <returns></returns>
        public override dynamic GetNamesake()
        {
            if (namesakeproperty != null)
                return namesakeproperty;

            TypeDefinition decKindNamesake = DeclaringKind.GetNamesake();
            if (decKindNamesake != null)
            {
                foreach (PropertyDefinition property in decKindNamesake.Properties)
                {
                    if (Utility.AreNamesakes(property, UnderlyingProperty))
                    {
                        namesakeproperty = property;
                        break;
                    }
                }
                return namesakeproperty;
            }

            return null;
        }
    }
}
