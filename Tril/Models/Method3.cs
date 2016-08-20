using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using Mono.Cecil;
using Mono.Cecil.Cil;

using Tril.Attributes;
using Tril.Codoms;
using Tril.Delegates;
using Tril.Exceptions;
using Tril.Utilities;

namespace Tril.Models
{
    public partial class Method
    {
        IlElementHolder _currIlElement;
        List<IlElementHolder> _ilElementsProc = new List<IlElementHolder>();
        List<Codom> _translatedCodes = new List<Codom>();
        List<Variable> _methodLocals = new List<Variable>();
        List<Report> _reportTrace = new List<Report>();
        Stack<Statement> _methodStack = new Stack<Statement>();
        Stack<Block> _currentBlockStack = new Stack<Block>();

        /// <summary>
        /// Gets a collection of all the codes in the method.
        /// If the method has been marked with [HideImplementationAttribute], null is returned
        /// </summary>
        /// <returns></returns>
        public Block GetBody()
        {
            return GetBody("*");
        }
        /// <summary>
        /// Gets a collection of all the codes in the method.
        /// If the method has been marked with [HideImplementationAttribute], null is returned
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <returns></returns>
        public Block GetBody(bool useDefaultOnly)
        {
            return GetBody(useDefaultOnly, "*");
        }
        /// <summary>
        /// Gets a collection of all the codes in the method.
        /// If the method has been marked with [HideImplementationAttribute], null is returned
        /// </summary>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public Block GetBody(params string[] targetPlats)
        {
            return GetBody(false, targetPlats);
        }
        /// <summary>
        /// Gets a body of all the codes in the method.
        /// If the method has been marked with [HideImplementationAttribute], null is returned
        /// </summary>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public Block GetBody(bool useDefaultOnly, params string[] targetPlats)
        {
            return GetBody(null, useDefaultOnly, targetPlats);
        }
        /// <summary>
        /// Gets a body of all the codes in the method.
        /// If the method has been marked with [HideImplementationAttribute], null is returned
        /// </summary>
        /// <param name="translator"></param>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public Block GetBody(CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            return GetBody(translator, false, true, useDefaultOnly, targetPlats);
        }
        /// <summary>
        /// Gets a body of all the codes in the method.
        /// If the method has been marked with [HideImplementationAttribute], null is returned
        /// </summary>
        /// <param name="translator">
        /// A delegate pointing to a method that can override the default string representation of codes.
        /// </param>
        /// <param name="returnPartial">
        /// If true, if an error occurs while reading the CIL of a method, it returns the partially-built method body,
        /// rather than throwing an exception.
        /// </param>
        /// <param name="optimize">
        /// If true, attempts are made to dress the generated codes to look codes written by a human being.
        /// Generally, this reduces the size of the codes, but this also makes the operation slower.
        /// </param>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        public Block GetBody(CodomTranslator translator, bool returnPartial, bool optimize, bool useDefaultOnly, params string[] targetPlats)
        {
            targetPlats = Utility.UnNullifyArray(targetPlats);

            ClearTrace();

            //if the implementation of the method is hidden, dont show it, even if useDeafault is true
            //I might change this behaviour later by if (!IsHiddenImplementation(targetPlats) || useDefault)
            if (!HasHiddenImplementation(targetPlats))
            {
                Block methodBody = new Block();

                //see if it has user-defined implementation
                if (HasUserDefinedImplementation(targetPlats) && !useDefaultOnly)
                {
                    AddToTrace(new Report("getting user-defined codes...", ReportType.OperationInProgress));

                    try
                    {
                        bool codeOutside = false;
                        var codeAttris = GetAllCustomAttributes().Where(a => a != null && a.AttributeType.FullName == "Tril.Attributes.CodeAttribute");
                        foreach (CustomAttribute attri in codeAttris)
                        {
                            string[] tgtLangsVal = GetTargetPlatforms(attri);
                            foreach (string lang in targetPlats)
                            {
                                if (tgtLangsVal.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                                {
                                    AddToTrace(new Report("getting user-defined codes from [CodeAttribute]...", ReportType.OperationInProgress));
                                    methodBody.Add(new UserCode(attri.ConstructorArguments[0].Value.ToString()));
                                    codeOutside = true;
                                    AddToTrace(new Report("getting user-defined codes from [CodeAttribute] succeeded", ReportType.OperationCompleted));
                                    break;
                                }
                            }
                        }
                        if (!codeOutside) //then code must be inside; the method is marked with the [CodeInside] attribute
                        {
                            AddToTrace(new Report("getting user-defined codes from method body...", ReportType.OperationInProgress));

                            //get the method
                            System.Reflection.MethodBase DotNetUnderlyingMethod = UnderlyingMethod.ToMethod(true);
                            //invoke the method
                            object instance = Activator.CreateInstance(DotNetUnderlyingMethod.DeclaringType);
                            //get the method parameters
                            //such a method accepts a param of params Object[]
                            //which the method can use to accept args in C#
                            //but the printed parameter list shud come from the [ParamsSecAttribute]
                            //we invoke the method passing a meaningless/dataless/empty (perhaps, in the future, null) Object[] object;
                            //the programmer must write the method with the assumption that Tril will pass a null Object[] object to the method
                            object[] paramList = new object[1]; paramList[0] = null;
                            object returned = DotNetUnderlyingMethod.Invoke(instance, paramList);
                            //such a method can return object (might be dynamic in the future)
                            //but its returned value is cast to IDictionary<string[], Block>
                            if (returned is IDictionary<string[], Block>)
                            {
                                IDictionary<string[], Block> returnedBody = returned as IDictionary<string[], Block>;
                                bool breakOuter = false;
                                foreach (string[] key in returnedBody.Keys)
                                {
                                    if (key != null)
                                    {
                                        foreach (string lang in targetPlats)
                                        {
                                            if (key.Any(l => l.Trim().ToLower() == lang.Trim().ToLower()))
                                            {
                                                methodBody = returnedBody[key];
                                                breakOuter = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (breakOuter)
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        var ex = new MethodNotWellFormedException("This method does not follow the expected format!", e);
                        AddToTrace(new Report(ex.Message, ReportType.Error));

                        if (returnPartial)
                            return methodBody;
                        else
                            throw ex;
                    }

                    AddToTrace(new Report("getting user-defined codes completed", ReportType.OperationCompleted));

                }
                //if not, get its CIL
                else
                {
                    AddToTrace(new Report("getting default codes...", ReportType.OperationInProgress));

                    try
                    {
                        var translatedCodes =
                            GetTranslatedCodes(GetIlProcessor(), translator,
                            returnPartial, optimize, useDefaultOnly, targetPlats);
                        foreach (Codom code in translatedCodes)
                        {
                            if (code != null)
                                methodBody.Add(code);
                        }
                    }
                    catch (StackPopException popEx)
                    {
                        var newPopEx = new MethodBodyNotWellFormedException("Pop operation failed on code stack!", popEx);
                        AddToTrace(new Report(newPopEx.Message, ReportType.Error));
                        if (returnPartial)
                            return methodBody;
                        else
                            throw newPopEx;
                    }
                    catch (StackPushException pushEx)
                    {
                        var newPushEx = new MethodBodyNotWellFormedException("Push operation failed on code stack!", pushEx);
                        AddToTrace(new Report(newPushEx.Message, ReportType.Error));
                        if (returnPartial)
                            return methodBody;
                        else
                            throw newPushEx;
                    }
                    catch (MethodBodyNotReadableException mthdEx)
                    {
                        if (returnPartial)
                            return methodBody;
                        else
                            throw mthdEx;
                    }
                    catch (MethodNotWellFormedException mthdEx)
                    {
                        if (returnPartial)
                            return methodBody;
                        else
                            throw mthdEx;
                    }
                    catch (Exception e)
                    {
                        var ex = new MethodBodyNotReadableException("The implementation for this method could not be retrieved!", e);
                        AddToTrace(new Report(ex.Message, ReportType.Error));
                        if (returnPartial)
                            return methodBody;
                        else
                            throw ex;
                    }

                    AddToTrace(new Report("getting default codes completed", ReportType.OperationCompleted));
                }

                return methodBody;
            }

            return null;
        }
        /// <summary>
        /// The magic method that gets the IL instructions in a method.
        /// </summary>
        /// <returns></returns>
        ILProcessor GetIlProcessor() 
        {
            try
            {
                ILProcessor ilprocessor = null;

                AddToTrace(new Report("reading method '" + UnderlyingMethod.Name + "'...", ReportType.OperationInProgress));
                var MethodDef = GetMethodDefinition();
                if (MethodDef == null || MethodDef.Body == null) 
                {
                    if (!MethodDef.IsIL)
                        throw new MethodBodyNotReadableException("Method body could not be found because method is not 'cil managed'!");
                    else if (MethodDef.IsPInvokeImpl)
                        throw new MethodBodyNotReadableException("Method body could not be found because method is implemented using 'PInvoke'!");
                    else
                        throw new MethodBodyNotReadableException("Method body could not be found!");
                }
                ilprocessor = MethodDef.Body.GetILProcessor();
                AddToTrace(new Report("reading method '" + UnderlyingMethod.Name + "' succeeded", ReportType.OperationCompleted));

                return ilprocessor;
            }
            catch (Exception ex)
            {
                AddToTrace(new Report(ex.Message, ReportType.Error));
                throw ex;
            }
        }
        /// <summary>
        /// Returns a list of codes generated from a list of CIL instructions
        /// </summary>
        /// <param name="ilProcessor"></param>
        /// <param name="translator"></param>
        /// <param name="returnPartial"></param>
        /// <param name="optimize"></param>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        List<Codom> GetTranslatedCodes(ILProcessor ilProcessor, CodomTranslator translator, bool returnPartial,
            bool optimize, bool useDefaultOnly, params string[] targetPlats) 
        {
            AddToTrace(new Report("getting IL...", ReportType.OperationInProgress));
            if (ilProcessor == null || ilProcessor.Body == null)
                throw new MethodBodyNotReadableException("Method body could not be found!");
            AddToTrace(new Report("getting IL succeeded", ReportType.OperationCompleted));

            List<IlElementHolder> ELEMENTHOLDERS = new List<IlElementHolder>();
            ClearCurrentIlElement();
            ClearProcessedIlElements();

            ClearCurrentBlockStack();

            ClearTranslatedCodes();
            ClearStack();

            ClearLocals();
            List<VariableReference> LOCALS = new List<VariableReference>();

            try
            {
                AddToTrace(new Report("declaring variables...", ReportType.OperationInProgress));
                #region begin data section
                DataSection dataSec = new DataSection();
                AddToTranslatedCodes(dataSec);
                PushToCurrentBlockStack(dataSec);
                ////add comment
                //Comment dataSecComment = new Comment("start of data section");
                //dataSecComment.IsBlockComment = false;
                //AddToTranslatedCodes(dataSecComment);
                #endregion
                if (ilProcessor.Body.Variables != null)
                    LOCALS.AddRange(ilProcessor.Body.Variables);
                //first, declare locals
                foreach (VariableReference variable in LOCALS)
                {
                    if (variable != null)
                    {
                        Variable v = Variable.GetCachedVariable(variable, this);
                        AddToLocals(v, useDefaultOnly, targetPlats);
                    }
                }
                #region end data section
                Block dataSecEnd = PeekFromCurrentBlockStack();
                if (dataSecEnd != null && dataSecEnd is DataSection)
                {
                    PopFromCurrentBlockStack();
                    ////add comment
                    //Comment dataSecEndComment = new Comment("end of data section");
                    //dataSecEndComment.IsBlockComment = false;
                    //AddToTranslatedCodes(dataSecEndComment);
                }
                #endregion
                AddToTrace(new Report("declaring variables succeeded", ReportType.OperationCompleted));

                #region Hold relevant information
                //hold the instructions
                if (ilProcessor.Body.Instructions != null && ilProcessor.Body.Instructions.Count > 0)
                {
                    foreach (Instruction instr in ilProcessor.Body.Instructions)
                    {
                        if (instr != null && instr.OpCode != null)
                        {
                            IlElementHolder holder = new IlElementHolder(instr);
                            ELEMENTHOLDERS.Add(holder);
                        }
                    }
                }
                //hold exception handlers and others
                if (ilProcessor.Body.HasExceptionHandlers)
                {
                    recordedTryStarts.Clear();
                    recordedTryEnds.Clear();
                    recordedFilterStarts.Clear();
                    foreach (ExceptionHandler handler in ilProcessor.Body.ExceptionHandlers)
                    {
                        IlElementHolder targetHolder = null;

                        //try start and end___________________________________________________________
                        //bearing in mind that multiple exception handlers can have the same try block
                        //attempts must be made to avoid recording a single try more than once
                        if (IsUniqueTryBlock(handler.TryStart, handler.TryEnd))
                        {
                            //try start___________________________________________________________
                            IlElementHolder tryStartHolder = new IlElementHolder(OtherIlElements.BeginTry);
                            targetHolder = ELEMENTHOLDERS.First(h => h.HoldsInstruction &&
                            GetLabel(handler.TryStart) == GetLabel((Instruction)h.ElementHeld));
                            if (targetHolder != null)
                            {
                                ELEMENTHOLDERS.Insert(ELEMENTHOLDERS.IndexOf(targetHolder), tryStartHolder);
                            }

                            //try end___________________________________________________________
                            IlElementHolder tryEndHolder = new IlElementHolder(OtherIlElements.EndTry);
                            targetHolder = ELEMENTHOLDERS.First(h => h.HoldsInstruction &&
                            GetLabel(handler.TryEnd) == GetLabel((Instruction)h.ElementHeld));
                            if (targetHolder != null)
                            {
                                ELEMENTHOLDERS.Insert(ELEMENTHOLDERS.IndexOf(targetHolder), tryEndHolder);
                            }
                        }

                        //filter start and end___________________________________________________________
                        //bearing in mind that multiple exception handlers may have the same filter block
                        //attempts must be made to avoid recording a single filter more than once
                        if (handler.HandlerType == ExceptionHandlerType.Filter && IsUniqueFilterBlock(handler.FilterStart))
                        {
                            //filter start___________________________________________________________
                            IlElementHolder filterStartHolder = new IlElementHolder(OtherIlElements.BeginFilter);
                            targetHolder = ELEMENTHOLDERS.First(h => h.HoldsInstruction &&
                            GetLabel(handler.FilterStart) == GetLabel((Instruction)h.ElementHeld));
                            if (targetHolder != null)
                            {
                                ELEMENTHOLDERS.Insert(ELEMENTHOLDERS.IndexOf(targetHolder), filterStartHolder);
                            }

                            //filter end___________________________________________________________
                            IlElementHolder filterEndHolder = new IlElementHolder(OtherIlElements.EndFilter);
                            targetHolder = ELEMENTHOLDERS.First(h => h.HoldsInstruction &&
                            GetLabel(handler.HandlerStart) == GetLabel((Instruction)h.ElementHeld));
                            if (targetHolder != null)
                            {
                                ELEMENTHOLDERS.Insert(ELEMENTHOLDERS.IndexOf(targetHolder), filterEndHolder);
                            }
                        }

                        //handler start___________________________________________________________
                        IlElementHolder handlerStartHolder = new IlElementHolder(handler);
                        targetHolder = ELEMENTHOLDERS.First(h => h.HoldsInstruction &&
                            GetLabel(handler.HandlerStart) == GetLabel((Instruction)h.ElementHeld));
                        if (targetHolder != null)
                        {
                            ELEMENTHOLDERS.Insert(ELEMENTHOLDERS.IndexOf(targetHolder), handlerStartHolder);
                        }

                        //handler end___________________________________________________________
                        IlElementHolder handlerEndHolder = null;
                        if (handler.HandlerType == ExceptionHandlerType.Catch)
                            handlerEndHolder = new IlElementHolder(OtherIlElements.EndCatch);
                        else if (handler.HandlerType == ExceptionHandlerType.Fault)
                            handlerEndHolder = new IlElementHolder(OtherIlElements.EndFault);
                        else if (handler.HandlerType == ExceptionHandlerType.Filter)
                            handlerEndHolder = new IlElementHolder(OtherIlElements.EndFilterHandler);
                        else if (handler.HandlerType == ExceptionHandlerType.Finally)
                            handlerEndHolder = new IlElementHolder(OtherIlElements.EndFinally);
                        targetHolder = ELEMENTHOLDERS.First(h => h.HoldsInstruction &&
                            GetLabel(handler.HandlerEnd) == GetLabel((Instruction)h.ElementHeld));
                        if (targetHolder != null)
                        {
                            ELEMENTHOLDERS.Insert(ELEMENTHOLDERS.IndexOf(targetHolder), handlerEndHolder);
                        }
                    }
                }
                #endregion

                AddToTrace(new Report("iterating through IL instructions...", ReportType.OperationInProgress));
                #region begin code section
                CodeSection codeSec = new CodeSection();
                AddToTranslatedCodes(codeSec);
                PushToCurrentBlockStack(codeSec);
                ////add comment
                //Comment codeSecComment = new Comment("start of code section");
                //codeSecComment.IsBlockComment = false;
                //AddToTranslatedCodes(codeSecComment);
                #endregion
                foreach (IlElementHolder holder in ELEMENTHOLDERS)
                {
                    if (holder != null)
                        SetAsCurrentIlElement(holder);

                    #region Instructions
                    if (holder != null && holder.HoldsInstruction) 
                    {
                        Instruction instr = (Instruction)holder.ElementHeld;
                        if (instr != null) 
                        {
                            if (instr.OpCode != null) 
                            {
                                #region Differences btwn System.Reflection.Emit.Opcodes and Mono.Cecil.Cil.Opcodes
                                /*
                                 * System.Reflection.Emit.Opcodes.Ldelem == Mono.Cecil.Cil.Opcodes.Ldelem_Any
                                 * System.Reflection.Emit.Opcodes.Stelem == Mono.Cecil.Cil.Opcodes.Stelem_Any
                                 * Mono.Cecil.Cil.Opcodes.No == ?cant find it?
                                 * System.Reflection.Emit.Opcodes.Prefix1, .Prefix2, ...Prefix7, .Prefixref are described as reserved 
                                 *      instructions, and do not exist in Mono.Cecil.Cil.Opcodes
                                 * System.Reflection.Emit.Tailcall == Mono.Cecil.Cil.Tail
                                 */
                                #endregion
                                #region OpCodes you have to confirm using other languages (not C#)
                                /*
                                 * Calli (not yet implemented)
                                 * Cpblk (not yet implemented)
                                 * Cpobj
                                 * Initblk (not yet implemented)
                                 * Jmp (not yet implemented)
                                 * Localloc
                                 */
                                #endregion
                                #region Loads
                                if (instr.OpCode.Code == Code.Ldarg || instr.OpCode.Code == Code.Ldarg_0 || instr.OpCode.Code == Code.Ldarg_1
                                    || instr.OpCode.Code == Code.Ldarg_2 || instr.OpCode.Code == Code.Ldarg_3 || instr.OpCode.Code == Code.Ldarg_S
                                    || instr.OpCode.Code == Code.Ldarga || instr.OpCode.Code == Code.Ldarga_S
                                    || instr.OpCode.Code == Code.Ldc_I4
                                    || instr.OpCode.Code == Code.Ldc_I4_0 || instr.OpCode.Code == Code.Ldc_I4_1 || instr.OpCode.Code == Code.Ldc_I4_2
                                    || instr.OpCode.Code == Code.Ldc_I4_3 || instr.OpCode.Code == Code.Ldc_I4_4 || instr.OpCode.Code == Code.Ldc_I4_5
                                    || instr.OpCode.Code == Code.Ldc_I4_6 || instr.OpCode.Code == Code.Ldc_I4_7 || instr.OpCode.Code == Code.Ldc_I4_8
                                    || instr.OpCode.Code == Code.Ldc_I4_M1 || instr.OpCode.Code == Code.Ldc_I4_S || instr.OpCode.Code == Code.Ldc_I8
                                    || instr.OpCode.Code == Code.Ldc_R4 || instr.OpCode.Code == Code.Ldc_R8
                                    || instr.OpCode.Code == Code.Ldelem_Any || instr.OpCode.Code == Code.Ldelem_I
                                    || instr.OpCode.Code == Code.Ldelem_I1 || instr.OpCode.Code == Code.Ldelem_I2
                                    || instr.OpCode.Code == Code.Ldelem_I4 || instr.OpCode.Code == Code.Ldelem_I8
                                    || instr.OpCode.Code == Code.Ldelem_R4 || instr.OpCode.Code == Code.Ldelem_R8
                                    || instr.OpCode.Code == Code.Ldelem_Ref || instr.OpCode.Code == Code.Ldelem_U1
                                    || instr.OpCode.Code == Code.Ldelem_U2 || instr.OpCode.Code == Code.Ldelem_U4
                                    || instr.OpCode.Code == Code.Ldelema
                                    || instr.OpCode.Code == Code.Ldfld
                                    || instr.OpCode.Code == Code.Ldflda || instr.OpCode.Code == Code.Ldftn
                                    || instr.OpCode.Code == Code.Ldind_I || instr.OpCode.Code == Code.Ldind_I1 || instr.OpCode.Code == Code.Ldind_I2
                                    || instr.OpCode.Code == Code.Ldind_I4 || instr.OpCode.Code == Code.Ldind_I8 || instr.OpCode.Code == Code.Ldind_R4
                                    || instr.OpCode.Code == Code.Ldind_R8
                                    || instr.OpCode.Code == Code.Ldind_Ref
                                    || instr.OpCode.Code == Code.Ldind_U1 || instr.OpCode.Code == Code.Ldind_U2 || instr.OpCode.Code == Code.Ldind_U4
                                    || instr.OpCode.Code == Code.Ldlen
                                    || instr.OpCode.Code == Code.Ldloc || instr.OpCode.Code == Code.Ldloc_0 || instr.OpCode.Code == Code.Ldloc_1
                                    || instr.OpCode.Code == Code.Ldloc_2 || instr.OpCode.Code == Code.Ldloc_3 || instr.OpCode.Code == Code.Ldloc_S
                                    || instr.OpCode.Code == Code.Ldloca || instr.OpCode.Code == Code.Ldloca_S
                                    || instr.OpCode.Code == Code.Ldnull
                                    || instr.OpCode.Code == Code.Ldobj
                                    || instr.OpCode.Code == Code.Ldsfld
                                    || instr.OpCode.Code == Code.Ldsflda
                                    || instr.OpCode.Code == Code.Ldstr
                                    || instr.OpCode.Code == Code.Ldtoken || instr.OpCode.Code == Code.Ldvirtftn
                                    )
                                {
                                    LoadOps(ilProcessor, instr, translator, useDefaultOnly, targetPlats);
                                }
                                #endregion
                                #region Stores
                                else if (instr.OpCode.Code == Code.Starg || instr.OpCode.Code == Code.Starg_S
                                    || instr.OpCode.Code == Code.Stelem_Any || instr.OpCode.Code == Code.Stelem_I || instr.OpCode.Code == Code.Stelem_I1
                                    || instr.OpCode.Code == Code.Stelem_I2 || instr.OpCode.Code == Code.Stelem_I4 || instr.OpCode.Code == Code.Stelem_I8
                                    || instr.OpCode.Code == Code.Stelem_R4 || instr.OpCode.Code == Code.Stelem_R8 || instr.OpCode.Code == Code.Stelem_Ref
                                    || instr.OpCode.Code == Code.Stfld || instr.OpCode.Code == Code.Stind_I || instr.OpCode.Code == Code.Stind_I1
                                    || instr.OpCode.Code == Code.Stind_I2 || instr.OpCode.Code == Code.Stind_I4 || instr.OpCode.Code == Code.Stind_I8
                                    || instr.OpCode.Code == Code.Stind_R4 || instr.OpCode.Code == Code.Stind_R8
                                    || instr.OpCode.Code == Code.Stind_Ref
                                    || instr.OpCode.Code == Code.Stloc || instr.OpCode.Code == Code.Stloc_0 || instr.OpCode.Code == Code.Stloc_1
                                    || instr.OpCode.Code == Code.Stloc_2 || instr.OpCode.Code == Code.Stloc_3 || instr.OpCode.Code == Code.Stloc_S
                                    || instr.OpCode.Code == Code.Stobj
                                    || instr.OpCode.Code == Code.Stsfld
                                    )
                                {
                                    StoreOps(ilProcessor, instr, translator, useDefaultOnly, targetPlats);
                                }
                                #endregion
                                #region Arithmetic, Logic Binary Operations
                                else if (instr.OpCode.Code == Code.Add || instr.OpCode.Code == Code.Add_Ovf || instr.OpCode.Code == Code.Add_Ovf_Un
                                    || instr.OpCode.Code == Code.And || instr.OpCode.Code == Code.Ceq || instr.OpCode.Code == Code.Cgt
                                    || instr.OpCode.Code == Code.Cgt_Un || instr.OpCode.Code == Code.Clt || instr.OpCode.Code == Code.Clt_Un
                                    || instr.OpCode.Code == Code.Div || instr.OpCode.Code == Code.Div_Un || instr.OpCode.Code == Code.Mul
                                    || instr.OpCode.Code == Code.Mul_Ovf || instr.OpCode.Code == Code.Mul_Ovf_Un || instr.OpCode.Code == Code.Or
                                    || instr.OpCode.Code == Code.Rem || instr.OpCode.Code == Code.Rem_Un || instr.OpCode.Code == Code.Shl
                                    || instr.OpCode.Code == Code.Shr || instr.OpCode.Code == Code.Shr_Un || instr.OpCode.Code == Code.Sub
                                    || instr.OpCode.Code == Code.Sub_Ovf || instr.OpCode.Code == Code.Sub_Ovf_Un || instr.OpCode.Code == Code.Xor)
                                {
                                    ArithLogicBiOps(ilProcessor, instr, translator, useDefaultOnly, targetPlats);
                                }
                                #endregion
                                #region Arithmetic, Logic Unary Operations
                                else if (instr.OpCode.Code == Code.Localloc || instr.OpCode.Code == Code.Neg || instr.OpCode.Code == Code.Not)
                                {
                                    ArithLogicUnOps(ilProcessor, instr, translator, useDefaultOnly, targetPlats);
                                }
                                #endregion
                                #region Conditional Branching Operations
                                else if (instr.OpCode.Code == Code.Beq || instr.OpCode.Code == Code.Beq_S || instr.OpCode.Code == Code.Bge
                                    || instr.OpCode.Code == Code.Bge_S || instr.OpCode.Code == Code.Bge_Un || instr.OpCode.Code == Code.Bge_Un_S
                                    || instr.OpCode.Code == Code.Bgt || instr.OpCode.Code == Code.Bgt_S || instr.OpCode.Code == Code.Bgt_Un
                                    || instr.OpCode.Code == Code.Bgt_Un_S || instr.OpCode.Code == Code.Ble || instr.OpCode.Code == Code.Ble_S
                                    || instr.OpCode.Code == Code.Ble_Un || instr.OpCode.Code == Code.Ble_Un_S || instr.OpCode.Code == Code.Blt
                                    || instr.OpCode.Code == Code.Blt_S || instr.OpCode.Code == Code.Blt_Un || instr.OpCode.Code == Code.Blt_Un_S
                                    || instr.OpCode.Code == Code.Bne_Un || instr.OpCode.Code == Code.Bne_Un_S || instr.OpCode.Code == Code.Brfalse
                                    || instr.OpCode.Code == Code.Brfalse_S || instr.OpCode.Code == Code.Brtrue || instr.OpCode.Code == Code.Brtrue_S)
                                {
                                    ConditionalBranchingOps(ilProcessor, instr, translator, useDefaultOnly, targetPlats);
                                }
                                else if (instr.OpCode.Code == Code.Switch)
                                {
                                    SwitchOps(ilProcessor, instr, translator, useDefaultOnly, targetPlats);
                                }
                                #endregion
                                #region Unconditional Branching Operations
                                else if (instr.OpCode.Code == Code.Br || instr.OpCode.Code == Code.Br_S
                                    || instr.OpCode.Code == Code.Leave || instr.OpCode.Code == Code.Leave_S
                                    || instr.OpCode.Code == Code.Ret || instr.OpCode.Code == Code.Rethrow || instr.OpCode.Code == Code.Throw)
                                {
                                    UnConditionalBranchingOps(ilProcessor, instr, translator, useDefaultOnly, targetPlats);
                                }
                                #endregion
                                #region Method and Constructor Calls
                                else if (instr.OpCode.Code == Code.Call /*|| instr.OpCode.Code == Code.Calli*/ || instr.OpCode.Code == Code.Callvirt
                                    || instr.OpCode.Code == Code.Newobj)
                                {
                                    MethodCallOps(ilProcessor, instr, translator, useDefaultOnly, targetPlats);
                                }
                                #endregion
                                #region Object Operations
                                else if (instr.OpCode.Code == Code.Cpobj)
                                {
                                    CopyObjectOps(ilProcessor, instr, translator, useDefaultOnly, targetPlats);
                                }
                                else if (instr.OpCode.Code == Code.Initobj)
                                {
                                    InitObjectOps(ilProcessor, instr, translator, useDefaultOnly, targetPlats);
                                }
                                #endregion
                                #region Array Operations
                                else if (instr.OpCode.Code == Code.Newarr)
                                {
                                    ArrayOps(ilProcessor, instr, translator, useDefaultOnly, targetPlats);
                                }
                                #endregion
                                #region Conversions
                                else if (instr.OpCode.Code == Code.Conv_I || instr.OpCode.Code == Code.Conv_I1
                                    || instr.OpCode.Code == Code.Conv_I2 || instr.OpCode.Code == Code.Conv_I4
                                    || instr.OpCode.Code == Code.Conv_I8 || instr.OpCode.Code == Code.Conv_R_Un
                                    || instr.OpCode.Code == Code.Conv_R4 || instr.OpCode.Code == Code.Conv_R8
                                    || instr.OpCode.Code == Code.Conv_U || instr.OpCode.Code == Code.Conv_U1
                                    || instr.OpCode.Code == Code.Conv_U2 || instr.OpCode.Code == Code.Conv_U4
                                    || instr.OpCode.Code == Code.Conv_U8
                                    || instr.OpCode.Code == Code.Conv_Ovf_I || instr.OpCode.Code == Code.Conv_Ovf_I1
                                    || instr.OpCode.Code == Code.Conv_Ovf_I2 || instr.OpCode.Code == Code.Conv_Ovf_I4
                                    || instr.OpCode.Code == Code.Conv_Ovf_I8 || instr.OpCode.Code == Code.Conv_Ovf_U
                                    || instr.OpCode.Code == Code.Conv_Ovf_U1 || instr.OpCode.Code == Code.Conv_Ovf_U2
                                    || instr.OpCode.Code == Code.Conv_Ovf_U4 || instr.OpCode.Code == Code.Conv_Ovf_U8
                                    || instr.OpCode.Code == Code.Conv_Ovf_I_Un || instr.OpCode.Code == Code.Conv_Ovf_I1_Un
                                    || instr.OpCode.Code == Code.Conv_Ovf_I2_Un || instr.OpCode.Code == Code.Conv_Ovf_I4_Un
                                    || instr.OpCode.Code == Code.Conv_Ovf_I8_Un || instr.OpCode.Code == Code.Conv_Ovf_U_Un
                                    || instr.OpCode.Code == Code.Conv_Ovf_U1_Un || instr.OpCode.Code == Code.Conv_Ovf_U2_Un
                                    || instr.OpCode.Code == Code.Conv_Ovf_U4_Un || instr.OpCode.Code == Code.Conv_Ovf_U8_Un)
                                {
                                    ConversionOps(ilProcessor, instr, translator, useDefaultOnly, targetPlats);
                                }
                                else if (instr.OpCode.Code == Code.Box)
                                {
                                    ValueStatement value = (ValueStatement)PopFromStack(); //expecting value statement
                                    Kind boxKind = Kind.GetCachedKind((TypeReference)instr.Operand);
                                    Box box = new Box(value.ToString(translator),
                                        Utility.GetAppropriateName(this.DeclaringKind, boxKind, useDefaultOnly, targetPlats), value, boxKind);
                                    PushToStack(box);
                                }
                                else if (instr.OpCode.Code == Code.Castclass)
                                {
                                    ValueStatement value = (ValueStatement)PopFromStack(); //expecting value statement
                                    Kind castKind = Kind.GetCachedKind((TypeReference)instr.Operand);
                                    Cast cast =
                                        new Cast(value.ToString(translator),
                                            Utility.GetAppropriateName(this.DeclaringKind, castKind, useDefaultOnly, targetPlats), value, castKind);
                                    PushToStack(cast);
                                }
                                else if (instr.OpCode.Code == Code.Isinst)
                                {
                                    ValueStatement value = (ValueStatement)PopFromStack(); //expecting value statement
                                    Kind castKind = Kind.GetCachedKind((TypeReference)instr.Operand);
                                    As _as =
                                        new As(value.ToString(translator),
                                            Utility.GetAppropriateName(this.DeclaringKind, castKind, useDefaultOnly, targetPlats), value, castKind);
                                    PushToStack(_as);
                                }
                                else if (instr.OpCode.Code == Code.Unbox)
                                {
                                    ValueStatement value = (ValueStatement)PopFromStack(); //expecting value statement
                                    Kind unboxKind = Kind.GetCachedKind((TypeReference)instr.Operand);
                                    UnBox unbox =
                                        new UnBox(value.ToString(translator),
                                            Utility.GetAppropriateName(this.DeclaringKind, unboxKind, useDefaultOnly, targetPlats), value, unboxKind);
                                    PushToStack(unbox);
                                }
                                else if (instr.OpCode.Code == Code.Unbox_Any) 
                                {
                                    ValueStatement value = (ValueStatement)PopFromStack(); //expecting value statement
                                    Kind unboxKind = Kind.GetCachedKind((TypeReference)instr.Operand);

                                    if (value.GetKind().UnderlyingType.IsValueType) //same as Unbox followed by Ldobj
                                    {
                                        //unbox
                                        UnBox unbox =
                                            new UnBox(value.ToString(translator),
                                                Utility.GetAppropriateName(this.DeclaringKind, unboxKind, useDefaultOnly, targetPlats), value, unboxKind);
                                        PushToStack(unbox);
                                        //followed by ldobj
                                        Load_PointerElement((TypeReference)instr.Operand, instr, translator, useDefaultOnly, targetPlats);
                                    }
                                    else //same as castclass
                                    {
                                        Cast cast =
                                        new Cast(value.ToString(translator),
                                            Utility.GetAppropriateName(this.DeclaringKind, unboxKind, useDefaultOnly, targetPlats), value, unboxKind);
                                        PushToStack(cast);
                                    }
                                }
                                #endregion
                                #region Ignored
                                #endregion
                                #region Not Yet Supported
                                else if (instr.OpCode.Code == Code.Calli)
                                {
                                    //when it becomes supported, put it under 'Method and Constructor Calls'
                                    throw new NotImplementedException("The 'calli' instruction is not yet supported!");
                                }
                                else if (instr.OpCode.Code == Code.Cpblk)
                                {
                                    throw new NotImplementedException("The 'cpblk' instruction is not yet supported!");
                                    //see "A Programmer's Introduction to C#.pdf", page 219/220 for hint on this instr
                                }
                                else if (instr.OpCode.Code == Code.Initblk)
                                {
                                    throw new NotImplementedException("The 'initblk' instruction is not yet supported!");
                                }
                                else if (instr.OpCode.Code == Code.Jmp)
                                {
                                    //when it becomes supported, put it under 'Method and Constructor Calls'
                                    throw new NotImplementedException("The 'jmp' instruction is not yet supported!");
                                }
                                #endregion
                                #region Miscellaneous
                                else
                                {
                                    if (instr.OpCode.Code == Code.Arglist)
                                    {
                                        //this pushes an instance of System.RuntimeArgumentHandle to the stack

                                        DynamicArglist arglist = new DynamicArglist();
                                        arglist.Label = GetLabel(instr);

                                        PushToStack(arglist);
                                    }
                                    else if (instr.OpCode.Code == Code.Break)
                                    {
                                        DebuggerBreak brk = new DebuggerBreak();
                                        brk.Label = GetLabel(instr);
                                        AddToTranslatedCodes(brk);
                                    }
                                    else if (instr.OpCode.Code == Code.Ckfinite)
                                    {
                                        ValueStatement value = (ValueStatement)PeekFromStack(); //expecting value statement
                                        CheckFloatingPointFinite checkFinite = new CheckFloatingPointFinite(value);
                                        checkFinite.Label = GetLabel(instr);
                                        AddToTranslatedCodes(checkFinite);
                                    }
                                    else if (instr.OpCode.Code == Code.Dup) //duplicates the topmost stack element
                                    {
                                        ValueStatement original = (ValueStatement)PeekFromStack(); //expecting value statement
                                        ValueStatement clone = original.Clone();
                                        clone.Label = GetLabel(instr);
                                        PushToStack(clone);
                                    }
                                    else if (instr.OpCode.Code == Code.Endfilter)
                                    {
                                        EndFilter endFilter = new EndFilter();
                                        endFilter.Label = GetLabel(instr);

                                        ////clears the stack
                                        //ClearStack();
                                        //actually, according to partition III (page 75), it removes only the topmost value
                                        PopFromStack();

                                        AddToTranslatedCodes(endFilter);
                                    }
                                    else if (instr.OpCode.Code == Code.Endfinally)
                                    {
                                        EndFinally endFinally = new EndFinally();
                                        endFinally.Label = GetLabel(instr);

                                        //clears the stack
                                        ClearStack();

                                        AddToTranslatedCodes(endFinally);
                                    }
                                    else if (instr.OpCode.Code == Code.Mkrefany)
                                    {
                                        ValueStatement pointer = (ValueStatement)PopFromStack();

                                        DynamicMakeRef makeref = new DynamicMakeRef(pointer);

                                        PushToStack(makeref);
                                    }
                                    else if (instr.OpCode.Code == Code.Nop) //do nothing
                                    {
                                        Nop nop = new Nop();
                                        nop.Label = GetLabel(instr);
                                        AddToTranslatedCodes(nop);
                                    }
                                    else if (instr.OpCode.Code == Code.Pop) //removes the topmost stack element
                                    {
                                        var value = PopFromStack();
                                        //if the value being popped is a method ref, it most likely means the method was called but its
                                        //returned value was never used.
                                        //so we add the method to translated codes
                                        if (value is MethodRef)
                                        {
                                            AddToTranslatedCodes(value);
                                        }
                                        //if the value being popped is an exception object reference
                                        //it was the exception being caught by, say, a catch block but was never used
                                        else if (value is DynamicObjRef)
                                        {
                                            Block currentBlock = PeekFromCurrentBlockStack();
                                            if (currentBlock != null && currentBlock is Catch)
                                            {
                                                (currentBlock as Catch).ShowExceptionObject = false;
                                            }
                                        }
                                    }
                                    else if (instr.OpCode.Code == Code.Refanytype)
                                    {
                                        ValueStatement typedRef = (ValueStatement)PopFromStack(); //expecting typed reference

                                        DynamicRefType reftype = new DynamicRefType(typedRef);

                                        PushToStack(reftype);
                                    }
                                    else if (instr.OpCode.Code == Code.Refanyval)
                                    {
                                        TypeReference type = (TypeReference)instr.Operand;
                                        Kind kind = Kind.GetCachedKind(type);
                                        string cachedKind = Utility.GetAppropriateName(this.DeclaringKind, kind, useDefaultOnly, targetPlats)
                                            + kind.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats);

                                        ValueStatement typedRef = (ValueStatement)PopFromStack(); //expecting typed reference

                                        DynamicRefValue refvalue = new DynamicRefValue(typedRef, kind, cachedKind);

                                        PushToStack(refvalue);
                                    }
                                    else if (instr.OpCode.Code == Code.Sizeof)
                                    {
                                        //if (instr.Operand is TypeReference) //expected, unnecessary
                                        {
                                            TypeReference ldArg = (TypeReference)instr.Operand;
                                            Kind kind = Kind.GetCachedKind(ldArg);
                                            string cachedDecKind = Utility.GetAppropriateName(this.DeclaringKind, kind, useDefaultOnly, targetPlats)
                                                + kind.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats);

                                            TypeToken typeToken = new TypeToken(cachedDecKind, cachedDecKind, kind);

                                            SizeOf sizeOf = new SizeOf(typeToken);
                                            sizeOf.Label = GetLabel(instr);

                                            PushToStack(sizeOf);
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion
                    #region Exception Handlers
                    else if (holder != null && holder.HoldsHandler)
                    {
                        ExceptionHandler handler = (ExceptionHandler)holder.ElementHeld;
                        if (handler.HandlerType == ExceptionHandlerType.Catch)
                        {
                            //create a catch block
                            //add it to translated codes
                            //make it the current block by pushing it to the current blocks stack
                            //push its exception object ref to the stack
                            TypeReference typeRef = handler.CatchType;
                            Kind exKind = Kind.GetCachedKind(typeRef);
                            DynamicObjRef exObj = null;
                            Catch _catch = null;

                            exObj = new DynamicObjRef("____ex" + DateTime.Now.Ticks.ToString(), exKind.GetLongName(useDefaultOnly, targetPlats), exKind);
                            _catch = new Catch(exKind.GetLongName(useDefaultOnly, targetPlats), exKind, exObj);

                            AddToTranslatedCodes(_catch);
                            PushToCurrentBlockStack(_catch);
                            PushToStack(exObj);
                        }
                        else if (handler.HandlerType == ExceptionHandlerType.Filter)
                        {
                            //create a filter handler block
                            //add it to translated codes
                            //make it the current block by pushing it to the current blocks stack
                            //push its exception object ref to the stack
                            TypeReference typeRef = new TypeDefinition("System", "Exception", TypeAttributes.Public); //handler.CatchType;
                            Kind exKind = Kind.GetCachedKind(typeRef);
                            DynamicObjRef exObj = null;
                            FilterHandler filterHandler = null;

                            exObj = new DynamicObjRef("____ex" + DateTime.Now.Ticks.ToString(), exKind.GetLongName(useDefaultOnly, targetPlats), exKind);
                            filterHandler = new FilterHandler();

                            AddToTranslatedCodes(filterHandler);
                            PushToCurrentBlockStack(filterHandler);
                            PushToStack(exObj);
                        }
                        else if (handler.HandlerType == ExceptionHandlerType.Finally)
                        {
                            //create a finally block
                            //add it to translated codes
                            //make it the current block by pushing it to the current blocks stack
                            Finally _finally = new Finally();
                            AddToTranslatedCodes(_finally);
                            PushToCurrentBlockStack(_finally);
                        }
                    }
                    #endregion
                    #region Others
                    else if (holder != null && holder.HoldsOthers)
                    {
                        OtherIlElements element = (OtherIlElements)holder.ElementHeld;
                        if (element == OtherIlElements.BeginTry)
                        {
                            //create a try block
                            //add it to translated codes
                            //make it the current block by pushing it to the current blocks stack
                            Try _try = new Try();
                            AddToTranslatedCodes(_try);
                            PushToCurrentBlockStack(_try);
                        }
                        else if (element == OtherIlElements.BeginFilter)
                        {
                            //create a filter block
                            //add it to translated codes
                            //make it the current block by pushing it to the current blocks stack
                            //push its exception object ref to the stack
                            TypeReference typeRef = new TypeDefinition("System", "Exception", TypeAttributes.Public); //handler.CatchType;
                            Kind exKind = Kind.GetCachedKind(typeRef);
                            DynamicObjRef exObj = null;
                            Filter filter = null;

                            exObj = new DynamicObjRef("____ex" + DateTime.Now.Ticks.ToString(), exKind.GetLongName(useDefaultOnly, targetPlats), exKind);
                            filter = new Filter();

                            AddToTranslatedCodes(filter);
                            PushToCurrentBlockStack(filter);
                            PushToStack(exObj);
                        }
                        else if (element == OtherIlElements.EndCatch)
                        {
                            Block currentBlock = PeekFromCurrentBlockStack();
                            if (currentBlock != null && currentBlock is Catch)
                            {
                                PopFromCurrentBlockStack();
                                //add comment
                                Comment comment = new Comment("end of catch block");
                                comment.IsBlockComment = false;
                                AddToTranslatedCodes(comment);
                            }
                        }
                        else if (element == OtherIlElements.EndFilter)
                        {
                            Block currentBlock = PeekFromCurrentBlockStack();
                            if (currentBlock != null && currentBlock is Filter)
                            {
                                PopFromCurrentBlockStack();
                                //add comment
                                Comment comment = new Comment("end of filter block");
                                comment.IsBlockComment = false;
                                AddToTranslatedCodes(comment);
                            }
                        }
                        else if (element == OtherIlElements.EndFilterHandler)
                        {
                            Block currentBlock = PeekFromCurrentBlockStack();
                            if (currentBlock != null && currentBlock is FilterHandler)
                            {
                                PopFromCurrentBlockStack();
                                //add comment
                                Comment comment = new Comment("end of filter handler block");
                                comment.IsBlockComment = false;
                                AddToTranslatedCodes(comment);
                            }
                        }
                        else if (element == OtherIlElements.EndFinally)
                        {
                            Block currentBlock = PeekFromCurrentBlockStack();
                            if (currentBlock != null && currentBlock is Finally)
                            {
                                PopFromCurrentBlockStack();
                                //add comment
                                Comment comment = new Comment("end of finally block");
                                comment.IsBlockComment = false;
                                AddToTranslatedCodes(comment);
                            }
                        }
                        else if (element == OtherIlElements.EndTry)
                        {
                            Block currentBlock = PeekFromCurrentBlockStack();
                            if (currentBlock != null && currentBlock is Try)
                            {
                                PopFromCurrentBlockStack();
                                //add comment
                                Comment comment = new Comment("end of try block");
                                comment.IsBlockComment = false;
                                AddToTranslatedCodes(comment);
                            }
                        }
                    }
                    #endregion

                    if (holder != null)
                    {
                        AddToProcessedIlElements(holder);
                    }
                }
                ClearCurrentIlElement();
                AddToTrace(new Report("iterating through IL instructions completed", ReportType.OperationCompleted));
                #region end code section
                Block codeSecEnd = PeekFromCurrentBlockStack();
                if (codeSecEnd != null && codeSecEnd is CodeSection)
                {
                    PopFromCurrentBlockStack();
                    ////add comment
                    //Comment codeSecEndComment = new Comment("end of code section");
                    //codeSecEndComment.IsBlockComment = false;
                    //AddToTranslatedCodes(codeSecEndComment);
                }
                #endregion

                return _translatedCodes;
            }
            catch (Exception ex)
            {
                AddToTrace(new Report(ex.Message, ReportType.Error));
                if (returnPartial)
                    return _translatedCodes;
                else throw ex;
            }
        }
        void LoadOps(ILProcessor ilProcessor, Instruction instr, CodomTranslator translator, 
            bool useDefaultOnly, params string[] targetPlats)
        {
            if (instr.Operand != null)
            {
                if (instr.OpCode.Code == Code.Ldc_I4 || instr.OpCode.Code == Code.Ldc_I4_S)
                {
                    int ldArg = Convert.ToInt32(instr.Operand.ToString());
                    IntConst intConst = new IntConst(ldArg);
                    Load_Constant(intConst, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.OpCode.Code == Code.Ldc_I8)
                {
                    long ldArg = Convert.ToInt64(instr.Operand.ToString());
                    LongConst intConst = new LongConst(ldArg);
                    Load_Constant(intConst, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.OpCode.Code == Code.Ldc_R4)
                {
                    float ldArg = Convert.ToSingle(instr.Operand.ToString());
                    FloatingPointConst intConst = new FloatingPointConst(ldArg);
                    Load_Constant(intConst, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.OpCode.Code == Code.Ldc_R8)
                {
                    double ldArg = Convert.ToDouble(instr.Operand.ToString());
                    FloatingPointConst intConst = new FloatingPointConst(ldArg);
                    Load_Constant(intConst, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.OpCode.Code == Code.Ldstr)
                {
                    string ldArg = Convert.ToString(instr.Operand);
                    StringConst strConst = new StringConst(ldArg);
                    Load_Constant(strConst, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.OpCode.Code == Code.Ldobj)
                {
                    Load_PointerElement((TypeReference)instr.Operand, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.OpCode.Code == Code.Ldelem_Any || instr.OpCode.Code == Code.Ldelema)
                {
                    Load_ArrayElement((TypeReference)instr.Operand, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.OpCode.Code == Code.Ldftn)
                {
                    //if (instr.Operand is MethodReference) //unnecessary
                    {
                        MethodReference ldArg = (MethodReference)instr.Operand;
                        Load_MethodFunction(ldArg, instr, translator, useDefaultOnly, targetPlats);
                    }
                }
                else if (instr.OpCode.Code == Code.Ldvirtftn)
                {
                    //if (instr.Operand is MethodReference) //unnecessary
                    {
                        MethodReference ldArg = (MethodReference)instr.Operand;
                        Load_MethodVirtualFunction(ldArg, instr, translator, useDefaultOnly, targetPlats);
                    }
                }
                else if (instr.OpCode.Code == Code.Ldtoken)
                {
                    if (instr.Operand is TypeReference)
                    {
                        TypeReference ldArg = (TypeReference)instr.Operand;
                        Load_TypeToken(ldArg, instr, translator, useDefaultOnly, targetPlats);
                    }
                    else if (instr.Operand is MethodReference)
                    {
                        MethodReference ldArg = (MethodReference)instr.Operand;
                        Load_MethodToken(ldArg, instr, translator, useDefaultOnly, targetPlats);
                    }
                    else if (instr.Operand is FieldReference)
                    {
                        FieldReference ldArg = (FieldReference)instr.Operand;
                        Load_FieldToken(ldArg, instr, translator, useDefaultOnly, targetPlats);
                    }
                }
                else if (instr.Operand is ParameterReference)
                {
                    ParameterReference ldArg = (ParameterReference)instr.Operand;
                    Load_ParameterReference(ldArg, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.Operand is VariableReference)
                {
                    VariableReference ldArg = (VariableReference)instr.Operand;
                    Load_VariableReference(ldArg, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.Operand is FieldReference)
                {
                    FieldReference ldArg = (FieldReference)instr.Operand;
                    Load_FieldReference(ldArg, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.Operand is MethodReference)
                {
                    MethodReference ldArg = (MethodReference)instr.Operand;
                    Load_MethodReference(ldArg, instr, translator, useDefaultOnly, targetPlats);
                }
            }
            else //if the operand of the load is null
            {
                if (instr.OpCode.Code == Code.Ldarg_0 || instr.OpCode.Code == Code.Ldarg_1
                    || instr.OpCode.Code == Code.Ldarg_2 || instr.OpCode.Code == Code.Ldarg_3)
                {
                    ParameterReference ldArg = null;

                    //loads first arg (for instance methods, this is the implicit "this")
                    if (instr.OpCode.Code == Code.Ldarg_0)
                    {
                        if (ilProcessor.Body.Method.IsStatic)
                        {
                            ldArg = ilProcessor.Body.Method.Parameters[0];
                            Load_ParameterReference(ldArg, instr, translator, useDefaultOnly, targetPlats);
                        }
                        else
                        {
                            ldArg = ilProcessor.Body.ThisParameter;
                            string cachedKind = (ldArg.ParameterType.FullName != null ? ldArg.ParameterType.FullName :
                                    ldArg.ParameterType.Name);
                            ThisParamRef thisParamRef =
                                new ThisParamRef(this.DeclaringKind.GetLongName(useDefaultOnly, targetPlats),
                                    this.DeclaringKind);
                            thisParamRef.Label = GetLabel(instr);
                            PushToStack(thisParamRef);
                        }
                    }
                    //loads second arg
                    else if (instr.OpCode.Code == Code.Ldarg_1)
                    {
                        if (ilProcessor.Body.Method.IsStatic)
                        {
                            ldArg = ilProcessor.Body.Method.Parameters[1];
                        }
                        else
                        {
                            ldArg = ilProcessor.Body.Method.Parameters[0];
                        }
                        Load_ParameterReference(ldArg, instr, translator, useDefaultOnly, targetPlats);
                    }
                    //loads third arg
                    else if (instr.OpCode.Code == Code.Ldarg_2)
                    {
                        if (ilProcessor.Body.Method.IsStatic)
                        {
                            ldArg = ilProcessor.Body.Method.Parameters[2];
                        }
                        else
                        {
                            ldArg = ilProcessor.Body.Method.Parameters[1];
                        }
                        Load_ParameterReference(ldArg, instr, translator, useDefaultOnly, targetPlats);
                    }
                    //loads 4th arg
                    else if (instr.OpCode.Code == Code.Ldarg_3)
                    {
                        if (ilProcessor.Body.Method.IsStatic)
                        {
                            ldArg = ilProcessor.Body.Method.Parameters[3];
                        }
                        else
                        {
                            ldArg = ilProcessor.Body.Method.Parameters[2];
                        }
                        Load_ParameterReference(ldArg, instr, translator, useDefaultOnly, targetPlats);
                    }
                }
                else if (instr.OpCode.Code == Code.Ldc_I4_0 || instr.OpCode.Code == Code.Ldc_I4_1 ||
                    instr.OpCode.Code == Code.Ldc_I4_2 || instr.OpCode.Code == Code.Ldc_I4_3
                    || instr.OpCode.Code == Code.Ldc_I4_4 || instr.OpCode.Code == Code.Ldc_I4_5
                    || instr.OpCode.Code == Code.Ldc_I4_6 || instr.OpCode.Code == Code.Ldc_I4_7
                    || instr.OpCode.Code == Code.Ldc_I4_8 || instr.OpCode.Code == Code.Ldc_I4_M1)
                {
                    IntConst intConst = null;

                    //Pushes the integer value of 0 onto the evaluation stack as an int32.
                    if (instr.OpCode.Code == Code.Ldc_I4_0)
                    {
                        intConst = new IntConst(0);
                    }
                    //Pushes the integer value of 1 onto the evaluation stack as an int32.
                    else if (instr.OpCode.Code == Code.Ldc_I4_1)
                    {
                        intConst = new IntConst(1);
                    }
                    //Pushes the integer value of 2 onto the evaluation stack as an int32.
                    else if (instr.OpCode.Code == Code.Ldc_I4_2)
                    {
                        intConst = new IntConst(2);
                    }
                    //Pushes the integer value of 3 onto the evaluation stack as an int32.
                    else if (instr.OpCode.Code == Code.Ldc_I4_3)
                    {
                        intConst = new IntConst(3);
                    }
                    //Pushes the integer value of 4 onto the evaluation stack as an int32.
                    else if (instr.OpCode.Code == Code.Ldc_I4_4)
                    {
                        intConst = new IntConst(4);
                    }
                    //Pushes the integer value of 5 onto the evaluation stack as an int32.
                    else if (instr.OpCode.Code == Code.Ldc_I4_5)
                    {
                        intConst = new IntConst(5);
                    }
                    //Pushes the integer value of 6 onto the evaluation stack as an int32.
                    else if (instr.OpCode.Code == Code.Ldc_I4_6)
                    {
                        intConst = new IntConst(6);
                    }
                    //Pushes the integer value of 7 onto the evaluation stack as an int32.
                    else if (instr.OpCode.Code == Code.Ldc_I4_7)
                    {
                        intConst = new IntConst(7);
                    }
                    //Pushes the integer value of 8 onto the evaluation stack as an int32.
                    else if (instr.OpCode.Code == Code.Ldc_I4_8)
                    {
                        intConst = new IntConst(8);
                    }
                    //Pushes the integer value of -1 onto the evaluation stack as an int32.
                    else if (instr.OpCode.Code == Code.Ldc_I4_M1)
                    {
                        intConst = new IntConst(-1);
                    }

                    Load_Constant(intConst, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.OpCode.Code == Code.Ldloc_0 || instr.OpCode.Code == Code.Ldloc_1
                    || instr.OpCode.Code == Code.Ldloc_2 || instr.OpCode.Code == Code.Ldloc_3)
                {
                    VariableReference ldArg = null;

                    //loads first local var
                    if (instr.OpCode.Code == Code.Ldloc_0)
                    {
                        ldArg = ilProcessor.Body.Variables[0];
                    }
                    //loads second local var
                    else if (instr.OpCode.Code == Code.Ldloc_1)
                    {
                        ldArg = ilProcessor.Body.Variables[1];
                    }
                    //loads third loc var
                    else if (instr.OpCode.Code == Code.Ldloc_2)
                    {
                        ldArg = ilProcessor.Body.Variables[2];
                    }
                    //loads 4th loc var
                    else if (instr.OpCode.Code == Code.Ldloc_3)
                    {
                        ldArg = ilProcessor.Body.Variables[3];
                    }

                    Load_VariableReference(ldArg, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.OpCode.Code == Code.Ldelem_I || instr.OpCode.Code == Code.Ldelem_I1
                    || instr.OpCode.Code == Code.Ldelem_I2 || instr.OpCode.Code == Code.Ldelem_I4
                    || instr.OpCode.Code == Code.Ldelem_I8 || instr.OpCode.Code == Code.Ldelem_R4
                    || instr.OpCode.Code == Code.Ldelem_R8 || instr.OpCode.Code == Code.Ldelem_U1
                    || instr.OpCode.Code == Code.Ldelem_U2 || instr.OpCode.Code == Code.Ldelem_U4
                    || instr.OpCode.Code == Code.Ldelem_Ref)
                {
                    Load_ArrayElement(null, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.OpCode.Code == Code.Ldind_I || instr.OpCode.Code == Code.Ldind_I1
                || instr.OpCode.Code == Code.Ldind_I2 || instr.OpCode.Code == Code.Ldind_I4
                || instr.OpCode.Code == Code.Ldind_I8 || instr.OpCode.Code == Code.Ldind_R4
                || instr.OpCode.Code == Code.Ldind_R8 || instr.OpCode.Code == Code.Ldind_U1
                || instr.OpCode.Code == Code.Ldind_U2 || instr.OpCode.Code == Code.Ldind_U4
                || instr.OpCode.Code == Code.Ldind_Ref)
                {
                    Load_PointerElement(null, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.OpCode.Code == Code.Ldlen)
                {
                    ValueStatement array = (ValueStatement)PopFromStack(); //expecting value statement
                    LengthOf length = new LengthOf(array);
                    PushToStack(length);
                }
                else if (instr.OpCode.Code == Code.Ldnull)
                {
                    Null nullRef = new Null();
                    Load_Constant(nullRef, instr, translator, useDefaultOnly, targetPlats);
                }
            }
        }
        void StoreOps(ILProcessor ilProcessor, Instruction instr, CodomTranslator translator,
            bool useDefaultOnly, params string[] targetPlats)
        {
            if (instr.Operand != null)
            {
                if (instr.OpCode.Code == Code.Stobj)
                {
                    Store_PointerElement((TypeReference)instr.Operand, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.OpCode.Code == Code.Stelem_Any)
                {
                    Store_ArrayElement((TypeReference)instr.Operand, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.Operand is ParameterReference)
                {
                    ParameterReference strArg = (ParameterReference)instr.Operand;
                    Store_ParameterReference(strArg, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.Operand is VariableReference)
                {
                    VariableReference strArg = (VariableReference)instr.Operand;
                    Store_VariableReference(strArg, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.Operand is FieldReference)
                {
                    FieldReference strArg = (FieldReference)instr.Operand;
                    Store_FieldReference(strArg, instr, translator, useDefaultOnly, targetPlats);
                }
            }
            else //if the operand of the store is null
            {
                if (instr.OpCode.Code == Code.Stloc_0 || instr.OpCode.Code == Code.Stloc_1
                       || instr.OpCode.Code == Code.Stloc_2 || instr.OpCode.Code == Code.Stloc_3)
                {
                    VariableReference strArg = null;

                    //loads first local var
                    if (instr.OpCode.Code == Code.Stloc_0)
                    {
                        strArg = ilProcessor.Body.Variables[0];
                    }
                    //loads second local var
                    else if (instr.OpCode.Code == Code.Stloc_1)
                    {
                        strArg = ilProcessor.Body.Variables[1];
                    }
                    //loads third loc var
                    else if (instr.OpCode.Code == Code.Stloc_2)
                    {
                        strArg = ilProcessor.Body.Variables[2];
                    }
                    //loads 4th loc var
                    else if (instr.OpCode.Code == Code.Stloc_3)
                    {
                        strArg = ilProcessor.Body.Variables[3];
                    }

                    Store_VariableReference(strArg, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.OpCode.Code == Code.Stelem_I
                    || instr.OpCode.Code == Code.Stelem_I1 || instr.OpCode.Code == Code.Stelem_I2
                    || instr.OpCode.Code == Code.Stelem_I4 || instr.OpCode.Code == Code.Stelem_I8
                    || instr.OpCode.Code == Code.Stelem_R4 || instr.OpCode.Code == Code.Stelem_R8
                    || instr.OpCode.Code == Code.Stelem_Ref)
                {
                    Store_ArrayElement(null, instr, translator, useDefaultOnly, targetPlats);
                }
                else if (instr.OpCode.Code == Code.Stind_I || instr.OpCode.Code == Code.Stind_I1
                    || instr.OpCode.Code == Code.Stind_I2 || instr.OpCode.Code == Code.Stind_I4
                    || instr.OpCode.Code == Code.Stind_I8 || instr.OpCode.Code == Code.Stind_R4
                    || instr.OpCode.Code == Code.Stind_R8 || instr.OpCode.Code == Code.Stind_Ref)
                {
                    Store_PointerElement(null, instr, translator, useDefaultOnly, targetPlats);
                }
            }
        }
        void ArithLogicBiOps(ILProcessor ilProcessor, Instruction instr, CodomTranslator translator,
            bool useDefaultOnly, params string[] targetPlats)
        {
            ValueStatement right = (ValueStatement)PopFromStack(); //expecting value statement
            ValueStatement left = (ValueStatement)PopFromStack(); //expecting value statement
            ValueStatement newRight = GetProperRightValue(left, right, translator, false, useDefaultOnly, targetPlats);

            if (instr.OpCode.Code == Code.Add)
            {
                Add add = new Add(left, newRight);
                PushToStack(add);
            }
            else if (instr.OpCode.Code == Code.Add_Ovf || instr.OpCode.Code == Code.Add_Ovf_Un)
            {
                Add add = new Add(left, newRight);
                Checked _checked = new Checked(add);
                PushToStack(_checked);
            }
            else if (instr.OpCode.Code == Code.And)
            {
                BitAnd and = new BitAnd(left, newRight);
                PushToStack(and);
            }
            else if (instr.OpCode.Code == Code.Ceq)
            {
                Compare compare = new Compare(left, newRight, Compare.Comparisons.Equal);
                PushToStack(compare);
            }
            else if (instr.OpCode.Code == Code.Cgt || instr.OpCode.Code == Code.Cgt_Un)
            {
                Compare compare = new Compare(left, newRight, Compare.Comparisons.GreaterThan);
                PushToStack(compare);
            }
            else if (instr.OpCode.Code == Code.Clt || instr.OpCode.Code == Code.Clt_Un)
            {
                Compare compare = new Compare(left, newRight, Compare.Comparisons.LessThan);
                PushToStack(compare);
            }
            else if (instr.OpCode.Code == Code.Div)
            {
                Div div = new Div(left, newRight);
                PushToStack(div);
            }
            else if (instr.OpCode.Code == Code.Div_Un)
            {
                UDiv div = new UDiv(left, newRight);
                PushToStack(div);
            }
            else if (instr.OpCode.Code == Code.Mul)
            {
                Mul mul = new Mul(left, newRight);
                PushToStack(mul);
            }
            else if (instr.OpCode.Code == Code.Mul_Ovf || instr.OpCode.Code == Code.Mul_Ovf_Un)
            {
                Mul mul = new Mul(left, newRight);
                Checked _checked = new Checked(mul);
                PushToStack(_checked);
            }
            else if (instr.OpCode.Code == Code.Or)
            {
                BitOr or = new BitOr(left, newRight);
                PushToStack(or);
            }
            else if (instr.OpCode.Code == Code.Rem)
            {
                Rem rem = new Rem(left, newRight);
                PushToStack(rem);
            }
            else if (instr.OpCode.Code == Code.Rem_Un)
            {
                URem rem = new URem(left, newRight);
                PushToStack(rem);
            }
            else if (instr.OpCode.Code == Code.Shl)
            {
                ShiftLeft leftShift = new ShiftLeft(left, newRight);
                PushToStack(leftShift);
            }
            else if (instr.OpCode.Code == Code.Shr)
            {
                ShiftRight rightShift = new ShiftRight(left, newRight, false);
                PushToStack(rightShift);
            }
            else if (instr.OpCode.Code == Code.Shr_Un)
            {
                ShiftRight rightShift = new ShiftRight(left, newRight, true);
                PushToStack(rightShift);
            }
            else if (instr.OpCode.Code == Code.Sub)
            {
                Sub sub = new Sub(left, newRight);
                PushToStack(sub);
            }
            else if (instr.OpCode.Code == Code.Sub_Ovf || instr.OpCode.Code == Code.Sub_Ovf_Un)
            {
                Sub sub = new Sub(left, newRight);
                Checked _checked = new Checked(sub);
                PushToStack(_checked);
            }
            else if (instr.OpCode.Code == Code.Xor)
            {
                BitXor xor = new BitXor(left, newRight);
                PushToStack(xor);
            }
        }
        void ArithLogicUnOps(ILProcessor ilProcessor, Instruction instr, CodomTranslator translator,
            bool useDefaultOnly, params string[] targetPlats)
        {
            ValueStatement operand = (ValueStatement)PopFromStack(); //expecting value statement

            if (instr.OpCode.Code == Code.Localloc)
            {
                LocAlloc locAlloc = new LocAlloc(operand);
                PushToStack(locAlloc);
            }
            else if (instr.OpCode.Code == Code.Neg)
            {
                Neg neg = new Neg(operand);
                PushToStack(neg);
            }
            else if (instr.OpCode.Code == Code.Not)
            {
                BitNot not = new BitNot(operand);
                PushToStack(not);
            }
        }
        void ConditionalBranchingOps(ILProcessor ilProcessor, Instruction instr, CodomTranslator translator,
            bool useDefaultOnly, params string[] targetPlats)
        {
            If _if = null;
            Compare compare = null;
            Goto _goto = null;

            //if (instr.Operand is Instruction) //expected, unnecessary
            {
                var target = (Instruction)instr.Operand;
                var targetLabel = GetLabel(target);
                _goto = new Goto(targetLabel);
                _goto.Label = GetLabel(instr);
            }

            //these ones get their two values from the stack
            if (instr.OpCode.Code == Code.Beq || instr.OpCode.Code == Code.Beq_S || instr.OpCode.Code == Code.Bge
                || instr.OpCode.Code == Code.Bge_S || instr.OpCode.Code == Code.Bge_Un || instr.OpCode.Code == Code.Bge_Un_S
                || instr.OpCode.Code == Code.Bgt || instr.OpCode.Code == Code.Bgt_S || instr.OpCode.Code == Code.Bgt_Un
                || instr.OpCode.Code == Code.Bgt_Un_S || instr.OpCode.Code == Code.Ble || instr.OpCode.Code == Code.Ble_S
                || instr.OpCode.Code == Code.Ble_Un || instr.OpCode.Code == Code.Ble_Un_S || instr.OpCode.Code == Code.Blt
                || instr.OpCode.Code == Code.Blt_S || instr.OpCode.Code == Code.Blt_Un || instr.OpCode.Code == Code.Blt_Un_S
                || instr.OpCode.Code == Code.Bne_Un || instr.OpCode.Code == Code.Bne_Un_S)
            {
                ValueStatement right = (ValueStatement)PopFromStack(); //expecting value statement
                ValueStatement left = (ValueStatement)PopFromStack(); //expecting value statement
                ValueStatement newRight = GetProperRightValue(left, right, translator, false, useDefaultOnly, targetPlats);

                if (instr.OpCode.Code == Code.Beq || instr.OpCode.Code == Code.Beq_S)
                {
                    compare = new Compare(left, newRight, Compare.Comparisons.Equal);
                }
                else if (instr.OpCode.Code == Code.Bge || instr.OpCode.Code == Code.Bge_S
                    || instr.OpCode.Code == Code.Bge_Un || instr.OpCode.Code == Code.Bge_Un_S)
                {
                    compare = new Compare(left, newRight, Compare.Comparisons.GreaterThanOrEqual);
                }
                else if (instr.OpCode.Code == Code.Bgt || instr.OpCode.Code == Code.Bgt_S
                    || instr.OpCode.Code == Code.Bgt_Un || instr.OpCode.Code == Code.Bgt_Un_S)
                {
                    compare = new Compare(left, newRight, Compare.Comparisons.GreaterThan);
                }
                else if (instr.OpCode.Code == Code.Ble || instr.OpCode.Code == Code.Ble_S
                   || instr.OpCode.Code == Code.Ble_Un || instr.OpCode.Code == Code.Ble_Un_S)
                {
                    compare = new Compare(left, newRight, Compare.Comparisons.LessThanOrEqual);
                }
                else if (instr.OpCode.Code == Code.Blt || instr.OpCode.Code == Code.Blt_S
                   || instr.OpCode.Code == Code.Blt_Un || instr.OpCode.Code == Code.Blt_Un_S)
                {
                    compare = new Compare(left, newRight, Compare.Comparisons.LessThan);
                }
                else if (instr.OpCode.Code == Code.Bne_Un || instr.OpCode.Code == Code.Bne_Un_S)
                {
                    compare = new Compare(left, newRight, Compare.Comparisons.NotEqual);
                }
            }
            //these ones get only their left value from the stack
            else if (instr.OpCode.Code == Code.Brfalse || instr.OpCode.Code == Code.Brfalse_S
                    || instr.OpCode.Code == Code.Brtrue || instr.OpCode.Code == Code.Brtrue_S)
            {
                ValueStatement left = (ValueStatement)PopFromStack(); //expecting value statement
                ValueStatement newRight = GetProperRightValue(left, new IntConst(0), translator, false, useDefaultOnly, targetPlats);

                //braches if left is false, 0, null*******************
                if (instr.OpCode.Code == Code.Brfalse || instr.OpCode.Code == Code.Brfalse_S)
                {
                    compare = new Compare(left, newRight, Compare.Comparisons.Equal);
                }
                //braches if left is NOT false, NOT 0, NOT null*******************
                else if (instr.OpCode.Code == Code.Brtrue || instr.OpCode.Code == Code.Brtrue_S)
                {
                    compare = new Compare(left, newRight, Compare.Comparisons.NotEqual);
                }
            }

            _if = new If(compare);
            if (_goto != null) //expected
                _if.Add(_goto);
            AddToTranslatedCodes(_if);
        }
        void SwitchOps(ILProcessor ilProcessor, Instruction instr, CodomTranslator translator,
            bool useDefaultOnly, params string[] targetPlats)
        {
            ValueStatement value = (ValueStatement)PopFromStack();
            Instruction[] targetInstructions = (Instruction[])instr.Operand; //array expected
            If switchIf = null; //the current case block in the switch
            for (int index = 0; index < targetInstructions.Length; index++)
            {
                Compare comp = new Compare(value, new UIntConst((uint)index), Compare.Comparisons.Equal);
                If _if = new If(comp);
                Goto _goto = new Goto(GetLabel(targetInstructions[index]));
                _if.Add(_goto);

                if (index == 0) //which means this is the first case in the switch
                {
                    AddToTranslatedCodes(_if);
                    switchIf = _if;
                }
                else
                {
                    if (!switchIf.HasElse)
                    {
                        switchIf.Else = new Else();
                    }

                    _if.Label = ""; //only the first if shud adopt the label
                    switchIf.Else.Add(_if);
                    switchIf = _if;
                }
            }
        }
        void UnConditionalBranchingOps(ILProcessor ilProcessor, Instruction instr, CodomTranslator translator,
            bool useDefaultOnly, params string[] targetPlats)
        {
            if (instr.OpCode.Code == Code.Br || instr.OpCode.Code == Code.Br_S)
            {
                var target = (Instruction)instr.Operand; //instruction expected
                var targetLabel = GetLabel(target);
                Goto _goto = new Goto(targetLabel);
                _goto.Label = GetLabel(instr);

                AddToTranslatedCodes(_goto);
            }
            else if (instr.OpCode.Code == Code.Leave || instr.OpCode.Code == Code.Leave_S)
            {
                var target = (Instruction)instr.Operand; //instruction expected
                var targetLabel = GetLabel(target);
                Leave leave = new Leave(targetLabel);
                leave.Label = GetLabel(instr);

                //the big difference btw br and leave is that leave is expected to clear the stack
                ClearStack();

                AddToTranslatedCodes(leave);
            }
            else if (instr.OpCode.Code == Code.Ret)
            {
                Return ret = null;
                //if method is void or 
                //(although I know this shud b "and" not "or", 
                //I used "or" to allow for scenarios where the return type of the method is user-defined) 
                //there is nothing on the stack, return nothing
                dynamic returnType = this.GetReturnKind(useDefaultOnly, targetPlats);
                if (returnType != null)
                {
                    try
                    {
                        if (returnType is string)
                        {
                            if ((returnType as string).Trim().ToLower().Contains("void"))
                            {
                                ret = new Return(null);
                            }
                        }
                        else if (returnType is Kind)
                        {
                            if ((returnType as Kind).GetName(useDefaultOnly, targetPlats).Trim().ToLower().Contains("void"))
                            {
                                ret = new Return(null);
                            }
                        }
                    }
                    catch
                    {
                        ret = new Return(null);
                    }
                }
                else //assume it to be void
                {
                    ret = new Return(null);
                }

                if (ret == null) //that means none of the conditions above were met
                {
                    if (GetStackCount() == 0) //nothing on the stack
                    {
                        ret = new Return(null);
                    }
                    else
                    {
                        ValueStatement retValue = (ValueStatement)PopFromStack(); //expecting value statement
                        ret = new Return(retValue);
                    }
                }

                //return clears the stack
                ClearStack();
                if (ret.Value == null)
                    ret.Label = GetLabel(instr);
                AddToTranslatedCodes(ret);
            }
            else if (instr.OpCode.Code == Code.Rethrow)
            {
                Throw _throw = new Throw(null);
                _throw.Label = GetLabel(instr);
                AddToTranslatedCodes(_throw);
            }
            else if (instr.OpCode.Code == Code.Throw)
            {
                ValueStatement throwValue = (ValueStatement)PopFromStack(); //expecting value statement
                Throw _throw = new Throw(throwValue);

                //this kind of throw clears the stack
                ClearStack();
                if (_throw.Value == null)
                    _throw.Label = GetLabel(instr);
                AddToTranslatedCodes(_throw);
            }
        }
        void MethodCallOps(ILProcessor ilProcessor, Instruction instr, CodomTranslator translator,
            bool useDefaultOnly, params string[] targetPlats)
        {
            if (instr.OpCode.Code == Code.Call || instr.OpCode.Code == Code.Callvirt || instr.OpCode.Code == Code.Newobj)
            {
                //if (instr.Operand is MethodReference) //expected, unnecessary
                {
                    Load_MethodReference((MethodReference)instr.Operand, instr, translator, useDefaultOnly, targetPlats);
                }
            }
            //else if (instr.OpCode.Code == Code.Calli) //the pointer to the method is on the stack
            //{
            //}
        }
        void CopyObjectOps(ILProcessor ilProcessor, Instruction instr, CodomTranslator translator,
            bool useDefaultOnly, params string[] targetPlats)
        {
            //if (instr.Operand is TypeReference) //expected, unnecessary
            {
                Kind kind = Kind.GetCachedKind((TypeReference)instr.Operand);
                if (kind.UnderlyingType.IsValueType)
                {//**************************not sure I got this right
                    Load_PointerElement((TypeReference)instr.Operand, instr, translator, useDefaultOnly, targetPlats);
                    Store_PointerElement((TypeReference)instr.Operand, instr, translator, useDefaultOnly, targetPlats);
                }
                else
                {
                    //*********this is equivalent to a ldind.ref followed by a stind.ref
                    Load_PointerElement((TypeReference)instr.Operand, instr, translator, useDefaultOnly, targetPlats);
                    Store_PointerElement((TypeReference)instr.Operand, instr, translator, useDefaultOnly, targetPlats);
                }
            }
        }
        void InitObjectOps(ILProcessor ilProcessor, Instruction instr, CodomTranslator translator,
            bool useDefaultOnly, params string[] targetPlats) 
        {
            //set value types to their default constructors
            //set reference types to null
            //Unlike newobj, the initobj instruction does not call any constructor method.
            //hence, this is the same as using the C# operator "default"

            //if (instr.Operand is TypeReference) //expected, unnecessary
            {
                Kind kind = Kind.GetCachedKind((TypeReference)instr.Operand);
                string cachedKind =
                        Utility.GetAppropriateName(this.DeclaringKind, kind, useDefaultOnly, targetPlats)
                        + kind.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats);
                Default _default = new Default(cachedKind, kind);
                _default.Label = GetLabel(instr);
                PushToStack(_default);
                Store_PointerElement((TypeReference)instr.Operand, instr, translator, useDefaultOnly, targetPlats);
            }
        }
        void ArrayOps(ILProcessor ilProcessor, Instruction instr, CodomTranslator translator,
            bool useDefaultOnly, params string[] targetPlats) 
        {
            //if (instr.Operand is TypeReference) //expected, unnecessary
            {
                TypeReference elementTypeRef = (TypeReference)instr.Operand;
                ValueStatement numOfElements = (ValueStatement)PopFromStack(); //expecting value statement
                Kind elementKind = Kind.GetCachedKind(elementTypeRef);
                ArrayObjInit arrayObjInit = null;

                //TypeReference arrayType = elementKind.UnderlyingType.MakeArrayType();
                //Kind arrayKind = Kind.GetCachedKind(arrayType);
                arrayObjInit = new ArrayObjInit(elementKind.GetLongName(useDefaultOnly, targetPlats), numOfElements, elementKind);

                PushToStack(arrayObjInit);
            }
        }
        void ConversionOps(ILProcessor ilProcessor, Instruction instr, CodomTranslator translator,
            bool useDefaultOnly, params string[] targetPlats)
        {
            ValueStatement value = (ValueStatement)PopFromStack(); //expecting value statement
            DataConversion convert = null;

            if (instr.OpCode.Code == Code.Conv_I || instr.OpCode.Code == Code.Conv_Ovf_I
                || instr.OpCode.Code == Code.Conv_Ovf_I_Un)
            {
                convert = new DataConversion(value, DataConversion.Conversions.ToNativeInt);
            }
            else if (instr.OpCode.Code == Code.Conv_I1 || instr.OpCode.Code == Code.Conv_Ovf_I1
                || instr.OpCode.Code == Code.Conv_Ovf_I1_Un)
            {
                convert = new DataConversion(value, DataConversion.Conversions.ToInt8);
            }
            else if (instr.OpCode.Code == Code.Conv_I2 || instr.OpCode.Code == Code.Conv_Ovf_I2
                || instr.OpCode.Code == Code.Conv_Ovf_I2_Un)
            {
                convert = new DataConversion(value, DataConversion.Conversions.ToInt16);
            }
            else if (instr.OpCode.Code == Code.Conv_I4 || instr.OpCode.Code == Code.Conv_Ovf_I4
                || instr.OpCode.Code == Code.Conv_Ovf_I4_Un)
            {
                convert = new DataConversion(value, DataConversion.Conversions.ToInt32);
            }
            else if (instr.OpCode.Code == Code.Conv_I8 || instr.OpCode.Code == Code.Conv_Ovf_I8
                || instr.OpCode.Code == Code.Conv_Ovf_I8_Un)
            {
                convert = new DataConversion(value, DataConversion.Conversions.ToInt64);
            }
            else if (instr.OpCode.Code == Code.Conv_R4)
            {
                convert = new DataConversion(value, DataConversion.Conversions.ToFloat32);
            }
            else if (instr.OpCode.Code == Code.Conv_R8)
            {
                convert = new DataConversion(value, DataConversion.Conversions.ToFloat64);
            }
            else if (instr.OpCode.Code == Code.Conv_U || instr.OpCode.Code == Code.Conv_Ovf_U
                || instr.OpCode.Code == Code.Conv_Ovf_U_Un)
            {
                convert = new DataConversion(value, DataConversion.Conversions.ToUNativeInt);
            }
            else if (instr.OpCode.Code == Code.Conv_U1 || instr.OpCode.Code == Code.Conv_Ovf_U1
                || instr.OpCode.Code == Code.Conv_Ovf_U1_Un)
            {
                convert = new DataConversion(value, DataConversion.Conversions.ToUInt8);
            }
            else if (instr.OpCode.Code == Code.Conv_U2 || instr.OpCode.Code == Code.Conv_Ovf_U2
                || instr.OpCode.Code == Code.Conv_Ovf_U2_Un)
            {
                convert = new DataConversion(value, DataConversion.Conversions.ToUInt16);
            }
            else if (instr.OpCode.Code == Code.Conv_U4 || instr.OpCode.Code == Code.Conv_Ovf_U4
                || instr.OpCode.Code == Code.Conv_Ovf_U4_Un)
            {
                convert = new DataConversion(value, DataConversion.Conversions.ToUInt32);
            }
            else if (instr.OpCode.Code == Code.Conv_U8 || instr.OpCode.Code == Code.Conv_Ovf_U8
                || instr.OpCode.Code == Code.Conv_Ovf_U8_Un)
            {
                convert = new DataConversion(value, DataConversion.Conversions.ToUInt64);
            }
            else //if (instr.OpCode.Code == Code.Conv_R_Un)
            {
                convert = new DataConversion(value, DataConversion.Conversions.ToFloatingPoint);
            }

            if (instr.OpCode.Code == Code.Conv_Ovf_I || instr.OpCode.Code == Code.Conv_Ovf_U
                || instr.OpCode.Code == Code.Conv_Ovf_I1 || instr.OpCode.Code == Code.Conv_Ovf_I2
                || instr.OpCode.Code == Code.Conv_Ovf_I4 || instr.OpCode.Code == Code.Conv_Ovf_I8
                || instr.OpCode.Code == Code.Conv_Ovf_U1 || instr.OpCode.Code == Code.Conv_Ovf_U2
                || instr.OpCode.Code == Code.Conv_Ovf_U4 || instr.OpCode.Code == Code.Conv_Ovf_U8
                || instr.OpCode.Code == Code.Conv_Ovf_I_Un || instr.OpCode.Code == Code.Conv_Ovf_U_Un
                || instr.OpCode.Code == Code.Conv_Ovf_I1_Un || instr.OpCode.Code == Code.Conv_Ovf_U1_Un
                || instr.OpCode.Code == Code.Conv_Ovf_I2_Un || instr.OpCode.Code == Code.Conv_Ovf_U2_Un
                || instr.OpCode.Code == Code.Conv_Ovf_I4_Un || instr.OpCode.Code == Code.Conv_Ovf_U4_Un
                || instr.OpCode.Code == Code.Conv_Ovf_I8_Un || instr.OpCode.Code == Code.Conv_Ovf_U8_Un)
                PushToStack(new Checked(convert));
            else
                PushToStack(convert);
        }

        #region Objects and Methods that help you monitor what the method is doing
        void ClearCurrentBlockStack()
        {
            _currentBlockStack.Clear();
        }
        int GetCurrentBlockStackCount()
        {
            return _currentBlockStack.Count();
        }
        Block PeekFromCurrentBlockStack()
        {
            try
            {
                if (_currentBlockStack.Count == 0)
                    return null;
                return _currentBlockStack.Peek();
            }
            catch
            {
                return null;
            }
        }
        Block PopFromCurrentBlockStack()
        {
            try
            {
                if (_currentBlockStack.Count == 0)
                    return null;
                return _currentBlockStack.Pop();
            }
            catch
            {
                return null;
            }
        }
        void PushToCurrentBlockStack(Block block)
        {
            try
            {
                _currentBlockStack.Push(block);
            }
            catch
            {
                //do nothing
            }
        }

        void ClearCurrentIlElement()
        {
            _currIlElement = null;
        }
        void SetAsCurrentIlElement(IlElementHolder ilElement)
        {
            _currIlElement = ilElement;
        }
        void AddToProcessedIlElements(IlElementHolder ilElement)
        {
            _ilElementsProc.Add(ilElement);
        }
        void ClearProcessedIlElements()
        {
            _ilElementsProc.Clear();
        }

        void AddToLocals(Variable variable, bool useDefaultOnly, params string[] targetPlats)
        {
            string varName = variable.GetName(useDefaultOnly, targetPlats);
            AddToTrace(new Report("declaring variable '" + varName + "'...", ReportType.OperationInProgress));
            Kind varKind = variable.VariableKind;

            string varKindStr = Utility.GetAppropriateName(this.DeclaringKind, varKind, useDefaultOnly, targetPlats) +
                varKind.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats); //this works, no need for Utility.GetTypeGenericArg...

            VarDec varDec = new VarDec(varName, varKindStr, variable);

            varDec.IsInline = false;
            AddToTranslatedCodes(varDec);
            _methodLocals.Add(variable);
            AddToTrace(new Report("declaring variable '" + varName + "' succeeded", ReportType.OperationCompleted));
        }
        void ClearLocals()
        {
            _methodLocals.Clear();
        }
        Variable[] GetLocals()
        {
            return _methodLocals.ToArray();
        }
        string GetVariableNameFromLocals(VariableReference variable, bool useDefaultOnly, params string[] targetPlats)
        {
            return _methodLocals[variable.Index].GetName(useDefaultOnly, targetPlats);
        }
        Variable GetVariableFromLocals(VariableReference variable)
        {
            return _methodLocals[variable.Index];
        }

        void ClearStack()
        {
            _methodStack.Clear();
        }
        int GetStackCount()
        {
            return _methodStack.Count();
        }
        Statement PeekFromStack()
        {
            Statement value;
            AddToTrace(new Report("peeking value from stack...", ReportType.OperationInProgress));
            try
            {
                value = _methodStack.Peek();
            }
            catch (Exception ex)
            {
                throw new StackPopException(ex.Message, ex);
            }
            AddToTrace(new Report("peeking '" + value.ToString() + "' from stack succeeded", ReportType.OperationCompleted));
            return value;
        }
        Statement PopFromStack()
        {
            Statement value;
            AddToTrace(new Report("popping value from stack...", ReportType.OperationInProgress));
            try
            {
                value = _methodStack.Pop();
            }
            catch (Exception ex)
            {
                throw new StackPopException(ex.Message, ex);
            }
            AddToTrace(new Report("popping '" + value.ToString() + "' from stack succeeded", ReportType.OperationCompleted));
            return value;
        }
        void PushToStack(Statement statement)
        {
            AddToTrace(new Report("pushing '" + statement.ToString() + "' to stack...", ReportType.OperationInProgress));
            try
            {
                statement.IsInline = true;
                _methodStack.Push(statement);
            }
            catch (Exception ex)
            {
                throw new StackPushException(ex.Message, ex);
            }
            AddToTrace(new Report("pushing '" + statement.ToString() + "' to stack succeeded", ReportType.OperationCompleted));
        }

        void AddToTranslatedCodes(Codom code)
        {
            code.IsInline = false;
            Block currentBlock = PeekFromCurrentBlockStack();
            if (currentBlock != null)
            {
                currentBlock.Add(code);
            }
            else
                _translatedCodes.Add(code);
            //AddToTrace(new Report("output:\n" + code.ToString(), ReportType.Information));
        }
        void ClearTranslatedCodes()
        {
            _translatedCodes.Clear();
        }

        void AddToTrace(Report report)
        {
            _reportTrace.Add(report);
        }
        void ClearTrace()
        {
            _reportTrace.Clear();
        }

        /// <summary>
        /// Gets the current IL element.
        /// On the occassion of an error during the processing of IL elements,
        /// the current IL element is usually the element that led to the error.
        /// Note that this could be null if an exception occures before the current
        /// element is set, or if all IL elements have been successfully processed.
        /// </summary>
        public IlElementHolder CurrentIlElement
        {
            get { return _currIlElement; }
        }
        /// <summary>
        /// Gets an array of the IL elements processed thus far
        /// </summary>
        public IlElementHolder[] ProcessedIlElements
        {
            get { return _ilElementsProc.ToArray(); }
        }
        /// <summary>
        /// Gets an array of the translated codes
        /// </summary>
        public Codom[] TranslatedCodes
        {
            get { return _translatedCodes.ToArray(); }
        }
        /// <summary>
        /// Gets the locals
        /// </summary>
        public Variable[] Locals
        {
            get { return _methodLocals.ToArray(); }
        }
        /// <summary>
        /// Gets the method stack
        /// </summary>
        public Statement[] Stack
        {
            get { return _methodStack.ToArray(); }
        }
        /// <summary>
        /// Gets the report trace
        /// </summary>
        public Report[] Reports
        {
            get { return _reportTrace.ToArray(); }
        }
        #endregion

        #region IL-Processing Helpers
        void Load_Constant(Constant ldArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            ldArg.Label = GetLabel(instr);
            PushToStack(ldArg);
        }
        void Load_TypeToken(TypeReference ldArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            Kind kind = Kind.GetCachedKind(ldArg);
            string cachedDecKind = Utility.GetAppropriateName(this.DeclaringKind, kind, useDefaultOnly, targetPlats)
                + kind.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats);

            TypeToken typeToken = new TypeToken(cachedDecKind, cachedDecKind, kind);

            typeToken.Label = GetLabel(instr);
            PushToStack(typeToken);
        }
        void Load_MethodReference(MethodReference ldArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            Method method = new Method(ldArg);

            //if the method has args, pop the args from the stack
            //because the args will be popped from the right-most arg to the left-most arg
            //u hv to reverse the args so that the args are entered from the left-most arg to the right-most arg
            List<ValueStatement> args = new List<ValueStatement>();
            if (method.UnderlyingMethod.Parameters.Count > 0)
            {
                for (int index = 0; index < method.UnderlyingMethod.Parameters.Count; index++)
                {
                    ValueStatement arg = (ValueStatement)PopFromStack(); //expecting value statement
                    args.Add(arg);
                }
                args.Reverse();
            }

            var MethodDef = method.GetMethodDefinition();
            //if it is an instance method
            if (MethodDef != null && !MethodDef.IsStatic)
            {
                ValueStatement owner = null;
                if (instr.OpCode.Code == Code.Newobj) //these constructors have no "owners" popped from the stack
                {
                    Kind dynObjKind = Kind.GetCachedKind(new object().GetType().ToTypeDefinition(true));
                    owner = new DynamicObjRef("dummy", dynObjKind.GetLongName(useDefaultOnly, targetPlats), dynObjKind); //dynamically created obj
                }
                else
                    owner = (ValueStatement)PopFromStack(); //expecting value statement

                bool isBaseMethodCall = false;
                bool isExplicitConstrCall = false;
                if (instr.OpCode.Code == Code.Call && owner is ThisParamRef)
                {
                    if (method.DeclaringKind.UnderlyingType.FullName != this.DeclaringKind.UnderlyingType.FullName)
                        isBaseMethodCall = true;
                    if (method.IsConstructor)
                        isExplicitConstrCall = true;
                }

                InstanceMethodRef methodRef = null;
                if (method.IsConstructor) //for constructors
                {
                    methodRef = new InstanceMethodRef(
                        Utility.GetAppropriateName(this.DeclaringKind, method.DeclaringKind, useDefaultOnly, targetPlats)
                        + method.DeclaringKind.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats),
                            method.DeclaringKind.GetLongName(useDefaultOnly, targetPlats), 
                            isBaseMethodCall, isExplicitConstrCall, owner, method, args.ToArray());
                }
                else 
                {
                    methodRef = new InstanceMethodRef(method.GetName(useDefaultOnly, targetPlats)
                        + method.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats),
                        method.DeclaringKind.GetLongName(useDefaultOnly, targetPlats), 
                        isBaseMethodCall, isExplicitConstrCall, owner, method, args.ToArray());
                }

                dynamic returnObj = method.GetReturnKind(useDefaultOnly, targetPlats);
                if (instr.OpCode.Code == Code.Newobj)
                {
                    //since this constructor has no "reaal owner", it behaves like a static method
                    if (args.Count == 0)
                        methodRef.Label = GetLabel(instr);
                    else
                        methodRef.Label = args[0].Label;
                    PushToStack(methodRef); //always push to stack  
                }
                else if (method.IsConstructor && method.DeclaringKind.UnderlyingType.IsValueType)
                { 
                    if (!(owner is DynamicObjRef))
                    {
                        string cachedKind =
                                                Utility.GetAppropriateName(this.DeclaringKind,
                                                Kind.GetCachedKind(owner.GetKind().UnderlyingType.GetElementType()), useDefaultOnly, targetPlats);

                        //**************************not sure we have to "dereference" the pointer
                        PointerElement ownerElement = new PointerElement(owner.ToString(translator), cachedKind, owner);

                        Assignment assign = new Assignment(ownerElement, GetProperRightValue(ownerElement, methodRef, translator, true, useDefaultOnly, targetPlats));

                        AddToTranslatedCodes(assign);
                    }
                    else
                    {
                        AddToTranslatedCodes(methodRef);
                    }
                }
                else if (method.IsConstructor || returnObj == null)
                    AddToTranslatedCodes(methodRef);
                //else if (returnObj is Kind &&
                //    (returnObj as Kind).GetLongName(useDefaultOnly, targetPlats).ToLower().Equals("system.void"))
                //    AddToTranslatedCodes(methodRef);
                else if (returnObj.ToString().ToLower().Equals("system.void"))
                    AddToTranslatedCodes(methodRef);
                else
                    PushToStack(methodRef);
            }
            //if method is static
            else 
            {
                StaticMethodRef staticMethodRef = null;

                if (method.IsConstructor) 
                {
                    staticMethodRef = new StaticMethodRef(
                        Utility.GetAppropriateName(this.DeclaringKind, method.DeclaringKind, useDefaultOnly, targetPlats)
                        + method.DeclaringKind.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats),
                            Utility.GetAppropriateName(this.DeclaringKind, method.DeclaringKind, useDefaultOnly, targetPlats)
                        + method.DeclaringKind.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats),
                            method, args.ToArray());
                }
                else
                {
                    staticMethodRef = new StaticMethodRef(method.GetName(useDefaultOnly, targetPlats)
                        + method.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats),
                        Utility.GetAppropriateName(this.DeclaringKind, method.DeclaringKind, useDefaultOnly, targetPlats)
                        + method.DeclaringKind.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats),
                        method, args.ToArray());
                }
                if (this.DeclaringKind.UnderlyingType.FullName == staticMethodRef.ReferencedMethod.DeclaringKind.UnderlyingType.FullName)
                    staticMethodRef.UseShortName = true;
                if (args.Count == 0)
                    staticMethodRef.Label = GetLabel(instr);

                dynamic returnObj = method.GetReturnKind(useDefaultOnly, targetPlats);
                if (instr.OpCode.Code == Code.Newobj)
                    PushToStack(staticMethodRef); //always push to stack 
                else if (method.IsConstructor || returnObj == null)
                    AddToTranslatedCodes(staticMethodRef);
                else if (returnObj.ToString().ToLower().Equals("system.void"))
                    AddToTranslatedCodes(staticMethodRef);
                else
                    PushToStack(staticMethodRef);
            }
        }
        void Load_MethodToken(MethodReference ldArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            Method method = new Method(ldArg);
            Kind declaringKind = method.DeclaringKind;
            string cachedDecKind = Utility.GetAppropriateName(this.DeclaringKind, declaringKind, useDefaultOnly, targetPlats)
                + method.DeclaringKind.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats);

            MethodToken methodToken = null;
            if (method.IsConstructor)
            {
                methodToken = new MethodToken(cachedDecKind, cachedDecKind, method);
                methodToken.UseShortName = true;
            }
            else
            {
                methodToken = new MethodToken(method.GetName(useDefaultOnly, targetPlats)
                    + method.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats),
                    cachedDecKind, method);
                if (this.DeclaringKind.UnderlyingType.FullName == methodToken.ReferencedMethod.DeclaringKind.UnderlyingType.FullName)
                    methodToken.UseShortName = true;
            }

            methodToken.Label = GetLabel(instr);
            PushToStack(methodToken);
        }
        void Load_MethodFunction(MethodReference ldArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            Method method = new Method(ldArg);
            Kind declaringKind = method.DeclaringKind;
            string cachedDecKind = Utility.GetAppropriateName(this.DeclaringKind, declaringKind, useDefaultOnly, targetPlats)
                + method.DeclaringKind.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats);

            MethodFunction methodFtn = null;
            if (method.IsConstructor)
            {
                methodFtn = new MethodFunction(cachedDecKind, cachedDecKind, method);
                methodFtn.UseShortName = true;
            }
            else
            {
                methodFtn = new MethodFunction(method.GetName(useDefaultOnly, targetPlats)
                    + method.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats),
                    cachedDecKind, method);
                if (this.DeclaringKind.UnderlyingType.FullName == methodFtn.ReferencedMethod.DeclaringKind.UnderlyingType.FullName)
                    methodFtn.UseShortName = true;
            }

            methodFtn.Label = GetLabel(instr);
            PushToStack(methodFtn);
        }
        void Load_MethodVirtualFunction(MethodReference ldArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            Method method = new Method(ldArg);
            Kind declaringKind = method.DeclaringKind;
            string cachedDecKind = Utility.GetAppropriateName(this.DeclaringKind, declaringKind, useDefaultOnly, targetPlats)
                + method.DeclaringKind.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats);

            ValueStatement owner = (ValueStatement)PopFromStack();

            MethodVirtualFunction methodVirtFtn = null;
            if (method.IsConstructor)
            {
                methodVirtFtn = new MethodVirtualFunction(cachedDecKind, cachedDecKind, owner, method);
                methodVirtFtn.UseShortName = true;
            }
            else
            {
                methodVirtFtn = new MethodVirtualFunction(method.GetName(useDefaultOnly, targetPlats)
                    + method.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats),
                    cachedDecKind, owner, method);
                if (this.DeclaringKind.UnderlyingType.FullName == methodVirtFtn.ReferencedMethod.DeclaringKind.UnderlyingType.FullName)
                    methodVirtFtn.UseShortName = true;
            }

            PushToStack(methodVirtFtn);
        }
        void Load_FieldReference(FieldReference ldArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            Field field = Field.GetCachedField(ldArg);

            //if it is an instance field
            if (instr.OpCode.Code == Code.Ldfld || instr.OpCode.Code == Code.Ldflda) 
            {
                ValueStatement owner = (ValueStatement)PopFromStack(); //expecting value statement
                InstanceFieldRef fieldRef = new InstanceFieldRef(field.GetName(useDefaultOnly, targetPlats),
                        field.DeclaringKind.GetLongName(useDefaultOnly, targetPlats), owner, field);

                //test for conflicting names___________________________________________________________
                //for conflicting variables
                foreach (var variable in GetLocals())
                {
                    if (fieldRef.CachedName == variable.GetName(useDefaultOnly, targetPlats))
                    {
                        fieldRef.UseShortName = false;
                        goto push;
                    }
                }
                //for conflicting parameters
                if (!HasUserDefinedParameterSection(targetPlats)) //if the param section is user-defined, there is little I can do
                {
                    Parameter[] paramz = (Parameter[])GetParameters(useDefaultOnly, targetPlats);
                    foreach (var param in paramz)
                    {
                        if (fieldRef.CachedName == param.GetName(useDefaultOnly, targetPlats))
                        {
                            fieldRef.UseShortName = false;
                            goto push;
                        }
                    }
                }

            push:
                if (instr.OpCode.Code == Code.Ldflda)
                    PushToStack(new AddressOf(fieldRef));
                else
                    PushToStack(fieldRef);
            }
            //if static
            else if (instr.OpCode.Code == Code.Ldsfld || instr.OpCode.Code == Code.Ldsflda)
            {
                StaticFieldRef staticFieldRef = new StaticFieldRef(field.GetName(useDefaultOnly, targetPlats),
                        Utility.GetAppropriateName(this.DeclaringKind, field.DeclaringKind, useDefaultOnly, targetPlats)
                        + field.DeclaringKind.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats),
                        field);
                if (this.DeclaringKind.UnderlyingType.FullName == staticFieldRef.ReferencedField.DeclaringKind.UnderlyingType.FullName)
                    staticFieldRef.UseShortName = true;
                staticFieldRef.Label = GetLabel(instr);

                //test for conflicting names___________________________________________________________
                //for conflicting variables
                foreach (var variable in GetLocals())
                {
                    if (staticFieldRef.CachedName == variable.GetName(useDefaultOnly, targetPlats))
                    {
                        staticFieldRef.UseShortName = false;
                        goto push;
                    }
                }
                //for conflicting parameters
                if (!HasUserDefinedParameterSection(targetPlats)) //if the param section is user-defined, there is little I can do
                {
                    Parameter[] paramz = (Parameter[])GetParameters(useDefaultOnly, targetPlats);
                    foreach (var param in paramz)
                    {
                        if (staticFieldRef.CachedName == param.GetName(useDefaultOnly, targetPlats))
                        {
                            staticFieldRef.UseShortName = false;
                            goto push;
                        }
                    }
                }

            push:
                if (instr.OpCode.Code == Code.Ldsflda)
                    PushToStack(new AddressOf(staticFieldRef));
                else
                    PushToStack(staticFieldRef);
            }
        }
        void Load_FieldToken(FieldReference ldArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            Field field = Field.GetCachedField(ldArg);
            Kind declaringKind = field.DeclaringKind;
            string cachedDecKind = Utility.GetAppropriateName(this.DeclaringKind, declaringKind, useDefaultOnly, targetPlats)
                + field.DeclaringKind.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats);

            FieldToken fieldToken = new FieldToken(field.GetName(useDefaultOnly, targetPlats), cachedDecKind, field);
            if (this.DeclaringKind.UnderlyingType.FullName == fieldToken.ReferencedField.DeclaringKind.UnderlyingType.FullName)
                fieldToken.UseShortName = true;

            fieldToken.Label = GetLabel(instr);
            PushToStack(fieldToken);
        }
        void Load_ParameterReference(ParameterReference ldArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            ParamRef paramRef = null;
            Parameter parameter = Parameter.GetCachedParameter(ldArg, this);
            dynamic parameterKind = parameter.GetParameterKind(useDefaultOnly, targetPlats);
            paramRef = new ParamRef(parameter.GetName(useDefaultOnly, targetPlats), parameterKind.ToString(), parameter);
            paramRef.Label = GetLabel(instr);

            if (instr.OpCode.Code == Code.Ldarga || instr.OpCode.Code == Code.Ldarga_S)
                PushToStack(new AddressOf(paramRef));
            else
                PushToStack(paramRef);
        }
        void Load_VariableReference(VariableReference ldArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            Variable variable = GetVariableFromLocals(ldArg);
            VarRef varRef = new VarRef(variable.GetName(useDefaultOnly, targetPlats),
                variable.VariableKind.GetLongName(useDefaultOnly, targetPlats), variable);
            varRef.Label = GetLabel(instr);

            if (instr.OpCode.Code == Code.Ldloca || instr.OpCode.Code == Code.Ldloca_S)
                PushToStack(new AddressOf(varRef));
            else
                PushToStack(varRef);
        }
        void Load_ArrayElement(TypeReference ldArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            //presently ldArg can be null, so test for it

            ValueStatement index = (ValueStatement)PopFromStack(); //expecting value statement
            ValueStatement array = (ValueStatement)PopFromStack(); //expecting value statement

            string cachedKind =
            Utility.GetAppropriateName(this.DeclaringKind, Kind.GetCachedKind(array.GetKind().UnderlyingType.GetElementType()), useDefaultOnly, targetPlats);
            ArrayElement arrayElement = new ArrayElement(array.ToString(translator), cachedKind, array, index);

            if (instr.OpCode.Code == Code.Ldelema)
                PushToStack(new AddressOf(arrayElement));
            else
                PushToStack(arrayElement);
        }
        void Load_PointerElement(TypeReference ldArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            //presently ldArg can be null (for ldind.xxx instr), so test for it
            //note that Unbox_Any also calls this method, just like Ldobj would i.e. ldArg is not null

            ValueStatement pointer = (ValueStatement)PopFromStack(); //expecting value statement 

            string cachedKind =
            Utility.GetAppropriateName(this.DeclaringKind, Kind.GetCachedKind(pointer.GetKind().UnderlyingType.GetElementType()), useDefaultOnly, targetPlats);
            PointerElement pointerElement = new PointerElement(pointer.ToString(translator), cachedKind, pointer);

            PushToStack(pointerElement);
        }
        void Store_FieldReference(FieldReference strArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            Field field = Field.GetCachedField(strArg);

            ValueStatement value = (ValueStatement)PopFromStack(); //expecting value statement

            //if it is an instance field
            if (instr.OpCode.Code == Code.Stfld)
            {
                InstanceFieldRef fieldRef = null;
                ValueStatement owner = (ValueStatement)PopFromStack(); //expecting value statement
                fieldRef = new InstanceFieldRef(field.GetName(useDefaultOnly, targetPlats),
                        field.DeclaringKind.GetLongName(useDefaultOnly, targetPlats), owner, field);

                //test for conflicting names___________________________________________________________
                //for conflicting variables
                foreach (var variable in GetLocals())
                {
                    if (fieldRef.CachedName == variable.GetName(useDefaultOnly, targetPlats))
                    {
                        fieldRef.UseShortName = false;
                        goto push;
                    }
                }
                //for conflicting parameters
                if (!HasUserDefinedParameterSection(targetPlats)) //if the param section is user-defined, there is little I can do
                {
                    Parameter[] paramz = (Parameter[])GetParameters(useDefaultOnly, targetPlats);
                    foreach (var param in paramz)
                    {
                        if (fieldRef.CachedName == param.GetName(useDefaultOnly, targetPlats))
                        {
                            fieldRef.UseShortName = false;
                            goto push;
                        }
                    }
                }

            push:
                Assignment assign = new Assignment(fieldRef, GetProperRightValue(fieldRef, value, translator, true, useDefaultOnly, targetPlats));
                AddToTranslatedCodes(assign);
            }

            //if static
            else if (instr.OpCode.Code == Code.Stsfld)
            {
                StaticFieldRef staticFieldRef = new StaticFieldRef(field.GetName(useDefaultOnly, targetPlats),
                        Utility.GetAppropriateName(this.DeclaringKind, field.DeclaringKind, useDefaultOnly, targetPlats)
                        + field.DeclaringKind.GetGenericArgumentsSection_CsStyle(useDefaultOnly, targetPlats),
                        field);
                if (this.DeclaringKind.UnderlyingType.FullName == staticFieldRef.ReferencedField.DeclaringKind.UnderlyingType.FullName)
                    staticFieldRef.UseShortName = true;

                //test for conflicting names___________________________________________________________
                //for conflicting variables
                foreach (var variable in GetLocals())
                {
                    if (staticFieldRef.CachedName == variable.GetName(useDefaultOnly, targetPlats))
                    {
                        staticFieldRef.UseShortName = false;
                        goto push;
                    }
                }
                //for conflicting parameters
                if (!HasUserDefinedParameterSection(targetPlats)) //if the param section is user-defined, there is little I can do
                {
                    Parameter[] paramz = (Parameter[])GetParameters(useDefaultOnly, targetPlats);
                    foreach (var param in paramz)
                    {
                        if (staticFieldRef.CachedName == param.GetName(useDefaultOnly, targetPlats))
                        {
                            staticFieldRef.UseShortName = false;
                            goto push;
                        }
                    }
                }

            push:
                Assignment assign = new Assignment(staticFieldRef, GetProperRightValue(staticFieldRef, value, translator, true, useDefaultOnly, targetPlats));
                AddToTranslatedCodes(assign);
            }
        }
        void Store_ParameterReference(ParameterReference strArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            Parameter parameter = Parameter.GetCachedParameter(strArg, this);
            ParamRef paramRef = null;
            dynamic parameterKind = parameter.GetParameterKind(useDefaultOnly, targetPlats);
            paramRef = new ParamRef(parameter.GetName(useDefaultOnly, targetPlats), parameterKind.ToString(), parameter);

            ValueStatement value = (ValueStatement)PopFromStack(); //expecting value statement

            Assignment assign = new Assignment(paramRef, GetProperRightValue(paramRef, value, translator, true, useDefaultOnly, targetPlats));
            AddToTranslatedCodes(assign);
        }
        void Store_VariableReference(VariableReference strArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            Variable variable = GetVariableFromLocals(strArg);
            VarRef varRef = new VarRef(variable.GetName(useDefaultOnly, targetPlats),
                variable.VariableKind.GetLongName(useDefaultOnly, targetPlats), variable);
            ValueStatement value = (ValueStatement)PopFromStack(); //expecting value statement

            Assignment assign = new Assignment(varRef, GetProperRightValue(varRef, value, translator, true, useDefaultOnly, targetPlats));
            AddToTranslatedCodes(assign);
        }
        void Store_ArrayElement(TypeReference strArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            //presently strArg can be null, so test for it

            ValueStatement value = (ValueStatement)PopFromStack(); //expecting value statement
            ValueStatement index = (ValueStatement)PopFromStack(); //expecting value statement
            ValueStatement array = (ValueStatement)PopFromStack(); //expecting value statement

            string cachedKind =
            Utility.GetAppropriateName(this.DeclaringKind, Kind.GetCachedKind(array.GetKind().UnderlyingType.GetElementType()), useDefaultOnly, targetPlats);
            ArrayElement arrayElement = new ArrayElement(array.ToString(translator), cachedKind, array, index);

            Assignment assign = new Assignment(arrayElement, GetProperRightValue(arrayElement, value, translator, true, useDefaultOnly, targetPlats));
            AddToTranslatedCodes(assign);
        }
        void Store_PointerElement(TypeReference strArg, Instruction instr, CodomTranslator translator, bool useDefaultOnly, params string[] targetPlats)
        {
            //presently strArg can be null (for stind.xxx instr), so test for it

            ValueStatement value = (ValueStatement)PopFromStack(); //expecting value statement
            ValueStatement pointer = (ValueStatement)PopFromStack(); //expecting value statement

            string cachedKind =
            Utility.GetAppropriateName(this.DeclaringKind, Kind.GetCachedKind(pointer.GetKind().UnderlyingType.GetElementType()), useDefaultOnly, targetPlats);
            PointerElement pointerElement = new PointerElement(pointer.ToString(translator), cachedKind, pointer);

            Assignment assign = new Assignment(pointerElement, GetProperRightValue(pointerElement, value, translator, true, useDefaultOnly, targetPlats));
            AddToTranslatedCodes(assign);
        }

        /// <summary>
        /// This method seeks to convert the right value to a format in which it can be assigned to or compared with the left value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="translator"></param>
        /// <param name="isAssignment"></param>
        /// <param name="useDefaultOnly"></param>
        /// <param name="targetPlats"></param>
        /// <returns></returns>
        ValueStatement GetProperRightValue(ValueStatement left, ValueStatement right, CodomTranslator translator, bool isAssignment, bool useDefaultOnly, params string[] targetPlats) //****************
        {
            try
            {
                /*
                 * Automatic "boxing" is done only if this is an assignment
                 * 
                 * ALGORITHM:
                 * if right is Null or Token
                 *      return right
                 *      
                 * set leftKind to left.GetKind(), rightKind to right.GetKind()
                 * if leftKind eq null or rightKind eq null //actually, the real test should be for only if rightKind eq null
                 *      return right //most likely when right is a reference to a constructor
                 *      
                 * set leftType = leftKind.UnderlyingType, rightType = rightKind.UnderlyingType
                 * if rightType extends leftType
                 *      return right
                 *      
                 * set number to null
                 * if right is UIntConst or right is IntConst
                 *      number = right.Value
                 * else if right is DataConversion
                 *      if right.Operand is UIntConst or right.Operand is IntConst
                 *          number = right.Operand.Value
                 *          
                 * if leftType is char
                 *      if number is not null
                 *          return Convert.ToChar(number)
                 *      else
                 *          if isAssignment
                 *              return (char)right;
                 * else if leftType is bool
                 *      if number is not null
                 *          return Convert.ToBool(number)
                 *      else
                 *          if isAssignment
                 *              return (bool)right
                 *              
                 * else if leftType is enum
                 *      if isAssignment
                 *          return (leftType)right
                 *          
                 * else //everything else (expected to be objects)
                 *      if number is not null
                 *          if number is 0
                 *              return Null
                 *      if isAssignment
                 *              return (leftType)right
                 *              
                 * return right //just return right is no condition above was met
                 */

                if (right is Null || right is Token)
                    return right;

                Kind leftKind = left.GetKind(), rightKind = right.GetKind();

                if (leftKind == null || rightKind == null)
                    return right; //most likely when right is a reference to a constructor

                TypeReference leftType = leftKind.UnderlyingType, rightType = rightKind.UnderlyingType;

                //if (leftType.FullName == rightType.FullName)
                //    return right;
                if (leftType.ToType(true).IsAssignableFrom(rightType.ToType(true)))
                    return right;

                dynamic number = null;
                if (right is UIntConst || right is IntConst)
                {
                    if (right is UIntConst)
                        number = (right as UIntConst).Value;
                    else //if (right is IntConst)
                        number = (right as IntConst).Value;
                }
                else if (right is DataConversion)
                {
                    DataConversion conv = (right as DataConversion);
                    if (conv.Operand is UIntConst || conv.Operand is IntConst)
                    {
                        if (conv.Operand is UIntConst)
                            number = (conv.Operand as UIntConst).Value;
                        else //if (conv.Operand is IntConst)
                            number = (conv.Operand as IntConst).Value;
                    }
                }

                if (leftType.FullName == new char().GetType().FullName)
                {
                    if (number != null)
                        return new CharConst(Convert.ToChar(number)) { Label = right.Label };
                    else if (isAssignment) //happens only if this is an assignment, not just a comparison
                        return new Box(right.ToString(translator), left.GetKind().GetLongName(useDefaultOnly, targetPlats),
                            right, left.GetKind());
                }
                else if (leftType.FullName == new bool().GetType().FullName)
                {
                    if (number != null)
                        return new BoolConst(Convert.ToBoolean(number)) { Label = right.Label };
                    else if (isAssignment) //happens only if this is an assignment, not just a comparison
                        return new Box(right.ToString(translator), left.GetKind().GetLongName(useDefaultOnly, targetPlats),
                            right, left.GetKind());
                }
                else if (leftType.Resolve() != null && leftType.Resolve().IsEnum)
                {
                    if (isAssignment)
                    {
                        return new Box(right.ToString(translator), left.GetKind().GetLongName(useDefaultOnly, targetPlats),
                              right, left.GetKind());
                    }
                }
                else //expected to be objects (type O)
                {
                    if (number != null && number.ToString() == "0")
                    {
                        return new Null() { Label = right.Label };
                    }
                    if (isAssignment)
                    {
                        return new Box(right.ToString(translator), left.GetKind().GetLongName(useDefaultOnly, targetPlats),
                              right, left.GetKind());
                    }
                }

                return right;
            }
            catch //because GetKind() can be null if the underlying type is generic
            {
                return right;
            }
        }
        #endregion

        #region Other Helpers
        string GetLabel(Instruction instr)
        {
            string str = instr.ToString();
            return str.Substring(0, str.IndexOf(':'));
        }
        List<string> recordedTryStarts = new List<string>(), recordedTryEnds = new List<string>();
        bool IsUniqueTryBlock(Instruction start, Instruction end)
        {
            string startStr = GetLabel(start);
            string endStr = GetLabel(end);
            bool isUnique = true;
            for (int index = 0; index < recordedTryStarts.Count; index++)
            {
                if (recordedTryStarts[index] == startStr && recordedTryEnds[index] == endStr)
                {
                    isUnique = false;
                    break;
                }
            }
            if (isUnique)
            {
                recordedTryStarts.Add(startStr);
                recordedTryEnds.Add(endStr);
            }

            return isUnique;
        }
        List<string> recordedFilterStarts = new List<string>();
        bool IsUniqueFilterBlock(Instruction start)
        {
            string startStr = GetLabel(start);
            bool isUnique = true;
            for (int index = 0; index < recordedFilterStarts.Count; index++)
            {
                if (recordedFilterStarts[index] == startStr)
                {
                    isUnique = false;
                    break;
                }
            }
            if (isUnique)
            {
                recordedFilterStarts.Add(startStr);
            }

            return isUnique;
        }
        #endregion

        /// <summary>
        /// Represents a report made during the translation process
        /// </summary>
        public class Report
        {
            string _msg = "";
            ReportType _type = ReportType.OperationInProgress;

            /// <summary>
            /// Creates a new instance of Tril.Models.Method.Report
            /// </summary>
            /// <param name="message"></param>
            public Report(string message)
                : this(message, ReportType.OperationInProgress) { }
            /// <summary>
            /// Creates a new instance of Tril.Models.Method.Report
            /// </summary>
            /// <param name="message"></param>
            /// <param name="type"></param>
            public Report(string message, ReportType type)
            {
                _msg = message == null ? "" : message.Trim();
                _type = type;
            }

            /// <summary>
            /// Gets the message of this report
            /// </summary>
            public string Message
            {
                get { return _msg; }
            }
            /// <summary>
            /// Gets the type of this report
            /// </summary>
            public ReportType Type
            {
                get { return _type; }
            }
            /// <summary>
            /// Returns the string equivalent of the message
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return Type.ToString() + ": " + Message;
            }
        }
        /// <summary>
        /// Lists the report types
        /// </summary>
        public enum ReportType
        {
            /// <summary>
            /// Error
            /// </summary>
            Error,
            /// <summary>
            /// Information
            /// </summary>
            Information,
            /// <summary>
            /// Completed
            /// </summary>
            OperationCompleted,
            /// <summary>
            /// Progress
            /// </summary>
            OperationInProgress,
            /// <summary>
            /// Warning
            /// </summary>
            Warning
        }
    }
}
