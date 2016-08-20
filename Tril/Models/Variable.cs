using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Tril.Models
{
    /// <summary>
    /// Represents a local variable
    /// </summary>
    [Serializable]
    public partial class Variable
    {
        /// <summary>
        /// Gets the name of this Tril.Models.Variable instance.
        /// The name is in that format: 
        /// "V_" followed by the local index of this variable;
        /// </summary>
        public string GetName()
        {
            if (_name == "")
            {
                if (UnderlyingVariable != null)
                    _name = "V_" + UnderlyingVariable.Index;
                else
                    _name = "V_"; //name must always containg "V_"
            }

            return _name;
        }
        /// <summary>
        /// Gets the name of this Tril.Models.Variable instance.
        /// The name is in that format: 
        /// The name of the variable kind followed by "V_" followed by the local index of this variable;
        /// </summary>
        public string GetName(bool useDefaultOnly, params string[] targetPlats)
        {
            if (_name == "")
            {
                //if (VariableKind != null && VariableKind.GetName(useDefaultOnly, targetPlats) != null && UnderlyingVariable != null)
                //{
                //    string typeSec = "";
                //    typeSec = VariableKind.GetName(useDefaultOnly, targetPlats);
                //    typeSec = Abbreviate(typeSec);
                //    _name = typeSec + "V_" + UnderlyingVariable.LocalIndex; 
                //}
                //else if (VariableKind != null && VariableKind.GetName(useDefaultOnly, targetPlats) != null)
                //{
                //    string typeSec = "";
                //    typeSec = VariableKind.GetName(useDefaultOnly, targetPlats);
                //    typeSec = Abbreviate(typeSec);
                //    _name = typeSec + "V_"; //name must always containg "V_" 
                //}
                //else 
                if (UnderlyingVariable != null)
                    _name = "V_" + UnderlyingVariable.Index;
                else
                    _name = "V_"; //name must always containg "V_"
            }

            return _name;
        }

        /// <summary>
        /// Gets the Tril.Models.Method that owns this variable
        /// </summary>
        public Method DeclaringMethod
        {
            get { return _declaringMethod; }
        }
        /// <summary>
        /// Gets the Kind of this code.
        /// </summary>
        /// <returns></returns>
        public Kind VariableKind
        {
            get
            {
                return Kind.GetCachedKind(UnderlyingVariable.VariableType);
            }
        }
        /// <summary>
        /// Gets the underlying Mono.Cecil.Cil.VariableReference from which this instance of Tril.Models.Variable was built
        /// </summary>
        public VariableReference UnderlyingVariable
        {
            get { return _underlyingVar; }
        }
    }
}
