using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Tril.Attributes
{
    /// <summary>
    /// Represents the user-defined name
    /// </summary>
    public abstract class NameAttribute : TrilAttribute
    {
        string _name = "";

        /// <summary>
        /// Creates a new instance of the Tril.NameAttribute class
        /// </summary>
        /// <param name="name">the user-defined name of the target</param>
        public NameAttribute(string name)
            : this(name, "*")
        {
        }
        /// <summary>
        /// Creates a new instance of the Tril.NameAttribute class
        /// </summary>
        /// <param name="name"></param>
        /// <param name="targetPlats"></param>
        public NameAttribute(string name, params string[] targetPlats)
            : base(targetPlats)
        {
            if (name == null)
                throw new NullReferenceException("Name cannot be null!");

            _name = name.Trim();
        }

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

        /// <summary>
        /// Gets the user-defined name
        /// </summary>
        public string Name
        {
            get { return _name; }
        }
    }

    /// <summary>
    /// Represents the user-defined name of a type or generic parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct |
        AttributeTargets.GenericParameter | AttributeTargets.Delegate,
        AllowMultiple = true, Inherited = false)]
    public sealed class TypeNameAttribute : NameAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.TypeNameAttribute class
        /// </summary>
        /// <param name="name">the user-defined name of the target type</param>
        public TypeNameAttribute(string name)
            : this(name, "*")
        { }
        /// <summary>
        /// Creates a new instance of the Tril.TypeNameAttribute class
        /// </summary>
        /// <param name="name">the user-defined name of the target type</param>
        /// <param name="targetPlats"></param>
        public TypeNameAttribute(string name, params string[] targetPlats)
            : base(name, targetPlats)
        { }
    }

    /// <summary>
    /// Represents the user-defined name of a member
    /// </summary>
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property,
        AllowMultiple = true, Inherited = true)]
    public sealed class MemberNameAttribute : NameAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.MemberNameAttribute class
        /// </summary>
        /// <param name="name">the user-defined name of the target member</param>
        public MemberNameAttribute(string name)
            : this(name, "*")
        { }
        /// <summary>
        /// Creates a new instance of the Tril.MemberNameAttribute class
        /// </summary>
        /// <param name="name">the user-defined name of the target member</param>
        /// <param name="targetPlats"></param>
        public MemberNameAttribute(string name, params string[] targetPlats)
            : base(name, targetPlats)
        { }
    }

    /// <summary>
    /// Represents the user-defined name of a method parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter,
        AllowMultiple = true, Inherited = true)]
    public sealed class ParamNameAttribute : NameAttribute
    {
        /// <summary>
        /// Creates a new instance of the Tril.ParamNameAttribute class
        /// </summary>
        /// <param name="name">the user-defined name of the target parameter</param>
        public ParamNameAttribute(string name)
            : this(name, "*")
        { }
        /// <summary>
        /// Creates a new instance of the Tril.ParamNameAttribute class
        /// </summary>
        /// <param name="name">the user-defined name of the target parameter</param>
        /// <param name="targetPlats"></param>
        public ParamNameAttribute(string name, params string[] targetPlats)
            : base(name, targetPlats)
        { }
    }
}
