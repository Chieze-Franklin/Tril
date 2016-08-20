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
    /// The base class for all Tril member classes
    /// </summary>
    [Serializable]
    public abstract class Member : Model
    {
        /// <summary>
        /// Creates a new instance of Tril.Models.Member
        /// </summary>
        /// <param name="underlyingMember"></param>
        public Member(MemberReference underlyingMember) : base(underlyingMember) { }

        /// <summary>
        /// Returns an array of all the access modifiers of this member.
        /// </summary>
        /// <returns></returns>
        public string[] GetAccessModifiers()
        {
            return GetAccessModifiers("*");
        }
        /// <summary>
        /// Returns an array of all the access modifiers of this member.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public string[] GetAccessModifiers(bool useDefaultOnly)
        {
            return GetAccessModifiers(useDefaultOnly, "*");
        }
        /// <summary>
        /// Returns an array of all the access modifiers of this member.
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string[] GetAccessModifiers(params string[] targetPlats)
        {
            return GetAccessModifiers(false, targetPlats);
        }
        /// <summary>
        /// Returns an array of all the access modifiers of this member.
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public string[] GetAccessModifiers(bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            if (HasUserDefinedAccessModifiers(targetPlats) && !useDefaultOnly)
            {
                List<string> accMods = new List<string>();

                var attriAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.AccessModifierAttribute");
                foreach (CustomAttribute attri in attriAttris)
                {
                    string[] tgtPlatsVal = GetTargetPlatforms(attri);
                    foreach (string lang in targetPlats)
                    {
                        if (tgtPlatsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                        {
                            if (attri.ConstructorArguments[0].Type.FullName == "Tril.Attributes.AccessModifiers")
                            {
                                if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.AccessModifiers.None))
                                    accMods.Add("");
                                else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.AccessModifiers.Internal))
                                    accMods.Add("internal");
                                else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.AccessModifiers.Private))
                                    accMods.Add("private");
                                else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.AccessModifiers.Protected))
                                    accMods.Add("protected");
                                else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.AccessModifiers.ProtectedInternal))
                                    accMods.Add("protected internal");
                                else if (((int)attri.ConstructorArguments[0].Value) == ((int)Tril.Attributes.AccessModifiers.Public))
                                    accMods.Add("public");
                            }
                            else
                            {
                                accMods.Add(attri.ConstructorArguments[0].Value.ToString());
                            }
                            break;
                        }
                    }
                }

                return accMods.ToArray();
            }
            else
            {
                return GetAccessModifiers_Default(useDefaultOnly, targetPlats);
            }
        }
        /// <summary>
        /// Returns default access modifiers
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        protected abstract string[] GetAccessModifiers_Default(bool useDefaultOnly, params string[] targetPlats);

        /// <summary>
        /// Gets a value indicating whether the access modifiers of this member were defined by the user 
        /// using Tril.Attributes.AccessModifierAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedAccessModifiers()
        {
            return HasUserDefinedAccessModifiers("*");
        }
        /// <summary>
        /// Gets a value indicating whether the access modifiers of this member were defined by the user 
        /// using Tril.Attributes.AccessModifierAttribute
        /// </summary>
        /// <returns></returns>
        public bool HasUserDefinedAccessModifiers(params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            var commAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.AccessModifierAttribute");

            return HasUserDefined(commAttris, targetPlats);
        }
    }
}
