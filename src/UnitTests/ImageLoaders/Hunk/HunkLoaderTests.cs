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

using Reko.Core;
using Reko.ImageLoaders.Hunk;
using Reko.Environments.AmigaOS;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rhino.Mocks;
using System.ComponentModel.Design;
using Reko.Core.Configuration;
using Reko.Arch.M68k;

namespace Reko.UnitTests.ImageLoaders.Hunk
{
    [TestFixture]
    public class HunkLoaderTests
    {
        private HunkMaker mh;
        private MockRepository mr;
        private ServiceContainer sc;

        [SetUp]
        public void Setup()
        {
            mr = new MockRepository();
            mh = new HunkMaker();
            var cfgSvc = mr.Stub<IConfigurationService>();
            var opEnv = mr.Stub<OperatingEnvironment>();
            cfgSvc.Stub(c => c.GetEnvironment("amigaOS")).Return(opEnv);
            cfgSvc.Stub(c => c.GetArchitecture("m68k")).Return(new M68kArchitecture());
            opEnv.Stub(o => o.Load(null, null))
                .IgnoreArguments()
                .Do(new Func<IServiceProvider, IProcessorArchitecture, IPlatform>((sp, arch) =>
                new AmigaOSPlatform(sp, arch)));
            sc = new ServiceContainer();
            sc.AddService<IConfigurationService>(cfgSvc);
        }


        [Test]
        public void Hunk_LoadFile()
        {
            mr.ReplayAll();
            var bytes = File.ReadAllBytes(
                FileUnitTester.MapTestPath("../../subjects/Hunk-m68k/FIBO"));
            var ldr = new HunkLoader(sc, "FIBO", bytes);
            ldr.Load(Address.Ptr32(0x10000)); 
        }

        [Test]
        public void Hunk_LoadEmpty()
        {
            mr.ReplayAll();
            var bytes = mh.MakeBytes(
                HunkType.HUNK_HEADER,
                "",
                0,
                0,
                0,
                0);
            var ldr = new HunkLoader(sc, "foo.bar", bytes);
            var ldImg = ldr.Load(Address.Ptr32(0x00010000));
            Assert.AreEqual(1, ldImg.ImageMap.Segments.Count);
            Assert.AreEqual(Address.Ptr32(0x00010000), ldImg.ImageMap.Segments.Values[0].Address);
        }

        [Test]
        public void Hunk_LoadCode()
        {
            mr.ReplayAll();
            var bytes = mh.MakeBytes(
                HunkType.HUNK_HEADER,
                "CODE",
                "",
                1,
                0,
                0,
                0x40,
                HunkType.HUNK_CODE,
                1,
                (ushort) 0x4E75,
                (ushort) 0,
                HunkType.HUNK_END);
            var ldr = new HunkLoader(sc, "foo.bar", bytes);
            var program = ldr.Load(Address.Ptr32(0x00010000));
            var rlImg = ldr.Relocate(program, Address.Ptr32(0x00010000));
            Assert.AreEqual(1, rlImg.EntryPoints.Count);
            Assert.AreEqual(0x00010000ul, rlImg.EntryPoints[0].Address.ToLinear());

        }
    }
}
