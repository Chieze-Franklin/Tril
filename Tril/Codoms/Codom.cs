using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Tril.Attributes;
using Tril.Delegates;
using Tril.Models;

namespace Tril.Codoms
{
    /// <summary>
    /// The base class for all code object models
    /// </summary>
    [Serializable]
    public abstract class Codom
    {
        /// <summary>
        /// static bool type definition
        /// </summary>
        protected static TypeDefinition boolDef;
        /// <summary>
        /// static char type definition
        /// </summary>
        protected static TypeDefinition charDef;
        /// <summary>
        /// static double type definition
        /// </summary>
        protected static TypeDefinition dblDef;
        /// <summary>
        /// static int type definition
        /// </summary>
        protected static TypeDefinition intDef;
        /// <summary>
        /// static long type definition
        /// </summary>
        protected static TypeDefinition longDef;
        /// <summary>
        /// static native int type definition
        /// </summary>
        protected static TypeDefinition nativeIntDef;
        /// <summary>
        /// static native unsigned int type definition
        /// </summary>
        protected static TypeDefinition nativeUIntDef;
        /// <summary>
        /// static runtime field handle type definition
        /// </summary>
        protected static TypeDefinition rtFldDef;
        /// <summary>
        /// static runtime method handle type definition
        /// </summary>
        protected static TypeDefinition rtMtdDef;
        /// <summary>
        /// static runtime type handle type definition
        /// </summary>
        protected static TypeDefinition rtTypDef;
        /// <summary>
        /// static string type definition
        /// </summary>
        protected static TypeDefinition strDef;

        /// <summary>
        /// initialize boolDef
        /// </summary>
        protected static void init_boolDef()
        {
            boolDef = new bool().GetType().ToTypeDefinition(true);
        }
        /// <summary>
        /// initialize charDef
        /// </summary>
        protected static void init_charDef()
        {
            charDef = new char().GetType().ToTypeDefinition(true);
        }
        /// <summary>
        /// initialize dblDef
        /// </summary>
        protected static void init_dblDef()
        {
            dblDef = new double().GetType().ToTypeDefinition(true);
        }
        /// <summary>
        /// initialize intDef
        /// </summary>
        protected static void init_intDef()
        {
            intDef = new int().GetType().ToTypeDefinition(true);
        }
        /// <summary>
        /// initialize longDef
        /// </summary>
        protected static void init_longDef()
        {
            longDef = new long().GetType().ToTypeDefinition(true);
        }
        /// <summary>
        /// initialize nativeIntDef
        /// </summary>
        protected static void init_nativeIntDef() 
        {
            //nativeIntDef = new IntPtr(new int()).GetType().ToTypeDefinition(true);
            if (IntPtr.Size == 8) //(Environment.Is64BitOperatingSystem) //64-bit
                nativeIntDef = new long().GetType().ToTypeDefinition(true);
            else //if (IntPtr.Size == 4) 32-bit
                nativeIntDef = new int().GetType().ToTypeDefinition(true);
        }
        /// <summary>
        /// initialize nativeUIntDef
        /// </summary>
        protected static void init_nativeUIntDef()
        {
            if (IntPtr.Size == 8) //(Environment.Is64BitOperatingSystem) //64-bit
                nativeIntDef = new ulong().GetType().ToTypeDefinition(true);
            else //if (IntPtr.Size == 4) 32-bit
                nativeIntDef = new uint().GetType().ToTypeDefinition(true);
        }
        /// <summary>
        /// initialize rtFldDef
        /// </summary>
        protected static void init_rtFldDef()
        {
            rtFldDef = new RuntimeFieldHandle().GetType().ToTypeDefinition(true);
        }
        /// <summary>
        /// initialize rtMtdDef
        /// </summary>
        protected static void init_rtMtdDef()
        {
            rtMtdDef = new RuntimeMethodHandle().GetType().ToTypeDefinition(true);
        }
        /// <summary>
        /// initialize rtTypDef
        /// </summary>
        protected static void init_rtTypDef()
        {
            rtTypDef = new RuntimeTypeHandle().GetType().ToTypeDefinition(true);
        }
        /// <summary>
        /// initialize strDef
        /// </summary>
        protected static void init_strDef()
        {
            strDef = "".GetType().ToTypeDefinition(true);
        }

