#region License
/* 
 * Copyright (C) 1999-2011 John K�ll�n.
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

using Decompiler.Arch.Intel;
using Decompiler.Assemblers.x86;
using Decompiler.Core;
using Decompiler.Core.Code;
using Decompiler.Core.Expressions;
using Decompiler.Core.Machine;
using Decompiler.Core.Operators;
using Decompiler.Core.Types;
using Decompiler.Core.Rtl;
using Decompiler.Evaluation;
using Decompiler.Loading;
using Decompiler.Scanning;
using Decompiler.UnitTests.Mocks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Decompiler.UnitTests.Scanning
{
	[TestFixture]
	public class BackwalkerTests
	{
        ProcedureBuilder m;
        private ProcessorState state;
        private ExpressionSimplifier expSimp;
        private Identifier SCZO;

        [SetUp]
        public void Setup()
        {
            m = new ProcedureBuilder();
            state = new IntelState();
            expSimp = new ExpressionSimplifier(new ScannerEvaluator(state));
            SCZO = m.Frame.EnsureFlagGroup((uint)(FlagM.SF | FlagM.CF | FlagM.ZF | FlagM.OF), "SCZO", PrimitiveType.Byte);
        }

        [Test]
        public void BackwalkAdd()
        {
            var eax = m.Frame.EnsureRegister(Registers.eax);
            var bw = new Backwalker(
                new RtlGoto(m.LoadDw(m.Add(eax, 0x10000))), expSimp);
            Assert.IsTrue(bw.BackwalkInstruction(m.Assign(eax, m.Add(eax, eax))));
            Assert.AreSame(Registers.eax, bw.Index);
            Assert.AreEqual("* 2", bw.Operations[0].ToString());
        }

        [Test]
        public void BackwalkAndMask()
        {
            var eax = m.Frame.EnsureRegister(Registers.eax);
            var bw = new Backwalker(
                new RtlGoto(m.LoadDw(m.Add(eax, 0x10000))), expSimp);
            Assert.IsFalse(bw.BackwalkInstruction(m.Assign(eax, m.And(eax, 0x7))));
            Assert.AreSame(Registers.eax, bw.Index);
            Assert.AreEqual("cmp 8", bw.Operations[0].ToString());
        }

        [Test]
        public void BackwalkJmp()
        {
            var eax = m.Frame.EnsureRegister(Registers.eax);
            var SCZO = m.Frame.EnsureFlagGroup((uint)(FlagM.SF|FlagM.ZF|FlagM.CF|FlagM.OF), "SCZO", PrimitiveType.Byte);
            var bw = new Backwalker(
                new RtlGoto(m.LoadDw(m.Add(eax, 0x1000))), expSimp);
            Assert.IsTrue(
                bw.BackwalkInstruction(
                    m.BranchIf(
                    new TestCondition(ConditionCode.UGT, SCZO),
                    "Nizze").Instruction));
            Assert.AreEqual("branch UGT", bw.Operations[0].ToString());
            Assert.AreEqual("SCZO", bw.UsedFlagIdentifier.ToString());
        }

        [Test]
        public void Comparison()
        {
            var eax = m.Frame.EnsureRegister(Registers.eax);
            var SCZO = m.Frame.EnsureFlagGroup((uint)(FlagM.SF | FlagM.ZF | FlagM.CF | FlagM.OF), "SCZO", PrimitiveType.Byte);
            var bw = new Backwalker(
                new RtlGoto(m.LoadDw(m.Add(eax, 0x1000))), expSimp);
            bw.UsedFlagIdentifier = m.Frame.EnsureFlagGroup((uint)FlagM.CF, "C", PrimitiveType.Byte);
            Assert.IsFalse(bw.BackwalkInstruction(
                m.Assign(SCZO, new ConditionOf(m.Sub(eax, 3)))), "Encountering this comparison should terminate the backwalk");
            Assert.AreSame(Registers.eax, bw.Index);
            Assert.AreEqual("cmp 3", bw.Operations[0].ToString());
        }

        [Test]
        public void BackwalkAndMaskWithHoles()
        {
            var eax = m.Frame.EnsureRegister(Registers.eax);
            var bw = new Backwalker(
                new RtlGoto(m.LoadDw(m.Add(eax, 0x10000))), expSimp);
            Assert.IsFalse(bw.BackwalkInstruction(m.Assign(eax, m.And(eax, 0x0A))));
            Assert.IsNull(bw.Index);
            Assert.AreEqual(0, bw.Operations.Count);
        }

        [Test]
        public void LoadIndexed()
        {
            var eax = m.Frame.EnsureRegister(Registers.eax);
            var edx = m.Frame.EnsureRegister(Registers.edx);
            var al = m.Frame.EnsureRegister(Registers.al);
            var SCZO = m.Frame.EnsureFlagGroup((uint)(FlagM.SF | FlagM.ZF | FlagM.CF | FlagM.OF), "SCZO", PrimitiveType.Byte);
            var bw = new Backwalker(
                new RtlGoto(m.LoadDw(m.Add(eax, 0x1000))), expSimp);
            Assert.IsTrue(bw.BackwalkInstruction(
                m.Assign(al, m.LoadB(m.Add(edx, 0x1004)))));
            Assert.AreSame(Registers.edx, bw.Index);
        }

        [Test]
        public void XorHiwordOfIndex()
        {
            var bx = m.Frame.EnsureRegister(Registers.bx);
            var bl = m.Frame.EnsureRegister(Registers.bl);
            var bh = m.Frame.EnsureRegister(Registers.bh);
            var bw = new Backwalker(
                new RtlGoto(m.LoadDw(m.Add(bx, 0x1000))), expSimp);

            Assert.IsTrue(bw.BackwalkInstruction(
                m.Assign(bh, m.Xor(bh, bh))));
            Assert.AreSame(Registers.bl, bw.Index);
            Assert.AreEqual("& 255", bw.Operations[0]);
        }

        [Test]
        public void BwSwitch32()
        {
            // samples of switch statement emitted
            // by the Microsoft VC compiler

            var esp = m.Frame.EnsureRegister(Registers.esp);
            var eax = m.Frame.EnsureRegister(Registers.eax);
            var edx = m.Frame.EnsureRegister(Registers.edx);
            var dl = m.Frame.EnsureRegister(Registers.dl);
            var SCZO = m.Frame.EnsureFlagGroup((uint)(FlagM.SF | FlagM.CF | FlagM.ZF | FlagM.OF), "SCZO", PrimitiveType.Byte);

            //m.Proc("foo");
            //m.Mov(m.eax, m.MemDw(Registers.esp, 4));
            //m.Cmp(m.eax, 3);
            //m.Ja("default");

            var block0 = m.CurrentBlock;
            m.Assign(eax, m.LoadDw(m.Add(esp, 4)));
            m.Assign(SCZO, new ConditionOf(m.Sub(eax, 3)));
            m.BranchIf(new TestCondition(ConditionCode.UGT, SCZO), "default");


            //m.Xor(m.edx, m.edx);
            //m.Mov(m.dl, m.MemB(Registers.eax, "bytes"));
            //m.Jmp(m.MemDw(Registers.edx, 4, "jumps"));

            var block1 = m.CurrentBlock;

            m.Assign(edx, m.Xor(edx, edx));
            m.Assign(SCZO, new ConditionOf(edx));
            m.Assign(dl, m.LoadB(m.Add(eax, 0x10000)));


            //m.Label("bytes");
            //m.Db(01, 0, 01, 02);

            //m.Label("jumps");
            //m.Dd("jump0", "jump1", "jump2");

            //m.Label("jump0");
            //m.Assign(eax, 0);
            //m.Jump("done");

            //m.Label("jump1");
            //m.Assign(eax, 1);
            //m.Jump("done");

            //m.Label("jump2");
            //m.Assign(eax, 2);
            //m.Jump("done");

            //m.Label("default");
            //m.Assign(eax, 01);

            //m.Label("done");
            //m.Store(new Constant(0x012310), eax);
            //m.Return();

            //m.Label("dummy");
            //m.Dd(0);

            RunTest(new IntelArchitecture(ProcessorMode.ProtectedFlat),
                new RtlGoto(m.LoadDw(m.Add(m.Mul(edx, 4), 0x10010))),
                "Scanning/BwSwitch32.txt");
        }

        [Test]
        public void BwSwitch16()
        {
            var sp = m.Frame.EnsureRegister(Registers.sp);
            var cs = m.Frame.EnsureRegister(Registers.cs);
            var ds = m.Frame.EnsureRegister(Registers.ds);
            var bl = m.Frame.EnsureRegister(Registers.bl);
            var bh = m.Frame.EnsureRegister(Registers.bh);
            var bx = m.Frame.EnsureRegister(Registers.bx);
            var si = m.Frame.EnsureRegister(Registers.si);

            m.Assign(sp, m.Sub(sp, 2));
            m.Store(sp, cs);
            m.Assign(ds, m.LoadW(sp));
            m.Assign(sp, m.Add(sp, 2));
            m.Assign(bl, m.LoadB(si));
            m.Assign(SCZO, new ConditionOf(m.Sub(bl, 2)));
            m.BranchIf(new TestCondition(ConditionCode.UGT, SCZO), "grox");

            m.Assign(bh, m.Xor(bh, bh));
            m.Assign(SCZO, new ConditionOf(bh));
            m.Assign(bx, m.Add(bx, bx));
            m.Assign(SCZO, new ConditionOf(bx));

            RunTest(new IntelArchitecture(ProcessorMode.Real),
                new RtlGoto(m.LoadW(m.Add(bx, 0x1234))),
                "Scanning/BwSwitch16.txt");
        }
		[Test]
		public void IbwInc()
		{
            var state = new IntelState();
            var di = new Identifier("di", 0, Registers.di.DataType, new RegisterStorage(Registers.di));
			Backwalker bw = new Backwalker(
                new RtlGoto(new MemoryAccess(di, di.DataType)),
                new ExpressionSimplifier(new ScannerEvaluator(state)));
			var instrs = new StatementList(new Block(null, "foo"));
			instrs.Add(0, new Assignment(di, new BinaryExpression(Operator.Add, di.DataType, di, Constant.Word16(1))));
			var r = bw.BackwalkInstructions(Registers.di, instrs);
			Assert.AreSame(Registers.di, bw.Index);
			Assert.AreEqual("+ 1", bw.Operations[0].ToString());
		}

		[Test]
		public void IbwPowersOfTwo()
		{
			Assert.IsTrue(Backwalker.IsEvenPowerOfTwo(2), "2 is power of two");
			Assert.IsTrue(Backwalker.IsEvenPowerOfTwo(4), "4 is power of two");
			Assert.IsTrue(Backwalker.IsEvenPowerOfTwo(8), "8 is power of two");
			Assert.IsTrue(Backwalker.IsEvenPowerOfTwo(16), "16 is power of two");
			Assert.IsTrue(Backwalker.IsEvenPowerOfTwo(256), "256 is power of two");
			Assert.IsFalse(Backwalker.IsEvenPowerOfTwo(3), "3 isn't power of two");
			Assert.IsFalse(Backwalker.IsEvenPowerOfTwo(7), "7 isn't power of two");
			Assert.IsFalse(Backwalker.IsEvenPowerOfTwo(127), "127 isn't power of two");
		}

        private void RunTest(IntelArchitecture arch, RtlTransfer rtlTransfer, string outputFile)
        {
            using (var fut = new FileUnitTester(outputFile))
            {
                m.Procedure.Write(false, fut.TextWriter);
                fut.TextWriter.Flush();

                var ibw = new Backwalker(rtlTransfer, new ExpressionSimplifier(new ScannerEvaluator(new IntelState())));
                var host = new BackwalkerHost();
                var bwoList = ibw.BackWalk(m.CurrentBlock, host);
                Assert.IsNotNull(bwoList);
                foreach (BackwalkOperation bwo in bwoList)
                {
                    fut.TextWriter.WriteLine(bwo);
                }
                fut.TextWriter.WriteLine("Index register: {0}", ibw.Index);
                fut.AssertFilesEqual();
            }
        }
         

		private void RunTest(string sourceFile, string outputFile, IntelArchitecture arch, Address addrBase, Address addrJump, IBackWalkHost host)
		{
			using (FileUnitTester fut = new FileUnitTester(outputFile))
			{
				AssemblerLoader ld = new AssemblerLoader(
                    new IntelTextAssembler(),
				    FileUnitTester.MapTestPath(sourceFile));
                Program prog = ld.Load(addrBase);
                Assert.IsTrue(prog.Architecture is IntelArchitecture);
				IntelDumper dumper = new IntelDumper(arch);
				dumper.ShowAddresses = true;
				dumper.ShowCodeBytes = true;
				dumper.DumpAssembler(prog.Image, prog.Image.BaseAddress, prog.Image.BaseAddress + prog.Image.Bytes.Length, fut.TextWriter);
				fut.TextWriter.Flush();

                Backwalker ibw = new Backwalker(null, null);
				List<BackwalkOperation> bws = ibw.BackWalk(addrJump, host);
				foreach (BackwalkOperation bwo in bws)
				{
					fut.TextWriter.WriteLine(bwo);
				}
				fut.TextWriter.WriteLine("Index register: {0}", ibw.Index);
				fut.AssertFilesEqual();
			}
		}

        private class BackwalkerHost : IBackWalkHost
        {
            #region IBackWalkHost Members

            public AddressRange GetSinglePredecessorAddressRange(Address block)
            {
                throw new NotImplementedException();
            }

            public Address GetBlockStartAddress(Address addr)
            {
                throw new NotImplementedException();
            }

            public Block GetSinglePredecessor(Block block)
            {
                return block.Procedure.ControlGraph.Predecessors(block).ToArray()[0];
            }

            #endregion
        }

		private class IbwSwitch16Helper : IBackWalkHost
		{
			public Address GetBlockStartAddress(Address addr)
			{
				switch (addr.Offset)
				{
					case 0x000D: return new Address(addr.Selector, 0x0009);
					default: throw new ArgumentException(string.Format("offset {0:X4} not handled", addr.Offset));
				}
			}

            public Block GetSinglePredecessor(Block block)
            {
                throw new NotImplementedException();
            }

			public AddressRange GetSinglePredecessorAddressRange(Address addrBegin)
			{
				if (addrBegin.Offset == 0x0009)
					return new AddressRange(new Address(addrBegin.Selector, 0), addrBegin);
				else
					return null;
			}
		}

		private class IbwSwitch32Helper : IBackWalkHost
		{
            public Block GetSinglePredecessor(Block block)
            {
                throw new NotImplementedException();
            }

			public AddressRange GetSinglePredecessorAddressRange(Address addr)
			{
				switch (addr.Offset)
				{
					case 0x1000000B: return new AddressRange(new Address(0x10000000), addr);
					default: throw new ArgumentException(string.Format("offset {0:X8} not handled", addr.Offset));
				}
			}

			public Address GetBlockStartAddress(Address addr)
			{
				switch (addr.Offset)
				{
					case 0x10000013: return new Address(0x1000000B);
					default: throw new ArgumentException(string.Format("offset {0:X8} not handled", addr.Offset));
				}
			}
		}
	}
}