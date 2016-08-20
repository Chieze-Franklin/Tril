using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using Tril.Utilities;

namespace Tril.Models
{
    /// <summary>
    /// Represents a property in a Tril.Models.Kind
    /// </summary>
    [Serializable]
    public partial class Property : Member
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

            if (GetMethod != null)
            {
                accMods.AddRange(GetMethod.GetAccessModifiers(useDefaultOnly, targetPlats));
            }
            else if (SetMethod != null)
            {
                accMods.AddRange(SetMethod.GetAccessModifiers(useDefaultOnly, targetPlats));
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

            if (GetMethod != null)
            {
                attris.AddRange(GetMethod.GetAttributes(useDefaultOnly, targetPlats));
            }
            else if (SetMethod != null)
            {
                attris.AddRange(SetMethod.GetAttributes(useDefaultOnly, targetPlats));
            }

            return attris.ToArray();
        }
        /// <summary>
        /// Returns the long name of this model.
        /// For a Tril.Models.Property, this is the long name of the declaring Kind plus two colons (::) plus the GetName.
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
            string _name = UnderlyingProperty.Name;

            _name = ReplaceInvalidChars(_name);

            return _name;
        }
        /// <summary>
        /// Gets the return type of this Tril.Models.Property instance.
        /// If there is no user-defined return type, the return type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined return type (specified using the Tril.Attributes.MemberTypeAttribute atttribute),
        /// it is returned as a System.String object.
        /// </summary>
        /// <returns></returns>
        public dynamic GetPropertyKind()
        {
            return GetPropertyKind("*");
        }
        /// <summary>
        /// Gets the return type of this Tril.Models.Property instance.
        /// If there is no user-defined return type, the return type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined return type (specified using the Tril.Attributes.MemberTypeAttribute atttribute),
        /// it is returned as a System.String object.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public dynamic GetPropertyKind(bool useDefaultOnly)
        {
            return GetPropertyKind(useDefaultOnly, "*");
        }
        /// <summary>
        /// Gets the return type of this Tril.Models.Property instance.
        /// If there is no user-defined return type, the return type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined return type (specified using the Tril.Attributes.MemberTypeAttribute atttribute),
        /// it is returned as a System.String object.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetPropertyKind(params string[] targetPlats)
        {
            return GetPropertyKind(false, targetPlats);
        }
        /// <summary>
        /// Gets the return type of this Tril.Models.Property instance.
        /// If there is no user-defined return type, the return type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined return type (specified using the Tril.Attributes.MemberTypeAttribute atttribute),
        /// it is returned as a System.String object.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetPropertyKind(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedPropertyKind(targetPlats) && !useDefaultOnly)
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

            if (UnderlyingProperty.PropertyType != null)
            {
                return Kind.GetCachedKind(UnderlyingProperty.PropertyType);
            }

            return null;
        }
        /// <summary>
        /// Gets a raw collection of all the attributes (without sugar-coating anything).
        /// If the property has a get method block, it uses the raw attributes of the get method;
        /// otherwise, it uses the raw attributes of the set methods.
        /// </summary>
        /// <returns></returns>
        public override string[] GetRawAttributes()
        {
            List<string> attris = new List<string>();

            if (GetMethod != null)
            {
                attris.AddRange(GetMethod.GetRawAttributes());
            }
            else if (SetMethod != null)
            {
                attris.AddRange(SetMethod.GetRawAttributes());
            }

            return attris.ToArray();
        }
        /// <summary>
        /// Gets a value indicating whether the bodies of the methods in this Tril.Models.Property instance can be accessed.
        /// If the underlying property has been marked with the [ShowImplementation] attribute, 
        /// this method would return false.
        /// If the underlying property has been marked with the [HideImplementation] attribute, 
        /// this method would return true.
        /// Otherwise, this method returns false.
        /// </summary>
        /// <returns></returns>
        public bool HasHiddenImplementation()
        {
            return HasHiddenImplementation("*");
        }
        /// <summary>
        /// Gets a value indicating whether the bodies of the methods in this Tril.Models.Property instance can be accessed.
        /// If the underlying property has been marked with the [ShowImplementation] attribute, 
        /// this method would return false.
        /// If the underlying property has been marked with the [HideImplementation] attribute, 
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

            return DeclaringKind.HasHiddenImplementation(targetPlats);
        }
        /// <summary>
        /// Gets a value indicating whether the return type of this Property instance was defined by the user using Tril.Attributes.MemberTypeAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedPropertyKind()
        {
            return HasUserDefinedPropertyKind("*");
        }
        /// <summary>
        /// Gets a value indicating whether the return type of this Property instance was defined by the user using Tril.Attributes.MemberTypeAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedPropertyKind(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var retAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MemberTypeAttribute");

            return HasUserDefined(retAttris, targetPlats);
        }

        /// <summary>
        /// Gets the Kind that owns this property
        /// </summary>
        public Kind DeclaringKind
        {
            get { return Kind.GetCachedKind(UnderlyingProperty.DeclaringType); }
        }
        /// <summary>
        /// Gets the Tril.Models.Method object that represents the get block of the underlying property.
        /// Returns null if the underlying property has no get block.
        /// </summary>
        public Method GetMethod
        {
            get
            {
                var PropDef = GetPropertyDefinition();
                if (PropDef != null)
                {
                    if (PropDef.GetMethod != null)
                    {
                        return Method.GetCachedMethod(PropDef.GetMethod);
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the the underlying property has a get block.
        /// </summary>
        public bool HasGetMethod
        {
            get { return (GetMethod != null); }
        }
        /// <summary>
        /// Gets a value indicating whether the the underlying property has a set block.
        /// </summary>
        public bool HasSetMethod
        {
            get { return (SetMethod != null); }
        }
        /// <summary>
        /// Gets the Tril.Models.Method object that represents the set block of the underlying property.
        /// Returns null if the underlying property has no set block.
        /// </summary>
        public Method SetMethod
        {
            get
            {
                var PropDef = GetPropertyDefinition();
                if (PropDef != null)
                {
                    if (PropDef.SetMethod != null)
                    {
                        return Method.GetCachedMethod(PropDef.SetMethod);
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// Gets the underlying Mono.Cecil.PropertyReference from which this instance of Tril.Models.Property was built
        /// </summary>
        public PropertyReference UnderlyingProperty
        {
            get { return ((PropertyReference)_underlyingInfo); }
        }
    }
}
