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
    /// Represents a debugger break; completely useless to us.
    /// Do NOT confuse this with a high-level break statement.
    /// </summary>
    [Serializable]
    public class DebuggerBreak : DoNothing
    {
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
                return "";
            }
        }
    }

    /// <summary>
    /// Represents an "endfault" instruction; not currently used. Remeber to make this public when it becomes supported.
    /// </summary>
    [Serializable]
    class EndFault : DoNothing
    {
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
                return "";
            }
        }
    }

    /// <summary>
    /// Represents an "endfilter" instruction; currently useless to us.
    /// </summary>
    [Serializable]
    public class EndFilter : DoNothing
    {
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
                return "";
            }
        }
    }

    /// <summary>
    /// Represents an "endfinally" (or an "endfault") instruction; currently useless to us.
    /// </summary>
    [Serializable]
    public class EndFinally : DoNothing
    {
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
                return "";
            }
        }
    }

    /// <summary>
    /// Represents an empty statement
    /// </summary>
    [Serializable]
    public sealed class Nop : DoNothing
    {
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
                    return "";
                return ";";
            }
        }
    }
}
