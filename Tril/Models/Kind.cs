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
    /// The base class for every kind of entity to be contained in a Tril.Models.Package object
    /// </summary>
    [Serializable]
    public partial class Kind : Member
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

            var TypeDef = GetTypeDefinition();
            if (TypeDef != null)
            {
                if (UnderlyingType.IsNested)
                {
                    if (TypeDef.IsNestedPublic)
                        accMods.Add("public");
                    else if (TypeDef.IsNestedPrivate)
                        accMods.Add("private");
                    else if (TypeDef.IsNestedAssembly)
                        accMods.Add("internal");
                    else if (TypeDef.IsNestedFamilyAndAssembly)
                        accMods.Add("internal");
                    else if (TypeDef.IsNestedFamily)
                        accMods.Add("protected");
                    else if (TypeDef.IsNestedFamilyOrAssembly)
                        accMods.Add("protected internal");
                }
                else
                {
                    //public or not public
                    if (TypeDef.IsPublic)
                        accMods.Add("public");
                }
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

            var TypeDef = GetTypeDefinition();
            if (TypeDef != null)
            {
                if (TypeDef.IsAbstract && TypeDef.IsSealed)
                    attris.Add("static");
                else if (TypeDef.IsAbstract)
                    attris.Add("abstract");
                else if (TypeDef.IsSealed)
                    attris.Add("sealed");
            }

            return attris.ToArray();
        }
        /// <summary>
        /// Returns an array of all the types extended by this Kind instance.
        /// For languages that do not support multiple inheritance, this array should contain atmost one element.
        /// If the base kinds are user-defined, an array of strings representing what the user defines is returned.
        /// If not, a single Kind object representing the base type of the underlying type is returned.
        /// </summary>
        /// <returns></returns>
        public dynamic GetBaseKinds()
        {
            return GetBaseKinds("*");
        }
        /// <summary>
        /// Returns an array of all the types extended by this Kind instance.
        /// For languages that do not support multiple inheritance, this array should contain atmost one element.
        /// If the base kinds are user-defined, an array of strings representing what the user defines is returned.
        /// If not, a single Kind object representing the base type of the underlying type is returned.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public dynamic GetBaseKinds(bool useDefaultOnly)
        {
            return GetBaseKinds(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns an array of all the types extended by this Kind instance.
        /// For languages that do not support multiple inheritance, this array should contain atmost one element.
        /// If the base kinds are user-defined, an array of strings representing what the user defines is returned.
        /// If not, a single Kind object representing the base type of the underlying type is returned.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetBaseKinds(params string[] targetPlats)
        {
            return GetBaseKinds(false, targetPlats);
        }
        /// <summary>
        /// Returns an array of all the types extended by this Kind instance.
        /// For languages that do not support multiple inheritance, this array should contain atmost one element.
        /// If the base kinds are user-defined, an array of strings representing what the user defines is returned.
        /// If not, a single Kind object representing the base type of the underlying type is returned.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetBaseKinds(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedBaseKinds(targetPlats) && !useDefaultOnly)
            {
                List<string> baseKinds = new List<string>();

                var commAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ExtendsAttribute");
                foreach (CustomAttribute attri in commAttris)
                {
                    string[] tgtPlatsVal = GetTargetPlatforms(attri);
                    foreach (string lang in targetPlats)
                    {
                        if (tgtPlatsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                        {
                            baseKinds.Add(attri.ConstructorArguments[0].Value.ToString());
                            break;
                        }
                    }
                }

                return baseKinds.ToArray();
            }
            else
            {
                var TypeDef = GetTypeDefinition();
                if (TypeDef != null)
                {
                    if (TypeDef.BaseType != null)
                        return new Kind(TypeDef.BaseType);
                }
                return null;
            }
        }
        /// <summary>
        /// Returns an array of all the events in this Kind instance.
        /// </summary>
        /// <returns></returns>
        public Event[] GetEvents()
        {
            return GetEvents("*");
        }
        /// <summary>
        /// Returns an array of all the events in this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public Event[] GetEvents(bool useDefaultOnly)
        {
            return GetEvents(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns an array of all the events in this Kind instance.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public Event[] GetEvents(params string[] targetPlats)
        {
            return GetEvents(false, targetPlats);
        }
        /// <summary>
        /// Returns an array of all the events in this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public Event[] GetEvents(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            List<Event> _events = new List<Event>();

            var TypeDef = GetTypeDefinition();
            if (TypeDef != null) 
            {
                var eventsDeclaredInThisKind = TypeDef.Events.Where
                    (e => e.DeclaringType.FullName == UnderlyingType.FullName);

                foreach (EventDefinition _event in eventsDeclaredInThisKind)
                {
                    Event e = Event.GetCachedEvent(_event);

                    if (e.IsHidden(targetPlats) && !useDefaultOnly)
                    {
                        goto done;
                    }

                    _events.Add(e);
                done: ;
                }
            }

            return _events.ToArray();
        }
        /// <summary>
        /// Returns an array of all the fields in this Kind instance.
        /// </summary>
        /// <returns></returns>
        public Field[] GetFields()
        {
            return GetFields("*");
        }
        /// <summary>
        /// Returns an array of all the fields in this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public Field[] GetFields(bool useDefaultOnly)
        {
            return GetFields(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns an array of all the fields in this Kind instance.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public Field[] GetFields(params string[] targetPlats)
        {
            return GetFields(false, targetPlats);
        }
        /// <summary>
        /// Returns an array of all the fields in this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public Field[] GetFields(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            List<Field> _fields = new List<Field>();

            var TypeDef = GetTypeDefinition();
            if (TypeDef != null)
            {
                var fldsDeclaredInThisKind = TypeDef.Fields.Where
                    (f => f.DeclaringType.FullName == UnderlyingType.FullName);

                foreach (FieldDefinition _field in fldsDeclaredInThisKind)
                {
                    Field f = Field.GetCachedField(_field);

                    if (f.IsHidden(targetPlats) && !useDefaultOnly)
                    {
                        goto done;
                    }

                    _fields.Add(f);
                done: ;
                }
            }

            return _fields.ToArray();
        }
        /// <summary>
        /// Returns an array of all the runtime generic arguments of this Kind instance.
        /// </summary>
        /// <returns></returns>
        public Kind[] GetGenericArguments()
        {
            List<Kind> genericParams = new List<Kind>();

            if (IsGenericInstance)
            {
                var genericInstance = (GenericInstanceType)UnderlyingType;
                foreach (TypeReference genericArg in genericInstance.GenericArguments)
                {
                    if (genericArg != null)
                    {
                        Kind genericKindParam = new Kind(genericArg);

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
        /// Returns an array of all the generic constraints for this Kind instance.
        /// If the generic constaints are user-defined, an array of strings representing what the user defined is returned.
        /// If not, an array of Kind object representing the generic constraints of the underlying type is returned.
        /// </summary>
        /// <returns></returns>
        public dynamic GetGenericConstraints() 
        {
            return GetGenericConstraints("*");
        }
        /// <summary>
        /// Returns an array of all the generic constraints for this Kind instance.
        /// If the generic constaints are user-defined, an array of strings representing what the user defined is returned.
        /// If not, an array of Kind object representing the generic constraints of the underlying type is returned.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public dynamic GetGenericConstraints(bool useDefaultOnly) 
        {
            return GetGenericConstraints(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns an array of all the generic constraints for this Kind instance.
        /// If the generic constaints are user-defined, an array of strings representing what the user defined is returned.
        /// If not, an array of Kind object representing the generic constraints of the underlying type is returned.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetGenericConstraints(params string[] targetPlats) 
        {
            return GetGenericConstraints(false, targetPlats);
        }
        /// <summary>
        /// Returns an array of all the generic constraints for this Kind instance.
        /// If the generic constaints are user-defined, an array of strings representing what the user defined is returned.
        /// If not, an array of Kind object representing the generic constraints of the underlying type is returned.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetGenericConstraints(bool useDefaultOnly, params string[] targetPlats)
        {
            if (HasUserDefinedGenericConstraints(targetPlats) && !useDefaultOnly)
            {
                List<string> constraints = new List<string>();

                //first look for Tril.Attributes.MustBeAttribute
                if (GetAllCustomAttributes().Any(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MustBeAttribute"))
                {
                    var beAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MustBeAttribute");
                    foreach (CustomAttribute attri in beAttris)
                    {
                        string[] tgtPlatsVal = GetTargetPlatforms(attri);
                        foreach (string lang in targetPlats)
                        {
                            if (tgtPlatsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                            {
                                constraints.Add(attri.ConstructorArguments[0].Value.ToString());
                                break;
                            }
                        }
                    }
                }
                //if there is no Tril.Attributes.MustBeAttribute, then use Tril.Attributes.MustExtendAttribute and Tril.Attributes.MustImplementAttribute
                else
                {
                    var extAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MustExtendAttribute");
                    foreach (CustomAttribute attri in extAttris)
                    {
                        string[] tgtLangsVal = GetTargetPlatforms(attri);
                        foreach (string lang in targetPlats)
                        {
                            if (tgtLangsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                            {
                                constraints.Add("<extends>" + attri.ConstructorArguments[0].Value.ToString());
                                break;
                            }
                        }
                    }
                    var impAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MustImplementAttribute");
                    foreach (CustomAttribute attri in impAttris)
                    {
                        string[] tgtLangsVal = GetTargetPlatforms(attri);
                        foreach (string lang in targetPlats)
                        {
                            if (tgtLangsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                            {
                                constraints.Add("<implements>" + attri.ConstructorArguments[0].Value.ToString());
                                break;
                            }
                        }
                    }
                }

                return constraints.ToArray();
            }

            List<Kind> genericParamCons = new List<Kind>();

            if (IsGenericParameter)
            {
                foreach (TypeReference genericParamConst in ((GenericParameter)UnderlyingType).Constraints)
                {
                    if (genericParamConst != null)
                    {
                        Kind genericKindParam = new Kind(genericParamConst);

                        genericParamCons.Add(genericKindParam);
                    }
                }
            }

            return genericParamCons.ToArray();
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
                var genAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.TypeGenConstraintsSecAttribute");
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
        /// Returns an array of all the generic parameters of the definition for this Kind instance.
        /// </summary>
        /// <returns></returns>
        public Kind[] GetGenericParameters()
        {
            List<Kind> genericParams = new List<Kind>();

            if (IsGenericDefinition)
            {
                //var genericInstance = (GenericInstanceType)UnderlyingType;
                //foreach (TypeReference genericParam in genericInstance.GenericParameters)//UnderlyingType.GenericParameters)
                //{
                //    if (genericParam != null)
                //    {
                //        Kind genericKindParam = new Kind(genericParam);

                //        genericParams.Add(genericKindParam);
                //    }
                //}
                var TypeDef = GetTypeDefinition();
                if (TypeDef != null)
                {
                    foreach (TypeReference genericParam in TypeDef.GenericParameters)
                    {
                        if (genericParam != null)
                        {
                            Kind genericKindParam = new Kind(genericParam);

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
                var genAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.TypeGenParamsSecAttribute");
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
        /// Returns an array of string containing of all the types imported by this Kind instance.
        /// </summary>
        /// <returns></returns>
        public string[] GetImports()
        {
            return GetImports("*");
        }
        /// <summary>
        /// Returns an array of string containing of all the types imported by this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string[] GetImports(bool useDefaultOnly)
        {
            return GetImports(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns an array of string containing of all the types imported by this Kind instance.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string[] GetImports(params string[] targetPlats)
        {
            return GetImports(false, targetPlats);
        }
        /// <summary>
        /// Returns an array of string containing of all the types imported by this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string[] GetImports(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            List<string> imports = new List<string>();

            if (HasUserDefinedImports(targetPlats) && !useDefaultOnly)
            {
                var importAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ImportAttribute");
                foreach (CustomAttribute attri in importAttris)
                {
                    string[] tgtPlatsVal = GetTargetPlatforms(attri);
                    foreach (string lang in targetPlats)
                    {
                        if (tgtPlatsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                        {
                            imports.Add(attri.ConstructorArguments[0].Value.ToString());
                            break;
                        }
                    }
                }

                return imports.ToArray();
            }
            else
            {
                //i dont know how to get the using statements yet
                //and i dont think CIL stores your usings/imports
                //it simply replaces all calls (methods, fields, types, properties...) with their full names
                return imports.ToArray();
            }
        }
        /// <summary>
        /// Returns an array of all the interfaces implemented by this Kind instance.
        /// If the interfaces are user-defined, an array of strings representing what the user defines is returned.
        /// If not, an array of Kind objects representing the interfaces of the underlying type is returned.
        /// </summary>
        /// <returns></returns>
        public dynamic GetInterfaces()
        {
            return GetInterfaces("*");
        }
        /// <summary>
        /// Returns an array of all the interfaces implemented by this Kind instance.
        /// If the interfaces are user-defined, an array of strings representing what the user defines is returned.
        /// If not, an array of Kind objects representing the interfaces of the underlying type is returned.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public dynamic GetInterfaces(bool useDefaultOnly)
        {
            return GetInterfaces(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns an array of all the interfaces implemented by this Kind instance.
        /// If the interfaces are user-defined, an array of strings representing what the user defines is returned.
        /// If not, an array of Kind objects representing the interfaces of the underlying type is returned.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetInterfaces(params string[] targetPlats)
        {
            return GetInterfaces(false, targetPlats);
        }
        /// <summary>
        /// Returns an array of all the interfaces implemented by this Kind instance.
        /// If the interfaces are user-defined, an array of strings representing what the user defines is returned.
        /// If not, an array of Kind objects representing the interfaces of the underlying type is returned.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetInterfaces(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedInterfaces(targetPlats) && !useDefaultOnly)
            {
                List<string> interfaces = new List<string>();

                var intfcAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ImplementsAttribute");
                foreach (CustomAttribute attri in intfcAttris)
                {
                    string[] tgtLangsVal = GetTargetPlatforms(attri);
                    foreach (string lang in targetPlats)
                    {
                        if (tgtLangsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                        {
                            interfaces.Add(attri.ConstructorArguments[0].Value.ToString());
                            break;
                        }
                    }
                }

                return interfaces.ToArray();
            }
            else
            {
                List<Kind> interfaces = new List<Kind>();

                var TypeDef = GetTypeDefinition();
                if (TypeDef != null)
                {
                    foreach (TypeReference interf in TypeDef.Interfaces)
                    {
                        if (interf != null)
                        {
                            Kind i = new Kind(interf);
                            interfaces.Add(i);
                        }
                    }
                }
                return interfaces.ToArray();
            }
        }
        /// <summary>
        /// Returns the long name of this model.
        /// For a Tril.Models.Kind, this is the GetNamespace plus a dot (.) plus the GetName.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public override string GetLongName(bool useDefaultOnly, params string[] targetPlats)
        {
            string nameSpace = GetNamespace(useDefaultOnly, targetPlats).Trim();
            if (nameSpace != "")
            {
                if (UnderlyingType.IsNested)
                {
                    if (DeclaringKind != null)
                    {
                        return DeclaringKind.GetLongName(useDefaultOnly, targetPlats).Trim() + "." + GetName(useDefaultOnly, targetPlats).Trim();
                    }
                    else
                        return nameSpace + "." + GetName(useDefaultOnly, targetPlats).Trim();
                }
                else
                    return nameSpace + "." + GetName(useDefaultOnly, targetPlats).Trim();
            }
            else
                return GetName(useDefaultOnly, targetPlats).Trim();
        }
        /// <summary>
        /// Returns the current meaning of this Kind instance.
        /// </summary>
        /// <returns></returns>
        public string GetMeaning()
        {
            return GetMeaning("*");
        }
        /// <summary>
        /// Returns the current meaning of this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string GetMeaning(bool useDefaultOnly)
        {
            return GetMeaning(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns the current meaning of this Kind instance.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetMeaning(params string[] targetPlats)
        {
            return GetMeaning(false, targetPlats);
        }
        /// <summary>
        /// Returns the current meaning of this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetMeaning(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedMeaning(targetPlats) && !useDefaultOnly)
            {
                var meanAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.TypeIsAttribute");
                foreach (CustomAttribute attri in meanAttris)
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

            if (IsClass)
                return "class";
            else if (IsEnum)
                return "enum";
            else if (IsInterface)
                return "interface";
            else if (IsStruct)
                return "struct";

            return "";
        }
        /// <summary>
        /// Returns an array of all the methods in this Kind instance.
        /// </summary>
        /// <returns></returns>
        public Method[] GetMethods()
        {
            return GetMethods("*");
        }
        /// <summary>
        /// Returns an array of all the methods in this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public Method[] GetMethods(bool useDefaultOnly)
        {
            return GetMethods(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns an array of all the methods in this Kind instance.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public Method[] GetMethods(params string[] targetPlats)
        {
            return GetMethods(false, targetPlats);
        }
        /// <summary>
        /// Returns an array of all the methods in this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public Method[] GetMethods(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            List<Method> _methods = new List<Method>();

            var TypeDef = GetTypeDefinition();
            if (TypeDef != null) 
            {
                var mthdsDeclaredInThisKind = TypeDef.Methods.Where
                    (m => m.DeclaringType.FullName == UnderlyingType.FullName);

                foreach (MethodDefinition _mthd in mthdsDeclaredInThisKind)
                {
                    Method m = Method.GetCachedMethod(_mthd);

                    if (m.IsHidden(targetPlats) && !useDefaultOnly)
                    {
                        goto done;
                    }

                    _methods.Add(m);
                done: ;
                }
            }

            return _methods.ToArray();
        }
        /// <summary>
        /// Returns the default name
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        protected override string GetName_Default(bool useDefaultOnly, params string[] targetPlats)
        {
            string _name = UnderlyingType.Name;

            _name = ReplaceInvalidChars(_name);

            return _name;
        }
        /// <summary>
        /// Returns an array of all the nested types in this Kind instance.
        /// </summary>
        /// <returns></returns>
        public Kind[] GetNestedKinds()
        {
            return GetNestedKinds("*");
        }
        /// <summary>
        /// Returns an array of all the nested types in this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public Kind[] GetNestedKinds(bool useDefaultOnly)
        {
            return GetNestedKinds(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns an array of all the nested types in this Kind instance.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public Kind[] GetNestedKinds(params string[] targetPlats)
        {
            return GetNestedKinds(false, targetPlats);
        }
        /// <summary>
        /// Returns an array of all the nested types in this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public Kind[] GetNestedKinds(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            List<Kind> _kinds = new List<Kind>();

            var TypeDef = GetTypeDefinition();
            if (TypeDef != null)
            {
                var typesDeclaredInThisKind = TypeDef.NestedTypes.Where
                    (t => t.DeclaringType.FullName == UnderlyingType.FullName);

                foreach (TypeDefinition _type in typesDeclaredInThisKind)
                {
                    Kind k = new Kind(_type);

                    if (k.IsHidden(targetPlats) && !useDefaultOnly)
                    {
                        goto done;
                    }

                    _kinds.Add(k);
                done: ;
                }
            }

            return _kinds.ToArray();
        }
        /// <summary>
        /// Returns a string representing the namespace of this Kind instance.
        /// If this Kind instance has user-defined namespace,
        /// the method returns a string representing the first matched namespace of this Kind instance.
        /// </summary>
        /// <returns></returns>
        public string GetNamespace()
        {
            return GetNamespace("*");
        }
        /// <summary>
        /// Returns a string representing the namespace of this Kind instance.
        /// If this Kind instance has user-defined namespace,
        /// the method returns a string representing the first matched namespace of this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string GetNamespace(bool useDefaultOnly)
        {
            return GetNamespace(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns a string representing the namespace of this Kind instance.
        /// If this Kind instance has user-defined namespace,
        /// the method returns a string representing the first matched namespace of this Kind instance.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetNamespace(params string[] targetPlats)
        {
            return GetNamespace(false, targetPlats);
        }
        /// <summary>
        /// Returns a string representing the namespace of this Kind instance.
        /// If this Kind instance has user-defined namespace,
        /// the method returns a string representing the first matched namespace of this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetNamespace(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedNamespace(targetPlats) && !useDefaultOnly)
            {
                var meanAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.NamespaceAttribute");
                foreach (CustomAttribute attri in meanAttris)
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

            string nameSpace = UnderlyingType.Namespace;
            return nameSpace != null ? nameSpace : "";
        }
        /// <summary>
        /// Returns a string representing the pre-body section of this Kind instance.
        /// If this Kind instance has user-defined pre-body sections,
        /// the method returns a string representing the first matched pre-body section of this Kind instance.
        /// </summary>
        /// <returns></returns>
        public string GetPreBodySection()
        {
            return GetPreBodySection("*");
        }
        /// <summary>
        /// Returns a string representing the pre-body section of this Kind instance.
        /// If this Kind instance has user-defined pre-body sections,
        /// the method returns a string representing the first matched pre-body section of this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string GetPreBodySection(bool useDefaultOnly)
        {
            return GetPreBodySection(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns a string representing the pre-body section of this Kind instance.
        /// If this Kind instance has user-defined pre-body sections,
        /// the method returns a string representing the first matched pre-body section of this Kind instance.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetPreBodySection(params string[] targetPlats)
        {
            return GetPreBodySection(false, targetPlats);
        }
        /// <summary>
        /// Returns a string representing the pre-body section of this Kind instance.
        /// If this Kind instance has user-defined pre-body sections,
        /// the method returns a string representing the first matched pre-body section of this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetPreBodySection(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedPreBodySection(targetPlats) && !useDefaultOnly)
            {
                var meanAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.TypePreBodyAttribute");
                foreach (CustomAttribute attri in meanAttris)
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

            return "";
        }
        /// <summary>
        /// Returns an array of all the properties in this Kind instance.
        /// </summary>
        /// <returns></returns>
        public Property[] GetProperties()
        {
            return GetProperties("*");
        }
        /// <summary>
        /// Returns an array of all the properties in this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public Property[] GetProperties(bool useDefaultOnly)
        {
            return GetProperties(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns an array of all the properties in this Kind instance.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public Property[] GetProperties(params string[] targetPlats)
        {
            return GetProperties(false, targetPlats);
        }
        /// <summary>
        /// Returns an array of all the properties in this Kind instance.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public Property[] GetProperties(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            List<Property> _props = new List<Property>();

            var TypeDef = GetTypeDefinition();
            if (TypeDef != null)
            {
                var propsDeclaredInThisKind = TypeDef.Properties.Where
                    (p => p.DeclaringType.FullName == UnderlyingType.FullName);

                foreach (PropertyDefinition _prop in propsDeclaredInThisKind)
                {
                    Property p = Property.GetCachedProperty(_prop);

                    if (p.IsHidden(targetPlats) && !useDefaultOnly)
                    {
                        goto done;
                    }

                    _props.Add(p);
                done: ;
                }
            }

            return _props.ToArray();
        }
        /// <summary>
        /// Gets a raw collection of all the attributes (without sugar-coating anything).
        /// </summary>
        /// <returns></returns>
        public override string[] GetRawAttributes()
        {
            List<string> attris = new List<string>();

            var TypeDef = GetTypeDefinition();
            if (TypeDef != null)
            {
                if (TypeDef.IsSpecialName)
                    attris.Add("specialname");

                if (TypeDef.IsRuntimeSpecialName)
                    attris.Add("rtspecialname");

                if (TypeDef.IsAbstract)
                    attris.Add("abstract");

                if (TypeDef.IsSealed)
                    attris.Add("sealed");
            }

            return attris.ToArray();
        }
        /// <summary>
        /// Gets a value indicating whether the bodies of the methods in this Kind instance can be accessed.
        /// If the underlying type has been marked with the [ShowImplementation] attribute, 
        /// this method would return false.
        /// If the underlying type has been marked with the [HideImplementation] attribute, 
        /// this method would return true.
        /// Otherwise, this method returns false.
        /// </summary>
        /// <returns></returns>
        public bool HasHiddenImplementation()
        {
            return HasHiddenImplementation("*");
        }
        /// <summary>
        /// Gets a value indicating whether the bodies of the methods in this Kind instance can be accessed.
        /// If the underlying type has been marked with the [ShowImplementation] attribute, 
        /// this method would return false.
        /// If the underlying type has been marked with the [HideImplementation] attribute, 
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

            if (DeclaringKind != null)
            {
                return DeclaringKind.HasHiddenImplementation(targetPlats);
            }

            return false;
        }
        /// <summary>
        /// Gets a value indicating whether the base kinds of this Kind instance was defined by the user 
        /// using Tril.ExtendsAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedBaseKinds()
        {
            return HasUserDefinedBaseKinds("*");
        }
        /// <summary>
        /// Gets a value indicating whether the base kinds of this Kind instance was defined by the user 
        /// using Tril.ExtendsAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedBaseKinds(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var extAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ExtendsAttribute");

            return HasUserDefined(extAttris, targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether the generic constraints of this Kind instance was defined by the user using any of the
        /// Tril.Attributes.MustBeSecAttribute, Tril.Attributes.MustExtendAttribute or Tril.Attributes.MustImplementAttribute attributes
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedGenericConstraints()
        {
            return HasUserDefinedGenericConstraints("*");
        }
        /// <summary>
        /// Gets a value indicating whether the generic constraints of this Kind instance was defined by the user using any of the
        /// Tril.Attributes.MustBeSecAttribute, Tril.Attributes.MustExtendAttribute or Tril.Attributes.MustImplementAttribute attributes
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedGenericConstraints(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var beAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MustBeSecAttribute");
            if (HasUserDefined(beAttris, targetPlats))
                return false;

            var extAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MustExtendAttribute");
            if (HasUserDefined(extAttris, targetPlats))
                return false;

            var impAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MustImplementAttribute");
            if (HasUserDefined(impAttris, targetPlats))
                return false;

            return false;
        }
        /// <summary>
        /// Gets a value indicating whether the generic constraints section of this Kind instance was defined by the user 
        /// using Tril.Attributes.TypeGenConstraintsSecAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedGenericConstraintsSection()
        {
            return HasUserDefinedGenericConstraintsSection("*");
        }
        /// <summary>
        /// Gets a value indicating whether the generic constraints section of this Kind instance was defined by the user 
        /// using Tril.Attributes.TypeGenConstraintsSecAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedGenericConstraintsSection(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var genAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.TypeGenConstraintsSecAttribute");

            return HasUserDefined(genAttris, targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether the generic parameters section of this Kind instance was defined by the user 
        /// using Tril.Attributes.TypeGenParamsSecAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedGenericParametersSection()
        {
            return HasUserDefinedGenericParametersSection("*");
        }
        /// <summary>
        /// Gets a value indicating whether the generic parameters section of this Kind instance was defined by the user 
        /// using Tril.Attributes.TypeGenParamsSecAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedGenericParametersSection(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var genAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.TypeGenParamsSecAttribute");

            return HasUserDefined(genAttris, targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether the imports of this Kind instance were defined by the user 
        /// using Tril.Attributes.ImportAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedImports()
        {
            return HasUserDefinedImports("*");
        }
        /// <summary>
        /// Gets a value indicating whether the imports of this Kind instance were defined by the user 
        /// using Tril.Attributes.ImportAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedImports(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var importAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ImportAttribute");

            return HasUserDefined(importAttris, targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether the interfaces of this Kind instance was defined by the user 
        /// using Tril.Attributes.ImplementsAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedInterfaces()
        {
            return HasUserDefinedInterfaces("*");
        }
        /// <summary>
        /// Gets a value indicating whether the interfaces of this Kind instance was defined by the user 
        /// using Tril.ImplementsAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedInterfaces(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var intfcAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ImplementsAttribute");

            return HasUserDefined(intfcAttris, targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether the meaning of this Kind instance was defined by the user 
        /// using Tril.Attributes.TypeIsAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedMeaning()
        {
            return HasUserDefinedMeaning("*");
        }
        /// <summary>
        /// Gets a value indicating whether the meaning of this Kind instance was defined by the user 
        /// using Tril.Attributes.TypeIsAttribute
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public bool HasUserDefinedMeaning(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var meanAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.TypeIsAttribute");

            return HasUserDefined(meanAttris, targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether the namespace of this Kind instance was defined by the user 
        /// using Tril.Attributes.NamespaceAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedNamespace()
        {
            return HasUserDefinedNamespace("*");
        }
        /// <summary>
        /// Gets a value indicating whether the namespace of this Kind instance was defined by the user 
        /// using Tril.Attributes.NamespaceAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedNamespace(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var nmspcAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.NamespaceAttribute");

            return HasUserDefined(nmspcAttris, targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether the pre-body section of this Kind instance was defined by the user 
        /// using Tril.Attributes.TypePreBodyAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedPreBodySection()
        {
            return HasUserDefinedPreBodySection("*");
        }
        /// <summary>
        /// Gets a value indicating whether the pre-body section of this Kind instance was defined by the user 
        /// using Tril.Attributes.TypePreBodyAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedPreBodySection(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var preAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.TypePreBodyAttribute");

            return HasUserDefined(preAttris, targetPlats);
        }

        /// <summary>
        /// Gets the Kind that owns this kind
        /// </summary>
        public Kind DeclaringKind
        {
            get 
            {
                TypeReference parentType = UnderlyingType.DeclaringType;
                if (parentType != null)
                {
                    return new Kind(parentType);
                }
                return null;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the Tril.Models.Kind represents an object that can be assigned successfully to System.Attribute
        /// </summary>
        public bool CanBeAttribute
        {
            get
            {
                return typeof(System.Attribute).IsAssignableFrom(UnderlyingType.ToType(true));
            }
        }
        /// <summary>
        /// Gets a value indicating whether the Tril.Models.Kind represents an object that can be assigned successfully to System.Exception
        /// </summary>
        public bool CanBeException
        {
            get
            {
                return typeof(System.Exception).IsAssignableFrom(UnderlyingType.ToType(true));
            }
        }
        /// <summary>
        /// Gets a value indicating whether the Kind represents a class
        /// </summary>
        public bool IsClass
        {
            get 
            {
                var TypeDef = GetTypeDefinition();
                if (TypeDef != null)
                {
                    return TypeDef.IsClass;
                }
                return false;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the Kind represents an enum
        /// </summary>
        public bool IsEnum
        {
            get
            {
                var TypeDef = GetTypeDefinition();
                if (TypeDef != null)
                {
                    return TypeDef.IsEnum;
                }
                return false;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the Kind represents a generic type definition
        /// </summary>
        public bool IsGenericDefinition
        {
            get { return UnderlyingType.HasGenericParameters; }
        }
        /// <summary>
        /// Gets a value indicating whether the Kind represents a generic type instance
        /// </summary>
        public bool IsGenericInstance
        {
            get
            {
                return UnderlyingType.IsGenericInstance;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the Kind represents a type parameter in the definition of a generic type or method.
        /// </summary>
        public bool IsGenericParameter
        {
            get { return UnderlyingType.IsGenericParameter; }
        }
        /// <summary>
        /// Gets a value indicating whether the Kind represents a place-holder type parameter in the definition of a generic type or method.
        /// </summary>
        public bool IsPlaceHolderGenericParameter
        {
            get { return (UnderlyingType.IsGenericParameter && (UnderlyingType.Namespace == "" || UnderlyingType.Namespace == null)); }
        }
        /// <summary>
        /// Gets a value indicating whether the Kind represents an interface
        /// </summary>
        public bool IsInterface
        {
            get
            {
                var TypeDef = GetTypeDefinition();
                if (TypeDef != null)
                {
                    return TypeDef.IsInterface;
                }
                return false;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the Tril.Models.Kind is a native int type.
        /// </summary>
        public bool IsNativeInt
        {
            get 
            { 
                //return UnderlyingType.FullName == "System.IntPtr";

                if (IntPtr.Size == 8) //(Environment.Is64BitOperatingSystem) //64-bit
                    return UnderlyingType.FullName == new long().GetType().FullName;
                else //if (IntPtr.Size == 4) 32-bit
                    return UnderlyingType.FullName == new int().GetType().FullName;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the Tril.Models.Kind is an unsigned native int type.
        /// </summary>
        public bool IsNativeUInt
        {
            get 
            {
                //return UnderlyingType.FullName == "System.UIntPtr"; 

                if (IntPtr.Size == 8) //(Environment.Is64BitOperatingSystem) //64-bit
                    return UnderlyingType.FullName == new ulong().GetType().FullName;
                else //if (IntPtr.Size == 4) 32-bit
                    return UnderlyingType.FullName == new uint().GetType().FullName;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the Tril.Models.Kind is the .NET System.Object type.
        /// </summary>
        public bool IsObject
        {
            get { return UnderlyingType.FullName == "System.Object"; }
        }
        /// <summary>
        /// Gets a value indicating whether the Kind represents a struct
        /// </summary>
        public bool IsStruct
        {
            get
            {
                var TypeDef = GetTypeDefinition();
                if (TypeDef != null)
                {
                    return (UnderlyingType.IsValueType && !TypeDef.IsEnum);
                }
                return false;
            }
        }
        /// <summary>
        /// Gets the underlying Mono.Cecil.TypeReference from which this instance of Kind was built
        /// </summary>
        public TypeReference UnderlyingType
        {
            get { return ((TypeReference)_underlyingInfo); }
        }
    }
}
