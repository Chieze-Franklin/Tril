using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Tril.Models
{
    public partial class Variable
    {
        static Dictionary<string, Variable> cachedVariables = new Dictionary<string, Variable>();

        Method _declaringMethod;
        VariableReference _underlyingVar;
        string _name = "";

        /// <summary>
        /// Creates a new instance of Tril.Models.Variable
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="method"></param>
        private Variable(VariableReference variable, Method method)
        {
            if (method == null)
                throw new NullReferenceException("Variable's method cannot be null!");
            if (variable == null)
                throw new NullReferenceException("Variable underlying type cannot be null!");

            _declaringMethod = method;
            _underlyingVar = variable;
        }

        /// <summary>
        /// Returns either a cached Variable if the specified VariableReference has been used before, otherwise a new Variable
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Variable GetCachedVariable(VariableReference variable, Method method)
        {
            return new Variable(variable, method);

            #region caching (produces undebuggable "An element with the same key already exists" errors sometimes)
            //if (variable.Name == null || variable.Name == "" ||
            //    method.UnderlyingMethod.FullName == null || method.UnderlyingMethod.FullName == "")
            //    return new Variable(variable, method);

            //string key = variable.Name + "," + method.UnderlyingMethod.FullName + method.UnderlyingMethod.Module.FullyQualifiedName;

            //if (cachedVariables.Keys.Contains(key))//.ContainsKey(key))
            //    return cachedVariables[key];
            //else
            //{
            //    Variable varr = new Variable(variable, method);
            //    cachedVariables.Add(key, varr);
            //    return varr;
            //}
            #endregion
        }

        string Abbreviate(string original)
        {
            #region Method1
            //original = original.Replace(" ", "").Replace(">", "");//.ToLower();

            ////remove all vowels, except the first char and those after "<" or "," chars
            ////

            ////replacements
            //original = original.Replace("<", "Of").Replace(",", "And").Replace("[]", "Array");
            #endregion

            #region Method2
            original = original.Replace(" ", "").Replace(",", "").Replace("[]", "Arr");//.ToLower();

            //remove generic part
            original = original.Replace('>', '<');
            //by changing '>' to '<' 
            //(1) I check for only '<' in the condition below
            //(2) I dont care if '>' appears before '<'
            if (original.Contains('<'))
                original = original.Remove(original.IndexOf('<'), (original.LastIndexOf('<') + 1) - original.IndexOf('<'));

            //shorten name
            if (original.Contains("Arr"))
            {
                if (original.Length > 3)
                    original = original.Substring(0, 3) + "Arr";
            }
            else
            {
                if (original.Length > 3)
                    original = original.Substring(0, 3);
            }
            #endregion
            return original;
        }
        /// <summary>
        /// Returns the string equivalent of this variable (i.e. GetName())
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetName();
        }
    }
}
