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
    /// Represents a logical AND operation
    /// </summary>
    [Serializable]
    public sealed class And : BinaryLogicOperation
    {
        /// <summary>
        /// Creates a new instance of Tril.Codoms.And
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public And(ValueStatement first, ValueStatement second)
            : base(first, second)
        {
        }

        /// <summary>
        /// Clones this statement
        /// </summary>
        /// <returns></returns>
        public override ValueStatement Clone()
        {
            And clone = new And(FirstOperand, SecondOperand);
            clone = (And)ValueStatement.Clone(this, clone);
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
                    return (ShowOuterBrackets ? "(" : "") + FirstOperand.ToString(translator) + " && " + SecondOperand.ToString(translator) + (ShowOuterBrackets ? ")" : "");
                return FirstOperand.ToString(translator) + " && " + SecondOperand.ToString(translator) + ";";
            }
        }
    }

    /// <summary>
    /// Represents an operation that compares two operands
    /// </summary>
    [Serializable]
    public sealed class Compare : BinaryLogicOperation, INegatable
    {
        Comparisons _comp;

        /// <summary>
        /// Creates a new instance of Tril.Codoms.Compare
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="comparison"></param>
        public Compare(ValueStatement first, ValueStatement second, Comparisons comparison)
            : base(first, second)
        {
            _comp = comparison;
        }

        /// <summary>
        /// Gets the comparison to be applied to the operands
        /// </summary>
        public Comparisons Comparison
        {
            get { return _comp; }
        }

        /// <summary>
        /// Clones this statement
        /// </summary>
        /// <returns></returns>
        public override ValueStatement Clone()
        {
            Compare clone = new Compare(FirstOperand, SecondOperand, Comparison);
            clone = (Compare)ValueStatement.Clone(this, clone);
            return clone;
        }
        /// <summary>
        /// Inverts the comparison and swaps the operands
        /// </summary>
        public void Invert()
        {
            InvertComparison();
            SwapOperands();
        }
        /// <summary>
        /// Changes the comparison such that if the operands were swapped, the resulting relationship
        /// would be identical to the relationship before the inversion took place.
        /// For instance, inverting "GreaterThan" gives you "LessThan"
        /// because A "GreaterThan" B is identical to B "LessThan" A.
        /// Do not confuse this with Negate() because Negate() would turn "GreaterThan" to "LessThanOrEqual" not to "LessThan".
        /// Also, negate would turn "Equal" to "NotEqual" while InvertComparison() has no effect on "Equal" because
        /// A "Equal" B is identical to B "Equal" A.
        /// Finally, note that InvertComparison() does NOT swap the operands; you have to explicitly call SwapOperands() or call
        /// Invert() which both inverts the comparison and swaps the operands.
        /// </summary>
        public void InvertComparison()
        {
            switch (Comparison)
            {
                case Comparisons.Equal:
                    _comp = Comparisons.Equal;
                    break;
                case Comparisons.GreaterThan:
                    _comp = Comparisons.LessThan;
                    break;
                case Comparisons.GreaterThanOrEqual:
                    _comp = Comparisons.LessThanOrEqual;
                    break;
                case Comparisons.LessThan:
                    _comp = Comparisons.GreaterThan;
                    break;
                case Comparisons.LessThanOrEqual:
                    _comp = Comparisons.GreaterThanOrEqual;
                    break;
                case Comparisons.NotEqual:
                    _comp = Comparisons.NotEqual;
                    break;
            }
        }
        /// <summary>
        /// Negates the current comparison.
        /// E.g turns "Equal" to "NotEqual", "GreaterThan" to "LessThanOrEqual" (that is, not "GreaterThan")
        /// </summary>
        public void Negate()
        {
            switch (Comparison)
            {
                case Comparisons.Equal:
                    _comp = Comparisons.NotEqual;
                    break;
                case Comparisons.GreaterThan:
                    _comp = Comparisons.LessThanOrEqual;
                    break;
                case Comparisons.GreaterThanOrEqual:
                    _comp = Comparisons.LessThan;
                    break;
                case Comparisons.LessThan:
                    _comp = Comparisons.GreaterThanOrEqual;
                    break;
                case Comparisons.LessThanOrEqual:
                    _comp = Comparisons.GreaterThan;
                    break;
                case Comparisons.NotEqual:
                    _comp = Comparisons.Equal;
                    break;
            }
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
                string op = "";
                switch (Comparison)
                {
                    case Comparisons.Equal:
                        op = "==";
                        break;
                    case Comparisons.GreaterThan:
                        op = ">";
                        break;
                    case Comparisons.GreaterThanOrEqual:
                        op = ">=";
                        break;
                    case Comparisons.LessThan:
                        op = "<";
                        break;
                    case Comparisons.LessThanOrEqual:
                        op = "<=";
                        break;
                    case Comparisons.NotEqual:
                        op = "!=";
                        break;
                }

                if (IsInline)
                    return (ShowOuterBrackets ? "(" : "") + FirstOperand.ToString(translator) + " " + op + " " + SecondOperand.ToString(translator) + (ShowOuterBrackets ? ")" : "");
                return FirstOperand.ToString(translator) + " " + op + " " + SecondOperand.ToString(translator) + ";";
            }
        }

        /// <summary>
        /// Gets the possible ways the operands can be compared
        /// </summary>
        public enum Comparisons
        {
            /// <summary>
            /// ==
            /// </summary>
            Equal,
            /// <summary>
            /// >
            /// </summary>
            GreaterThan,
            /// <summary>
            /// >=
            /// </summary>
            GreaterThanOrEqual,
            /// <summary>
            /// &lt;
            /// </summary>
            LessThan,
            /// <summary>
            /// &lt;=
            /// </summary>
            LessThanOrEqual,
            /// <summary>
            /// !=
            /// </summary>
            NotEqual
        }
    }

    /// <summary>
    /// Represents a logical OR operation
    /// </summary>
    [Serializable]
    public sealed class Or : BinaryLogicOperation
    {
        /// <summary>
        /// Creates a new instance of Tril.Codoms.Or
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public Or(ValueStatement first, ValueStatement second)
            : base(first, second)
        {
        }

        /// <summary>
        /// Clones this statement
        /// </summary>
        /// <returns></returns>
        public override ValueStatement Clone()
        {
            Or clone = new Or(FirstOperand, SecondOperand);
            clone = (Or)ValueStatement.Clone(this, clone);
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
                    return (ShowOuterBrackets ? "(" : "") + FirstOperand.ToString(translator) + " || " + SecondOperand.ToString(translator) + (ShowOuterBrackets ? ")" : "");
                return FirstOperand.ToString(translator) + " || " + SecondOperand.ToString(translator) + ";";
            }
        }
    }
}
