using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Collections.Generic;

namespace Tril.Models
{
    public partial class Model
    {
        /// <summary>
        /// The underlying data
        /// </summary>
        protected dynamic _underlyingInfo;

        /// <summary>
        /// Creates a new instance of Tril.Models.Model
        /// </summary>
        /// <param name="underlyingInfo"></param>
        public Model(dynamic underlyingInfo)
        {
            if (underlyingInfo == null)
                throw new NullReferenceException("Underlying object for model cannot be null!");

            _underlyingInfo = underlyingInfo;
        }

        /// <summary>
        /// Disposing
        /// </summary>
        public virtual void Dispose()
        {
            _underlyingInfo = null;
        }

        /// <summary>
        /// Replaces invalid characters in a name/identifier eith valid ones
        /// </summary>
        /// <param name="_name"></param>
        /// <returns></returns>
        protected string ReplaceInvalidChars(string _name) 
        {
            if (_name.Contains('`')) //which would be true for generic methods
            {
                _name = _name.Substring(0, _name.IndexOf('`'));
            }

            //for auto-generated members
            if (_name.Contains('<'))
                _name = _name.Replace('<', '_');
            if (_name.Contains('>'))
                _name = _name.Replace('>', '_');
            if (_name.Contains('$'))
                _name = _name.Replace('$', '_');
            if (_name.Contains('\''))
                _name = _name.Replace('\'', '_');

            //for names that are the same as keywords
            //I wont do anything for now because
            //1. some keywords are contextual, and some langs allow such names/identifiers; they use the position and/or context
            //      to determine if the id is a keyword or not
            //2. even C# allows ur id to be the same as classes, structs, interfase e.g. private Button Button = new Button();
            //      and some "keywords" are just aliases just structs e.g. int for Int32, long for Int64, ...
            //3. there are too many keywords, processing all of them might be an unnecessary waste of processor

            return _name;
        }