        bool _inline = false;
        string _lbl = "";

        /// <summary>
        /// Gets or sets a value to determine whether the code should be treated as an inline code or not.
        /// An inline code is one that is embedded in another code, like one passed as an argument to a method.
        /// In languages like C#, codes that are not inline end with semicolons.
        /// Below is a simple assignment that is NOT inline:
        /// A = B; (ends with semicolon).
        /// Below are two ways to make the above code inline:
        /// C = A = B;
        /// C = Method(A = B);
        /// Inline codes can have a pair of parenthesis surrounding them, as shown below:
        /// C = (A = B);
        /// C = Method((A = B));
        /// </summary>
        public bool IsInline
        {
            get { return _inline; }
            set { _inline = value; }
        }
        /// <summary>
        /// Gets or sets the label of this code.
        /// This is the label that can be referenced by statements like goto, labeled break, labeled continue.
        /// </summary>
        public string Label
        {
            get { return _lbl; }
            set { _lbl = value; }
        }

        /// <summary>
        /// Returns an array of CodeDom objects that can be used to represent this.
        /// </summary>
        /// <returns></returns>
        //public abstract CodeObject[] ToCodeDoms();
        //ToExpressions();
        //ToXml();
        /// <summary>
        /// Returns the string representation of this code, as generated by the specified translator.
        /// </summary>
        /// <param name="translator"></param>
        /// <returns></returns>
        public abstract string ToString(CodomTranslator translator);
        /// <summary>
        /// Returns the string representation of the specified code, as generated by the specified translator.
        /// Returns null if the translation failed.
        /// </summary>
        /// <param name="translator"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        protected static string ToString(CodomTranslator translator, Codom code)
        {
            try
            {
                if (translator == null) return null;
                return translator(code);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Represents a statement code.
    /// </summary>
    [Serializable]
    public abstract class Statement : Codom
    { }

    /// <summary>
    /// Represents an unconditional branching statement.
    /// Branching statements are not inline by default.
    /// </summary>
    [Serializable]
    public abstract class BranchingCode : Statement
    {
        /// <summary>
        /// Creates a new instance of Tril.Codoms.BranchingCode
        /// </summary>
        public BranchingCode()
        {
            IsInline = false;
        }
    }

    /// <summary>
    /// Represents a check for finite on a value.
    /// </summary>
    [Serializable]
    public abstract class CheckFinite : Statement
    {
        ValueStatement _val;

        /// <summary>
        /// Creates a new instance of Tril.Codoms.CheckFinite
        /// </summary>
        /// <param name="value"></param>
        public CheckFinite(ValueStatement value)
        {
            if (value == null)
                throw new NullReferenceException(this.GetType().FullName + ": Value cannot be null!");
            _val = value;
            _val.IsInline = true;

            IsInline = false;
        }

        /// <summary>
        /// Gets the value
        /// </summary>
        public ValueStatement Value
        {
            get { return _val; }
        }
    }

    /// <summary>
    /// Represents a statement that does nothing from our point of view, statements like nop, break...
    /// They are included to ensure there is no "missing" label.
    /// These codes are not inline by default.
    /// </summary>
    [Serializable]
    public abstract class DoNothing : Statement
    {
        /// <summary>
        /// Creates a new instance of Tril.Codoms.DoNothing
        /// </summary>
        public DoNothing()
        {
            IsInline = false;
        }
    }

    /// <summary>
    /// Represents a statement code that can return a value.
    /// Such statements include variable references, field references, method calls (that return values), operators...
    /// To know if a statement qualifies to be treated as a value statement, ask if it can be on the right side of an assignment.
    /// </summary>
    [Serializable]
    public abstract class ValueStatement : Statement
    {
        bool _shwbr = true;

        /// <summary>
        /// Returns a clone
        /// </summary>
        /// <returns></returns>
        public abstract ValueStatement Clone();
        /// <summary>
        /// Sets the properties of a clone, possibly from those of the original
        /// </summary>
        /// <param name="original"></param>
        /// <param name="clone"></param>
        protected static ValueStatement Clone(ValueStatement original, ValueStatement clone) 
        {
            clone.IsInline = original.IsInline;
            clone.Label = ""; //clones should have no labels, but can have their labels set to that of the instruction that invoked this method
            return clone;
        }
        /// <summary>
        /// Gets the kind
        /// </summary>
        /// <returns></returns>
        public abstract Kind GetKind();

        /// <summary>
        /// Gets or sets a value to determine whether the other bracket pair is shown
        /// </summary>
        public bool ShowOuterBrackets
        {
            get { return _shwbr; }
            set { _shwbr = value; }
        }
    }

    /// <summary>
    /// Represents an operation, especially one primitive to the language.
    /// </summary>
    [Serializable]
    public abstract class Operation : ValueStatement
    {
        /// <summary>
        /// Creates a new instance of Tril.Codoms.Operation.
        /// Operations are inline be default.
        /// </summary>
        public Operation()
        {
            IsInline = true;
        }
    }

    /// <summary>
    /// Represents an statement that references an object.
    /// ReferenceStatement is an inline code by default.
    /// </summary>
    [Serializable]
    public abstract class ReferenceStatement : ValueStatement
    {
        string _cachedName;
        string _cachedKind;
        bool _useShortName = false;

        /// <summary>
        /// Creates a new instance of Tril.Codoms.ReferenceStatement
        /// </summary>
        /// <param name="cachedName"></param>
        /// <param name="cachedKind"></param>
        public ReferenceStatement(string cachedName, string cachedKind)
        {
            if (cachedName == null || cachedName.Trim() == "")
                throw new NullReferenceException(this.GetType().FullName + ": Cached name cannot be null!");
            if (cachedKind == null || cachedKind.Trim() == "")
                throw new NullReferenceException(this.GetType().FullName + ": Cached kind cannot be null!");

            _cachedName = cachedName.Trim();
            _cachedKind = cachedKind.Trim();
            IsInline = true;
        }

        /// <summary>
        /// Gets the name cashed at the point of creating this instance.
        /// </summary>
        public string CachedName
        {
            get { return _cachedName; }
        }
        /// <summary>
        /// Gets the kind cashed at the point of creating this instance.
        /// </summary>
        public string CachedKind
        {
            get { return _cachedKind; }
        }
        /// <summary>
        /// Gets or sets a value to determine if only name is to be used, not full name.
        /// </summary>
        public bool UseShortName
        {
            get { return _useShortName; }
            set { _useShortName = value; }
        }
    }

    /// <summary>
    /// Represents a field reference
    /// </summary>
    [Serializable]
    public abstract class FieldRef : ReferenceStatement
    {
        Field _refedField;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cachedFieldName"></param>
        /// <param name="cachedFieldKind"></param>
        /// <param name="refedField"></param>
        public FieldRef(string cachedFieldName, string cachedFieldKind, Field refedField)
            : base(cachedFieldName, cachedFieldKind)
        {
            if (refedField == null)
                throw new NullReferenceException(this.GetType().FullName + ": Referenced field cannot be null!");
            _refedField = refedField;
        }

        /// <summary>
        /// Gets the Kind of this code.
        /// </summary>
        /// <returns></returns>
        public override Kind GetKind()
        {
            return ReferencedField.GetFieldKind(true); //use default only
        }

        /// <summary>
        /// Gets the referenced field.
        /// </summary>
        public Field ReferencedField
        {
            get { return _refedField; }
        }
    }

    /// <summary>
    /// Represents a method reference.
    /// </summary>
    [Serializable]
    public abstract class MethodRef : ReferenceStatement
    {
        Method _refedMethod;
        ValueStatement[] _args;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cachedMethodName"></param>
        /// <param name="cachedReturnKind"></param>
        /// <param name="refedMethod"></param>
        /// <param name="arguments"></param>
        public MethodRef(string cachedMethodName, string cachedReturnKind, Method refedMethod, params ValueStatement[] arguments)
            : base(cachedMethodName, cachedReturnKind)
        {
            if (refedMethod == null)
                throw new NullReferenceException(this.GetType().FullName + ": Referenced method cannot be null!");

            if (refedMethod.IsConstructor)// && !refedMethod.UnderlyingMethod.IsStatic)
                UseShortName = true;

            _refedMethod = refedMethod;
            _args = arguments == null ? new ValueStatement[] { } : arguments;
            for (int index = 0; index < _args.Length; index++)
            {
                _args[index].IsInline = true;
                _args[index].ShowOuterBrackets = false;
            }
        }

        /// <summary>
        /// Gets the Kind of this code.
        /// Returns null if the referenced method is a constructor
        /// </summary>
        /// <returns></returns>
        public override Kind GetKind()
        {
            return ReferencedMethod.GetReturnKind(true); //use default only
        }

        /// <summary>
        /// Gets the arguments supplied to method
        /// </summary>
        public ValueStatement[] Arguments
        {
            get { return _args; }
        }
        /// <summary>
        /// Gets the referenced method.
        /// </summary>
        public Method ReferencedMethod
        {
            get { return _refedMethod; }
        }
    }

    /// <summary>
    /// Represents an operation with one operand
    /// </summary>
    [Serializable]
    public abstract class UnaryOperation : Operation
    {
        ValueStatement _operand;

        /// <summary>
        /// Creates a new instance of Tril.UnaryOperation
        /// </summary>
        /// <param name="operand"></param>
        public UnaryOperation(ValueStatement operand)
        {
            if (operand == null)
                throw new NullReferenceException(this.GetType().FullName + ": Operand cannot be null!");

            _operand = operand;
            _operand.IsInline = true;

            this.Label = operand.Label;
        }

        /// <summary>
        /// Gets the operand this operation acts on
        /// </summary>
        public ValueStatement Operand
        {
            get { return _operand; }
        }
    }

    /// <summary>
    /// Represents a unary operation that is either mathematical or logical
    /// </summary>
    [Serializable]
    public abstract class UnaryArithmeticLogicOperation : UnaryOperation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="operand"></param>
        public UnaryArithmeticLogicOperation(ValueStatement operand)
            : base(operand)
        { }
    }

    /// <summary>
    /// Represents a unary arithmetic operation
    /// </summary>
    [Serializable]
    public abstract class UnaryArithmeticOperation : UnaryArithmeticLogicOperation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="operand"></param>
        public UnaryArithmeticOperation(ValueStatement operand)
            : base(operand)
        { }
    }

