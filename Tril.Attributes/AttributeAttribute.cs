using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// Represents an attribute to be applied to a type
    /// </summary>
    [AttributeUsage(AttributeTargets.All,
        AllowMultiple = true, Inherited = false)]
    public sealed class AttributeAttribute : TrilAttribute
    {
        string _attri = "";

        /// <summary>
        /// Creates a new instance of the Tril.AttributeAttribute class
        /// </summary>
        /// <param name="Attribute"></param>
        public AttributeAttribute(string Attribute)
            : this(Attribute, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.AttributeAttribute class
        /// </summary>
        /// <param name="Attribute"></param>
        /// <param name="targetPlats"></param>
        public AttributeAttribute(string Attribute, params string[] targetPlats)
            :base(targetPlats)
        {
            _attri = Attribute == null ? "" : Attribute.Trim();
        }
        /// <summary>
        /// Creates a new instance of the Tril.AttributeAttribute class
        /// </summary>
        /// <param name="Attribute"></param>
        public AttributeAttribute(Attributes Attribute)
            : this(Attribute, "*") { }
        /// <summary>
        /// Creates a new instance of the Tril.AttributeAttribute class
        /// </summary>
        /// <param name="Attribute"></param>
        /// <param name="targetPlats"></param>
        public AttributeAttribute(Attributes Attribute, params string[] targetPlats)
            : base(targetPlats)
        {
            if (Attribute == Attributes.None)
                _attri = "";
            else if (Attribute == Attributes.Abstract)
                _attri = "abstract";
            else if (Attribute == Attributes.Const)
                _attri = "const";
            else if (Attribute == Attributes.MustOverride)
                _attri = "MustOverride";
            else if (Attribute == Attributes.Override)
                _attri = "override";
            else if (Attribute == Attributes.Overrides)
                _attri = "Overrides";
            else if (Attribute == Attributes.Readonly)
                _attri = "readonly";
            else if (Attribute == Attributes.Sealed)
                _attri = "sealed";
            else if (Attribute == Attributes.Shared)
                _attri = "Shared";
            else if (Attribute == Attributes.Static)
                _attri = "static";
            else if (Attribute == Attributes.StaticReadonly)
                _attri = "static readonly";
            else if (Attribute == Attributes.Virtual)
                _attri = "virtual";
            else
                _attri = "";
        }

        /// <summary>
        /// Gets the attribute applied to a type
        /// </summary>
        public string Attribute
        {
            get { return _attri; }
        }
    }

    /// <summary>
    /// Lists the recognised attributes
    /// </summary>
    public enum Attributes
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// Abstract
        /// </summary>
        Abstract,
        /// <summary>
        /// Const
        /// </summary>
        Const,
        /// <summary>
        /// Readonly
        /// </summary>
        Readonly,
        /// <summary>
        /// Sealed
        /// </summary>
        Sealed,
        /// <summary>
        /// Shared
        /// </summary>
        Shared,
        /// <summary>
        /// Static
        /// </summary>
        Static,
        /// <summary>
        /// StaticReadonly
        /// </summary>
        StaticReadonly,
        /// <summary>
        /// Virtual
        /// </summary>
        Virtual,
        /// <summary>
        /// Override
        /// </summary>
        Override,
        /// <summary>
        /// Overrides
        /// </summary>
        Overrides,
        /// <summary>
        /// MustOverride
        /// </summary>
        MustOverride
    }
}