        private Collection<CustomAttribute> allCustomAttrs;
        /// <summary>
        /// Returns a collection of custom attributes
        /// </summary>
        /// <returns></returns>
        protected Collection<CustomAttribute> GetAllCustomAttributes()
        {
            if (allCustomAttrs == null)
            {
                dynamic namesake = GetNamesake();

                if (namesake != null)
                {
                    //if (namesake is IMemberDefinition)
                    //    allCustomAttrs = ((IMemberDefinition)namesake).CustomAttributes;
                    //else //if (namesake is ParameterDefinition)
                    //    allCustomAttrs = ((ParameterDefinition)namesake).CustomAttributes;
                    dynamic resolve = null;

                    if (namesake is TypeDefinition)
                        resolve = ((TypeDefinition)namesake);
                    else if (namesake is TypeReference)
                        resolve = ((TypeReference)namesake).Resolve();
                    else if (namesake is MethodDefinition)
                        resolve = ((MethodDefinition)namesake);
                    else if (namesake is MethodReference)
                        resolve = ((MethodReference)namesake).Resolve();
                    else if (namesake is PropertyDefinition)
                        resolve = ((PropertyDefinition)namesake);
                    else if (namesake is PropertyReference)
                        resolve = ((PropertyReference)namesake).Resolve();
                    else if (namesake is FieldDefinition)
                        resolve = ((FieldDefinition)namesake);
                    else if (namesake is FieldReference)
                        resolve = ((FieldReference)namesake).Resolve();
                    else if (namesake is EventDefinition)
                        resolve = ((EventDefinition)namesake);
                    else if (namesake is EventReference)
                        resolve = ((EventReference)namesake).Resolve();
                    else if (namesake is ParameterDefinition)
                        resolve = ((ParameterDefinition)namesake);
                    else if (namesake is ParameterReference)
                        resolve = ((ParameterReference)namesake).Resolve();

                    if (resolve != null)
                        allCustomAttrs = resolve.CustomAttributes;
                }
                else
                {
                    //if (_underlyingInfo is IMemberDefinition)
                    //    allCustomAttrs = ((IMemberDefinition)_underlyingInfo).CustomAttributes;
                    //else if (_underlyingInfo is TypeReference || _underlyingInfo is MethodReference || _underlyingInfo is PropertyReference
                    //    || _underlyingInfo is FieldReference || _underlyingInfo is EventReference || _underlyingInfo is ParameterReference) 
                    //{
                    //    dynamic resolve = null;

                    //    if (_underlyingInfo is TypeReference)
                    //        resolve = ((TypeReference)_underlyingInfo).Resolve();
                    //    else if (_underlyingInfo is MethodReference)
                    //        resolve = ((MethodReference)_underlyingInfo).Resolve();
                    //    else if (_underlyingInfo is PropertyReference)
                    //        resolve = ((PropertyReference)_underlyingInfo).Resolve();
                    //    else if (_underlyingInfo is FieldReference)
                    //        resolve = ((FieldReference)_underlyingInfo).Resolve();
                    //    else if (_underlyingInfo is EventReference)
                    //        resolve = ((EventReference)_underlyingInfo).Resolve();
                    //    else if (_underlyingInfo is ParameterReference)
                    //        resolve = ((ParameterReference)_underlyingInfo).Resolve();

                    //    if (resolve != null)
                    //        allCustomAttrs = resolve.CustomAttributes;
                    //}
                    //else //if (_underlyingInfo is ParameterDefinition)
                    //    allCustomAttrs = ((ParameterDefinition)_underlyingInfo).CustomAttributes;//MemberReference

                    dynamic resolve = null;

                    if (_underlyingInfo is TypeDefinition)
                        resolve = ((TypeDefinition)_underlyingInfo);
                    else if (_underlyingInfo is TypeReference)
                        resolve = ((TypeReference)_underlyingInfo).Resolve();
                    else if (_underlyingInfo is MethodDefinition)
                        resolve = ((MethodDefinition)_underlyingInfo);
                    else if (_underlyingInfo is MethodReference)
                        resolve = ((MethodReference)_underlyingInfo).Resolve();
                    else if (_underlyingInfo is PropertyDefinition)
                        resolve = ((PropertyDefinition)_underlyingInfo);
                    else if (_underlyingInfo is PropertyReference)
                        resolve = ((PropertyReference)_underlyingInfo).Resolve();
                    else if (_underlyingInfo is FieldDefinition)
                        resolve = ((FieldDefinition)_underlyingInfo);
                    else if (_underlyingInfo is FieldReference)
                        resolve = ((FieldReference)_underlyingInfo).Resolve();
                    else if (_underlyingInfo is EventDefinition)
                        resolve = ((EventDefinition)_underlyingInfo);
                    else if (_underlyingInfo is EventReference)
                        resolve = ((EventReference)_underlyingInfo).Resolve();
                    else if (_underlyingInfo is ParameterDefinition)
                        resolve = ((ParameterDefinition)_underlyingInfo);
                    else if (_underlyingInfo is ParameterReference)
                        resolve = ((ParameterReference)_underlyingInfo).Resolve();

                    if (resolve != null)
                        allCustomAttrs = resolve.CustomAttributes;
                }
            }

            if (allCustomAttrs == null)
                allCustomAttrs = new Collection<CustomAttribute>();
            return allCustomAttrs;
        }

        /// <summary>
        /// Returns the equivalent object to this object from the NamesakeAssembly
        /// </summary>
        /// <returns></returns>
        public abstract dynamic GetNamesake();
        /// <summary>
        /// Sets the namesake assembly from the specified path
        /// </summary>
        /// <param name="assemblyPath"></param>
        public static void SetNamesakeAssemblyPath(string assemblyPath) 
        {
            if (assemblyPath == null || assemblyPath.Trim() == "")
                return;

            try 
            {
                namesakeAsm = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters(ReadingMode.Immediate));
            }
            catch { }
        }
        static AssemblyDefinition namesakeAsm;
        /// <summary>
        /// Gets the assembly from which namesake objects are to be derived.
        /// </summary>
        protected static AssemblyDefinition NamesakeAssembly
        {
            get { return namesakeAsm; }
        }
        /// <summary>
        /// Returns the string equivalent of the model
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetLongName();
        }
    }
}