    /// <summary>
    /// Represents a unary logical operation
    /// </summary>
    [Serializable]
    public abstract class UnaryLogicOperation : UnaryArithmeticLogicOperation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="operand"></param>
        public UnaryLogicOperation(ValueStatement operand)
            : base(operand)
        { }

        /// <summary>
        /// Gets the Kind of this code.
        /// </summary>
        /// <returns></returns>
        public override Kind GetKind()
        {
            if (boolDef == null)
                init_boolDef();
            return Kind.GetCachedKind(boolDef);
        }
    }

    /// <summary>
    /// Represents an operation with two operands
    /// </summary>
    [Serializable]
    public abstract class BinaryOperation : Operation
    {
        ValueStatement _first, _second;

        /// <summary>
        /// Creates a new instance of Tril.Codoms.BinaryOperation
        /// </summary>
        /// <param name="firstOperand"></param>
        /// <param name="secondOperand"></param>
        public BinaryOperation(ValueStatement firstOperand, ValueStatement secondOperand)
        {
            if (firstOperand == null)
                throw new NullReferenceException(this.GetType().FullName + ": First operand cannot be null!");
            if (secondOperand == null)
                throw new NullReferenceException(this.GetType().FullName + ": Second operand cannot be null!");

            _first = firstOperand;
            _second = secondOperand;
            _first.IsInline = _second.IsInline = true;
        }

