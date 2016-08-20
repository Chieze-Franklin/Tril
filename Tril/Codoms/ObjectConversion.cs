﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

using Tril.Attributes;
using Tril.Delegates;
using Tril.Models;

namespace Tril.Codoms
{
    /// <summary>
    /// Represents a statement that converts an object or a value to another type.
    /// </summary>
    [Serializable]
    public abstract class ObjectConversion : ReferenceStatement
    {
        ValueStatement _obj;
        Kind _tgtKind;

        /// <summary>
        /// Creates a new instance of Tril.Codoms.ObjectConversion
        /// </summary>
        /// <param name="cachedName"></param>
        /// <param name="cachedtargetKind"></param>
        /// <param name="objToConvert"></param>
        /// <param name="targetKind"></param>
        public ObjectConversion(string cachedName, string cachedtargetKind, ValueStatement objToConvert, Kind targetKind)
            : base(cachedName, cachedtargetKind)
        {
            if (objToConvert == null)
                throw new NullReferenceException(this.GetType().FullName + ": Object to convert cannot be null!");
            if (targetKind == null)
                throw new NullReferenceException(this.GetType().FullName + ": Target kind cannot be null!");

            _obj = objToConvert;
            this.Label = objToConvert.Label;

            _tgtKind = targetKind;

            IsInline = true;
        }

        /// <summary>
        /// Gets the Kind of this code.
        /// </summary>
        /// <returns></returns>
        public override Kind GetKind()
        {
            return _tgtKind;
        }

        /// <summary>
        /// Gets the subject of the conversion.
        /// </summary>
        public ValueStatement ObjectToConvert
        {
            get { return _obj; }
        }
    }

    /// <summary>
    /// Represents a statement that casts an object to another type, but returns null if the cast fails.
    /// </summary>
    [Serializable]
    public sealed class As : ObjectConversion
    {
        /// <summary>
        /// Creates a new instance of Tril.Codoms.As
        /// </summary>
        /// <param name="cachedName"></param>
        /// <param name="cachedtargetKind"></param>
        /// <param name="objToConvert"></param>
        /// <param name="targetKind"></param>
        public As(string cachedName, string cachedtargetKind, ValueStatement objToConvert, Kind targetKind)
            : base(cachedName, cachedtargetKind, objToConvert, targetKind)
        {
        }

