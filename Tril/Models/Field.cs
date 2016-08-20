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
    /// Represents a field in a Tril.Models.Kind
    /// </summary>
    [Serializable]
    public partial class Field : Member
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

            var FieldDef = GetFieldDefinition();
            if (FieldDef != null)
            {
                if (FieldDef.IsPublic)
                    accMods.Add("public");
                else if (FieldDef.IsPrivate)
                    accMods.Add("private");
                else if (FieldDef.IsAssembly)
                    accMods.Add("internal");
                else if (FieldDef.IsFamily)
                    accMods.Add("protected");
                else if (FieldDef.IsFamilyAndAssembly)
                    accMods.Add("internal");
                else if (FieldDef.IsFamilyOrAssembly)
                    accMods.Add("protected internal");
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

            var FieldDef = GetFieldDefinition();
            if (FieldDef != null)
            {
                if (FieldDef.IsLiteral)
                    attris.Add("const");
                else
                {
                    if (FieldDef.IsStatic && FieldDef.IsInitOnly)
                        attris.Add("static readonly");
                    else
                    {
                        if (FieldDef.IsStatic)
                            attris.Add("static");
                        if (FieldDef.IsInitOnly)
                            attris.Add("readonly");
                    }
                }
            }

            return attris.ToArray();
        }
        /// <summary>
        /// Returns the long name of this model.
        /// For a Tril.Models.Field, this is the long name of the declaring Kind plus two colons (::) plus the GetName.
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
            string _name = UnderlyingField.Name;

            _name = ReplaceInvalidChars(_name);

            return _name;
        }
        /// <summary>
        /// Gets the field type of this Tril.Models.Field instance.
        /// If there is no user-defined field type, the field type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined field type (specified using the Tril.Attributes.MemberTypeAttribute atttribute), 
        /// it is returned as a System.String object.
        /// </summary>
        /// <returns></returns>
        public dynamic GetFieldKind()
        {
            return GetFieldKind("*");
        }
        /// <summary>
        /// Gets the field type of this Tril.Models.Field instance.
        /// If there is no user-defined field type, the field type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined field type (specified using the Tril.Attributes.MemberTypeAttribute atttribute), 
        /// it is returned as a System.String object.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public dynamic GetFieldKind(bool useDefaultOnly)
        {
            return GetFieldKind(useDefaultOnly, "*");
        }
        /// <summary>
        /// Gets the field type of this Tril.Models.Field instance.
        /// If there is no user-defined field type, the field type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined field type (specified using the Tril.Attributes.MemberTypeAttribute atttribute), 
        /// it is returned as a System.String object.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetFieldKind(params string[] targetPlats)
        {
            return GetFieldKind(false, targetPlats);
        }
        /// <summary>
        /// Gets the field type of this Tril.Models.Field instance.
        /// If there is no user-defined field type, the field type is returned as a Tril.Models.Kind object.
        /// If there is a user-defined field type (specified using the Tril.Attributes.MemberTypeAttribute atttribute), 
        /// it is returned as a System.String object.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public dynamic GetFieldKind(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedFieldKind(targetPlats) && !useDefaultOnly)
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

            if (UnderlyingField.FieldType != null)
            {
                return Kind.GetCachedKind((UnderlyingField).FieldType);
            }

            return null;
        }
        /// <summary>
        /// Gets a raw collection of all the attributes (without sugar-coating anything).
        /// </summary>
        /// <returns></returns>
        public override string[] GetRawAttributes()
        {
            List<string> attris = new List<string>();

            var FieldDef = GetFieldDefinition();
            if (FieldDef != null)
            {
                if (FieldDef.IsSpecialName)
                    attris.Add("specialname");

                if (FieldDef.IsRuntimeSpecialName)
                    attris.Add("rtspecialname");

                if (FieldDef.IsStatic)
                    attris.Add("static");

                if (FieldDef.IsLiteral)
                    attris.Add("literal");

                if (FieldDef.IsInitOnly)
                    attris.Add("initonly");
            }

            return attris.ToArray();
        }
        /// <summary>
        /// Gets a value indicating whether the field type of this Field instance was defined by the user using Tril.Attributes.MemberTypeAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedFieldKind()
        {
            return HasUserDefinedFieldKind("*");
        }
        /// <summary>
        /// Gets a value indicating whether the field type of this Field instance was defined by the user using Tril.Attributes.MemberTypeAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedFieldKind(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var retAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.MemberTypeAttribute");

            return HasUserDefined(retAttris, targetPlats);
        }

        /// <summary>
        /// Gets the declaring Tril.Models.Kind that owns this field
        /// </summary>
        public Kind DeclaringKind
        {
            get { return Kind.GetCachedKind(UnderlyingField.DeclaringType); }
        }
        /// <summary>
        /// Gets the underlying Mono.Cecil.FieldReference from which this instance of Tril.Property was built
        /// </summary>
        public FieldReference UnderlyingField
        {
            get { return ((FieldReference)_underlyingInfo); }
        }
    }
}