        /// <summary>
        /// Gets the first operand of this operation
        /// </summary>
        public ValueStatement FirstOperand
        {
            get { return _first; }
        }
        /// <summary>
        /// Gets the second operand of this operation
        /// </summary>
        public ValueStatement SecondOperand
        {
            get { return _second; }
        }

        /// <summary>
        /// Swaps the operands
        /// </summary>
        public void SwapOperands()
        {
            ValueStatement temp = _first;
            _first = _second;
            _second = temp;
        }
    }

    /// <summary>
    /// This represents a binary operation that performs an actual calculation.
    /// Assignment does not qualify.
    /// </summary>
    [Serializable]
    public abstract class BinaryArithmeticLogicOperation : BinaryOperation
    {
        /// <summary>
        /// Creates a new instance of Tril.Codoms.BinaryArithmeticLogicOperation
        /// </summary>
        /// <param name="firstOperand"></param>
        /// <param name="secondOperand"></param>
        public BinaryArithmeticLogicOperation(ValueStatement firstOperand, ValueStatement secondOperand)
            : base(firstOperand, secondOperand)
        {
            this.Label = firstOperand.Label;
        }
    }

    /// <summary>
    /// An arithmetic operation
    /// </summary>
    [Serializable]
    public abstract class BinaryArithmeticOperation : BinaryArithmeticLogicOperation
    {
        /// <summary>
        /// Creates a new instance of Tril.Codoms.BinaryArithmeticOperation
        /// </summary>
        /// <param name="firstOperand"></param>
        /// <param name="secondOperand"></param>
        public BinaryArithmeticOperation(ValueStatement firstOperand, ValueStatement secondOperand)
            : base(firstOperand, secondOperand)
        {
        }
    }

