using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using Tril.Utilities;

namespace Tril.Models
{
    public partial class Method
    {
        static Dictionary<string, Method> cachedMethods = new Dictionary<string, Method>();

        bool? _isEventMethod = null;
        bool? _isOperatorMethod = null;
        bool? _isPropertyMethod = null;
        EventDefinition _declaringEvent;
        PropertyDefinition _declaringProp;

        /// <summary>
        /// Creates a new instance of Tril.Models.Method
        /// </summary>
        /// <param name="methodBase"></param>
        private Method(MethodReference methodBase)
            : base(methodBase) { }

        /// <summary>
        /// Returns either a cached Method if the specified MethodReference has been used before, otherwise a new Method
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Method GetCachedMethod(MethodReference method)
        {
            return new Method(method);

            #region caching (produces undebuggable "An element with the same key already exists" errors sometimes)
            //if (method.FullName == null || method.FullName == "")
            //    return new Method(method);

            //string key = method.FullName + "," + method.Module.FullyQualifiedName;

            //if (cachedMethods.Keys.Contains(key))//.ContainsKey(key))
            //    return cachedMethods[key];
            //else
            //{
            //    Method mthd = new Method(method);
            //    cachedMethods.Add(key, mthd);
            //    return mthd;
            //}
            #endregion
        }
        /// <summary>
        /// Returns the appropriate type definition for the declaring type reference
        /// </summary>
        /// <returns></returns>
        public TypeDefinition GetDeclaringTypeDefinition()
        {
            return UnderlyingMethod.DeclaringType.Resolve();
        }
        /// <summary>
        /// Returns the appropriate method definition for the underlying method reference
        /// </summary>
        /// <returns></returns>
        public MethodDefinition GetMethodDefinition()
        {
            return UnderlyingMethod.Resolve();
        }

        MethodDefinition namesakemethod;
        /// <summary>
        /// Returns the equivalent object to this object from the NamesakeAssembly
        /// </summary>
        /// <returns></returns>
        public override dynamic GetNamesake()
        {
            if (namesakemethod != null)
                return namesakemethod;

            TypeDefinition decKindNamesake = DeclaringKind.GetNamesake();
            if (decKindNamesake != null) 
            {
                foreach (MethodDefinition method in decKindNamesake.Methods) 
                {
                    if (Utility.AreNamesakes(method, UnderlyingMethod))
                    {
                        namesakemethod = method;
                        break;
                    }
                }
                return namesakemethod;
            }

            return null;
        }
    }
}
