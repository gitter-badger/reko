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
using Reko.Gui;
using Reko.Gui.Windows;
using Reko.UnitTests.Mocks;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace Reko.UnitTests.Gui.Windows
{
    [TestFixture]
    public class CodeViewerServiceTests
    {
        private ServiceContainer sc;
        private Program program;

        [SetUp]
        public void Setup()
        {
            sc = new ServiceContainer();
            this.program = new Program();
        }

        [Test]
        public void Cvp_CreateViewerIfNotVisible()
        {
            var m = new ProcedureBuilder();
            m.Return();

            var uiSvc = AddMockService<IDecompilerShellUiService>();
            uiSvc.Expect(s => s.FindDocumentWindow(
                    "codeViewerWindow" , m.Procedure))
                .Return(null);
            var windowFrame = MockRepository.GenerateStub<IWindowFrame>();
            uiSvc.Expect(s => s.CreateDocumentWindow(
                    Arg<string>.Is.Equal("codeViewerWindow"),
                Arg<string>.Is.Equal(m.Procedure),
                Arg<string>.Is.Equal(m.Procedure.Name),
                Arg<IWindowPane>.Is.Anything))
                .Return(windowFrame);
            windowFrame.Expect(s => s.Show());

            var codeViewerSvc = new CodeViewerServiceImpl(sc);
            codeViewerSvc.DisplayProcedure(program, m.Procedure);

            uiSvc.VerifyAllExpectations();
        }

        private T AddMockService<T>() where T : class
        {
            var svc = MockRepository.GenerateMock<T>();
            sc.AddService(typeof (T), svc);
            return svc;
        }
    }
}
