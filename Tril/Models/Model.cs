using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Collections.Generic;

using Tril.Utilities;

namespace Tril.Models
{
    /// <summary>
    /// An interface to be implemented by every Tril model
    /// </summary>
    [Serializable]
    public abstract partial class Model : IDisposable
    {
        /// <summary>
        /// Returns an array of all the annotations on this model
        /// </summary>
        /// <returns></returns>
        public string[] GetAnnotations()
        {
            return GetAnnotations("*");
        }
        /// <summary>
        /// Returns an array of all the annotations on this model
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string[] GetAnnotations(bool useDefaultOnly)
        {
            return GetAnnotations(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns an array of all the annotations on this model
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string[] GetAnnotations(params string[] targetPlats)
        {
            return GetAnnotations(false, targetPlats);
        }
        /// <summary>
        /// Returns an array of all the annotation on this model
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string[] GetAnnotations(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            List<string> attris = new List<string>();

            if (HasUserDefinedAnnotations(targetPlats) && !useDefaultOnly)
            {
                var annotAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.AnnotationAttribute");
                foreach (CustomAttribute attri in annotAttris)
                {
                    string[] tgtPlatsVal = GetTargetPlatforms(attri);
                    foreach (string lang in targetPlats)
                    {
                        if (tgtPlatsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                        {
                            attris.Add(attri.ConstructorArguments[0].Value.ToString());
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (var customAttri in GetAllCustomAttributes())
                {
                    Kind kindAttri = Kind.GetCachedKind(customAttri.AttributeType);

                    string allArgs = "";
                    foreach (var constrArg in customAttri.ConstructorArguments)
                    {
                        if (constrArg.Value is CustomAttributeArgument[])//see if it is an array
                        {
                            var cArgs = (CustomAttributeArgument[])constrArg.Value;
                            for (int index = 0; index < cArgs.Length; index++)
                            {
                                //remember to change "True" to "true", False to "false"
                                allArgs += (cArgs[index].Type.FullName == "System.Boolean" ?
                                    cArgs[index].Value.ToString().ToLower() : cArgs[index].Value.ToString()) + ", ";
                            }
                        }
                        else
                        {
                            //remember to change "True" to "true", False to "false"
                            allArgs += (constrArg.Type.FullName == "System.Boolean" ?
                                constrArg.Value.ToString().ToLower() : constrArg.Value.ToString()) + ", ";
                        }
                    }
                    foreach (var fld in customAttri.Fields)//i dont know if this is right, that is, displaying fields
                    {
                        if (fld.Argument.Value is CustomAttributeArgument[])//see if it is an array
                        {
                            var fArgs = (CustomAttributeArgument[])fld.Argument.Value;
                            for (int index = 0; index < fArgs.Length; index++)
                            {
                                //remember to change "True" to "true", False to "false"
                                allArgs += fld.Name + " = " +
                                    (fArgs[index].Type.FullName == "System.Boolean" ? fArgs[index].Value.ToString().ToLower() : 
                                    fArgs[index].Value.ToString()) + ", ";
                            }
                        }
                        else
                        {
                            allArgs += fld.Name + " = " +
                                (fld.Argument.Type.FullName == "System.Boolean" ? fld.Argument.Value.ToString().ToLower() :
                                fld.Argument.Value.ToString()) + ", ";
                        }
                    }
                    foreach (var prop in customAttri.Properties)
                    {
                        if (prop.Argument.Value is CustomAttributeArgument[])//see if it is an array
                        {
                            var pArgs = (CustomAttributeArgument[])prop.Argument.Value;
                            for (int index = 0; index < pArgs.Length; index++)
                            {
                                //remember to change "True" to "true", False to "false"
                                allArgs += prop.Name + " = " +
                                    (pArgs[index].Type.FullName == "System.Boolean" ? pArgs[index].Value.ToString().ToLower() : 
                                    pArgs[index].Value.ToString()) + ", ";
                            }
                        }
                        else
                        {
                            allArgs += prop.Name + " = " +
                                (prop.Argument.Type.FullName == "System.Boolean" ? prop.Argument.Value.ToString().ToLower() :
                                prop.Argument.Value.ToString()) + ", ";
                        }
                    }
                    allArgs = allArgs.TrimEnd(' ').TrimEnd(',');
                    if (allArgs.Trim() != "")
                        allArgs = "(" + allArgs + ")";

                    attris.Add(kindAttri.GetLongName(targetPlats) + allArgs);
                }

            }

            return attris.ToArray();
        }
        /// <summary>
        /// Returns an array of all the attributes of this model
        /// </summary>
        /// <returns></returns>
        public string[] GetAttributes()
        {
            return GetAttributes("*");
        }
        /// <summary>
        /// Returns an array of all the attributes of this model
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string[] GetAttributes(bool useDefaultOnly)
        {
            return GetAttributes(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns an array of all the attributes of this model
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string[] GetAttributes(params string[] targetPlats)
        {
            return GetAttributes(false, targetPlats);
        }
        /// <summary>
        /// Returns an array of all the attributes of this model
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string[] GetAttributes(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedAttributes(targetPlats) && !useDefaultOnly)
            {
                List<string> attris = new List<string>();

                if (_underlyingInfo is MemberReference)
                {
                    var attriAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.AttributeAttribute");
                    foreach (CustomAttribute attri in attriAttris)
                    {
                        string[] tgtLangsVal = GetTargetPlatforms(attri);
                        foreach (string lang in targetPlats)
                        {
                            if (tgtLangsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                            {
                                if (attri.ConstructorArguments[0].Type.FullName == "Tril.Attributes.Attributes")
                                {
                                    if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.Attributes.None))
                                        attris.Add("");
                                    else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.Attributes.Abstract))
                                        attris.Add("abstract");
                                    else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.Attributes.Const))
                                        attris.Add("const");
                                    else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.Attributes.MustOverride))
                                        attris.Add("MustOverride");
                                    else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.Attributes.Override))
                                        attris.Add("override");
                                    else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.Attributes.Overrides))
                                        attris.Add("Overrides");
                                    else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.Attributes.Readonly))
                                        attris.Add("readonly");
                                    else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.Attributes.Sealed))
                                        attris.Add("sealed");
                                    else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.Attributes.Shared))
                                        attris.Add("Shared");
                                    else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.Attributes.Static))
                                        attris.Add("static");
                                    else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.Attributes.StaticReadonly))
                                        attris.Add("static readonly");
                                    else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.Attributes.Virtual))
                                        attris.Add("virtual");
                                }
                                else
                                {
                                    attris.Add(attri.ConstructorArguments[0].Value.ToString());
                                }
                                break;
                            }
                        }
                    }
                }
                else if (_underlyingInfo is ParameterReference)
                {
                    var paramisAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ParamIsAttribute");
                    foreach (CustomAttribute attri in paramisAttris)
                    {
                        string[] tgtPlatsVal = GetTargetPlatforms(attri);
                        foreach (string lang in targetPlats)
                        {
                            if (tgtPlatsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                            {
                                if (attri.ConstructorArguments[0].Type.FullName == "Tril.Attributes.ParamAttributes")
                                {
                                    if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.ParamAttributes.None))
                                        attris.Add("");
                                    else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.ParamAttributes.Out))
                                        attris.Add("out");
                                    else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.ParamAttributes.Ref))
                                        attris.Add("ref");
                                }
                                else
                                {
                                    attris.Add(attri.ConstructorArguments[0].Value.ToString());
                                }
                                break;
                            }
                        }
                    }
                }

                return attris.ToArray();
            }
            else
            {
                return GetAttributes_Default(useDefaultOnly, targetPlats);
            }
        }
        /// <summary>
        /// Returns the default attribute
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        protected abstract string[] GetAttributes_Default(bool useDefaultOnly, params string[] targetPlats);
        /// <summary>
        /// Returns an array of all the comments on this model
        /// </summary>
        /// <returns></returns>
        public string[] GetComments()
        {
            return GetComments("*");
        }
        /// <summary>
        /// Returns an array of all the comments on this model
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string[] GetComments(bool useDefaultOnly)
        {
            return GetComments(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns an array of all the comments on this model
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string[] GetComments(params string[] targetPlats)
        {
            return GetComments(false, targetPlats);
        }
        /// <summary>
        /// Returns an array of all the comments on this model
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string[] GetComments(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            List<string> attris = new List<string>();

            if (HasUserDefinedComments(targetPlats) && !useDefaultOnly)
            {
                var commAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.CommentAttribute");
                foreach (CustomAttribute attri in commAttris)
                {
                    string[] tgtPlatsVal = GetTargetPlatforms(attri);
                    foreach (string lang in targetPlats)
                    {
                        if (tgtPlatsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                        {
                            attris.Add(attri.ConstructorArguments[0].Value.ToString());
                            break;
                        }
                    }
                }
            }
            else
            {
                //dont know how to get documentation comments yet
                //although I think doc comments shud be stored in another file with the same name but diff extension as the assembly dll file...
            }

            return attris.ToArray();
        }
        ///// <summary>
        ///// Returns an array of string containing of all the user-defined forbidden platforms of this model.
        ///// </summary>
        ///// <returns></returns>
        //public string[] GetForbiddenPlatforms()
        //{
        //    List<string> forbiddenPlats = new List<string>();

        //    var forbidAttris = getAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ForbidAttribute");
        //    foreach (CustomAttribute attri in forbidAttris)
        //    {
        //        var proptargetPlats = attri.Properties.Single(p => p.Name == "TargetLanguages");
        //        string[] proptargetPlatsVal = (string[])proptargetPlats.Argument.Value;
        //        foreach (string lang in proptargetPlatsVal)
        //        {
        //            if (!forbiddenPlats.Contains(lang))
        //                forbiddenPlats.Add(lang);
        //        }
        //    }

        //    return forbiddenPlats.ToArray();
        //}
        /// <summary>
        /// Returns the long name of this model.
        /// For a Tril.Models.Kind, this is the GetNamespace plus a dot (.) plus the GetName.
        /// For a Tril.Models.Method, this is the long name of the declaring Kind plus two colons (::) plus the GetName plus the parameters section.
        /// For a Tril.Models.Property, this is the long name of the declaring Kind plus two colons (::) plus the GetName.
        /// For a Tril.Models.Field, this is the long name of the declaring Kind plus two colons (::) plus the GetName.
        /// For a Tril.Models.Parameter, this is the long name of the parameter Kind (or the name if the parameter Kind is a generic place holder)
        /// plus space ( ) plus the GetName.
        /// </summary>
        /// <returns></returns>
        public string GetLongName()
        {
            return GetLongName("*");
        }
        /// <summary>
        /// Returns the long name of this model.
        /// For a Tril.Models.Kind, this is the GetNamespace plus a dot (.) plus the GetName.
        /// For a Tril.Models.Method, this is the long name of the declaring Kind plus two colons (::) plus the GetName plus the parameters section.
        /// For a Tril.Models.Property, this is the long name of the declaring Kind plus two colons (::) plus the GetName.
        /// For a Tril.Models.Field, this is the long name of the declaring Kind plus two colons (::) plus the GetName.
        /// For a Tril.Models.Parameter, this is the long name of the parameter Kind (or the name if the parameter Kind is a generic place holder)
        /// plus space ( ) plus the GetName.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string GetLongName(bool useDefaultOnly)
        {
            return GetLongName(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns the long name of this model.
        /// For a Tril.Models.Kind, this is the GetNamespace plus a dot (.) plus the GetName.
        /// For a Tril.Models.Method, this is the long name of the declaring Kind plus two colons (::) plus the GetName plus the parameters section.
        /// For a Tril.Models.Property, this is the long name of the declaring Kind plus two colons (::) plus the GetName.
        /// For a Tril.Models.Field, this is the long name of the declaring Kind plus two colons (::) plus the GetName.
        /// For a Tril.Event, this is the long name of the declaring Kind plus two colons (::) plus the GetName.
        /// For a Tril.Models.Parameter, this is the long name of the parameter Kind (or the name if the parameter Kind is a generic place holder)
        /// plus space ( ) plus the GetName.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetLongName(params string[] targetPlats)
        {
            return GetLongName(false, targetPlats);
        }
        /// <summary>
        /// Returns the long name of this model.
        /// For a Tril.Models.Kind, this is the GetNamespace plus a dot (.) plus the GetName.
        /// For a Tril.Models.Method, this is the long name of the declaring Kind plus two colons (::) plus the GetName plus the parameters section.
        /// For a Tril.Models.Property, this is the long name of the declaring Kind plus two colons (::) plus the GetName.
        /// For a Tril.Models.Field, this is the long name of the declaring Kind plus two colons (::) plus the GetName.
        /// For a Tril.Models.Parameter, this is the long name of the parameter Kind (or the name if the parameter Kind is a generic place holder)
        /// plus space ( ) plus the GetName.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public abstract string GetLongName(bool useDefaultOnly, params string[] targetPlats);
        /// <summary>
        /// Returns a string representing the name of this model.
        /// If this model has user-defined name,
        /// the method returns a string representing the first matched name.
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return GetName("*");
        }
        /// <summary>
        /// Returns a string representing the name of this model.
        /// If this model has user-defined name,
        /// the method returns a string representing the first matched name.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string GetName(bool useDefaultOnly)
        {
            return GetName(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns a string representing the name of this model.
        /// If this model has user-defined name,
        /// the method returns a string representing the first matched name.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetName(params string[] targetPlats)
        {
            return GetName(false, targetPlats);
        }
        /// <summary>
        /// Returns a string representing the name of this model.
        /// If this model has user-defined name,
        /// the method returns a string representing the first matched name.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string GetName(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedName(targetPlats) && !useDefaultOnly)
            {
                IEnumerable<CustomAttribute> nameAttris = null;
                if (_underlyingInfo is TypeReference)
                {
                    nameAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.TypeNameAttribute");
                }
                else if (_underlyingInfo is MethodReference || _underlyingInfo is PropertyReference || _underlyingInfo is FieldReference || _underlyingInfo is EventReference)
                {
                    nameAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MemberNameAttribute");
                }
                else if (_underlyingInfo is ParameterReference)
                {
                    nameAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ParamNameAttribute");
                }
                foreach (CustomAttribute attri in nameAttris)
                {
                    string[] tgtLangsVal = GetTargetPlatforms(attri);
                    foreach (string lang in targetPlats)
                    {
                        if (tgtLangsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                        {
                            return attri.ConstructorArguments[0].Value.ToString();
                        }
                    }
                }
            }

            return GetName_Default(useDefaultOnly, targetPlats);
        }
        /// <summary>
        /// Returns the default name
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        protected abstract string GetName_Default(bool useDefaultOnly, params string[] targetPlats);
        ///// <summary>
        ///// Returns an array of string containing of all the user-defined target platforms of this model.
        ///// </summary>
        ///// <returns></returns>
        //public string[] GetTargetPlatforms()
        //{
        //    List<string> targetPlats = new List<string>();

        //    var forbidAttris = getAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.TargetAttribute");
        //    foreach (CustomAttribute attri in forbidAttris)
        //    {
        //        var proptargetPlats = attri.Properties.Single(p => p.Name == "TargetLanguages");
        //        string[] proptargetPlatsVal = (string[])proptargetPlats.Argument.Value;
        //        foreach (string lang in proptargetPlatsVal)
        //        {
        //            if (!targetPlats.Contains(lang))
        //                targetPlats.Add(lang);
        //        }
        //    }

        //    return targetPlats.ToArray();
        //}
        /// <summary>
        /// Gets a raw collection of all the attributes (without sugar-coating anything).
        /// </summary>
        /// <returns></returns>
        public abstract string[] GetRawAttributes();
        /// <summary>
        /// Returns the target platforms of the custom attributes
        /// </summary>
        /// <param name="attri"></param>
        /// <returns></returns>
        protected string[] GetTargetPlatforms(CustomAttribute attri)
        {
            string[] tgtPlatsVal = null;
            if (attri.ConstructorArguments.Count == 2 && attri.ConstructorArguments[1].Value != null
                && attri.ConstructorArguments[1].Value is CustomAttributeArgument[])
            {
                var tgtPlatsArgs = (CustomAttributeArgument[])attri.ConstructorArguments[1].Value;
                tgtPlatsVal = new string[tgtPlatsArgs.Length];
                for (int index = 0; index < tgtPlatsArgs.Length; index++)
                {
                    tgtPlatsVal[index] = tgtPlatsArgs[index].Value.ToString();
                }
            }
            else
            {
                tgtPlatsVal = new string[] { "*" };
            }

            return tgtPlatsVal;
        }
        /// <summary>
        /// Returns a value to determine if any custom attribute targeting any of the specified languages exists.
        /// </summary>
        /// <param name="customAttris"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        protected bool HasUserDefined(IEnumerable<CustomAttribute> customAttris, params string[] targetPlats)
        {
            foreach (CustomAttribute attri in customAttris)
            {
                string[] tgtPlatsVal = GetTargetPlatforms(attri);
                foreach (string lang in targetPlats)
                {
                    if (tgtPlatsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Gets a value indicating whether the annotations on this model were defined by the user 
        /// using Tril.Attributes.AnnotationAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedAnnotations()
        {
            return HasUserDefinedAnnotations("*");
        }
        /// <summary>
        /// Gets a value indicating whether the annotations on this model were defined by the user 
        /// using Tril.Attributes.AnnotationAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedAnnotations(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var annotAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.AnnotationAttribute");

            return HasUserDefined(annotAttris, targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether the attributes of this model were defined by the user using Tril.Attributes.AttributeAttribute
        /// </summary>
        public bool HasUserDefinedAttributes()
        {
            return HasUserDefinedAttributes("*");
        }
        /// <summary>
        /// Gets a value indicating whether the attributes of this model were defined by the user using Tril.Attributes.AttributeAttribute
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public bool HasUserDefinedAttributes(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (_underlyingInfo is MemberReference)
            {
                var attriAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.AttributeAttribute");

                return HasUserDefined(attriAttris, targetPlats);
            }
            else if (_underlyingInfo is ParameterReference)
            {
                var paramisAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ParamIsAttribute");

                return HasUserDefined(paramisAttris, targetPlats);
            }

            return false;
        }
        /// <summary>
        /// Gets a value indicating whether the comments on this model were defined by the user 
        /// using Tril.Attributes.CommentAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedComments()
        {
            return HasUserDefinedComments("*");
        }
        /// <summary>
        /// Gets a value indicating whether the comments on this model were defined by the user 
        /// using Tril.Attributes.CommentAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedComments(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var commAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.CommentAttribute");

            return HasUserDefined(commAttris, targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether the name of this model was defined by the user 
        /// using Tril.Attributes.TypeNameAttribute (for types, including generic parameters), 
        /// Tril.Attributes.MemberNameAttribute (for members other than types),
        /// and Tril.Attributes.ParamNameAttribute for parameters.
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedName()
        {
            return HasUserDefinedName("*");
        }
        /// <summary>
        /// Gets a value indicating whether the name of this model was defined by the user 
        /// using Tril.Attributes.TypeNameAttribute (for types, including generic parameters), 
        /// Tril.Attributes.MemberNameAttribute (for members other than types),
        /// and Tril.Attributes.ParamNameAttribute for parameters.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public bool HasUserDefinedName(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            IEnumerable<CustomAttribute> nameAttris = null;
            if (_underlyingInfo is TypeReference)
            {
                nameAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.TypeNameAttribute");
            }
            else if (_underlyingInfo is MethodReference || _underlyingInfo is PropertyReference || _underlyingInfo is FieldReference || _underlyingInfo is EventReference)
            {
                nameAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MemberNameAttribute");
            }
            else if (_underlyingInfo is ParameterReference)
            {
                nameAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ParamNameAttribute");
            }

            return HasUserDefined(nameAttris, targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether this model is hidden from any of the specified platforms
        /// </summary>
        /// <returns></returns>
        public bool IsHidden()
        {
            return IsHidden("*");
        }
        /// <summary>
        /// Gets a value indicating whether this model is hidden from any of the specified platforms
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public bool IsHidden(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var showAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ShowAttribute");
            if (HasUserDefined(showAttris, targetPlats))
                return false;
            
            var hideAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.HideAttribute");
            if (HasUserDefined(hideAttris, targetPlats))
                return true;

            return false;
        }

        ///// <summary>
        ///// Gets a value indicating whether at least one forbidden language was defined for this model by the user 
        ///// using Tril.ForbidAttribute
        ///// </summary>
        ///// <returns></returns>
        //public bool HasUserDefinedForbiddenLanguage
        //{
        //    get
        //    {
        //        return getAllCustomAttributes().Any(a => a != null && a.AttributeType.FullName == "Tril.Attributes.ForbidAttribute");
        //    }
        //}
        ///// <summary>
        ///// Gets a value indicating whether at least one target language was defined for this model by the user 
        ///// using Tril.TargetAttribute
        ///// </summary>
        ///// <returns></returns>
        //public bool HasUserDefinedTargetLanguage
        //{
        //    get
        //    {
        //        return getAllCustomAttributes().Any(a => a != null && a.AttributeType.FullName == "Tril.Attributes.TargetAttribute");
        //    }
        //}
    }
}
