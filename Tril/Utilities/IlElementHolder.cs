using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Tril.Utilities
{
    /// <summary>
    /// Encapsulates all IL elements
    /// </summary>
    public class IlElementHolder
    {
        dynamic _held;
        /// <summary>
        /// Creates an instance of IlElementHolder to encapsulate a Mono.Cecil.Cil.ExceptionHandler object
        /// </summary>
        /// <param name="handler"></param>
        public IlElementHolder(ExceptionHandler handler) { _held = handler; }
        /// <summary>
        /// Creates an instance of IlElementHolder to encapsulate a Mono.Cecil.Cil.Instruction object
        /// </summary>
        /// <param name="instruction"></param>
        public IlElementHolder(Instruction instruction) { _held = instruction; }
        /// <summary>
        /// Creates an instance of IlElementHolder to encapsulate a Tril.Utiltities.OtherIlElements object
        /// </summary>
        /// <param name="element"></param>
        public IlElementHolder(OtherIlElements element) { _held = element; }

        /// <summary>
        /// Gets the element held by this
        /// </summary>
        public dynamic ElementHeld
        {
            get { return _held; }
        }
        /// <summary>
        /// Gets a value to determine whether the held element is an exception handler
        /// </summary>
        public bool HoldsHandler
        {
            get { return _held is ExceptionHandler; }
        }
        /// <summary>
        /// Gets a value to determine whether the held element is an instruction
        /// </summary>
        public bool HoldsInstruction
        {
            get { return _held is Instruction; }
        }
        /// <summary>
        /// Gets a value to determine whether the held element is another element type
        /// </summary>
        public bool HoldsOthers
        {
            get { return _held is OtherIlElements; }
        }
        /// <summary>
        /// Returns the string equivalent of the IL element holder
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_held is OtherIlElements)
            {
                OtherIlElements otherIlElements = (OtherIlElements)_held;
                if (otherIlElements == OtherIlElements.BeginTry)
                    return "try {";
                else if (otherIlElements == OtherIlElements.BeginFilter)
                    return "filter {";
                else if (otherIlElements == OtherIlElements.EndCatch)
                    return "} //end of catch block";
                else if (otherIlElements == OtherIlElements.EndFault)
                    return "} //end of fault block";
                else if (otherIlElements == OtherIlElements.EndFilter)
                    return "} //end of filter block";
                else if (otherIlElements == OtherIlElements.EndFilterHandler)
                    return "} //end of filter handler block";
                else if (otherIlElements == OtherIlElements.EndFinally)
                    return "} //end of finally block";
                else if (otherIlElements == OtherIlElements.EndTry)
                    return "} //end of try block";
            }
            else if (_held is ExceptionHandler)
            {
                ExceptionHandler exHandler = (_held as ExceptionHandler);
                if (exHandler.HandlerType == ExceptionHandlerType.Catch)
                {
                    string str = "catch";
                    if (exHandler.CatchType != null)
                    {
                        if (exHandler.CatchType.FullName != null)
                            str += " (" + exHandler.CatchType.FullName + " ____ex" + DateTime.Now.Ticks.ToString() + ")";
                    }
                    str += " {";
                    return str;
                }
                else if (exHandler.HandlerType == ExceptionHandlerType.Fault)
                    return "fault {";
                else if (exHandler.HandlerType == ExceptionHandlerType.Filter)
                    return "filter handler {";
                else if (exHandler.HandlerType == ExceptionHandlerType.Finally)
                    return "finally {";
            }
            return _held.ToString();
        }
    }

    /// <summary>
    /// Other IL Elements
    /// </summary>
    public enum OtherIlElements
    {
        /// <summary>
        /// begin try
        /// </summary>
        BeginTry,
        /// <summary>
        /// begin filter
        /// </summary>
        BeginFilter,
        /// <summary>
        /// end catch
        /// </summary>
        EndCatch,
        /// <summary>
        /// end fault
        /// </summary>
        EndFault,
        /// <summary>
        /// end filter
        /// </summary>
        EndFilter,
        /// <summary>
        /// end filter handler
        /// </summary>
        EndFilterHandler,
        /// <summary>
        /// end finally
        /// </summary>
        EndFinally,
        /// <summary>
        /// end try
        /// </summary>
        EndTry
    }
}
