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
    /// Represents a goto statement
    /// </summary>
    [Serializable]
    public sealed class Goto : BranchingCode
    {
        string _tgt = "";

        /// <summary>
        /// Creates a new instance of Tril.Codoms.Goto
        /// </summary>
        /// <param name="target"></param>
        public Goto(string target)
        {
            if (target == null || target.Trim() == "")
                throw new NullReferenceException(this.GetType().FullName + ": Target of a \"goto\" cannot be null!");

            _tgt = target.Trim();
        }

        /// <summary>
        /// Gets the target of the this tril.Goto statement
        /// </summary>
        public string Target
        {
            get { return _tgt; }
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
                    return "goto " + Target;
                return "goto " + Target + ";";
            }
        }
    }

    /// <summary>
    /// Represents a CIL leave instruction
    /// </summary>
    [Serializable]
    public sealed class Leave : BranchingCode
    {
        string _tgt = "";

        /// <summary>
        /// Creates a new instance of Tril.Codoms.Leave
        /// </summary>
        /// <param name="target"></param>
        public Leave(string target)
        {
            if (target == null || target.Trim() == "")
                throw new NullReferenceException(this.GetType().FullName + ": Target of a \"leave\" cannot be null!");

            _tgt = target.Trim();
        }

        /// <summary>
        /// Gets the target of the this tril.Goto statement
        /// </summary>
        public string Target
        {
            get { return _tgt; }
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
                    return "goto " + Target;
                return "goto " + Target + ";";
            }
        }
    }

    /// <summary>
    /// Represents a return statement
    /// </summary>
    [Serializable]
    public sealed class Return : BranchingCode
    {
        ValueStatement _val;

        /// <summary>
        /// Creates a new instance of Tril.Codoms.Return
        /// </summary>
        /// <param name="value">the value to be returned; pass null if there is no value to return</param>
        public Return(ValueStatement value)
        {
            _val = value;
            if (value != null)
            {
                value.IsInline = true;
                value.ShowOuterBrackets = false;
                this.Label = value.Label;
            }
        }

        /// <summary>
        /// Gets the value of the this Tril.Codoms.Return statement.
        /// If the return statement returns nothing (like in a void method),
        /// Value is null.
        /// </summary>
        public ValueStatement Value
        {
            get { return _val; }
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
                {
                    if (Value != null)
                        return "return " + Value.ToString(translator);
                    return "return";
                }
                else
                {
                    if (Value != null)
                        return "return " + Value.ToString(translator) + ";";
                    return "return;";
                }
            }
        }
    }

    /// <summary>
    /// Represents a throw statement
    /// </summary>
    [Serializable]
    public sealed class Throw : BranchingCode
    {
        ValueStatement _val;

        /// <summary>
        /// Creates a new instance of Tril.Codoms.Throw
        /// </summary>
        /// <param name="value">the value to be thrown; pass null if there is no value to throw</param>
        public Throw(ValueStatement value)
        {
            _val = value;
            if (value != null)
            {
                value.IsInline = true;
                value.ShowOuterBrackets = false;
                this.Label = value.Label;
            }
        }

        /// <summary>
        /// Gets the value of the this tril.Throw statement.
        /// If the throw statement returns nothing (like in a CIL rethrow),
        /// Value is null.
        /// </summary>
        public ValueStatement Value
        {
            get { return _val; }
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
                {
                    if (Value != null)
                        return "throw " + Value.ToString(translator);
                    return "throw";
                }
                else
                {
                    if (Value != null)
                        return "throw " + Value.ToString(translator) + ";";
                    return "throw;";
                }
            }
        }
    }
}