        /// <summary>
        /// Clones this statement
        /// </summary>
        /// <returns></returns>
        public override ValueStatement Clone()
        {
            As clone = new As(CachedName, CachedKind, ObjectToConvert, GetKind());
            clone = (As)ValueStatement.Clone(this, clone);
            return clone;
        }
        /// <summary>
        /// gets the C# representation of this code
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(null);
        }
        /// <summary>
        /// Returns the string representation of this code, as generated by the specified translator.
        /// </summary>
        /// <param name="translator"></param>
        /// <returns></returns>
        public override string ToString(CodomTranslator translator)
        {
            string trans = Codom.ToString(translator, this);
            if (trans != null)
                return trans;
            else
            {
                if (GetKind().UnderlyingType.IsValueType)
                {
                    if (IsInline)
                        return (ShowOuterBrackets ? "(" : "") + "(" + CachedKind + ")" + /*CachedName*/ObjectToConvert.ToString(translator) + (ShowOuterBrackets ? ")" : "");
                    return "(" + CachedKind + ")" + /*CachedName*/ObjectToConvert.ToString(translator) + ";";
                }
                else
                {
                    if (IsInline)
                        return (ShowOuterBrackets ? "(" : "") + /*CachedName*/ObjectToConvert.ToString(translator) + " as " + CachedKind + (ShowOuterBrackets ? ")" : "");
                    return /*CachedName*/ObjectToConvert.ToString(translator) + " as " + CachedKind + ";";
                }
                
            }
        }
    }

    /// <summary>
    /// Represents a statement that boxes a value.
    /// </summary>
    [Serializable]
    public sealed class Box : ObjectConversion
    {
        /// <summary>
        /// Creates a new instance of Tril.Codoms.Box
        /// </summary>
        /// <param name="cachedName"></param>
        /// <param name="cachedtargetKind"></param>
        /// <param name="objToConvert"></param>
        /// <param name="targetKind"></param>
        public Box(string cachedName, string cachedtargetKind, ValueStatement objToConvert, Kind targetKind)
            : base(cachedName, cachedtargetKind, objToConvert, targetKind)
        {
        }

        /// <summary>
        /// Clones this statement
        /// </summary>
        /// <returns></returns>
        public override ValueStatement Clone()
        {
            Box clone = new Box(CachedName, CachedKind, ObjectToConvert, GetKind());
            clone = (Box)ValueStatement.Clone(this, clone);
            return clone;
        }
        /// <summary>
        /// gets the C# representation of this code
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(null);
        }
        /// <summary>
        /// Returns the string representation of this code, as generated by the specified translator.
        /// </summary>
        /// <param name="translator"></param>
        /// <returns></returns>
        public override string ToString(CodomTranslator translator)
        {
            string trans = Codom.ToString(translator, this);
            if (trans != null)
                return trans;
            else
            {
                if (IsInline)
                    return (ShowOuterBrackets ? "(" : "") + "(" + CachedKind + ")" + /*CachedName*/ObjectToConvert.ToString(translator) + (ShowOuterBrackets ? ")" : "");
                return "(" + CachedKind + ")" + /*CachedName*/ObjectToConvert.ToString(translator) + ";";
            }
        }
    }

    /// <summary>
    /// Represents a statement that casts an object to another type and throws an exception if the cast fails.
    /// </summary>
    [Serializable]
    public sealed class Cast : ObjectConversion
    {
        /// <summary>
        /// Creates a new instance of Tril.Codoms.Cast
        /// </summary>
        /// <param name="cachedName"></param>
        /// <param name="cachedtargetKind"></param>
        /// <param name="objToConvert"></param>
        /// <param name="targetKind"></param>
        public Cast(string cachedName, string cachedtargetKind, ValueStatement objToConvert, Kind targetKind)
            : base(cachedName, cachedtargetKind, objToConvert, targetKind)
        {
        }

        /// <summary>
        /// Clones this statement
        /// </summary>
        /// <returns></returns>
        public override ValueStatement Clone()
        {
            Cast clone = new Cast(CachedName, CachedKind, ObjectToConvert, GetKind());
            clone = (Cast)ValueStatement.Clone(this, clone);
            return clone;
        }
        /// <summary>
        /// gets the C# representation of this code
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(null);
        }
        /// <summary>
        /// Returns the string representation of this code, as generated by the specified translator.
        /// </summary>
        /// <param name="translator"></param>
        /// <returns></returns>
        public override string ToString(CodomTranslator translator)
        {
            string trans = Codom.ToString(translator, this);
            if (trans != null)
                return trans;
            else
            {
                //if (IsInline)
                //    return (ShowOuterBrackets ? "(" : "") + /*CachedName*/ObjectToConvert.ToString(translator) + " as " + CachedKind + (ShowOuterBrackets ? ")" : "");
                //return "(" + CachedKind + ")" + /*CachedName*/ObjectToConvert.ToString(translator) + " as " + CachedKind + ";";

                if (IsInline)
                    return (ShowOuterBrackets ? "(" : "") + "(" + CachedKind + ")" + /*CachedName*/ObjectToConvert.ToString(translator) + (ShowOuterBrackets ? ")" : "");
                return "(" + CachedKind + ")" + /*CachedName*/ObjectToConvert.ToString(translator) + ";";
            }
        }
    }

    /// <summary>
    /// Represents a statement that unboxes a value.
    /// </summary>
    [Serializable]
    public class UnBox : ObjectConversion
    {
        /// <summary>
        /// Creates a new instance of Tril.Codoms.UnBox
        /// </summary>
        /// <param name="cachedName"></param>
        /// <param name="cachedtargetKind"></param>
        /// <param name="objToConvert"></param>
        /// <param name="targetKind"></param>
        public UnBox(string cachedName, string cachedtargetKind, ValueStatement objToConvert, Kind targetKind)
            : base(cachedName, cachedtargetKind, objToConvert, targetKind)
        {
        }

        
        /// <summary>
        /// Clones this statement
        /// </summary>
        /// <returns></returns>
        public override ValueStatement Clone()
        {
            UnBox clone = new UnBox(CachedName, CachedKind, ObjectToConvert, base.GetKind()); //use base.GetKind here bcuz the kind of Unbox is actually a poiter to base.GetKind()
            clone = (UnBox)ValueStatement.Clone(this, clone);
            return clone;
        }
        /// <summary>
        /// Gets the Kind of this code.
        /// </summary>
        /// <returns></returns>
        public override Kind GetKind()
        {
            Kind baseKind = base.GetKind();
            TypeReference baseType = baseKind.UnderlyingType;
            TypeReference pointerToBaseType = baseType.MakePointerType();
            return Kind.GetCachedKind(pointerToBaseType);
        }
        /// <summary>
        /// gets the C# representation of this code
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(null);
        }
        /// <summary>
        /// Returns the string representation of this code, as generated by the specified translator.
        /// </summary>
        /// <param name="translator"></param>
        /// <returns></returns>
        public override string ToString(CodomTranslator translator)
        {
            string trans = Codom.ToString(translator, this);
            if (trans != null)
                return trans;
            else
            {
                if (IsInline)
                    return (ShowOuterBrackets ? "(" : "") + "(" + CachedKind + ")" + /*CachedName*/ObjectToConvert.ToString(translator) + (ShowOuterBrackets ? ")" : "");
                return "(" + CachedKind + ")" + /*CachedName*/ObjectToConvert.ToString(translator) + ";";
            }
        }
    }
}