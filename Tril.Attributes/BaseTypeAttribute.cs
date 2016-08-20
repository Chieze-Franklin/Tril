using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// The base class for attributes that determine the base types and interfaces of target types,
    /// as well as types for return and parameter values.
    /// </summary>
    public abstract class BaseTypeAttribute : TrilAttribute
    {
        string _typePath = "";

        /// <summary>
        /// Creates a new instance of the Tril.BaseTypesAttribute class
        /// </summary>
        /// <param name="typePath">the user-defined base class or interface for the target type</param>
        public BaseTypeAttribute(string typePath)
            :this(typePath, "*")
        {
        }
        /// <summary>
        /// Creates a new instance of the Tril.BaseTypesAttribute class
        /// </summary>
        /// <param name="typePath">the user-defined base class or interface for the target type</param>
        /// <param name="targetPlats"></param>
        public BaseTypeAttribute(string typePath, params string[] targetPlats)
            :base(targetPlats)
        {
            if (typePath == null)
                throw new NullReferenceException("Class path cannot be null!");

            _typePath = typePath.Trim();
        }

        //bool IsValidPath(string classPath)
        //{
        //    if (classPath == null)
        //        return false; //null obj paths are NOT allowed

        //    if (classPath.StartsWith(".") || classPath.EndsWith("."))
        //        return false;

        //    //when you split the string into parts by dot (.), each part must be a valid name
        //    if (classPath.Contains('.'))
        //    {
        //        foreach (string name in classPath.Split('.'))
        //        {
        //            if (!IsValidName(name))
        //                return false;
        //        }
        //    }
        //    else
        //    {
        //        if (!IsValidName(classPath))
        //            return false;
        //    }

        //    return true;
        //}

        //bool IsValidName(string name)
        //{
        //    //a valid name contains alphas, numes and underscore, plus it must not start with a nume plus it cannot be == "_"
        //    if (name == "_")
        //        return false;
        //    for (int index = 0; index < name.Length; index++)
        //    {
        //        if (index == 0 && char.IsDigit(name[index])) //first char cannot be a digit
        //            return false;
        //        //if the char is not a letter or digit and not underscore, and allow [ ] < > commas and '`' for generics
        //        if (!char.IsLetterOrDigit(name[index]) && name[index] != '_')
        //            return false;
        //    }

        //    return true;
        //}

        /// <summary>
        /// Gets the type path
        /// </summary>
        public string TypePath
        {
            get { return _typePath; }
        }
    }

    /// <summary>
    /// Represents the user-defined base class for the target type of the Tril.Attributes.ExtendsAttribute attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate,
        AllowMultiple = true, Inherited = false)]
    public sealed class ExtendsAttribute : BaseTypeAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.ExtendsAttribute class
        /// </summary>
        /// <param name="classPath">the user-defined base class of the target type</param>
        public ExtendsAttribute(string classPath)
            : this(classPath, "*")
        {
        }
        /// <summary>
        /// Creates a new instance of the Tril.ExtendsAttribute class
        /// </summary>
        /// <param name="classPath">the user-defined base class of the target type</param>
        /// <param name="targetPlats"></param>
        public ExtendsAttribute(string classPath, params string[] targetPlats)
            : base(classPath, targetPlats)
        {
        }
    }

    /// <summary>
    /// Represents the user-defined interface to be implemented by the target type of the Tril.Attributes.ImplementsAttribute attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate,
        AllowMultiple = true, Inherited = false)]
    public sealed class ImplementsAttribute : BaseTypeAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.ImplementsAttribute class
        /// </summary>
        /// <param name="interfacePath">the user-defined interface to be implemented by the target type</param>
        public ImplementsAttribute(string interfacePath)
            : this(interfacePath, "*")
        {
        }
        /// <summary>
        /// Creates a new instance of the Tril.ImplementsAttribute class
        /// </summary>
        /// <param name="interfacePath">the user-defined interface to be implemented by the target type</param>
        /// <param name="targetPlats"></param>
        public ImplementsAttribute(string interfacePath, params string[] targetPlats)
            : base(interfacePath, targetPlats)
        {
        }
    }

    /// <summary>
    /// Represents a namespace or package to be imported
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate,
        AllowMultiple = true, Inherited = false)]
    public sealed class ImportAttribute : BaseTypeAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.ImportAttribute class
        /// </summary>
        /// <param name="typePath">the user-defined imported type of the target type</param>
        public ImportAttribute(string typePath)
            : this(typePath, "*")
        {
        }
        /// <summary>
        /// Creates a new instance of the Tril.ImportAttribute class
        /// </summary>
        /// <param name="typePath">the user-defined imported type of the target type</param>
        /// <param name="targetPlats"></param>
        public ImportAttribute(string typePath, params string[] targetPlats)
            : base(typePath, targetPlats)
        {
        }

        //bool IsValidNamespace(string nameSpace)
        //{
        //    if (nameSpace == null)
        //        return false; //here, null namespaces are NOT allowed

        //    if (nameSpace.Trim() == "")
        //        return false;

        //    if (nameSpace.StartsWith(".") || nameSpace.EndsWith("."))
        //        return false;

        //    //when you split the string into parts by dot (.), each part must be a valid name
        //    foreach (string name in nameSpace.Split('.')) 
        //    {
        //        if (!IsValidName(name))
        //            return false;
        //    }

        //    return true;
        //}

        //bool IsValidName(string name)
        //{
        //    //a valid name contains alphas, numes and underscore, plus it must not start with a nume plus it cannot be == "_" or "*"
        //    if (name == "_"/* || name == "*"*/)
        //        return false;
        //    for (int index = 0; index < name.Length; index++)
        //    {
        //        if (index == 0 && char.IsDigit(name[index])) //first char cannot be a digit
        //            return false;
        //        //if the char is not a letter or digit and not underscore and not asterisk
        //        if (!char.IsLetterOrDigit(name[index]) && name[index] != '_' && name[index] != '*') 
        //            return false;
        //    }

        //    return true;
        //}
    }

    /// <summary>
    /// Represents the namespace a type should be placed under
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate,
        AllowMultiple = true, Inherited = false)]
    public sealed class NamespaceAttribute : BaseTypeAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.NamespaceAttribute class
        /// </summary>
        /// <param name="typePath">the namespace under which the target type should be placed</param>
        public NamespaceAttribute(string typePath)
            : this(typePath, "*")
        {
        }
        /// <summary>
        /// Creates a new instance of the Tril.NamespaceAttribute class
        /// </summary>
        /// <param name="typePath">the namespace under which the target type should be placed</param>
        /// <param name="targetPlats"></param>
        public NamespaceAttribute(string typePath, params string[] targetPlats)
            : base(typePath, targetPlats)
        {
        }

        //bool IsValidNamespace(string nameSpace) 
        //{
        //    if (nameSpace == null)
        //        return true; //null namespaces are allowed

        //    if (nameSpace.StartsWith(".") || nameSpace.EndsWith("."))
        //        return false;

        //    //when you split the string into parts by dot (.), each part must be a valid name
        //    foreach (string name in nameSpace.Split('.'))
        //    {
        //        if (!IsValidName(name))
        //            return false;
        //    }

        //    return true;
        //}

        //bool IsValidName(string name)
        //{
        //    //a valid name contains alphas, numes and underscore, plus it must not start with a nume plus it cannot be == "_"
        //    if (name == "_")
        //        return false;
        //    for (int index = 0; index < name.Length; index++)
        //    {
        //        if (index == 0 && char.IsDigit(name[index])) //first char cannot be a digit
        //            return false;
        //        if (!char.IsLetterOrDigit(name[index]) && name[index] != '_') //if the char is not a letter or digit and not underscore
        //            return false;
        //    }

        //    return true;
        //}
    }

    /// <summary>
    /// Represents the user-defined return type or data type for the target of the Tril.Attributes.MemberTypeAttribute attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Field,
        AllowMultiple = true, Inherited = true)]
    public sealed class MemberTypeAttribute : BaseTypeAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.Attributes.MemberTypeAttribute class
        /// </summary>
        /// <param name="classPath">the user-defined return type of the target method</param>
        public MemberTypeAttribute(string classPath)
            : this(classPath, "*")
        {
        }
        /// <summary>
        /// Creates a new instance of the Tril.Attributes.MemberTypeAttribute class
        /// </summary>
        /// <param name="typePath"></param>
        /// <param name="targetPlats"></param>
        public MemberTypeAttribute(string typePath, params string[] targetPlats)
            : base(typePath, targetPlats)
        {
        }
    }

    /// <summary>
    /// Represents the user-defined type for the target parameter of the Tril.Attributes.ParamTypeAttribute attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.GenericParameter,
        AllowMultiple = true, Inherited = true)]
    public sealed class ParamTypeAttribute : BaseTypeAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.Attributes.ParamTypeAttribute class
        /// </summary>
        /// <param name="typePath">the user-defined type of the target parameter</param>
        public ParamTypeAttribute(string typePath)
            : this(typePath, "*")
        {
        }
        /// <summary>
        /// Creates a new instance of the Tril.Attributes.ParamTypeAttribute class
        /// </summary>
        /// <param name="typePath">the user-defined type of the target parameter</param>
        /// <param name="targetPlats"></param>
        public ParamTypeAttribute(string typePath, params string[] targetPlats)
            : base(typePath, targetPlats)
        {
        }
    }

    /// <summary>
    /// Represents the user-defined generic parameter constraint that specifies the class that the target type of the 
    /// Tril.Attributes.MustExtendAttribute attribute must inherit from.
    /// </summary>
    [AttributeUsage(AttributeTargets.GenericParameter,
        AllowMultiple = true, Inherited = false)]
    public sealed class MustExtendAttribute : BaseTypeAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.Attributes.MustExtendAttribute class
        /// </summary>
        /// <param name="classPath">the user-defined base class of the target generic parameter type</param>
        public MustExtendAttribute(string classPath)
            : this(classPath, "*")
        {
        }
        /// <summary>
        /// Creates a new instance of the Tril.Attributes.MustExtendAttribute class
        /// </summary>
        /// <param name="classPath">the user-defined base class of the target generic parameter type</param>
        /// <param name="targetPlats"></param>
        public MustExtendAttribute(string classPath, params string[] targetPlats)
            : base(classPath, targetPlats)
        {
        }
    }

    /// <summary>
    /// Represents the user-defined generic parameter constraint that specifies an interface that the target type of the 
    /// Tril.Attributes.MustImplementAttribute attribute must implement.
    /// </summary>
    [AttributeUsage(AttributeTargets.GenericParameter,
        AllowMultiple = true, Inherited = false)]
    public sealed class MustImplementAttribute : BaseTypeAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.Attributes.MustImplementAttribute class
        /// </summary>
        /// <param name="interfacePath">a user-defined interface to be implemented by the target generic parameter type</param>
        public MustImplementAttribute(string interfacePath)
            : this(interfacePath, "*")
        {
        }
        /// <summary>
        /// Creates a new instance of the Tril.Attributes.MustImplementAttribute class
        /// </summary>
        /// <param name="interfacePath">a user-defined interface to be implemented by the target generic parameter type</param>
        /// <param name="targetPlats"></param>
        public MustImplementAttribute(string interfacePath, params string[] targetPlats)
            : base(interfacePath, targetPlats)
        {
        }
    }
}
