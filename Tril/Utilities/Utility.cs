using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;

using Tril.Models;

namespace Tril.Utilities
{
    /// <summary>
    /// Holds utility functions
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Returns true if the two events are namesakes
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool AreNamesakes(EventReference left, EventReference right)
        {
            if (left == null || right == null)
                return false;

            if (left.FullName != right.FullName)
                return false;

            return true;
        }
        /// <summary>
        /// Returns true if the two fields are namesakes
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool AreNamesakes(FieldReference left, FieldReference right)
        {
            if (left == null || right == null)
                return false;

            if (left.FullName != right.FullName)
                return false;

            return true;
        }
        /// <summary>
        /// Returns true if the two methods have the same signature
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool AreNamesakes(MethodReference left, MethodReference right)
        {
            if (left == null || right == null)
                return false;

            //compare names
            if (left.FullName != right.FullName)
                return false;

            //compare return types
            if (!AreNamesakes(left.ReturnType, right.ReturnType))
                return false;

            //compare if they are both generic
            if (left.IsGenericInstance != right.IsGenericInstance)
                return false;
            //compare generic parameters
            if (left.HasGenericParameters != right.HasGenericParameters)
                return false;
            if (left.GenericParameters.Count != right.GenericParameters.Count)
                return false;

            //compare types of parallel parameters
            bool foundMatchingParams = false;
            for (int index = 0; index < left.Parameters.Count; index++)
            {
                if (!AreNamesakes(left.Parameters[index].ParameterType, right.Parameters[index].ParameterType))
                    continue;

                foundMatchingParams = true;
                break;
            }
            if (left.Parameters.Count == 0)
                foundMatchingParams = true;
            if (!foundMatchingParams)
                return false;

            return true;
        }
        /// <summary>
        /// Returns true if the two properties are namesakes
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool AreNamesakes(PropertyReference left, PropertyReference right)
        {
            if (left == null || right == null)
                return false;

            if (left.FullName != right.FullName)
                return false;

            return true;
        }
        /// <summary>
        /// Returns true if the two types are namesakes
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool AreNamesakes(TypeReference left, TypeReference right)
        {
            if (left == null || right == null || left.FullName == null || left.FullName == "" || right.FullName == null || right.FullName == "")
                return false;

            if (left.FullName != right.FullName)
                return false;

            return true;
        }
        /// <summary>
        /// Finds the namesake of the specified type in the specified assembly
        /// </summary>
        /// <param name="typeRef"></param>
        /// <param name="targetAsm"></param>
        /// <returns></returns>
        public static TypeDefinition FindNamesakeInAssembly(TypeReference typeRef, AssemblyDefinition targetAsm)
        {
            if (typeRef == null || targetAsm == null)
                return null;

            TypeDefinition namesaketype = null;

            foreach (TypeDefinition targetType in targetAsm.MainModule.Types) //first look for it in the main module
            {
                namesaketype = FindNamesakeInType(typeRef, targetType);
                if (namesaketype != null)
                    return namesaketype;
            }
            for (int index = 0; index < targetAsm.Modules.Count(); index++) //look for the type in other modules
            {
                foreach (TypeDefinition targetType in targetAsm.Modules[index].Types)
                {
                    namesaketype = FindNamesakeInType(typeRef, targetType);
                    if (namesaketype != null)
                        return namesaketype;
                }
            }

            return null;
        }
        /// <summary>
        /// Finds the namesake of the specified type in the specified type definition
        /// </summary>
        /// <param name="typeRef"></param>
        /// <param name="targetTypeDef"></param>
        /// <returns></returns>
        public static TypeDefinition FindNamesakeInType(TypeReference typeRef, TypeDefinition targetTypeDef)
        {
            if (typeRef == null || targetTypeDef == null)
                return null;

            if (AreNamesakes(typeRef, targetTypeDef))
            {
                return targetTypeDef;
            }
            foreach (TypeDefinition target in targetTypeDef.NestedTypes)
            {
                TypeDefinition td = FindNamesakeInType(typeRef, target);
                if (td != null)
                    return td;
            }

            return null;
        }
        /// <summary>
        /// Returns the short name or long name of the target kind,
        /// depending on the relationship between the target kind and the calling kind.
        /// </summary>
        /// <param name="callingKind"></param>
        /// <param name="targetKind"></param>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetLangs"></param>
        /// <returns></returns>
        public static string GetAppropriateName(Kind callingKind, Kind targetKind, bool useDefaultOnly, params string[] targetLangs)
        {
            if (targetKind.IsPlaceHolderGenericParameter)
                return targetKind.GetName(useDefaultOnly, targetLangs);
            else
            {
                if (targetKind.UnderlyingType.FullName == callingKind.UnderlyingType.FullName)
                    return targetKind.GetName(useDefaultOnly, targetLangs);
                else if ((callingKind.UnderlyingType.Resolve() != null) && (callingKind.UnderlyingType.Resolve()).NestedTypes
                    .Any(t => t.FullName == targetKind.UnderlyingType.FullName))
                    return targetKind.GetName(useDefaultOnly, targetLangs);
                //else if the targetKind is imported use GetName (not yet implemented)
                //else just return the long name
                else
                    return targetKind.GetLongName(useDefaultOnly, targetLangs);
            }
        }
        /// <summary>
        /// Returns the long name of the target kind,
        /// or just the short name if the kind is a place holder generic parameter.
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetLangs"></param>
        /// <returns></returns>
        public static string GetAppropriateLongName(Kind kind, bool useDefaultOnly, params string[] targetLangs)
        {
            if (kind.IsPlaceHolderGenericParameter)
                return kind.GetName(useDefaultOnly, targetLangs);
            else
            {
                return kind.GetLongName(useDefaultOnly, targetLangs);
            }
        }
        /// <summary>
        /// Ensures the array is not null and does not contain any null element
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string[] UnNullifyArray(string[] array)
        {
            if (array == null)
                return new string[] { };
            return array.Select<string, string>(e => { if (e == null) return ""; return e; }).ToArray();
        }
    }
}
