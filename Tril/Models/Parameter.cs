using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using Tril.Utilities;

namespace Tril.Models
{
    /// <summary>
    /// Represents a parameter in a Tril.Models.Method
    /// </summary>
    [Serializable]
    public partial class Parameter : Model
    {
        /// <summary>
        /// Returns the default attributes
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        protected override string[] GetAttributes_Default(bool useDefaultOnly, params string[] targetPlats)
        {
            List<string> attris = new List<string>();

            var ParamDef = GetParameterDefinition();
            if (ParamDef != null)
            {
                if (IsVarArg)
                    attris.Add("params");
                else if (ParamDef.IsOut)
                    attris.Add("out");
                else if (!ParamDef.IsOut && GetLongName(useDefaultOnly, targetPlats).Contains("&"))
                    attris.Add("ref");

                //_ParamDef.IsIn;
                //ParamDef.IsLcid;
                //ParamDef.IsReturnValue;
            }

            return attris.ToArray();
        }
        /// <summary>
        /// Returns the long name of this model.
        /// For a Tril.Models.Parameter, this is the long name of the parameter Kind (or the name if the parameter Kind is a generic place holder)
        /// plus space ( ) plus the GetName.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public override string GetLongName(bool useDefaultOnly, params string[] targetPlats)
        {
            string paramTypeStr = "";
            if (HasUserDefinedParameterKind(targetPlats) && !useDefaultOnly) //this means the return type would be returned as a string
            {
                paramTypeStr = GetParameterKind(useDefaultOnly, targetPlats);
            }
            else //this means the return type would be returned as a Tril.Kind
            {
                Kind paramType = GetParameterKind(useDefaultOnly, targetPlats);
                if (paramType != null)
                {
                    if (paramType.IsPlaceHolderGenericParameter)
                        paramTypeStr = paramType.GetName(useDefaultOnly, targetPlats);
                    else
                        paramTypeStr = paramType.GetLongName(useDefaultOnly, targetPlats);
                }
            }

            return paramTypeStr.Trim() + " " + GetName(useDefaultOnly, targetPlats).Trim();
        }
        /// <summary>
        /// Returns the default name
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        protected override string GetName_Default(bool useDefaultOnly, params string[] targetPlats)
        {
            string _name = UnderlyingParameter.Name;

            _name = ReplaceInvalidChars(_name);

            return _name;
        }
        /// <summary>
        /// Gets the parameter type of this Tril.Models.Parameter instance.
        /// If there is no user-defined parameter type, the parameter type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined parameter type, it is returned as a System.String object.
        /// </summary>
        /// <returns></returns>
        public dynamic GetParameterKind()
        {
            return GetParameterKind("*");
        }
        /// <summary>
        /// Gets the parameter type of this Tril.Models.Parameter instance.
        /// If there is no user-defined parameter type, the parameter type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined parameter type, it is returned as a System.String object.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public dynamic GetParameterKind(bool useDefaultOnly)
        {
            return GetParameterKind(useDefaultOnly, "*");
        }
        /// <summary>
        /// Gets the parameter type of this Tril.Models.Parameter instance.
        /// If there is no user-defined parameter type, the parameter type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined parameter type, it is returned as a System.String object.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetParameterKind(params string[] targetPlats)
        {
            return GetParameterKind(false, targetPlats);
        }
        /// <summary>
        /// Gets the parameter type of this Tril.Models.Parameter instance.
        /// If there is no user-defined parameter type, the parameter type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined parameter type, it is returned as a System.String object.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetParameterKind(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedParameterKind(targetPlats) && !useDefaultOnly)
            {
                var retTypeAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ParamTypeAttribute");
                foreach (CustomAttribute attri in retTypeAttris)
                {
                    string[] tgtPlatsVal = GetTargetPlatforms(attri);
                    foreach (string lang in targetPlats)
                    {
                        if (tgtPlatsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                        {
                            return attri.ConstructorArguments[0].Value.ToString();
                        }
                    }
                }
            }

            if (UnderlyingParameter.ParameterType != null)
            {
                return Kind.GetCachedKind(UnderlyingParameter.ParameterType);
            }

            return null;
        }
        /// <summary>
        /// Gets a raw collection of all the attributes (without sugar-coating anything).
        /// </summary>
        /// <returns></returns>
        public override string[] GetRawAttributes()
        {
            List<string> attris = new List<string>();

            var ParamDef = GetParameterDefinition();
            if (ParamDef != null)
            {
                //if (IsVarArg)
                //    attris.Add("params");
                //else 
                if (ParamDef.IsOut)
                    attris.Add("out");

                //_ParamDef.IsIn;
                //ParamDef.IsLcid;
                //ParamDef.IsReturnValue;
            }

            return attris.ToArray();
        }
        /// <summary>
        /// Gets a value indicating whether the parameter type of this Parameter instance was defined by the user using 
        /// Tril.Attributes.ParamTypeAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedParameterKind()
        {
            return HasUserDefinedParameterKind("*");
        }
        /// <summary>
        /// Gets a value indicating whether the parameter type of this Parameter instance was defined by the user using 
        /// Tril.Attributes.ParamTypeAttribute
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public bool HasUserDefinedParameterKind(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var retAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ParamTypeAttribute");

            return HasUserDefined(retAttris, targetPlats);
        }

        /// <summary>
        /// Gets the Tril.Models.Method that owns this parameter
        /// </summary>
        public Method DeclaringMethod
        {
            get { return _declaringMethod; }
        }
        /// <summary>
        /// Gets a value indicating the default value if the parameter has a constant value
        /// </summary>
        public object ConstantValue
        {
            get 
            {
                var ParamDef = GetParameterDefinition();
                if (ParamDef != null) 
                {
                    return ParamDef.Constant;
                }
                return null;
            }
        }
        ///// <summary>
        ///// Gets a value indicating the default value if the parameter has a default value
        ///// </summary>
        //public object DefaultValue
        //{
        //    get
        //    {
        //        var ParamDef = GetParameterDefinition();
        //        if (ParamDef != null)
        //        {
        //            return ParamDef.??;
        //        }
        //        return null;
        //    }
        //}
        /// <summary>
        /// Gets a value indicating whether the parameter has a constant value
        /// </summary>
        public bool HasConstantValue
        {
            get
            {
                var ParamDef = GetParameterDefinition();
                if (ParamDef != null)
                {
                    return ParamDef.HasConstant;
                }
                return false;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the parameter has a default value
        /// </summary>
        public bool HasDefaultValue
        {
            get
            {
                var ParamDef = GetParameterDefinition();
                if (ParamDef != null)
                {
                    return ParamDef.HasDefault;
                }
                return false;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the parameter is optional
        /// </summary>
        public bool IsOptional
        {
            get
            {
                var ParamDef = GetParameterDefinition();
                if (ParamDef != null)
                {
                    return ParamDef.IsOptional;
                }
                return false;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the parameter is a variable arg
        /// </summary>
        public bool IsVarArg
        {
            get
            {
                return GetAllCustomAttributes().Any(a => a != null && a.AttributeType.FullName == "System.ParamArrayAttribute");
            }
        }
        /// <summary>
        /// Gets the underlying Mono.Cecil.ParameterReference from which this instance of Tril.Models.Parameter was built
        /// </summary>
        public ParameterReference UnderlyingParameter
        {
            get { return ((ParameterReference)_underlyingInfo); }
        }
    }
}
