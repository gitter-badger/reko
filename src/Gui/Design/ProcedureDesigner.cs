﻿#region License
/* 
 * Copyright (C) 1999-2015 John Källén.
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
using Reko.Core.Expressions;
using Reko.Gui;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Procedure_v1 = Reko.Core.Serialization.Procedure_v1;

namespace Reko.Gui.Design
{
    public class ProcedureDesigner : TreeNodeDesigner, IEquatable<ProcedureDesigner>
    {
        private Program program;
        private Procedure procedure;
        private string name;
        private Procedure_v1 userProc;

        public ProcedureDesigner(Program program, Procedure procedure, Procedure_v1 userProc, Address address)
        {
            base.Component = procedure;
            this.program = program;
            this.procedure = procedure;
            this.userProc = userProc;
            this.Address = address;
            if (userProc != null && !string.IsNullOrEmpty(userProc.Name))
                this.name = userProc.Name;
            else
                this.name = procedure.Name;
        }

        public Address Address { get; set; }

        public override void Initialize(object obj)
        {
            base.Initialize(obj);
            base.Component = obj;
            SetTreeNodeText();
        }

        private void SetTreeNodeText()
        {
            TreeNode.Text = name;
            TreeNode.ToolTipText = Address.ToString();
            TreeNode.ImageName = userProc != null ? "Userproc.ico" : "Procedure.ico";
        }

        public override void DoDefaultAction()
        {
            Services.RequireService<ICodeViewerService>().DisplayProcedure(procedure);
        }

        public override bool QueryStatus(CommandID cmdId, CommandStatus status, CommandText text)
        {
            if (cmdId.Guid == CmdSets.GuidReko)
            {
                switch (cmdId.ID)
                {
                case CmdIds.ViewGoToAddress:
                case CmdIds.ViewFindWhatPointsHere:
                case CmdIds.ActionEditSignature:
                case CmdIds.EditRename:
                    status.Status = MenuStatus.Visible | MenuStatus.Enabled;
                    return true;
                case CmdIds.ActionAssumeRegisterValues:
                    status.Status = MenuStatus.Visible | MenuStatus.Enabled;
                    return true;

                }
            }
            return false;
        }

        public override bool Execute(CommandID cmdId)
        {
            if (cmdId.Guid == CmdSets.GuidReko)
            {
                switch (cmdId.ID)
                {
                case CmdIds.ViewGoToAddress:
                    Services.RequireService<ILowLevelViewService>().ShowMemoryAtAddress(program, Address);
                    return true;
                case CmdIds.ActionEditSignature:
                    return true;
                case CmdIds.ActionAssumeRegisterValues:
                    AssumeRegisterValues();
                    return true;
                case CmdIds.ViewFindWhatPointsHere:
                    ViewWhatPointsHere();
                    return true;
                case CmdIds.EditRename:
                    Rename();
                    return true;
                }
            }
            return false;
        }

        private void ViewWhatPointsHere()
        {
            var resultSvc = Services.GetService<ISearchResultService>();
            if (resultSvc == null)
                return;
            var arch = program.Architecture;
            var image = program.Image;
            var rdr = program.Architecture.CreateImageReader(program.Image, 0);
            var addrControl = arch.CreatePointerScanner(
                program.ImageMap,
                rdr,
                new Address[]  { 
                    this.Address,
                },
                PointerScannerFlags.All);
            resultSvc.ShowSearchResults(new AddressSearchResult(Services, addrControl.Select(a => new AddressSearchHit(program, a))));
        }

        private void Rename()
        {
        }

        private void AssumeRegisterValues()
        {
            var dlgFactory = Services.RequireService<IDialogFactory>();
            var uiSvc = Services.RequireService<IDecompilerShellUiService>();
            using (var dlg = dlgFactory.CreateAssumedRegisterValuesDialog(program.Architecture))
            {
                dlg.Values = GetAssumedRegisterValues(Address);
                if (uiSvc.ShowModalDialog(dlg) != DialogResult.OK)
                    return;
                SetAssumedRegisterValues(Address, dlg.Values);
                SetTreeNodeText();
            }
        }

        private Dictionary<RegisterStorage, string> GetAssumedRegisterValues(Address Address)
        {
            Procedure_v1 up;
            if (!program.UserProcedures.TryGetValue(this.Address, out up))
                return new Dictionary<RegisterStorage, string>();
            return up.Assume
                .Select(ass => new
                {
                    Register = program.Architecture.GetRegister(ass.Register),
                    Value = ass.Value
                })
                .Where(ass => ass.Register != null)
                .ToDictionary(
                    ass => ass.Register,
                    ass => ass.Value);
        }

        private void SetAssumedRegisterValues(Address Address, Dictionary<RegisterStorage, string> dictionary)
        {
            userProc = program.EnsureUserProcedure(this.Address, procedure.Name);
            userProc.Assume = dictionary
                .Select(de => new Core.Serialization.RegisterValue_v2
                {
                    Register = de.Key.Name,
                    Value = de.Value
                })
                .ToArray();
        }

        void tv_BeforeLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return Address.ToLinear().GetHashCode();
        }

        public bool Equals(ProcedureDesigner other)
        {
            return Address.ToLinear() == other.Address.ToLinear();
        }
    }
}