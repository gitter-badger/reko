﻿#region License
/* 
 * Copyright (C) 1999-2016 John Källén.
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

using NUnit.Framework;
using Reko.Core;
using Reko.Core.Lib;
using Reko.Scanning;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reko.UnitTests.Scanning
{
    [TestFixture]
    public class VectorBuilderTests
    {
        private MockRepository mr;
        private Program program;

        [SetUp]
        public void Setup()
        {
            mr = new MockRepository();
        }

        private void Given_Program(byte [] bytes)
        {
            var arch = mr.Stub<IProcessorArchitecture>();
            arch.Stub(a => a.ReadCodeAddress(0, null, null)).IgnoreArguments()
                .Do(new Func<int, ImageReader, ProcessorState, Address>(
                    (s, r, st) => Address.Ptr32(r.ReadLeUInt32())));
            var image = new LoadedImage(Address.Ptr32(0x00010000), bytes);
            this.program = new Program
            {
                Architecture = arch,
                Image = image,
                ImageMap = image.CreateImageMap(),
            };
        }

        [Test(Description = "Should create a list of vector destinations")]
        public void Vb_CreateVector_ModifiesImageMap()
        {
            Given_Program(new byte[]
            {
                0x10, 0x00, 0x01, 0x00,
                0x11, 0x00, 0x01, 0x00,
                0x12, 0x00, 0x01, 0x00,
                0xCC, 0xCC, 0xCC, 0xCC,
                0xC3, 0xC3, 0xC3, 0xCC,
            });
            var scanner = mr.Stub<IScanner>();
            scanner.Stub(s => s.CreateReader(program.Image.BaseAddress))
                .Return(program.Image.CreateLeReader(0));
            var state = mr.Stub<ProcessorState>();
        
            mr.ReplayAll();

            var vb = new VectorBuilder(scanner, program, new DirectedGraphImpl<object>());
            var vector = vb.BuildTable(program.Image.BaseAddress, 12, null, 4, state);
            Assert.AreEqual(3, vector.Count);
        }
    }
}
