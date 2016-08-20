using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using Tril.Utilities;

namespace Tril.Models
{
    public partial class Field
    {
        static Dictionary<string, Field> cachedFields = new Dictionary<string, Field>();

        /// <summary>
        /// Creates a new instance of Tril.Models.Field
        /// </summary>
        /// <param name="field"></param>
        private Field(FieldReference field) : base(field) { }

        /// <summary>
        /// Returns either a cached Field if the specified FieldReference has been used before, otherwise a new Field
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Field GetCachedField(FieldReference field)
        {
            return new Field(field);

            #region caching (produces undebuggable "An element with the same key already exists" errors sometimes)
            //if (field.FullName == null || field.FullName == "")
            //    return new Field(field);

            //string key = field.FullName + "," + field.Module.FullyQualifiedName;
            //if (cachedFields.Keys.Contains(key))//.ContainsKey(key))
            //    return cachedFields[key];
            //else
            //{
            //    Field fld = new Field(field);
            //    cachedFields.Add(key, fld);
            //    return fld;
            //}
            #endregion
        }
        /// <summary>
        /// Returns the appropriate type definition for the declaring type reference
        /// </summary>
        /// <returns></returns>
        public TypeDefinition GetDeclaringTypeDefinition()
        {
            return UnderlyingField.DeclaringType.Resolve();
        }
        /// <summary>
        /// Returns the appropriate property definition for the underlying property reference
        /// </summary>
        /// <returns></returns>
        public FieldDefinition GetFieldDefinition()
        {
            return UnderlyingField.Resolve();
        }

        FieldDefinition namesakefield;
        /// <summary>
        /// Returns the equivalent object to this object from the NamesakeAssembly
        /// </summary>
        /// <returns></returns>
        public override dynamic GetNamesake()
        {
            if (namesakefield != null)
                return namesakefield;

            TypeDefinition decKindNamesake = DeclaringKind.GetNamesake();
            if (decKindNamesake != null)
            {
                foreach (FieldDefinition field in decKindNamesake.Fields)
                {
                    if (Utility.AreNamesakes(field, UnderlyingField))
                    {
                        namesakefield = field;
                        break;
                    }
                }
                return namesakefield;
            }

            return null;
        }
    }
}
