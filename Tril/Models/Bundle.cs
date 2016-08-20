using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Mono.Cecil;
using Tril.Utilities;

namespace Tril.Models
{
    /// <summary>
    /// A collection of packages and other resources.
    /// </summary>
    [Serializable]
    public class Bundle
    {
        AssemblyDefinition underlying_assembly;
        Assembly net_assembly; //.NET assembly is used sometimes, e.g. to get resources

        /// <summary>
        /// Creates an instance of the Tril.Models.Bundle class.
        /// </summary>
        /// <param name="assembly">the underlying assembly from which this instance of Tril.Models.Bundle is built</param>
        public Bundle(AssemblyDefinition assembly)
        {
            if (assembly == null)
                throw new NullReferenceException("Specified assembly cannot be null!");

            underlying_assembly = assembly;
            //underlying_assembly;see if u can find a place to specify refed assemblies
        }
        /// <summary>
        /// Creates an instance of the Tril.Models.Bundle class.
        /// </summary>
        /// <param name="assemblyPath">the file path of the DLL/EXE containing the underlying assembly from which this instance of Tril.Models.Bundle is built</param>
        public Bundle(string assemblyPath)
            : this(AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters(ReadingMode.Immediate)))
        {
            ////load specified assembly refs
            //var asmRefsFile = FileSystemServices.GetAbsolutePath(@"a.r", asmPath);
            //if (File.Exists(asmRefsFile))
            //{
            //    string[] asmRefs = File.ReadAllLines(asmRefsFile).Where(ar => !string.IsNullOrWhiteSpace(ar))
            //        .Select(ar => FileSystemServices.GetAbsolutePath(ar, asmPath))
            //        .Where(ar => ar != asmPath).ToArray();
            //    foreach (string asmref in asmRefs)
            //    {
            //        domain.Load(AssemblyName.GetAssemblyName(asmref));
            //    }
            //}
        }

        /// <summary>
        /// Gets the .NET equivalent assembly
        /// </summary>
        /// <returns></returns>
        public Assembly GetDotNetAssembly() 
        {
            if (net_assembly == null)
                net_assembly = Assembly.LoadFrom(Location);

            return net_assembly;
        }

        //public void GetFiles() 
        //{
        //    var asm = GetDotNetAssembly();
        //    var files = asm.GetFiles();
        //    var files2 = asm.GetFiles(true);
        //    var resfiles = asm.GetManifestResourceNames();
        //    var aa = asm.GetReferencedAssemblies();
        //}

        /// <summary>
        /// Gets all the packages in the bundle
        /// </summary>
        /// <returns>returns an IEnumerable&lt;Package> object holding all the Tril.Models.Package objects in this bundle</returns>
        public IEnumerable<Package> GetPackages()
        {
            return GetPackages("*");
        }
        /// <summary>
        /// Gets all the packages in the bundle
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns>returns an IEnumerable&lt;Package> object holding all the Tril.Models.Package objects in this bundle</returns>
        public IEnumerable<Package> GetPackages(bool useDefaultOnly)
        {
            return GetPackages(useDefaultOnly, "*");
        }
        /// <summary>
        /// Gets all the packages in the bundle
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns>returns an IEnumerable&lt;Package> object holding all the Tril.Models.Package objects in this bundle</returns>
        public IEnumerable<Package> GetPackages(params string[] targetPlats)
        {
            return GetPackages(false, targetPlats);
        }
        /// <summary>
        /// Gets all the packages in the bundle
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns>returns an IEnumerable&lt;Package> object holding all the Tril.Models.Package objects in this bundle</returns>
        public IEnumerable<Package> GetPackages(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            List<Package> packages = new List<Package>();

            //loop through all the types
            //arrange them by their namespaces
            foreach (ModuleDefinition module in underlying_assembly.Modules)
            {
                foreach (TypeDefinition currentType in module.Types)
                {
                    if (currentType.IsNested) //ignore nested types; they will be treated when their parents are treated
                        continue;

                    try
                    {
                        Kind kind = Kind.GetCachedKind(currentType);
                        
                        Package package;
                        if (packages.Any(pkg => pkg.Namespace == kind.GetNamespace(useDefaultOnly, targetPlats)))
                        {
                            package = packages.First(pkg => pkg.Namespace == kind.GetNamespace(useDefaultOnly, targetPlats));
                            package.Add(kind, useDefaultOnly, targetPlats);
                        }
                        else
                        {
                            package = new Package(kind.GetNamespace(useDefaultOnly, targetPlats));
                            if (package.Add(kind, useDefaultOnly, targetPlats))
                                packages.Add(package);
                        }
                    }
                    catch { continue; }
                }
            }

            return packages.AsEnumerable<Package>();
        }

        /// <summary>
        /// Gets the location of the file from which this bundle was created.
        /// </summary>
        public string Location
        {
            get { return underlying_assembly.MainModule.FullyQualifiedName; }
        }
        /// <summary>
        /// Gets the simple name of the bundle
        /// </summary>
        public string Name
        {
            get { return underlying_assembly.Name.Name; }
        }
        /// <summary>
        /// Gets the underlying Mono.Cecil.AssemblyDefinition from which this instance of Tril.Models.Bundle was built
        /// </summary>
        public AssemblyDefinition UnderlyingAssembly
        {
            get { return underlying_assembly; }
        }
    }
}
