using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using Tril.Utilities;

namespace Tril.Models
{
    public partial class Parameter
    {
        static Dictionary<string, Parameter> cachedParameters = new Dictionary<string, Parameter>();

        Method _declaringMethod;

        /// <summary>
        /// Creates a new instance of Tril.Models.Parameter
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="method"></param>
        private Parameter(ParameterReference parameter, Method method)
            : base(parameter)
        {
            if (method == null)
                throw new NullReferenceException("Parameter's declaring method cannot be null!");

            _declaringMethod = method;
        }

        /// <summary>
        /// Returns either a cached Parameter if the specified ParameterReference has been used before, otherwise a new Parameter
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Parameter GetCachedParameter(ParameterReference parameter, Method method)
        {
            return new Parameter(parameter, method);

            #region caching (produces undebuggable "An element with the same key already exists" errors sometimes)
            //if (parameter.Name == null || parameter.Name == "" ||
            //    method.UnderlyingMethod.FullName == null || method.UnderlyingMethod.FullName == "")
            //    return new Parameter(parameter, method);

            //string key = parameter.Name + "," + method.UnderlyingMethod.FullName + method.UnderlyingMethod.Module.FullyQualifiedName;

            //if (cachedParameters.Keys.Contains(key))//.ContainsKey(key))
            //    return cachedParameters[key];
            //else
            //{
            //    Parameter param = new Parameter(parameter, method);
            //    cachedParameters.Add(key, param);
            //    return param;
            //}
            #endregion
        }
        /// <summary>
        /// Returns the appropriate parameter definition for the underlying parameter reference
        /// </summary>
        /// <returns></returns>
        public ParameterDefinition GetParameterDefinition()
        {
            return UnderlyingParameter.Resolve();
        }

        ParameterDefinition namesakeparameter;
        /// <summary>
        /// Returns the equivalent object to this object from the NamesakeAssembly
        /// </summary>
        /// <returns></returns>
        public override dynamic GetNamesake()
        {
            if (namesakeparameter != null)
                return namesakeparameter;

            MethodDefinition decMthdNamesake = DeclaringMethod.GetNamesake();
            if (decMthdNamesake != null)
            {
                try
                {
                    namesakeparameter = decMthdNamesake.Parameters[UnderlyingParameter.Index];
                }
                catch { }
                return namesakeparameter;
            }

            return null;
        }
    }
}
