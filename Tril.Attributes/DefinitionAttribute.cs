using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// The base class for all attributes that define their targets
    /// </summary>
    public abstract class DefinitionAttribute : TrilAttribute
    {
        string _definition = "";

        /// <summary>
        /// Creates a new instance of the Tril.DefinitionAttribute class
        /// </summary>
        /// <param name="definition">the user-defined "definition" of the target</param>
        public DefinitionAttribute(string definition)
            : this(definition, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.DefinitionAttribute class
        /// </summary>
        /// <param name="definition">the user-defined "definition" of the target</param>
        /// <param name="targetPlats"></param>
        public DefinitionAttribute(string definition, params string[] targetPlats) 
            :base(targetPlats)
        {
            _definition = definition == null ? "" : definition.Trim();
        }

        /// <summary>
        /// Gets the user-defined "definition" of the target
        /// </summary>
        public string Definition
        {
            get { return _definition; }
        }
    }

    /// <summary>
    /// Represents the "meaning" of a type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Delegate,
        AllowMultiple = true, Inherited = true)]
    public sealed class TypeIsAttribute : DefinitionAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.TypeIsAttribute class
        /// </summary>
        /// <param name="definition">the user-defined "definition" of the target parameter type</param>
        public TypeIsAttribute(string definition)
            : this(definition, "*")
        { }
        /// <summary>
        /// Creates a new instance of the Tril.TypeIsAttribute class
        /// </summary>
        /// <param name="definition">the user-defined "definition" of the target parameter type</param>
        /// <param name="targetPlats"></param>
        public TypeIsAttribute(string definition, params string[] targetPlats)
            : base(definition, targetPlats)
        { }

        /// <summary>
        /// Creates a new instance of the Tril.TypeIsAttribute class
        /// </summary>
        /// <param name="type"></param>
        public TypeIsAttribute(Types type)
            : this(type, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.TypeIsAttribute class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="targetPlats"></param>
        public TypeIsAttribute(Types type, params string[] targetPlats)
            : base(
            (type == Types.Class) ? "class" :
            ((type == Types.Enum) ? "enum" :
            ((type == Types.Interface) ? "interface" :
            ((type == Types.Struct) ? "struct" : ""))), targetPlats)
        { }
    }
    /// <summary>
    /// Lists the recognized type definitions
    /// </summary>
    public enum Types
    {
        /// <summary>
        /// class
        /// </summary>
        Class,
        /// <summary>
        /// enum
        /// </summary>
        Enum,
        /// <summary>
        /// interface
        /// </summary>
        Interface,
        /// <summary>
        /// struct
        /// </summary>
        Struct
    }

    /// <summary>
    /// Represents the user-defined description for the target parameter of the Tril.Attributes.ParamIsAttribute attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter,
        AllowMultiple = true, Inherited = true)]
    public sealed class ParamIsAttribute : DefinitionAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.Attributes.ParamIsAttribute class
        /// </summary>
        /// <param name="definition">the user-defined "definition" of the target parameter type</param>
        public ParamIsAttribute(string definition)
            : this(definition, "*")
        { }
        /// <summary>
        /// Creates a new instance of the Tril.Attributes.ParamIsAttribute class
        /// </summary>
        /// <param name="definition">the user-defined "definition" of the target parameter type</param>
        /// <param name="targetPlats"></param>
        public ParamIsAttribute(string definition, params string[] targetPlats)
            : base(definition, targetPlats)
        { }

        /// <summary>
        /// Creates a new instance of the Tril.Attributes.ParamIsAttribute class
        /// </summary>
        /// <param name="attribute"></param>
        public ParamIsAttribute(ParamAttributes attribute)
            : this(attribute, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.Attributes.ParamIsAttribute class
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="targetPlats"></param>
        public ParamIsAttribute(ParamAttributes attribute, params string[] targetPlats)
            : base(
            ((attribute == ParamAttributes.None) ? "" :
            ((attribute == ParamAttributes.Out) ? "out" :
            ((attribute == ParamAttributes.Ref) ? "ref" : ""))), targetPlats)
        { }
    }
    /// <summary>
    /// Lists the recognized parameter attributes
    /// </summary>
    public enum ParamAttributes
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// out
        /// </summary>
        Out,
        /// <summary>
        /// ref
        /// </summary>
        Ref
    }

    /// <summary>
    /// Represents the user-defined generic parameter constraint that specifies what the target type of the 
    /// Tril.Attributes.MustBeAttribute attribute must be.
    /// This handles C# generic parameter constraints like
    /// where T : class
    /// where U : struct
    /// where V : new()
    /// </summary>
    [AttributeUsage(AttributeTargets.GenericParameter,
        AllowMultiple = true, Inherited = false)]
    public sealed class MustBeAttribute : DefinitionAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.Attributes.MustBeAttribute class
        /// </summary>
        /// <param name="definition">the user-defined "definition" of the target generic parameter type</param>
        public MustBeAttribute(string definition)
            : this(definition, "*")
        { }
        /// <summary>
        /// Creates a new instance of the Tril.Attributes.MustBeAttribute class
        /// </summary>
        /// <param name="definition">the user-defined "definition" of the target generic parameter type</param>
        /// <param name="targetPlats"></param>
        public MustBeAttribute(string definition, params string[] targetPlats)
            : base(definition, targetPlats)
        { }

        /// <summary>
        /// Creates a new instance of the Tril.Attributes.MustBeAttribute class
        /// </summary>
        /// <param name="constraint"></param>
        public MustBeAttribute(GenericParamConstraints constraint)
            : this(constraint, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.Attributes.MustBeAttribute class
        /// </summary>
        /// <param name="constraint"></param>
        /// <param name="targetPlats"></param>
        public MustBeAttribute(GenericParamConstraints constraint, params string[] targetPlats)
            : base(
            (constraint == GenericParamConstraints.None) ? "" :
            ((constraint == GenericParamConstraints.Class) ? "class" :
            ((constraint == GenericParamConstraints.DefaultConstructor) ? "new()" :
            ((constraint == GenericParamConstraints.Struct) ? "struct" : ""))), targetPlats)
        { }
    }
    /// <summary>
    /// List the recogized generic parameter constraints,
    /// aside from inheritance and interface constraints.
    /// </summary>
    public enum GenericParamConstraints
    {
        /// <summary>
        /// No constraint
        /// </summary>
        None,
        /// <summary>
        /// Must be a reference type
        /// </summary>
        Class,
        /// <summary>
        /// Must have a default (parameterless) constructor
        /// </summary>
        DefaultConstructor,
        /// <summary>
        /// Must be a value type
        /// </summary>
        Struct
    }
}
