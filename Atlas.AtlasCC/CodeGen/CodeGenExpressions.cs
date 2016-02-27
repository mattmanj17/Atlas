﻿using Antlr4.Runtime;
using Atlas.Architecture;
using Atlas.AtlasCC.Emitters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.AtlasCC
{
    partial class AtlasCodeGen
    {
        public void EmitExpression()
        {
            //expression ',' assignmentExpression

            /*
             Comma operator
                The comma operator expression has the form
                    lhs , rhs		
                where
                    lhs	-	any expression
                    rhs	-	any expression other than another comma operator (in other words, comma operator's associativity is left-to-right)
                
                First, the left operand, lhs, is evaluated and its result value is discarded.
                
                Then, the right operand, rhs, is evaluated and its result is returned by the comma operator as a non-lvalue.
                
                Notes
                The type of the lhs may be void (that is, it may be a call to a function that returns void, or it can be an expression cast to void)
                The comma operator returns an rvalue
             */

            throw new NotImplementedException();
        }

        public void EmitAssinmentOperation()
        {
            throw new NotImplementedException();
        }

        public void EmitCompoundAssinmentMultiply()
        {
            EmitMultiplyOperation();
            EmitAssinmentOperation();
        }

        public void EmitCompoundAssinmentDivide()
        {
            EmitDivideOperation();
            EmitAssinmentOperation();
        }

        public void EmitCompoundAssinmentMod()
        {
            EmitModOperation();
            EmitAssinmentOperation();
        }

        public void EmitCompoundAssinmentAdd()
        {
            EmitAddOperation();
            EmitAssinmentOperation();
        }

        public void EmitCompoundAssinmentSub()
        {
            EmitSubOperation();
            EmitAssinmentOperation();
        }

        public void EmitCompoundAssinmentShiftLeft()
        {
            EmitLeftShiftOperation();
            EmitAssinmentOperation();
        }

        public void EmitCompoundAssinmentShiftRight()
        {
            EmitRightShiftOperation();
            EmitAssinmentOperation();
        }

        public void EmitCompoundAssinmentAnd()
        {
            EmitAndOperation();
            EmitAssinmentOperation();
        }

        public void EmitCompoundAssinmentXor()
        {
            EmitXorOperation();
            EmitAssinmentOperation();
        }

        public void EmitCompoundAssinmentOr()
        {
            EmitOrOperation();
            EmitAssinmentOperation();
        }

        public void EmitCoditionalExpresion()
        {
            throw new NotImplementedException();
        }

        public void EmitLogicalOr()
        {
            throw new NotImplementedException();
        }

        public void EmitLogicalAnd()
        {
            throw new NotImplementedException();
        }

        public void EmitOrOperation()
        {
            Expression result = HandleArithmeticOperands();
            result.Add(new OpCodeEmitter(OpCode.OR));
            PushExpression(result);
        }

        public void EmitXorOperation()
        {
            Expression result = HandleArithmeticOperands();
            result.Add(new OpCodeEmitter(OpCode.XOR));
            PushExpression(result);
        }

        public void EmitAndOperation()
        {
            Expression result = HandleArithmeticOperands();
            result.Add(new OpCodeEmitter(OpCode.AND));
            PushExpression(result);
        }

        public void EmitAreEqualOperation()
        {
            throw new NotImplementedException();
        }

        public void EmitNotEqualOperation()
        {
            EmitAreEqualOperation();
            EmitLogicalNotOperation();
        }

        public void EmitLessThanOperation()
        {
            Expression result = HandleArithmeticOperands();
            result.Add(new OpCodeEmitter(OpCode.LESS));
            PushExpression(result);
        }

        public void EmitGreaterThanOperation()
        {
            SwapArgs();
            EmitLessThanOperation();
        }

        public void EmitLessThanOrEqualOperation()
        {
            Expression rhs = MakeExpressionRValue(PopExpression());
            rhs.Add(new OpCodeEmitter(OpCode.PUSHW, "1"));
            rhs.Add(new OpCodeEmitter(OpCode.ADD));
            PushExpression(rhs);
            EmitLessThanOperation();
        }

        public void EmitGreaterThanOrEqualOperation()
        {
            SwapArgs();
            EmitLessThanOrEqualOperation();
        }

        private void SwapArgs()
        {
            Expression a = PopExpression();
            Expression b = PopExpression();

            PushExpression(a);
            PushExpression(b);
        }

        public void EmitLeftShiftOperation()
        {
            Expression result = HandleArithmeticOperands();
            result.Add(new OpCodeEmitter(OpCode.SLL));
            PushExpression(result);
        }

        public void EmitRightShiftOperation()
        {
            Expression result = HandleArithmeticOperands();
            result.Add(new OpCodeEmitter(OpCode.SRL));
            PushExpression(result);
        }

        public void EmitAddOperation()
        {
            throw new NotImplementedException();
        }

        public void EmitSubOperation()
        {
            throw new NotImplementedException();
        }

        public void EmitMultiplyOperation()
        {
            Expression result = HandleArithmeticOperands();
            result.Add(new OpCodeEmitter(OpCode.MUL));
        }

        public void EmitDivideOperation()
        {
            Expression result = HandleArithmeticOperands();
            //
            throw new NotImplementedException();
        }

        public void EmitModOperation()
        {
            Expression result = HandleArithmeticOperands();
            //
            throw new NotImplementedException();
        }

        private Expression HandleArithmeticOperands()
        {
            Expression rhs = MakeExpressionRValue(PopExpression());
            Expression lhs = MakeExpressionRValue(PopExpression());

            if (!lhs.Type.IsInteger || !rhs.Type.IsInteger)
            {
                CodeGenError("Arithmetic operations require integer operands");
            }

            rhs = rhs.PromoteInteger();
            lhs = lhs.PromoteInteger();

            Expression result = new Expression(CTypeInfo.FromFundamentalType(FundamentalType.unsignedInt32), ValueCatagory.RValue, lhs.ConstantValue * rhs.ConstantValue, lhs.IsConstant && rhs.IsConstant);

            result.Add(lhs);
            result.Add(rhs);
            return result;
        }

        public void EmitCastExpresion(CTypeInfo type)
        {
            //handled by inline function probably
            Expression rhs = MakeExpressionRValue(PopExpression());

            if(!rhs.CanCastTo(type))
            {
                CodeGenError("cannot cast expresion of type " + rhs.Type + " to type " + type);
            }
            else
            {
                PushExpression(rhs.CastTo(type));
            }
        }

        public void EmitSizeOfType(CTypeInfo type)
        {
            Expression expr = new Expression(CTypeInfo.FromFundamentalType(FundamentalType.unsignedInt32),ValueCatagory.RValue,type.SizeOf,true);
            expr.Add(new OpCodeEmitter(OpCode.PUSHW, type.SizeOf.ToString()));
            PushExpression(expr);
        }

        public void EmitSizeOfExpression()
        {
            /*
             * The operands of the sizeof operator are expressions that are not evaluated  
             * "Thus, size_t n = sizeof(printf("%d", 4));" does not perform console output.
             */

            Expression rhs = PopExpression();
            EmitSizeOfType(rhs.Type);
        }

        public void EmitPreDecrement()
        {
            EmitIncrementDecrement(PrefixType.Prefix, IncrementType.Decrement);
        }

        public void EmitPreIncrement()
        {
            EmitIncrementDecrement(PrefixType.Prefix, IncrementType.Increment);
        }

        public void EmitAddressOfOperation()
        {
            Expression rhs = PopExpression();

            if (rhs.Type.IsPointer && rhs.Type.TypePointedTo.IsFunction)
            {
                //ignore taking the address of a function (it is already a pointer)
                PushExpression(rhs);
            }
            else
            {
                if (rhs.valueCatagory != ValueCatagory.LValue)
                {
                    CodeGenError("cannot take the addres of a non lvalue");
                }

                //todo handle const
                Expression expr = new Expression(rhs.Type.GetPointerType(), ValueCatagory.RValue, 0, false);
                expr.Add(rhs);

                PushExpression(expr);
            }
        }

        public void EmitDereferenceOperation()
        {
            //unaryOperator castExpression
            /*
             The dereference or indirection expression has the form
                *pointer-expression		
             where
                    pointer-expression	-	an expression of any pointer type
            If pointer-expression is a pointer to function, the result of the dereference operator is a function designator for that function.
            If pointer-expression is a pointer to object, the result is an lvalue expression that designates the pointed-to object.
            
             Dereferencing a null pointer, a pointer to an object outside of its lifetime (a dangling pointer), a misaligned pointer, or a pointer with indeterminate value is undefined behavior (handled in architecture)
             */

            Expression rhs = MakeExpressionRValue(PopExpression());

            if (!rhs.Type.IsPointer)
            {
                CodeGenError("cannot dereference non pointer type");
            }

            Expression result;

            if (rhs.Type.TypePointedTo.IsFunction)
            {
                //a function designator with type “function returning type” is converted to an expression that has type “pointer to function returning type”
                //ie, do nothing

                result = rhs;
            }
            else
            {
                //todo handle pointer to const
                result = new Expression(rhs.Type.TypePointedTo, ValueCatagory.LValue, 0, false);
                result.Add(rhs);
                result.Add(new OpCodeEmitter(OpCode.LW));
            }

            PushExpression(result);
        }

        public void EmitPositiveOperation()
        {
            /*
                the unary arithmetic operator expressions have the form
                    + expression	(1)	
                    - expression	(2)	
                1) unary plus (promotion)
                2) unary minus (negation)
                where
                    expression	-	expression of any arithmetic type
                Both unary plus and unary minus first apply integral promotions to their operand, and then
                unary plus returns the value after promotion
                unary minus returns the negative of the value after promotion (except that the negative of a NaN is another NaN)
                The type of the expression is the type after promotion, and the value category is non-lvalue.
             */

            Expression expr = MakeExpressionRValue(PopExpression());

            if(!expr.Type.IsArithmetic)
            {
                CodeGenError("sign manipulation operations can only be applied to arithmetic types");
            }

            expr = expr.PromoteInteger();

            PushExpression(expr);
        }

        public void EmitNegativeOperation()
        {
            EmitPositiveOperation();

            Expression expr = PopExpression();

            expr.Add(new OpCodeEmitter(OpCode.NEG));
            expr = expr.InvertSign();

            PushExpression(expr);
        }

        public void EmitBitwiseNotOperation()
        {
            //~ rhs
            //the operator ~ performs integer promotions on its only operand
            // for unsigned types (after promotion), the expression ~E is equivalent to the maximum value representable by the result type minus the original value of E
            
            Expression rhs =  MakeExpressionRValue(PopExpression());

            if(!rhs.Type.IsInteger)
            {
                CodeGenError("cannot perform bitwise not on non integer type");
            }

            rhs = rhs.PromoteInteger();

            //todo handle constant
            Expression result = new Expression(CTypeInfo.FromFundamentalType(FundamentalType.unsignedInt32),ValueCatagory.RValue,~rhs.ConstantValue,rhs.IsConstant);
            result.Add(rhs);
            result.Add(new OpCodeEmitter(OpCode.NOT));

            
        }

        public void EmitLogicalNotOperation()
        {
            /*
                The logical NOT expression has the form
                    ! expression		
                where
                    expression	-	an expression of any scalar type
                The logical NOT operator has type int. Its value is ​0​ if expression evaluates to a value that compares unequal to zero. 
                Its value is 1 if expression evaluates to a value that compares equal to zero. (so !E is the same as (E==0))
                see note on ==
             */

            EmitConstant("0");
            EmitAreEqualOperation();
        }

        public void EmitPostDecrement()
        {
            EmitIncrementDecrement(PrefixType.Postfix, IncrementType.Decrement);
        }

        public void EmitPostIncrement()
        {
            EmitIncrementDecrement(PrefixType.Postfix, IncrementType.Increment);
        }

        private enum PrefixType
        {
            Prefix,
            Postfix
        }

        private enum IncrementType
        {
            Increment,
            Decrement
        }

        private void EmitIncrementDecrement(PrefixType prefixType, IncrementType incrementType)
        {
            // The operand expr of both prefix and postfix increment or decrement must be a modifiable lvalue of integer type, floating type, or a pointer type.
            // The result of the postfix increment and decrement operators is the value of expr.
            // The result of the prefix increment operator is the result of adding the value 1 to the value of expr: the expression ++e is equivalent to e+=1. 
            // The result of the prefix decrement operator is the result of subtracting the value 1 from the value of expr: the expression --e is equivalent to e-=1

            Expression expr = PopExpression();

            if (expr.valueCatagory != ValueCatagory.LValue)
            {
                CodeGenError("The operand of both prefix and postfix increment or decrement must be a modifiable lvalue");
            }

            if (!expr.Type.IsInteger && !expr.Type.IsFloating && !expr.Type.IsPointer)
            {
                CodeGenError("The operand of both prefix and postfix increment or decrement must be integer type, floating type, or a pointer type");
            }

            Expression result = new Expression(expr.Type, ValueCatagory.RValue, 0, false);

            result.Add(expr);
            if (prefixType == PrefixType.Postfix)
            {
                result.Add(new OpCodeEmitter(OpCode.LW));
                //TODO fix it so we dont have to do this iefficent thing
                //do some twiddling with the stack pointer
                result.Add(expr);
            }
            else
            {
                result.Add(DuplicatePreviousExpresionValue());
            }
            result.Add(DuplicatePreviousExpresionValue());
            result.Add(new OpCodeEmitter(OpCode.LW));
            result.Add(new OpCodeEmitter(OpCode.PUSHW, (expr.Type.IsPointer ? expr.Type.TypePointedTo.SizeOf : 1).ToString()));

            if (incrementType == IncrementType.Increment)
            {
                result.Add(new OpCodeEmitter(OpCode.ADD));
            }
            else if (incrementType == IncrementType.Decrement)
            {
                result.Add(new OpCodeEmitter(OpCode.SUB));
            }

            result.Add(new OpCodeEmitter(OpCode.SW));
            if (prefixType == PrefixType.Prefix)
            {
                result.Add(new OpCodeEmitter(OpCode.LW));
            }

        }

        public void EmitMemberAccess(LabelInfo label)
        {
            Expression obj = PopExpression();

            if (obj.valueCatagory != ValueCatagory.LValue)
            {
                CodeGenError("cannot get member from rvalue expression");
            }

            if (obj.Type.HasMembers && obj.Type.HasMember(label))
            {
                //todo handel const members
                Expression result = new Expression(obj.Type.GetMemberType(label), ValueCatagory.LValue, 0, false);
                result.Add(obj);
                result.Add(new OpCodeEmitter(OpCode.PUSHW, obj.Type.GetMemberOffset(label).ToString()));
                result.Add(new OpCodeEmitter(OpCode.ADD));

                PushExpression(result);
            }
            else
            {
                CodeGenError("type " + obj.Type.ToString() + " has no member named \"" + label.ToString() + "\""); CodeGenError("type " + obj.Type.ToString() + " has no members");
            }
        }

        public void EmitPointerAccess(LabelInfo label)
        {
            EmitDereferenceOperation();
            EmitMemberAccess(label);
        }

        public void EmitFunctionCall(int numPassedArguments)
        {
            // postfixExpression '(' argumentExpressionList? ')'
            /*
              The function call expression has the form
                expression ( argument-list(optional) )		
                    where
                        expression	-	any expression of pointer-to-function type (after lvalue conversions)
                        argument-list	-	comma-separated list of expressions (which cannot be comma operators) of any complete object type. May be omitted when calling functions that take no arguments.
             */
            //The type of each parameter must he a type such that implicit conversion as if by assignment exists that converts the unqualified type of the corresponding argument to the type of the parameter.
            //Function is executed, and the value it returns becomes the value of the function call expression (if the function returns void, the function call expression is a void expression)

            List<Expression> arguments = new List<Expression>();

            for (int i = 0; i < numPassedArguments; i++)
            {
                arguments.Add(MakeExpressionRValue(PopExpression()));
            }

            Expression functionName = PopExpression();

            if (!functionName.Type.IsPointer || !functionName.Type.TypePointedTo.IsFunction)
            {
                CodeGenError("canot call expresion of type " + functionName.Type.ToString() + ": expected pointer to function");
            }

            IReadOnlyList<CTypeInfo> argTyps = functionName.Type.TypePointedTo.GetArgumentTypes();

            if (numPassedArguments < argTyps.Count)
            {
                CodeGenError("to few arguments passed to function");
            }
            else if (numPassedArguments > argTyps.Count)
            {
                CodeGenError("to many arguments passed to function");
            }

            for (int i = 0; i < numPassedArguments; i++)
            {
                if (!argTyps[i].IsAssignableFrom(arguments[i].Type))
                {
                    CodeGenError("error in " + i + "th argument: cannot convert argument of type " + arguments[i].Type + " to " + argTyps[i]);
                }
                else
                {
                    arguments[i].CastTo(argTyps[i]);
                }
            }

            CTypeInfo RetType = functionName.Type.TypePointedTo.ReturnType;

            Expression result = new Expression(RetType, ValueCatagory.RValue, 0, false);

            result.Add(new OpCodeEmitter(OpCode.BEGINARGS));
            foreach (Expression e in arguments)
            {
                result.Add(e);
            }
            result.Add(functionName);
            result.Add(new OpCodeEmitter(OpCode.CALL));

            PushExpression(result);
        }

        public void EmitArrayAccess()
        {
            //postfixExpression '[' expression ']'

            // The array subscrpt expression has the form
            // pointer-expression [ integer-expression ]	
            /*
             * 
                pointer-expression	-	an expression of type pointer to complete object
                integer-expression	-	an expression of integer type
                The subscript operator expression is is lvalue expression whose type is the type of the object pointed to by pointer-expression.
                
                By definition, the subscript operator E1[E2] is exactly identical to *((E1)+(E2)). 
             */

            Expression index = PopExpression();
            Expression array = PopExpression();

            if (!array.Type.IsPointer)
            {
                CodeGenError("cannot use array subscript operator on non pointer type");
            }

            //do we do implicet conversion here?
            if (!index.Type.IsInteger)
            {
                CodeGenError("array index expression must be of integral type");
            }

            int size = array.Type.TypePointedTo.SizeOf;

            //todo support const arrays/ const array elements
            Expression result = new Expression(array.Type.TypePointedTo, ValueCatagory.LValue, 0, false);

            result.Add(MakeExpressionRValue(array));
            result.Add(MakeExpressionRValue(index));
            result.Add(new OpCodeEmitter(OpCode.PUSHW, size.ToString()));
            result.Add(new OpCodeEmitter(OpCode.MUL));
            result.Add(new OpCodeEmitter(OpCode.ADD));

            PushExpression(result);
        }

        public void EmitStringLiteral(IEnumerable<string> literals)
        {
            // string literals are sequences of characters of type char[], char16_t[], char32_t[], or wchar_t[] that represent null-terminated strings

            // first, adjacent string literals (that is, string literals separated by whitespace only) are concatenated

            // Only two narrow or two wide string literals may be concatenated.
            // If one literal is unprefixed, the resulting string literal has the width/encoding specified by the prefixed literal. 
            // If the two string literals have different encoding prefixes, concatenation is implementation-defined.

            // String literals are not modifiable (and in fact may be placed in read-only memory such as .rodata). 
            // If a program attempts to modify the static array formed by a string literal, the behavior is undefined.

            // It is neither required nor forbidden for identical string literals to refer to the same location in memory. 
            // Moreover, overlapping string literals or string literals that are substrings of other string literals may be combined.
            // example : "def" == 3+"abcdef"; may be 1 or 0, implementation-defined

            throw new NotImplementedException();
        }

        public void EmitIdentifierReference(LabelInfo label)
        {
            //TODO constant labels
            Expression result = new Expression(label.Type, ValueCatagory.LValue, 0, false);

            if (label.IsMember)
            {
                throw new InvalidOperationException("member label passed in regular scope");
            }

            if (label.IsLocal)
            {
                //get address of local variable relative to stack pointer
                result.Add(new OpCodeEmitter(OpCode.PUSHBP));
                result.Add(new OpCodeEmitter(OpCode.PUSHW, label.Offset.ToString()));
                result.Add(new OpCodeEmitter(OpCode.ADD));
            }
            else
            {
                //the assembler will take care of figuring out the addresses of global variables
                result.Add(new OpCodeEmitter(OpCode.PUSHW, label.ToString()));
            }

            PushExpression(result);
        }

        public void EmitConstant(string literal)
        {
            /*
             * Constants and literals
                Constant values of certain types may be embedded in the source code of a C program using specialized expressions known as literals
                integer constants are decimal, octal, or hexadecimal numbers of integer type.
                character constants are individual characters of type int suitable for conversion to a character type
                floating constants are values of type float, double, or long double
             */

            int value;
            CTypeInfo type;
            ParseAsConstant(literal, out value, out type);

            Expression constant = new Expression(type, ValueCatagory.RValue, value, true);
            constant.Add(new OpCodeEmitter(OpCode.PUSHW, literal));

            PushExpression(constant);
        }

        private void ParseAsConstant(string literal, out int value, out CTypeInfo type)
        {
            throw new NotImplementedException();
        }

        public Expression MakeExpressionRValue(Expression exp)
        {
            Expression converted;

            if (exp.valueCatagory != ValueCatagory.RValue)
            {
                converted = new Expression(exp.Type, exp.valueCatagory, exp.ConstantValue, exp.IsConstant);
                converted.Add(exp);
                converted.Add(new OpCodeEmitter(OpCode.LW));
                return converted;
            }
            else
            {
                converted = exp;
            }

            return converted;
        }

        private IEmitter DuplicatePreviousExpresionValue()
        {
            EmitterList emitters = new EmitterList();

            emitters.Add(new OpCodeEmitter(OpCode.PUSHSP));
            emitters.Add(new OpCodeEmitter(OpCode.PUSHW, "4"));
            emitters.Add(new OpCodeEmitter(OpCode.SUB));
            emitters.Add(new OpCodeEmitter(OpCode.LW));

            return emitters;
        }

        public Expression CurrentExpresion
        {
            get
            {
                if (Emmiters.Count == 0)
                {
                    throw new InvalidOperationException("emitter stack is empty");
                }

                IEmitter emitter = Emmiters.Peek();

                if ((emitter as Expression) == null)
                {
                    throw new InvalidOperationException("expected expression but got " + emitter.GetType().Name);
                }

                return (emitter as Expression);
            }
        }

        private Expression PopExpression()
        {
            if (Emmiters.Count == 0)
            {
                throw new InvalidOperationException("emitter stack is empty");
            }

            IEmitter emitter = Emmiters.Pop();

            if ((emitter as Expression) == null)
            {
                throw new InvalidOperationException("expected expression but got " + emitter.GetType().Name);
            }

            return (emitter as Expression);
        }

        private void PushExpression(Expression exp)
        {
            Emmiters.Push(exp);
        }
    }
}
