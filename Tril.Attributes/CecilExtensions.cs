/*
 * The MakeXXX extension methods belong to Jb Evain (jbevain@gmail.com)
 * Copyright: copyright (c) 2008 - 2011 Jb Evain
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Tril.Attributes
{
    /// <summary>
    /// Cecil extension methods
    /// </summary>
    public static class CecilExtensions
    {
        static TypeDefinition GetTypeDefinition(Type type, TypeDefinition typeDef)
        {
            /*
             * For nested types, mono uses '/' while C# uses '+', so I replace '/' with '+'
             */

            if (typeDef.FullName.Replace('/', '+') == type.FullName)
            {
                return typeDef;
            }
            foreach (TypeDefinition t in typeDef.NestedTypes)
            {
                TypeDefinition td = GetTypeDefinition(type, t);
                if (td != null)
                    return td;
            }

            return null;
        }

        /// <summary>
        /// Returns an array type reference of the specified type reference
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static ArrayType MakeArrayType(this TypeReference self) 
        {
            return new ArrayType(self);
        }
        /// <summary>
        /// Returns an array type reference of the specified type reference
        /// </summary>
        /// <param name="self"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        public static ArrayType MakeArrayType(this TypeReference self, int rank)
        {
            if (rank == 0)
                throw new ArgumentOutOfRangeException("Array rank cannot be zero!");

            var array = new ArrayType(self);
            for (int i = 1; i < rank; i++)
                array.Dimensions.Add(new ArrayDimension());

            return array;
        }
        /// <summary>
        /// Returns a pointer type reference of the specified type reference
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static PointerType MakePointerType(this TypeReference self)
        {
            return new PointerType(self);
        }
        /// <summary>
        /// Returns a by ref type reference of the specified type reference
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static ByReferenceType MakeByReferenceType(this TypeReference self)
        {
            return new ByReferenceType(self);
        }
        /// <summary>
        /// Returns an optional modifier type reference of the specified type reference
        /// </summary>
        /// <param name="self"></param>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public static OptionalModifierType MakeOptionalModifierType(this TypeReference self, TypeReference modifier)
        {
            return new OptionalModifierType(modifier, self);
        }
        /// <summary>
        /// Returns a required modifier type reference of the specified type reference
        /// </summary>
        /// <param name="self"></param>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public static RequiredModifierType MakeRequiredModifierType(this TypeReference self, TypeReference modifier)
        {
            return new RequiredModifierType(modifier, self);
        }
        /// <summary>
        /// Returns a generic instance type reference of the specified type reference
        /// </summary>
        /// <param name="self"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static GenericInstanceType MakeGenericInstanceType(this TypeReference self, params TypeReference[] arguments)
        {
            if (self == null)
                throw new ArgumentNullException("Type reference cannot be null!");
            if (arguments == null)
                throw new ArgumentNullException("Type reference arguments cannot be null!");
            if (arguments.Length == 0)
                throw new ArgumentException("Type reference arguments cannot be zero in number!");
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException("Type reference argument mismatch!");

            var instance = new GenericInstanceType(self);

            foreach (var arg in arguments)
                instance.GenericArguments.Add(arg);

            return instance;
        }
        /// <summary>
        /// Returns a pinned type reference of the specified type reference
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static PinnedType MakePinnedType(this TypeReference self)
        {
            return new PinnedType(self);
        }
        /// <summary>
        /// Returns a sentinel type reference of the specified type reference
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static SentinelType MakeSentinelType(this TypeReference self)
        {
            return new SentinelType(self);
        }

        #region From DotNet to Mono (These are my codes, not Evain's)
        static Dictionary<string, AssemblyDefinition> cachedAssemblyDefs = new Dictionary<string, AssemblyDefinition>();

        /// <summary>
        /// Converts a System.Type object to a Mono.Cecil.TypeReference
        /// </summary>
        /// <param name="type"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        public static TypeDefinition ToTypeDefinition(this Type type, bool throwException = false)
        {
            try
            {
                if (type == null)
                    return null;

                string asmPath = type.Assembly.Location;
                AssemblyDefinition asmDef = null;
                if (cachedAssemblyDefs.ContainsKey(asmPath.ToLower()))
                    asmDef = cachedAssemblyDefs[asmPath.ToLower()];
                else
                {
                    asmDef =
                    AssemblyDefinition.ReadAssembly(type.Assembly.Location, new ReaderParameters(ReadingMode.Immediate));
                    cachedAssemblyDefs.Add(asmPath.ToLower(), asmDef);
                }
                TypeDefinition typeDef = null;
                foreach (TypeDefinition t in asmDef.MainModule.Types) //first look for it in the main module
                {
                    typeDef = GetTypeDefinition(type, t);
                    if (typeDef != null)
                        break;
                }
                if (typeDef == null) //look for the type in other modules
                {
                    for (int index = 0; index < asmDef.Modules.Count(); index++)
                    {
                        foreach (TypeDefinition t in asmDef.Modules[index].Types)
                        {
                            typeDef = GetTypeDefinition(type, t);
                            if (typeDef != null)
                                goto outerBreak;
                        }
                    }
                outerBreak: ;
                }
                if (typeDef == null && throwException)
                    throw new Exception("Could not convert System.Type to Mono.Cecil.TypeReference");

                return typeDef;
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }

        public static MethodDefinition ToMethodDefinition(this MethodBase method, bool throwException = false) 
        {
            try 
            {
                TypeDefinition typeDef = method.DeclaringType.ToTypeDefinition(true);
                MethodDefinition methodDef = null;
                MethodDefinition[] possibleMethodDefs = typeDef.Methods.Where((md, n) => md.Name == method.Name).ToArray();
                foreach (MethodDefinition md in possibleMethodDefs) 
                {
                    //compare names
                    if (md.Name != method.Name)
                        continue;

                    //compare return types
                    if (method is MethodInfo) 
                    {
                        if (md.ReturnType.Name != (method as MethodInfo).ReturnType.Name)
                            continue;
                        //does not work for generics and nested types
                        if (!md.ReturnType.IsGenericParameter && !md.ReturnType.IsNested)
                        {
                            if (md.ReturnType.Namespace != (method as MethodInfo).ReturnType.Namespace)
                                continue;
                        }

                        //compare if they are both generic 
                        //doesnt work when u r comparing non-generic method to a generic one in a generic type
                        //reason: .NET seems to count all methods in generic types as generic (even the non-generic ones)
                        //if (md.HasGenericParameters != method.ContainsGenericParameters)
                        //    return false;

                        if (md.HasGenericParameters != (method.IsGenericMethod || method.IsGenericMethodDefinition))
                            continue;

                        //this if is important so that non-generic methods (like constructors which throw exceptions) dont enter here
                        if (md.HasGenericParameters)
                        {
                            //compare number of generic parameters
                            if (md.GenericParameters.Count != method.GetGenericArguments().Count())
                                continue;
                        }
                    }

                    //compare number of parameters
                    if (md.Parameters.Count != method.GetParameters().Count())
                        continue;

                    //compare types of each parameter
                    bool foundMatchingParams = false;
                    ParameterInfo[] methodParams = method.GetParameters();
                    for (int index = 0; index < md.Parameters.Count; index++)
                    {
                        if (md.Parameters[index].ParameterType.Name != methodParams[index].ParameterType.Name)
                            continue;
                        //does not work for generics and nested types
                        if (!md.Parameters[index].ParameterType.IsGenericParameter && !md.Parameters[index].ParameterType.IsNested)
                        {
                            if (md.Parameters[index].ParameterType.Namespace != methodParams[index].ParameterType.Namespace)
                                continue;
                        }

                        foundMatchingParams = true;
                        break;
                    }
                    if (md.Parameters.Count == 0)
                        foundMatchingParams = true;
                    if (!foundMatchingParams)
                        continue;

                    //a match
                    methodDef = md;
                    break;
                }

                if (methodDef == null)
                    throw new Exception("Method could not be found!");

                return methodDef;
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }
        #endregion

        #region From Mono to DotNet (These are my codes, not Evain's)
        static Dictionary<string, Assembly> cachedAssemblies = new Dictionary<string, Assembly>();

        static void AddSearchDirectories(ref DefaultAssemblyResolver resolver, string startDirectory)
        {
            startDirectory = startDirectory.Trim();

            //first add the containing folder of the current module
            if (Directory.Exists(startDirectory))
                resolver.AddSearchDirectory(startDirectory);

            //then add all paths in the r.r file, each path being on a separate line
            if (File.Exists(startDirectory + @"\r.r"))
            {
                string[] allSearchDirs = File.ReadAllLines(startDirectory + @"\r.r").Where(d => !string.IsNullOrWhiteSpace(d))
                            .Select(d => GetAbsolutePath(d, startDirectory))
                            .Where(d => Directory.Exists(d)).ToArray();
                foreach (string searchDir in allSearchDirs)
                {
                    resolver.AddSearchDirectory(searchDir);
                }
            }
        }

        static Assembly ToAssembly(this AssemblyDefinition asmDef, bool throwException = false) { return null; } //not implemented yet

        static Module ToModule(this ModuleDefinition modDef, bool throwException = false) { return null; } //not implemented yet
        static Module ToModule(this ModuleReference modRef, bool throwException = false) { return null; } //not implemented yet

        public static Type ToType(this TypeDefinition typeDef, bool throwException = false)
        {
            try
            {
                string asmPath = typeDef.Module.FullyQualifiedName;
                Assembly asm = null;
                if (cachedAssemblies.ContainsKey(asmPath.ToLower()))
                    asm = cachedAssemblies[asmPath.ToLower()];
                else
                {
                    asm = Assembly.LoadFrom(asmPath);
                    cachedAssemblies.Add(asmPath.ToLower(), asm);
                }

                //mono uses "/" for nested types while .net uses "+"
                //var tt = asm.GetType(typeDef.FullName.Replace('/', '+')); //for testing
                return asm.GetType(typeDef.FullName.Replace('/', '+'));
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }
        public static Type ToType(this TypeReference typeRef, bool throwException = false)
        {
            try
            {
                if (typeRef.IsGenericParameter)
                {
                    GenericParameter genParam = (GenericParameter)typeRef;
                    var owner = genParam.Owner;
                    if (owner is MethodDefinition)
                    {
                        MethodBase method = ((MethodDefinition)owner).ToMethod(throwException);
                        Type t = method.GetGenericArguments()[genParam.Position];
                        return t;
                    }
                    else if (owner is TypeDefinition)
                    {
                        Type type = ((TypeDefinition)owner).ToType(throwException);
                        Type t = type.GetGenericArguments()[genParam.Position];
                        return t;
                    }
                    //unlikely conditions below
                    else if (owner is MethodReference)
                    {
                        MethodBase method = ((MethodReference)owner).ToMethod(throwException);
                        Type t = method.GetGenericArguments()[genParam.Position];
                        return t;
                    }
                    else if (owner is TypeReference)
                    {
                        Type type = ((TypeReference)owner).ToType(throwException);
                        Type t = type.GetGenericArguments()[genParam.Position];
                        return t;
                    }
                    throw new NotSupportedException("Generic type provider not supported!");
                }
                else
                    return typeRef.Resolve().ToType(throwException);
            }
            catch (AssemblyResolutionException asmEx)
            {
                try
                {
                    DefaultAssemblyResolver asmRes = new DefaultAssemblyResolver(); //to help us resolve referenced assemblies

                    FileInfo containingModule = new FileInfo(typeRef.Module.FullyQualifiedName);
                    AddSearchDirectories(ref asmRes, containingModule.DirectoryName);

                    //then get the asm, type
                    var resolvedAsm = asmRes.Resolve(asmEx.AssemblyReference);
                    var requiredType = resolvedAsm.MainModule.GetType(typeRef.FullName);
                    if (requiredType == null) //look for the type in other modules
                    {
                        for (int index = 0; index < resolvedAsm.Modules.Count(); index++)
                        {
                            requiredType = resolvedAsm.Modules[index].GetType(typeRef.FullName);
                            if (requiredType != null)
                                break;
                        }
                    }
                    //return the type
                    if (requiredType == null)
                        throw new InvalidOperationException("No matching type found in the sequence!");
                    else
                        return requiredType.ToType(throwException);
                }
                catch
                {
                    if (throwException)
                        throw;
                    return null;
                }
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }

        public static MethodBase ToMethod(this MethodDefinition mthdDef, bool throwException = false)
        {
            try
            {
                Type declaringType = mthdDef.DeclaringType.ToType(throwException);
                List<MethodBase> allMethodsAndConstructors = new List<MethodBase>();
                //I find that searching for only the methods declared in the type is the right thing to do
                allMethodsAndConstructors.
                    AddRange(declaringType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
                allMethodsAndConstructors.
                    AddRange(declaringType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

                MethodBase method = null;

                #region Method 2
                MethodBase[] possibleMethods = allMethodsAndConstructors.Where((m, n) => m.Name == mthdDef.Name).ToArray();
                foreach (MethodBase m in possibleMethods)
                {
                    //compare names
                    if (mthdDef.Name != m.Name)
                        continue;

                    //compare return types
                    if (m is MethodInfo)
                    {
                        if (mthdDef.ReturnType.Name != (m as MethodInfo).ReturnType.Name)
                            continue;
                        //does not work for generics and nested types
                        if (!mthdDef.ReturnType.IsGenericParameter && !mthdDef.ReturnType.IsNested)
                        {
                            if (mthdDef.ReturnType.Namespace != (m as MethodInfo).ReturnType.Namespace)
                                continue;
                        }

                        //compare if they are both generic 
                        //doesnt work when u r comparing non-generic method to a generic one in a generic type
                        //reason: .NET seems to count all methods in generic types as generic (even the non-generic ones)
                        //if (mthdDef.HasGenericParameters != m.ContainsGenericParameters)
                        //    return false;
                        if (mthdDef.HasGenericParameters != (m.IsGenericMethod || m.IsGenericMethodDefinition))
                            continue;

                        //this if is important so that non-generic methods (like constructors which throw exceptions) dont enter here
                        if (mthdDef.HasGenericParameters)
                        {
                            //compare number of generic parameters
                            if (mthdDef.GenericParameters.Count != m.GetGenericArguments().Count())
                                continue;
                        }
                    }

                    //compare number of parameters
                    if (mthdDef.Parameters.Count != m.GetParameters().Count())
                        continue;

                    //compare types of each parameter
                    bool foundMatchingParams = false;
                    ParameterInfo[] methodParams = m.GetParameters();
                    for (int index = 0; index < mthdDef.Parameters.Count; index++)
                    {
                        if (mthdDef.Parameters[index].ParameterType.Name != methodParams[index].ParameterType.Name)
                            continue;
                        //does not work for generics and nested types
                        if (!mthdDef.Parameters[index].ParameterType.IsGenericParameter && !mthdDef.Parameters[index].ParameterType.IsNested)
                        {
                            if (mthdDef.Parameters[index].ParameterType.Namespace != methodParams[index].ParameterType.Namespace)
                                continue;
                        }

                        foundMatchingParams = true;
                        break;
                    }
                    if (mthdDef.Parameters.Count == 0)
                        foundMatchingParams = true;
                    if (!foundMatchingParams)
                        continue;

                    //a match
                    method = m;
                    break;
                }

                if (method == null)
                    throw new InvalidOperationException("No matching method found in the sequence!");
                #endregion

                #region Method 1
                //method = allMethodsAndConstructors.Single
                //    (m =>
                //    {
                //        //compare names
                //        if (mthdDef.Name != m.Name)
                //            return false;

                //        //compare return types
                //        if (m is MethodInfo)
                //        {
                //            if (mthdDef.ReturnType.Name != (m as MethodInfo).ReturnType.Name)
                //                return false;
                //            //does not work for generics and nested types
                //            if (!mthdDef.ReturnType.IsGenericParameter && !mthdDef.ReturnType.IsNested)
                //            {
                //                if (mthdDef.ReturnType.Namespace != (m as MethodInfo).ReturnType.Namespace)
                //                    return false;
                //            }

                //            //compare if they are both generic 
                //            //doesnt work when u r comparing non-generic method to a generic one in a generic type
                //            //reason: .NET seems to count all methods in generic types as generic (even the non-generic ones)
                //            //if (mthdDef.HasGenericParameters != m.ContainsGenericParameters)
                //            //    return false;
                //            if (mthdDef.HasGenericParameters != (m.IsGenericMethod || m.IsGenericMethodDefinition))
                //                return false;

                //            //this if is important so that non-generic methods (like constructors which throw exceptions) dont enter here
                //            if (mthdDef.HasGenericParameters)
                //            {
                //                //compare number of generic parameters
                //                if (mthdDef.GenericParameters.Count != m.GetGenericArguments().Count())
                //                    return false;
                //            }
                //        }

                //        //compare number of parameters
                //        if (mthdDef.Parameters.Count != m.GetParameters().Count())
                //            return false;

                //        //compare types of each parameter
                //        ParameterInfo[] methodParams = m.GetParameters();
                //        for (int index = 0; index < mthdDef.Parameters.Count; index++)
                //        {
                //            if (mthdDef.Parameters[index].ParameterType.Name != methodParams[index].ParameterType.Name)
                //                return false;
                //            //does not work for generics and nested types
                //            if (!mthdDef.Parameters[index].ParameterType.IsGenericParameter && !mthdDef.Parameters[index].ParameterType.IsNested)
                //            {
                //                if (mthdDef.Parameters[index].ParameterType.Namespace != methodParams[index].ParameterType.Namespace)
                //                    return false;
                //            }
                //        }

                //        //a match
                //        return true;
                //    }
                //    );
                #endregion

                return method;
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }
        public static MethodBase ToMethod(this MethodReference mthdRef, bool throwException = false)
        {
            try
            {
                return mthdRef.Resolve().ToMethod(throwException);
            }
            catch (NullReferenceException)
            {
                Type declaringType = mthdRef.DeclaringType.ToType(throwException);
                List<MethodBase> allMethodsAndConstructors = new List<MethodBase>();
                //I find that searching for only the methods declared in the type is the right thing to do
                allMethodsAndConstructors.
                    AddRange(declaringType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
                allMethodsAndConstructors.
                    AddRange(declaringType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

                MethodBase method = null;
                MethodBase[] possibleMethods = allMethodsAndConstructors.Where((m, n) => m.Name == mthdRef.Name).ToArray();
                foreach (MethodBase m in possibleMethods)
                {
                    //compare names
                    if (mthdRef.Name != m.Name)
                        continue;

                    //compare return types
                    if (m is MethodInfo)
                    {
                        if (mthdRef.ReturnType.Name != (m as MethodInfo).ReturnType.Name)
                            continue;
                        //does not work for generics and nested types
                        if (!mthdRef.ReturnType.IsGenericParameter && !mthdRef.ReturnType.IsNested)
                        {
                            if (mthdRef.ReturnType.Namespace != (m as MethodInfo).ReturnType.Namespace)
                                continue;
                        }

                        //compare if they are both generic 
                        //doesnt work when u r comparing non-generic method to a generic one in a generic type
                        //reason: .NET seems to count all methods in generic types as generic (even the non-generic ones)
                        //if (mthdRef.HasGenericParameters != m.ContainsGenericParameters)
                        //    return false;
                        if (mthdRef.HasGenericParameters != (m.IsGenericMethod || m.IsGenericMethodDefinition))
                            continue;

                        //this if is important so that non-generic methods (like constructors which throw exceptions) dont enter here
                        if (mthdRef.HasGenericParameters)
                        {
                            //compare number of generic parameters
                            if (mthdRef.GenericParameters.Count != m.GetGenericArguments().Count())
                                continue;
                        }
                    }

                    //compare number of parameters
                    if (mthdRef.Parameters.Count != m.GetParameters().Count())
                        continue;

                    //compare types of each parameter
                    bool foundMatchingParams = false;
                    ParameterInfo[] methodParams = m.GetParameters();
                    for (int index = 0; index < mthdRef.Parameters.Count; index++)
                    {
                        if (mthdRef.Parameters[index].ParameterType.Name != methodParams[index].ParameterType.Name)
                            continue;
                        //does not work for generics and nested types
                        if (!mthdRef.Parameters[index].ParameterType.IsGenericParameter && !mthdRef.Parameters[index].ParameterType.IsNested)
                        {
                            if (mthdRef.Parameters[index].ParameterType.Namespace != methodParams[index].ParameterType.Namespace)
                                continue;
                        }

                        foundMatchingParams = true;
                        break;
                    }
                    if (mthdRef.Parameters.Count == 0)
                        foundMatchingParams = true;
                    if (!foundMatchingParams)
                        continue;

                    //a match
                    method = m;
                    break;
                }

                if (method == null)
                    throw new InvalidOperationException("No matching method found in the sequence!");

                return method;
            }
            catch (AssemblyResolutionException asmEx)
            {
                try
                {
                    DefaultAssemblyResolver asmRes = new DefaultAssemblyResolver(); //to help us resolve referenced assemblies

                    FileInfo containingModule = new FileInfo(mthdRef.DeclaringType.Module.FullyQualifiedName);
                    AddSearchDirectories(ref asmRes, containingModule.DirectoryName);

                    //then get the asm, type, methods
                    var resolvedAsm = asmRes.Resolve(asmEx.AssemblyReference);
                    var requiredType = resolvedAsm.MainModule.GetType(mthdRef.DeclaringType.FullName);
                    MethodDefinition requiredMthd = null;
                    MethodDefinition[] possibleMethodDefs = requiredType.Methods.Where((md, n) => md.Name == mthdRef.Name).ToArray();
                    foreach (MethodDefinition md in possibleMethodDefs)
                    {
                        //compare names
                        if (mthdRef.Name != md.Name)
                            continue;

                        //compare return types
                        if (mthdRef.ReturnType != null)
                        {
                            //does not work for generics
                            //might not work for nested types
                            if (!mthdRef.ReturnType.IsGenericParameter && !mthdRef.ReturnType.IsNested)
                            {
                                if ((mthdRef.ReturnType.FullName != md.ReturnType.FullName))
                                    continue;
                            }
                        }

                        //compare generic parameters
                        if (mthdRef.HasGenericParameters != md.HasGenericParameters)
                            continue;
                        if (mthdRef.HasGenericParameters)
                        {
                            if (mthdRef.GenericParameters.Count != md.GenericParameters.Count)
                                continue;
                        }

                        //compare number of parameters
                        if (mthdRef.Parameters.Count != md.Parameters.Count)
                            continue;

                        //compare types of parameters
                        bool foundMatchingParams = false;
                        for (int index = 0; index < mthdRef.Parameters.Count; index++)
                        {
                            //does not work for generics
                            //might not work for nested types
                            if (!mthdRef.Parameters[index].ParameterType.IsGenericParameter && !mthdRef.Parameters[index].ParameterType.IsNested)
                            {
                                if (mthdRef.Parameters[index].ParameterType.FullName != md.Parameters[index].ParameterType.FullName)
                                    continue;
                            }

                            foundMatchingParams = true;
                            break;
                        }
                        if (mthdRef.Parameters.Count == 0)
                            foundMatchingParams = true;
                        if (!foundMatchingParams)
                            continue;

                        //a match
                        requiredMthd = md;
                        break;
                    }

                    //return the method
                    if (requiredMthd == null)
                        throw new InvalidOperationException("No matching method found in the sequence!");
                    else
                        return requiredMthd.ToMethod(throwException);
                }
                catch
                {
                    if (throwException)
                        throw;
                    return null;
                }
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }

        public static PropertyInfo ToProperty(this PropertyDefinition propDef, bool throwException = false)
        {
            try
            {
                Type declaringType = propDef.DeclaringType.ToType(throwException);
                //I find that searching for only the properties declared in the type is the right thing to do
                PropertyInfo property = declaringType.GetProperty
                    (propDef.Name, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                return property;
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }
        public static PropertyInfo ToProperty(this PropertyReference propRef, bool throwException = false)
        {
            try
            {
                return propRef.Resolve().ToProperty(throwException);
            }
            catch (AssemblyResolutionException asmEx)
            {
                try
                {
                    DefaultAssemblyResolver asmRes = new DefaultAssemblyResolver(); //to help us resolve referenced assemblies

                    FileInfo containingModule = new FileInfo(propRef.DeclaringType.Module.FullyQualifiedName);
                    AddSearchDirectories(ref asmRes, containingModule.DirectoryName);

                    //then get the asm, type, methods
                    var resolvedAsm = asmRes.Resolve(asmEx.AssemblyReference);
                    var requiredType = resolvedAsm.MainModule.GetType(propRef.DeclaringType.FullName);
                    PropertyDefinition requiredProp = requiredType.Properties.Single(prop => prop.Name == propRef.Name);

                    //return the method
                    if (requiredProp == null)
                        throw new InvalidOperationException("No matching property found in the sequence!");
                    else
                        return requiredProp.ToProperty(throwException);
                }
                catch
                {
                    if (throwException)
                        throw;
                    return null;
                }
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }

        public static FieldInfo ToField(this FieldDefinition fieldDef, bool throwException = false)
        {
            try
            {
                Type declaringType = fieldDef.DeclaringType.ToType(throwException);
                //I find that searching for only the properties declared in the type is the right thing to do
                FieldInfo field = declaringType.GetField
                    (fieldDef.Name, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                return field;
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }
        public static FieldInfo ToField(this FieldReference fieldRef, bool throwException = false)
        {
            try
            {
                return fieldRef.Resolve().ToField(throwException);
            }
            catch (AssemblyResolutionException asmEx)
            {
                try
                {
                    DefaultAssemblyResolver asmRes = new DefaultAssemblyResolver(); //to help us resolve referenced assemblies

                    FileInfo containingModule = new FileInfo(fieldRef.DeclaringType.Module.FullyQualifiedName);
                    AddSearchDirectories(ref asmRes, containingModule.DirectoryName);

                    //then get the asm, type, methods
                    var resolvedAsm = asmRes.Resolve(asmEx.AssemblyReference);
                    var requiredType = resolvedAsm.MainModule.GetType(fieldRef.DeclaringType.FullName);
                    FieldDefinition requiredFld = requiredType.Fields.Single(fld => fld.Name == fieldRef.Name);

                    //return the method
                    if (requiredFld == null)
                        throw new InvalidOperationException("No matching field found in the sequence!");
                    else
                        return requiredFld.ToField(throwException);
                }
                catch
                {
                    if (throwException)
                        throw;
                    return null;
                }
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }

        public static ParameterInfo ToParameter(this ParameterDefinition paramDef, MethodBase method, bool throwException = false)
        {
            try
            {
                ParameterInfo parameter = method.GetParameters()[paramDef.Index];
                return parameter;
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }
        public static ParameterInfo ToParameter(this ParameterDefinition paramDef, MethodDefinition method, bool throwException = false)
        {
            try
            {
                return paramDef.ToParameter(method.ToMethod(throwException), throwException);
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }
        public static ParameterInfo ToParameter(this ParameterDefinition paramDef, MethodReference method, bool throwException = false)
        {
            try
            {
                return paramDef.ToParameter(method.ToMethod(throwException), throwException);
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }
        public static ParameterInfo ToParameter(this ParameterReference paramRef, MethodBase method, bool throwException = false)
        {
            try
            {
                return paramRef.Resolve().ToParameter(method, throwException);
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }
        public static ParameterInfo ToParameter(this ParameterReference paramRef, MethodDefinition method, bool throwException = false)
        {
            try
            {
                return paramRef.Resolve().ToParameter(method.ToMethod(throwException), throwException);
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }
        public static ParameterInfo ToParameter(this ParameterReference paramRef, MethodReference method, bool throwException = false)
        {
            try
            {
                return paramRef.Resolve().ToParameter(method.ToMethod(throwException), throwException);
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }

        static LocalVariableInfo ToVariable(this VariableDefinition varDef, bool throwException = false) { return null; } //not implemented yet
        static LocalVariableInfo ToVariable(this VariableReference varRef, bool throwException = false)
        {
            try
            {
                return varRef.Resolve().ToVariable(throwException);
            }
            catch
            {
                if (throwException)
                    throw;
                return null;
            }
        }
        #endregion

        //----------------------
        static string GetAbsolutePath(string mayBeRelativePath, string baseDirectory = null)
        {
            if (baseDirectory == null)
                baseDirectory = Environment.CurrentDirectory;
            else if (File.Exists(baseDirectory)) //if the base dir is actually a file and not a dir
                baseDirectory = new FileInfo(baseDirectory).DirectoryName;

            var root = Path.GetPathRoot(mayBeRelativePath);
            if (string.IsNullOrEmpty(root))
                return Path.GetFullPath(Path.Combine(baseDirectory, mayBeRelativePath));
            if (root == "\\")
                return Path.GetFullPath(Path.Combine(Path.GetPathRoot(baseDirectory), mayBeRelativePath.Remove(0, 1)));

            return mayBeRelativePath;
        }
    }
}
