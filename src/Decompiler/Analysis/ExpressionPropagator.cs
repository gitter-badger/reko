﻿#region License
/* 
 * Copyright (C) 1999-2012 John Källén.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; see the file COPYING.  If not, write to
 * the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 */
#endregion

using Decompiler.Core;
using Decompiler.Core.Code;
using Decompiler.Core.Expressions;
using Decompiler.Core.Operators;
using Decompiler.Evaluation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decompiler.Analysis
{
    public class ExpressionPropagator : InstructionVisitor<Instruction>, ExpressionVisitor<Expression>
    {
        private IProcessorArchitecture arch;
        private ExpressionSimplifier eval;
        private SymbolicEvaluationContext ctx;
        private ProgramDataFlow flow;

        public ExpressionPropagator(IProcessorArchitecture arch, ExpressionSimplifier simplifier, SymbolicEvaluationContext ctx, ProgramDataFlow flow)
        {
            this.arch = arch;
            this.eval = simplifier;
            this.ctx = ctx;
            this.flow = flow;
        }

        public Instruction VisitAssignment(Assignment a)
        {
            var src = a.Src.Accept(this);
            ctx.SetValue(a.Dst, src);
            return new Assignment(a.Dst, src);
        }

        public Instruction VisitBranch(Branch b)
        {
            return new Branch(
                b.Condition.Accept(this),
                b.Target);
        }

        public Instruction VisitCallInstruction(CallInstruction ci)
        {
            ci.CallSite.StackDepthBefore = GetStackDepthBeforeCall(ci.CallSite.SizeOfReturnAddressOnStack);
            var proc = ci.Callee as Procedure;
            if (proc != null)
            {
                ctx.UpdateRegistersTrashedByProcedure(flow[proc]);
            }
            return ci;
        }

        private int GetStackDepthBeforeCall(int returnAddressBytes)
        {
            throw new NotImplementedException();
        }

        public Instruction VisitDeclaration(Declaration decl)
        {
            throw new NotImplementedException();
        }

        public Instruction VisitDefInstruction(DefInstruction def)
        {
            throw new NotImplementedException();
        }

        public Instruction VisitGotoInstruction(GotoInstruction gotoInstruction)
        {
            throw new NotImplementedException();
        }

        public Instruction VisitPhiAssignment(PhiAssignment phi)
        {
            throw new NotImplementedException();
        }

        public Instruction VisitIndirectCall(IndirectCall ic)
        {
            throw new NotImplementedException();
        }

        public Instruction VisitReturnInstruction(ReturnInstruction ret)
        {
            if (ret.Expression != null)
                ret.Expression.Accept(this);
            return ret;
        }

        public Instruction VisitSideEffect(SideEffect side)
        {
            return new SideEffect(side.Expression.Accept(this));
        }

        public Instruction VisitStore(Store store)
        {
            var src = SimplifyExpression(store.Src.Accept(this));
            var dst = SimplifyExpression(store.Dst.Accept(this));

            if (dst != Constant.Invalid)
                ctx.SetValueEa(dst, src);

            var idDst = dst as Identifier;
            if (idDst != null)
                return new Assignment(idDst, src);
            else
                return new Store(dst, src);
        }

        public Instruction VisitSwitchInstruction(SwitchInstruction si)
        {
            throw new NotImplementedException();
        }

        public Instruction VisitUseInstruction(UseInstruction u)
        {
            throw new NotImplementedException();
        }

        public Expression VisitAddress(Address addr)
        {
            return addr;
        }

        public Expression VisitApplication(Application appl)
        {
            throw new NotImplementedException();
        }

        public Expression VisitArrayAccess(ArrayAccess acc)
        {
            throw new NotImplementedException();
        }

        public Expression VisitBinaryExpression(BinaryExpression binExp)
        {
            var l = SimplifyExpression(binExp.Left.Accept(this));
            var r = SimplifyExpression(binExp.Right.Accept(this));
            var b = new BinaryExpression(binExp.op, binExp.DataType, l, r);
            return SimplifyExpression(b);
        }

        private Expression SimplifyExpression(Expression e)
        {
            var simp = e.Accept(eval);
            if (simp == Constant.Invalid)
                return e;
            if (ctx.IsFramePointer(simp))
                return simp;
            if (simp is Constant)
                return simp;

            var binExp = simp as BinaryExpression;
            if (binExp == null)
                return e;
            if (binExp.op != Operator.Add && binExp.op != Operator.Sub)
                return e;
            if (ctx.IsFramePointer(binExp.Left))
                return simp;
            else
                return e;
        }

        private Expression ConvertToParamOrLocal(Expression exp)
        {
            var m = exp as MemoryAccess;
            if (m == null)
                return exp;
            if (ctx.IsFramePointer(m.EffectiveAddress))
                return ctx.Frame.EnsureStackArgument(0, m.DataType);
            var bin = m.EffectiveAddress as BinaryExpression;
            if (bin == null)
                return exp;
            if (!ctx.IsFramePointer(bin.Left))
                return exp;
            var c = bin.Right as Constant;
            if (c == null)
                return exp;
            int cc = c.ToInt32();
            if (bin.op == Operator.Sub)
                cc = -cc;
            return ctx.Frame.EnsureStackVariable(cc, exp.DataType);
        }

        public Expression VisitCast(Cast cast)
        {
            var e = cast.Expression.Accept(this);
            return SimplifyExpression(new Cast(cast.DataType, e));
        }

        public Expression VisitConditionOf(ConditionOf cof)
        {
            var c = SimplifyExpression(cof.Expression.Accept(this));
            return SimplifyExpression(new ConditionOf(c));
        }

        public Expression VisitConstant(Constant c)
        {
            return c;
        }

        public Expression VisitDepositBits(DepositBits dpb)
        {
            var d = new DepositBits(
                dpb.Source.Accept(this),
                dpb.InsertedBits.Accept(this),
                dpb.BitPosition,
                dpb.BitCount);
            return SimplifyExpression(d);
        }

        public Expression VisitDereference(Dereference deref)
        {
            throw new NotImplementedException();
        }

        public Expression VisitFieldAccess(FieldAccess acc)
        {
            throw new NotImplementedException();
        }

        public Expression VisitIdentifier(Identifier id)
        {
            return SimplifyExpression(id.Accept(eval));
        }

        public Expression VisitMemberPointerSelector(MemberPointerSelector mps)
        {
            throw new NotImplementedException();
        }

        public Expression VisitMemoryAccess(MemoryAccess access)
        {
            var m = new MemoryAccess(
                access.MemoryId,
                access.EffectiveAddress.Accept(this),
                access.DataType);
            return ConvertToParamOrLocal(SimplifyExpression(m));
        }

        public Expression VisitMkSequence(MkSequence seq)
        {
            throw new NotImplementedException();
        }

        public Expression VisitPhiFunction(PhiFunction phi)
        {
            throw new NotImplementedException();
        }

        public Expression VisitPointerAddition(PointerAddition pa)
        {
            throw new NotImplementedException();
        }

        public Expression VisitProcedureConstant(ProcedureConstant pc)
        {
            return pc;
        }

        public Expression VisitScopeResolution(ScopeResolution scopeResolution)
        {
            throw new NotImplementedException();
        }

        public Expression VisitSegmentedAccess(SegmentedAccess access)
        {
            var m = new SegmentedAccess(
                access.MemoryId, 
                access.BasePointer.Accept(this),
                access.EffectiveAddress.Accept(this),
                access.DataType);
            return SimplifyExpression(m);
        }

        public Expression VisitSlice(Slice slice)
        {
            var s = new Slice(
                slice.DataType,
                slice.Expression.Accept(this),
                (byte) slice.Offset);
            return SimplifyExpression(s);
        }

        public Expression VisitTestCondition(TestCondition tc)
        {
            var t = new TestCondition(
                tc.ConditionCode,
                tc.Expression.Accept(this));
            return SimplifyExpression(t);
        }

        public Expression VisitUnaryExpression(UnaryExpression unary)
        {
            return SimplifyExpression(
                new UnaryExpression(unary.op, unary.DataType,
                    unary.Expression.Accept(this)));
        }
    }
}