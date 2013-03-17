﻿#region License
/* 
 * Copyright (C) 1999-2013 John Källén.
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

using Decompiler.Core.Types;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decompiler.Tools.C2Xml.UnitTests
{
    [TestFixture]
    public class NamedDataTypeExtractorTests
    {
        private NamedDataType nt;
        private Hashtable typedefs;

        [SetUp]
        public void Setup()
        {
            typedefs = new Hashtable();
        }

        private void Run(DeclSpec[] declSpecs, Declarator decl)
        {
            this.nt = NamedDataTypeExtractor.GetNameAndType(declSpecs, decl, typedefs);
        }

        private TypeSpec SType(CTokenType type)
        {
            return new SimpleTypeSpec { Type = type };
        }

        [Test]
        public void NamedDataTypeExtractor_Ulong()
        {
            Run(new[] { SType(CTokenType.Unsigned), SType(CTokenType.Long) },
                new IdDeclarator { Name = "Bob" });

            Assert.AreEqual("Bob", nt.Name);
            Assert.AreEqual(PrimitiveType.UInt32, nt.DataType);
        }

        [Test]
        public void NamedDataTypeExtractor_PtrChar()
        {
            Run(new [] { SType(CTokenType.Char),},
                new PointerDeclarator {
                    Pointee = new IdDeclarator { Name="Sue" }
                });
            Assert.AreEqual("Sue", nt.Name);
            Assert.IsInstanceOf<Pointer>(nt.DataType);
            var ptr = (Pointer) nt.DataType;
            Assert.AreEqual(PrimitiveType.Char, ptr.Pointee);
        }
    }
}