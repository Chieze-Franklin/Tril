using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using Tril.Attributes;
using Tril.Utilities;

namespace Tril.Models
{
    /// <summary>
    /// Represents a method in a Tril.Models.Kind
    /// </summary>
    [Serializable]
    public partial class Method : Member
    {
        /// <summary>
        /// Returns the default access modifiers
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        protected override string[] GetAccessModifiers_Default(bool useDefaultOnly, params string[] targetPlats)
        {
            List<string> accMods = new List<string>();

            var MethodDef = GetMethodDefinition();
            if (MethodDef != null)
            {
                //public or not public
                if (MethodDef.IsPublic)
                    accMods.Add("public");
                else if (MethodDef.IsPrivate)
                    accMods.Add("private");
                else if (MethodDef.IsAssembly)
                    accMods.Add("internal");
                else if (MethodDef.IsFamily)
                    accMods.Add("protected");
                else if (MethodDef.IsFamilyAndAssembly)
                    accMods.Add("internal");
                else if (MethodDef.IsFamilyOrAssembly)
                    accMods.Add("protected internal");
            }

            return accMods.ToArray();
        }
        /// <summary>
        /// Returns the default attributes
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        protected override string[] GetAttributes_Default(bool useDefaultOnly, params string[] targetPlats)
        {
            List<string> attris = new List<string>();

            var MethodDef = GetMethodDefinition();
            if (MethodDef != null)
            {
                if (MethodDef.IsStatic)
                    attris.Add("static");
                else if (MethodDef.IsAbstract)
                    attris.Add("abstract");
                else if (MethodDef.IsFinal)
                    attris.Add("sealed");
                else if (MethodDef.IsVirtual && MethodDef.IsNewSlot)
                    attris.Add("virtual");
                else if (MethodDef.IsVirtual)
                    attris.Add("override");
                //else if (MethodDef.HasOverrides)//(MethodDef.Overrides.Count > 0)
                //    attris.Add("override");
            }

            return attris.ToArray();
        }
        /// <summary>
        /// Returns an array of all the runtime generic arguments of this Method instance.
        /// </summary>
        /// <returns></returns>
        public Kind[] GetGenericArguments()
        {
            List<Kind> genericParams = new List<Kind>();

            if (IsGenericInstance)
            {
                var genericInstance = (GenericInstanceMethod)UnderlyingMethod;
                foreach (TypeReference genericArg in genericInstance.GenericArguments)
                {
                    if (genericArg != null)
                    {
                        Kind genericKindParam = Kind.GetCachedKind(genericArg);

                        genericParams.Add(genericKindParam);
                    }
                }
            }

            return genericParams.ToArray();
        }
        /// <summary>
        /// Returns the generic argument section.
        /// </summary>
        /// <returns></returns>
        public string GetGenericArgumentsSection_CsStyle()
        {
            return GetGenericArgumentsSection_CsStyle("*");
        }
        /// <summary>
        /// Returns the generic argument section.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string GetGenericArgumentsSection_CsStyle(bool useDefaultOnly)
        {
            return GetGenericArgumentsSection_CsStyle(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns the generic argument section.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetGenericArgumentsSection_CsStyle(params string[] targetPlats)
        {
            return GetGenericArgumentsSection_CsStyle(false, targetPlats);
        }
        /// <summary>
        /// Returns the generic argument section.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetGenericArgumentsSection_CsStyle(bool useDefaultOnly, params string[] targetPlats)
        {
            string argSec = "";
            Kind[] genericArgs = GetGenericArguments();
            //generic params------------------------------------------------------
            if (genericArgs.Length > 0)
            {
                argSec = "<";
                foreach (Kind genericParam in genericArgs)
                {
                    argSec += Utility.GetAppropriateLongName(genericParam, useDefaultOnly, targetPlats);
                    if (genericParam.IsGenericInstance)
                    {
                        argSec += genericParam.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats);
                    }
                    argSec += ", ";
                }
                argSec = argSec.TrimEnd(' ');
                argSec = argSec.TrimEnd(',');
                argSec += ">";
            }

            return argSec;
        }
        /// <summary>
        /// Returns the generic constaint section.
        /// </summary>
        /// <returns></returns>
        public string GetGenericConstraintsSection_CsStyle()
        {
            return GetGenericConstraintsSection_CsStyle("*");
        }
        /// <summary>
        /// Returns the generic constaint section.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string GetGenericConstraintsSection_CsStyle(bool useDefaultOnly)
        {
            return GetGenericConstraintsSection_CsStyle(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns the generic constaint section.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetGenericConstraintsSection_CsStyle(params string[] targetPlats)
        {
            return GetGenericConstraintsSection_CsStyle(false, targetPlats);
        }
        /// <summary>
        /// Returns the generic constaint section.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetGenericConstraintsSection_CsStyle(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedGenericConstraintsSection(targetPlats) && !useDefaultOnly)
            {
                var genAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MemberGenConstraintsSecAttribute");
                foreach (CustomAttribute attri in genAttris)
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

            string consSec = "";
            Kind[] genericParams = GetGenericParameters();
            foreach (Kind genericParam in genericParams)
            {
                if (genericParam.IsGenericParameter)
                {
                    if (genericParam.GetGenericConstraints(useDefaultOnly, targetPlats) is string[])
                    {
                        string[] genericParamsCons = genericParam.GetGenericConstraints(useDefaultOnly, targetPlats);
                        if (genericParamsCons.Length > 0)
                        {
                            consSec += " where " + Utility.GetAppropriateLongName(genericParam, useDefaultOnly, targetPlats) + " : ";
                            for (int index = 0; index < genericParamsCons.Length; index++)
                            {
                                if (genericParamsCons[index].Trim() != "")
                                {
                                    //consSec += genericParamsCons[index].Replace("<extends>", "").Replace("<implements>", "");
                                    if (genericParamsCons[index].StartsWith("<extends>"))
                                    {
                                        consSec += genericParamsCons[index].Substring(9, genericParamsCons[index].Length - 9);
                                    }
                                    else if (genericParamsCons[index].StartsWith("<implements>"))
                                    {
                                        consSec += genericParamsCons[index].Substring(12, genericParamsCons[index].Length - 12);
                                    }
                                    else
                                    {
                                        consSec += genericParamsCons[index];
                                    }
                                    consSec += ", ";
                                }
                            }
                            consSec = consSec.TrimEnd(' ');
                            consSec = consSec.TrimEnd(',');
                        }
                    }
                    else
                    {
                        Kind[] genericParamsCons = genericParam.GetGenericConstraints(useDefaultOnly, targetPlats);
                        if (genericParamsCons.Length > 0)
                        {
                            consSec += " where " + Utility.GetAppropriateLongName(genericParam, useDefaultOnly, targetPlats) + " : ";
                            for (int index = 0; index < genericParamsCons.Length; index++)
                            {
                                Kind constraint = genericParamsCons[index];
                                consSec += Utility.GetAppropriateLongName(constraint, useDefaultOnly, targetPlats);
                                if (constraint.IsGenericInstance)
                                {
                                    Kind[] constraintArgs = constraint.GetGenericArguments();
                                    if (constraintArgs.Length > 0)
                                    {
                                        consSec += constraint.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats);
                                    }
                                }
                                else if (constraint.IsGenericDefinition)
                                {
                                    Kind[] constraintParams = constraint.GetGenericParameters();
                                    if (constraintParams.Length > 0)
                                    {
                                        consSec += constraint.GetGenericParametersSection_CsStyle(useDefaultOnly, targetPlats);
                                    }
                                }
                                consSec += ", ";
                            }
                            consSec = consSec.TrimEnd(' ');
                            consSec = consSec.TrimEnd(',');
                        }
                    }
                }
            }

            return consSec.Trim();
        }
        /// <summary>
        /// Returns an array of all the generic parameters of the definition for this Method instance.
        /// </summary>
        /// <returns></returns>
        public Kind[] GetGenericParameters()
        {
            List<Kind> genericParams = new List<Kind>();

            if (IsGenericDefinition)
            {
                //var genericInstance = (GenericInstanceMethod)UnderlyingMethod;
                //foreach (TypeReference genericParam in genericInstance.GenericParameters)//UnderlyingMethod.GenericParameters)
                //{
                //    if (genericParam != null)
                //    {
                //        Kind genericKindParam = Kind.GetCachedKind(genericParam);

                //        genericParams.Add(genericKindParam);
                //    }
                //}
                var MethodDef = GetMethodDefinition();
                if (MethodDef != null)
                {
                    foreach (TypeReference genericParam in MethodDef.GenericParameters)
                    {
                        if (genericParam != null)
                        {
                            Kind genericKindParam = Kind.GetCachedKind(genericParam);

                            genericParams.Add(genericKindParam);
                        }
                    }
                }
            }

            return genericParams.ToArray();
        }
        /// <summary>
        /// Returns the generic parameter section.
        /// </summary>
        /// <returns></returns>
        public string GetGenericParametersSection_CsStyle()
        {
            return GetGenericParametersSection_CsStyle("*");
        }
        /// <summary>
        /// Returns the generic parameter section.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string GetGenericParametersSection_CsStyle(bool useDefaultOnly)
        {
            return GetGenericParametersSection_CsStyle(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns the generic parameter section.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetGenericParametersSection_CsStyle(params string[] targetPlats)
        {
            return GetGenericParametersSection_CsStyle(false, targetPlats);
        }
        /// <summary>
        /// Returns the generic parameter section.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetGenericParametersSection_CsStyle(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedGenericParametersSection(targetPlats) && !useDefaultOnly)
            {
                var genAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MemberGenParamsSecAttribute");
                foreach (CustomAttribute attri in genAttris)
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

            string paramSec = "";
            Kind[] genericParams = GetGenericParameters();
            //generic params------------------------------------------------------
            if (genericParams.Length > 0)
            {
                paramSec = "<";
                foreach (Kind genericParam in genericParams)
                {
                    paramSec += Utility.GetAppropriateLongName(genericParam, useDefaultOnly, targetPlats);
                    if (genericParam.IsGenericInstance)
                    {
                        paramSec += genericParam.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats);
                    }
                    else if (genericParam.IsGenericDefinition)
                    {
                        paramSec += genericParam.GetGenericParametersSection_CsStyle(useDefaultOnly, targetPlats);
                    }
                    paramSec += ", ";
                }
                paramSec = paramSec.TrimEnd(' ');
                paramSec = paramSec.TrimEnd(',');
                paramSec += ">";
            }

            return paramSec;
        }
        /// <summary>
        /// Returns the long name of this model.
        /// For a Tril.Models.Method, this is the long name of the declaring Kind plus two colons (::) plus the GetName.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public override string GetLongName(bool useDefaultOnly, params string[] targetPlats)
        {
            return DeclaringKind.GetLongName(useDefaultOnly, targetPlats).Trim() + "::" + GetName(useDefaultOnly, targetPlats).Trim();
        }
        /// <summary>
        /// Returns the default name
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        protected override string GetName_Default(bool useDefaultOnly, params string[] targetPlats)
        {
            string _name = null;

            if (IsConstructor)
            {
                _name = DeclaringKind.GetName(useDefaultOnly, targetPlats);
            }
            else if (IsPropertyMethod)
            {
                PropertyReference underlyingProp = DeclaringProperty.UnderlyingProperty;

                if (underlyingProp != null)
                {
                    Property methodProp = Property.GetCachedProperty(underlyingProp);
                    if ((UnderlyingMethod).Name.StartsWith("get_"))
                    {
                        _name = "get_" + methodProp.GetName(useDefaultOnly, targetPlats);
                    }
                    else if ((UnderlyingMethod).Name.StartsWith("set_"))
                    {
                        _name = "set_" + methodProp.GetName(useDefaultOnly, targetPlats);
                    }
                }
            }
            else if (IsEventMethod)
            {
                EventReference underlyingEvent = DeclaringEvent.UnderlyingEvent;

                if (underlyingEvent != null)
                {
                    Event methodEvnt = Event.GetCachedEvent(underlyingEvent);
                    if ((UnderlyingMethod).Name.StartsWith("add_"))
                    {
                        _name = "add_" + methodEvnt.GetName(useDefaultOnly, targetPlats);
                    }
                    else if ((UnderlyingMethod).Name.StartsWith("remove_"))
                    {
                        _name = "remove_" + methodEvnt.GetName(useDefaultOnly, targetPlats);
                    }
                }
            }
            else
                _name = UnderlyingMethod.Name;

            _name = ReplaceInvalidChars(_name);

            return _name;
        }
        /// <summary>
        /// Returns an array of all the parameters of this Tril.Models.Method instance.
        /// If the generic parameter section is user-defined, a single string representing 
        /// the first matched generic parameter section is returned.
        /// If not, an array of Tril.Models.Parameter objects is returned.
        /// </summary>
        /// <returns></returns>
        public dynamic GetParameters()
        {
            return GetParameters("*");
        }
        /// <summary>
        /// Returns an array of all the parameters of this Tril.Models.Method instance.
        /// If the generic parameter section is user-defined, a single string representing 
        /// the first matched generic parameter section is returned.
        /// If not, an array of Tril.Models.Parameter objects is returned.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public dynamic GetParameters(bool useDefaultOnly)
        {
            return GetParameters(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns an array of all the parameters of this Tril.Models.Method instance.
        /// If the generic parameter section is user-defined, a single string representing 
        /// the first matched generic parameter section is returned.
        /// If not, an array of Tril.Models.Parameter objects is returned.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetParameters(params string[] targetPlats)
        {
            return GetParameters(false, targetPlats);
        }
        /// <summary>
        /// Returns an array of all the parameters of this Tril.Models.Method instance.
        /// If the generic parameter section is user-defined, a single string representing 
        /// the first matched generic parameter section is returned.
        /// If not, an array of Tril.Models.Parameter objects is returned.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetParameters(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedParameterSection(targetPlats) && !useDefaultOnly)
            {
                var genAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ParamsSecAttribute");
                foreach (CustomAttribute attri in genAttris)
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

            List<Parameter> paramz = new List<Parameter>();

            foreach (ParameterReference param in UnderlyingMethod.Parameters)
            {
                Parameter p = Parameter.GetCachedParameter(param, this);

                paramz.Add(p);
            }

            return paramz.ToArray();
        }
        /// <summary>
        /// Returns a string representing the pre-body section of this Method instance.
        /// If this Method instance has user-defined pre-body sections,
        /// the method returns a string representing the first matched pre-body section of this Method instance.
        /// </summary>
        /// <returns></returns>
        public string GetPreBodySection()
        {
            return GetPreBodySection("*");
        }
        /// <summary>
        /// Returns a string representing the pre-body section of this Method instance.
        /// If this Method instance has user-defined pre-body sections,
        /// the method returns a string representing the first matched pre-body section of this Method instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string GetPreBodySection(bool useDefaultOnly)
        {
            return GetPreBodySection(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns a string representing the pre-body section of this Method instance.
        /// If this Method instance has user-defined pre-body sections,
        /// the method returns a string representing the first matched pre-body section of this Method instance.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetPreBodySection(params string[] targetPlats)
        {
            return GetPreBodySection(false, targetPlats);
        }
        /// <summary>
        /// Returns a string representing the pre-body section of this Method instance.
        /// If this Method instance has user-defined pre-body sections,
        /// the method returns a string representing the first matched pre-body section of this Method instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetPreBodySection(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedPreBodySection(targetPlats) && !useDefaultOnly)
            {
                var preAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MemberPreBodyAttribute");
                foreach (CustomAttribute attri in preAttris)
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

                string throwString = "";
                var throwsAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ThrowsAttribute");
                foreach (CustomAttribute attri in throwsAttris)
                {
                    string[] tgtLangsVal = GetTargetPlatforms(attri);
                    foreach (string lang in targetPlats)
                    {
                        if (tgtLangsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                        {
                            if (attri.ConstructorArguments[0].Value is TypeReference) 
                            {
                                TypeReference typeToThrow = ((TypeReference)attri.ConstructorArguments[0].Value);
                                throwString += Utility.GetAppropriateLongName(Kind.GetCachedKind(typeToThrow), useDefaultOnly, targetPlats) + ", ";
                                break;
                            }
                        }
                    }
                }
                throwString = throwString.TrimEnd(' ').TrimEnd(',');
                if (throwString.Trim() != "")
                    return "throws " + throwString;
            }

            return "";
        }
        /// <summary>
        /// Gets a raw collection of all the attributes (without sugar-coating anything).
        /// </summary>
        /// <returns></returns>
        public override string[] GetRawAttributes() 
        {
            List<string> attris = new List<string>();

            var MethodDef = GetMethodDefinition();
            if (MethodDef != null)
            {
                if (MethodDef.IsHideBySig)
                    attris.Add("hidebysig");

                if (MethodDef.IsSpecialName)
                    attris.Add("specialname");

                if (MethodDef.IsRuntimeSpecialName)
                    attris.Add("rtspecialname");

                if (MethodDef.IsStatic)
                    attris.Add("static");
                else if (MethodDef.IsAbstract)
                    attris.Add("abstract");
                else if (MethodDef.IsFinal)
                    attris.Add("final");

                if (MethodDef.IsNewSlot)
                    attris.Add("newslot");

                if (MethodDef.IsVirtual)
                    attris.Add("virtual");
            }

            return attris.ToArray();
        }
        /// <summary>
        /// Returns the return type of this Tril.Models.Method instance.
        /// If there is no user-defined return type, the return type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined return type (specified using the Tril.Attributes.MemberTypeAttribute atttribute), 
        /// it is returned as a System.String object.
        /// </summary>
        /// <returns></returns>
        public dynamic GetReturnKind()
        {
            return GetReturnKind("*");
        }
        /// <summary>
        /// Returns the return type of this Tril.Models.Method instance.
        /// If there is no user-defined return type, the return type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined return type (specified using the Tril.Attributes.MemberTypeAttribute atttribute), 
        /// it is returned as a System.String object.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public dynamic GetReturnKind(bool useDefaultOnly)
        {
            return GetReturnKind(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns the return type of this Tril.Models.Method instance.
        /// If there is no user-defined return type, the return type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined return type (specified using the Tril.Attributes.MemberTypeAttribute atttribute), 
        /// it is returned as a System.String object.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetReturnKind(params string[] targetPlats)
        {
            return GetReturnKind(false, targetPlats);
        }
        /// <summary>
        /// Returns the return type of this Tril.Models.Method instance.
        /// If there is no user-defined return type, the return type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined return type (specified using the Tril.Attributes.MemberTypeAttribute atttribute), 
        /// it is returned as a System.String object.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetReturnKind(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedReturnKind(targetPlats) && !useDefaultOnly)
            {
                var retTypeAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MemberTypeAttribute");
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

            var MethodDef = GetMethodDefinition();
            if (MethodDef != null)
            {
                if (MethodDef.IsConstructor)
                    return null;
            }
            var retType = UnderlyingMethod.ReturnType;
            if (retType != null) 
            {
                return Kind.GetCachedKind(retType);
            }

            return null;
        }
        /// <summary>
        /// Returns the signature of this method.
        /// This is the GetName plus the parameters section plus a colon (:) plus the return kind.
        /// </summary>
        /// <returns></returns>
        public string GetShortSignature_CsStyle()
        {
            return GetShortSignature_CsStyle("*");
        }
        /// <summary>
        /// Returns the signature of this method.
        /// This is the GetName plus the parameters section plus a colon (:) plus the return kind.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string GetShortSignature_CsStyle(bool useDefaultOnly)
        {
            return GetShortSignature_CsStyle(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns the signature of this method.
        /// This is the GetName plus the parameters section plus a colon (:) plus the return kind.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetShortSignature_CsStyle(params string[] targetPlats)
        {
            return GetShortSignature_CsStyle(false, targetPlats);
        }
        /// <summary>
        /// Returns the signature of this method.
        /// This is the GetName plus the parameters section plus a colon (:) plus the return kind.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetShortSignature_CsStyle(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            string returnKindSection = "";
            dynamic retKind = GetReturnKind(useDefaultOnly, targetPlats);
            if (retKind != null)
            {
                if (retKind is string)
                    returnKindSection = ":" + retKind;
                else if (retKind is Kind)
                {
                    Kind retKindObj = ((Kind)retKind);
                    returnKindSection = Utility.GetAppropriateLongName(retKindObj, useDefaultOnly, targetPlats);
                    if (retKindObj.IsGenericInstance)
                        returnKindSection += retKindObj.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats);
                    else if (retKindObj.IsGenericDefinition)
                        returnKindSection += retKindObj.GetGenericParametersSection_CsStyle(useDefaultOnly, targetPlats);
                    returnKindSection = ":" + returnKindSection;
                }
            }

            string paramSection = "";
            foreach (Parameter param in GetParameters(true))
            {
                string paramLongName = param.GetLongName(useDefaultOnly, targetPlats);
                if (paramLongName != null && paramLongName.Trim() != "")
                {
                    //I'm not interested in the param name here, so I remove it
                    if (paramLongName.Contains(' '))
                    {
                        paramLongName = paramLongName.Substring(0, paramLongName.LastIndexOf(' '));
                    }

                    paramSection += paramLongName.Trim() + ",";
                }
            }
            paramSection = paramSection.TrimEnd(',');

            return GetName(useDefaultOnly, targetPlats) +
                GetGenericParametersSection_CsStyle(useDefaultOnly, targetPlats) + "(" + paramSection + ")" + returnKindSection;
        }
        /// <summary>
        /// Returns the signature of this method.
        /// This is the return kind plus the long name of the declaring Kind plus two colons (::) plus the GetName plus the parameters section.
        /// </summary>
        /// <returns></returns>
        public string GetSignature_CsStyle()
        {
            return GetSignature_CsStyle("*");
        }
        /// <summary>
        /// Returns the signature of this method.
        /// This is the return kind plus the long name of the declaring Kind plus two colons (::) plus the GetName plus the parameters section.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string GetSignature_CsStyle(bool useDefaultOnly)
        {
            return GetSignature_CsStyle(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns the signature of this method.
        /// This is the return kind plus the long name of the declaring Kind plus two colons (::) plus the GetName plus the parameters section.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetSignature_CsStyle(params string[] targetPlats)
        {
            return GetSignature_CsStyle(false, targetPlats);
        }
        /// <summary>
        /// Returns the signature of this method.
        /// This is the return kind plus the long name of the declaring Kind plus two colons (::) plus the GetName plus the parameters section.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetSignature_CsStyle(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            string returnKindSection = "";
            dynamic retKind = GetReturnKind(useDefaultOnly, targetPlats);
            if (retKind != null)
            {
                if (retKind is string)
                    returnKindSection = retKind + " ";
                else if (retKind is Kind)
                {
                    Kind retKindObj = (retKind as Kind);
                    returnKindSection = Utility.GetAppropriateLongName(retKindObj, useDefaultOnly, targetPlats);
                    if (retKindObj.IsGenericInstance)
                        returnKindSection += retKindObj.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats);
                    else if (retKindObj.IsGenericDefinition)
                        returnKindSection += retKindObj.GetGenericParametersSection_CsStyle(useDefaultOnly, targetPlats);
                    returnKindSection += " ";
                }
            }

            string paramSection = "";
            foreach (Parameter param in GetParameters(true))
            {
                string paramLongName = param.GetLongName(useDefaultOnly, targetPlats);
                if (paramLongName != null && paramLongName.Trim() != "")
                {
                    //I'm not interested in the param name here, so I remove it
                    if (paramLongName.Contains(' '))
                    {
                        paramLongName = paramLongName.Substring(0, paramLongName.LastIndexOf(' '));
                    }

                    paramSection += paramLongName.Trim() + ", ";
                }
            }
            paramSection = paramSection.TrimEnd(' ');
            paramSection = paramSection.TrimEnd(',');

            return returnKindSection + DeclaringKind.GetLongName(useDefaultOnly, targetPlats) + "::" + GetName(useDefaultOnly, targetPlats) +
                GetGenericParametersSection_CsStyle(useDefaultOnly, targetPlats) + "(" + paramSection + ")";
        }
        /// <summary>
        /// Gets a value indicating whether the bodies of this Method instance can be accessed.
        /// If the underlying method has been marked with the [ShowImplementation] attribute, 
        /// this method would return false.
        /// If the underlying methods has been marked with the [HideImplementation] attribute, 
        /// this method would return true.
        /// Otherwise, this method returns false.
        /// </summary>
        /// <returns></returns>
        public bool HasHiddenImplementation()
        {
            return HasHiddenImplementation("*");
        }
        /// <summary>
        /// Gets a value indicating whether the bodies of this Method instance can be accessed.
        /// If the underlying method has been marked with the [ShowImplementation] attribute, 
        /// this method would return false.
        /// If the underlying methods has been marked with the [HideImplementation] attribute, 
        /// this method would return true.
        /// Otherwise, this method returns false.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public bool HasHiddenImplementation(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var showAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ShowImplementationAttribute");
            if (HasUserDefined(showAttris, targetPlats))
                return false;

            var hideAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.HideImplementationAttribute");
            if (HasUserDefined(hideAttris, targetPlats))
                return true;

            if (IsPropertyMethod && DeclaringProperty != null)
            {
                return DeclaringProperty.HasHiddenImplementation(targetPlats);
            }

            if (IsEventMethod && DeclaringEvent != null)
            {
                return DeclaringEvent.HasHiddenImplementation(targetPlats);
            }

            return DeclaringKind.HasHiddenImplementation(targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether the generic constraints section of this Method instance was defined by the user 
        /// using Tril.Attributes.MemberGenConstraintsSecAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedGenericConstraintsSection()
        {
            return HasUserDefinedGenericConstraintsSection("*");
        }
        /// <summary>
        /// Gets a value indicating whether the generic constraints section of this Method instance was defined by the user 
        /// using Tril.Attributes.MemberGenConstraintsSecAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedGenericConstraintsSection(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var genAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MemberGenConstraintsSecAttribute");

            return HasUserDefined(genAttris, targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether the generic parameters section of this Method instance was defined by the user 
        /// using Tril.Attributes.MemberGenParamsSecAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedGenericParametersSection()
        {
            return HasUserDefinedGenericParametersSection("*");
        }
        /// <summary>
        /// Gets a value indicating whether the generic parameters section of this Method instance was defined by the user 
        /// using Tril.Attributes.MemberGenParamsSecAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedGenericParametersSection(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var genAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MemberGenParamsSecAttribute");

            return HasUserDefined(genAttris, targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether the underlying method of thid Method instance
        /// has user-defined code. That is, the underlying method is marked with either the
        /// [CodeAttribute] attribute or the [CodeInsideAttribute] attribute.
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedImplementation()
        {
            return HasUserDefinedImplementation("*");
        }
        /// <summary>
        /// Gets a value indicating whether the underlying method of thid Method instance
        /// has user-defined code. That is, the underlying method is marked with either the
        /// [CodeAttribute] attribute or the [CodeInsideAttribute] attribute.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public bool HasUserDefinedImplementation(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var codeAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.CodeAttribute");
            if (HasUserDefined(codeAttris, targetPlats))
                return true;

            var codeInsAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.CodeInsideAttribute");
            if (HasUserDefined(codeInsAttris, targetPlats))
                return true;

            return false;
        }
        /// <summary>
        /// Gets a value indicating whether the parameter section of this Tril.Models.Method instance was defined by the user 
        /// using Tril.Attributes.ParamsSecAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedParameterSection()
        {
            return HasUserDefinedParameterSection("*");
        }
        /// <summary>
        /// Gets a value indicating whether the parameter section of this Tril.Models.Method instance was defined by the user 
        /// using Tril.Attributes.ParamsSecAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedParameterSection(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var paramsAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ParamsSecAttribute");

            return HasUserDefined(paramsAttris, targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether the pre-body section of this Method instance was defined by the user 
        /// using either the Tril.Attributes.MemberPreBodyAttribute or the Tril.Attributes.ThrowsAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedPreBodySection()
        {
            return HasUserDefinedPreBodySection("*");
        }
        /// <summary>
        /// Gets a value indicating whether the pre-body section of this Method instance was defined by the user 
        /// using either the Tril.Attributes.MemberPreBodyAttribute or the Tril.Attributes.ThrowsAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedPreBodySection(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var preAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MemberPreBodyAttribute");
            if (HasUserDefined(preAttris, targetPlats))
                return true;

            var throwsAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ThrowsAttribute");
            if (HasUserDefined(throwsAttris, targetPlats))
                return true;

            return false;
        }
        /// <summary>
        /// Gets a value indicating whether the return type of this Method instance was defined by the user using Tril.MemberTypeAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedReturnKind()
        {
            return HasUserDefinedReturnKind("*");
        }
        /// <summary>
        /// Gets a value indicating whether the return type of this Method instance was defined by the user using Tril.MemberTypeAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedReturnKind(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var retAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MemberTypeAttribute");

            return HasUserDefined(retAttris, targetPlats);
        }

        /// <summary>
        /// Gets the Kind that owns this method
        /// </summary>
        public Kind DeclaringKind
        {
            get { return Kind.GetCachedKind(UnderlyingMethod.DeclaringType); }
        }
        /// <summary>
        /// Gets the Event from which this method was generated.
        /// Returns null if no such event exists.
        /// </summary>
        public Event DeclaringEvent
        {
            get
            {
                if (_declaringEvent != null)
                    return Event.GetCachedEvent(_declaringEvent);

                return null;
            }
        }
        /// <summary>
        /// Gets the Property from which this method was generated.
        /// Returns null if no such property exists.
        /// </summary>
        public Property DeclaringProperty
        {
            get
            {
                if (_declaringProp != null)
                    return Property.GetCachedProperty(_declaringProp);

                return null;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the this method is actually a constructor
        /// </summary>
        public bool IsConstructor
        {
            get
            {
                var MethodDef = GetMethodDefinition();
                if (MethodDef != null)
                {
                    return (MethodDef.IsConstructor);
                }
                return false;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the this method is the add or remove method of an event
        /// </summary>
        public bool IsEventMethod
        {
            get
            {
                if (_isEventMethod == null)
                {
                    _isEventMethod = false;

                    string mName = UnderlyingMethod.Name;
                    if (mName.StartsWith("add_") || mName.StartsWith("remove_"))
                    {
                        string evName = "";
                        if (mName.StartsWith("add_"))
                            evName = mName.Substring(4, mName.Length - 4);
                        else if (mName.StartsWith("remove_"))
                            evName = mName.Substring(7, mName.Length - 7);
                        var TypeDef = GetDeclaringTypeDefinition();
                        if (TypeDef != null)
                        {
                            var evsDeclaredInThisKind = TypeDef.Events.Where
                                (e => e.DeclaringType.FullName == UnderlyingMethod.DeclaringType.FullName);
                            foreach (EventDefinition _event in evsDeclaredInThisKind)
                            {
                                if (_event.Name == evName)
                                {
                                    _declaringEvent = _event;
                                    _isEventMethod = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                return _isEventMethod.Value;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the Method represents a generic method definition
        /// </summary>
        public bool IsGenericDefinition
        {
            get { return UnderlyingMethod.HasGenericParameters; }
        }
        /// <summary>
        /// Gets a value indicating whether the Method represents a generic method instance
        /// </summary>
        public bool IsGenericInstance
        {
            get
            {
                return UnderlyingMethod.IsGenericInstance;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the this method is generated from an overloaded operator
        /// </summary>
        public bool IsOperatorMethod
        {
            get
            {
                if (_isOperatorMethod == null)
                {
                    _isOperatorMethod = false;

                    _isOperatorMethod = UnderlyingMethod.Name.StartsWith("op_");
                }

                return _isOperatorMethod.Value;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the this method is the get or set method of a property
        /// </summary>
        public bool IsPropertyMethod
        {
            get 
            {
                if (_isPropertyMethod == null)
                {
                    _isPropertyMethod = false;

                    string mName = UnderlyingMethod.Name;
                    if (mName.StartsWith("get_") || mName.StartsWith("set_"))
                    {
                        string propName = mName.Substring(4, mName.Length - 4);
                        var TypeDef = GetDeclaringTypeDefinition();
                        if (TypeDef != null)
                        {
                            var propsDeclaredInThisKind = TypeDef.Properties.Where
                                (p => p.DeclaringType.FullName == UnderlyingMethod.DeclaringType.FullName);
                            foreach (PropertyDefinition _prop in propsDeclaredInThisKind)
                            {
                                if (_prop.Name == propName)
                                {
                                    _declaringProp = _prop;
                                    _isPropertyMethod = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                return _isPropertyMethod.Value;
            }
        }
        /// <summary>
        /// Gets the underlying Mono.Cecil.MethodReference from which this instance of Tril.Models.Method was built
        /// </summary>
        public MethodReference UnderlyingMethod
        {
            get { return ((MethodReference)_underlyingInfo); }
        }
    }
}