    /// <summary>
    /// A logic operation
    /// </summary>
    [Serializable]
    public abstract class BinaryLogicOperation : BinaryArithmeticLogicOperation
    {
        /// <summary>
        /// Creates a new instance of Tril.Codoms.BinaryLogicOperation
        /// </summary>
        /// <param name="firstOperand"></param>
        /// <param name="secondOperand"></param>
        public BinaryLogicOperation(ValueStatement firstOperand, ValueStatement secondOperand)
            : base(firstOperand, secondOperand)
        { }

        /// <summary>
        /// Gets the Kind of this code.
        /// </summary>
        /// <returns></returns>
        public override Kind GetKind()
        {
            if (boolDef == null)
                init_boolDef();
            return Kind.GetCachedKind(boolDef);
        }
    }

    /// <summary>
    /// Represents a block code.
    /// Block codes are not inline by default.
    /// </summary>
    [Serializable]
    public class Block : Codom, IList<Codom>
    {
        List<Codom> _listCode = new List<Codom>();

        /// <summary>
        /// Creates a new instance of Tril.Codoms.Body
        /// </summary>
        public Block()
        {
            IsInline = false;
        }

        /// <summary>
        /// Returns the index of the item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(Codom item)
        {
            return _listCode.IndexOf(item);
        }
        /// <summary>
        /// Inserts the specified item
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, Codom item)
        {
            _listCode.Insert(index, item);
        }
        /// <summary>
        /// Removes item at index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            _listCode.RemoveAt(index);
        }
        /// <summary>
        /// Accesses index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Codom this[int index]
        {
            get
            {
                return _listCode[index];
            }
            set
            {
                _listCode[index] = value;
            }
        }
        /// <summary>
        /// Adds item
        /// </summary>
        /// <param name="item"></param>
        public void Add(Codom item)
        {
            _listCode.Add(item);
        }
        /// <summary>
        /// Clears collection
        /// </summary>
        public void Clear()
        {
            _listCode.Clear();
        }
        /// <summary>
        /// Returns true if item is found
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(Codom item)
        {
            return _listCode.Contains(item);
        }
        /// <summary>
        /// Copies collection to array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(Codom[] array, int arrayIndex)
        {
            _listCode.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Returns number of items
        /// </summary>
        public int Count
        {
            get { return _listCode.Count; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }
        /// <summary>
        /// Removes item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(Codom item)
        {
            return _listCode.Remove(item);
        }
        /// <summary>
        /// enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Codom> GetEnumerator()
        {
            return _listCode.GetEnumerator();
        }
        /// <summary>
        /// enumerator
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _listCode.GetEnumerator();
        }

        /// <summary>
        /// Returns the C# representation of this code
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
                string codeString = "";
                codeString += " {\r\n";
                foreach (Codom bodyCode in this)
                {
                    codeString += bodyCode.ToString(translator) + "\r\n";
                }
                codeString += "}";

                return codeString;
            }
        }
    }

    /// <summary>
    /// A block that naturally executes the codes in its body based on conditions.
    /// </summary>
    [Serializable]
    public abstract class SelectionBlock : Block
    { }

    //[Serializable]public class Switch : SelectionBlock { }

    /// <summary>
    /// A block that naturally executes the codes in its body in a loop based on conditions.
    /// </summary>
    [Serializable]
    public abstract class LoopingBlock : Block
    { }

    //[Serializable]public class For : LoopingBlock { }
    //[Serializable]public class While : LoopingBlock { }
}
