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
using Reko.Core.CLanguage;
using Reko.Core.Code;
using Reko.Core.Expressions;
using Reko.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Reko.Analysis
{
    /// <summary>
    /// Builds ProcedureSignatures from user-supplied signatures.
    /// </summary>
    public class UserSignatureBuilder
    {
        private Program program;

        public UserSignatureBuilder(Program program)
        {
            this.program = program;
        }

        /// <summary>
        /// For each user-supplied signature, locate a scanned procedure
        /// and apply the signature to it.
        /// </summary>
        public void BuildSignatures()
        {
            foreach (var de in program.User.Procedures)
            {
                Procedure proc;
                if (!program.Procedures.TryGetValue(de.Key, out proc))
                    continue;
                var sProc = DeserializeSignature(de.Value, proc);
                if (sProc == null)
                    continue;
                var ser = program.CreateProcedureSerializer();
                var sig = ser.Deserialize(sProc.Signature, proc.Frame);
                if (sig != null)
                {
                    proc.Name = sProc.Name;
                    ApplySignatureToProcedure(de.Key, sig, proc);
                }
            }
        }

        public ProcedureBase_v1 DeserializeSignature(Procedure_v1 userProc, Procedure proc)
        {
            if (!string.IsNullOrEmpty(userProc.CSignature))
            {
                return ParseFunctionDeclaration(userProc.CSignature, proc.Frame);
            }
            return null;
        }

        public void ApplySignatureToProcedure(Address addr, ProcedureSignature sig, Procedure proc)
        {
            proc.Signature = sig;

            int i = 0;
            var stmts = proc.EntryBlock.Succ[0].Statements;
            var linAddr = addr.ToLinear();
            foreach (var param in sig.Parameters)
            {
                var starg = param.Storage as StackArgumentStorage;
                if (starg != null)
                {
                    proc.Frame.EnsureStackArgument(
                        starg.StackOffset + sig.ReturnAddressOnStack,
                        param.DataType,
                        param.Name);
                }
                else
                {
                    var paramId = proc.Frame.EnsureIdentifier(param.Storage);
                    paramId.DataType = param.DataType;

                    // Need to take an extra step with parameters being passed
                    // in a register. It's perfectly possible for a user to 
                    // create a variable which they want to call 'r2' but which
                    // the calling convention of the machine wants to call 'r1'.
                    // To avoid this, we create a temporary identifier for 
                    // the formal parameter, and inject an copy statement in the
                    // entry block that moves the parameter value into the 
                    // register.
                    stmts.Insert(i, linAddr, NewMethod(param, paramId));
                    ++i;
                }
            }
        }

        private static Assignment NewMethod(Identifier param, Identifier dst)
        {
            return new Assignment(dst, param);
        }

        public ProcedureBase_v1 ParseFunctionDeclaration(string fnDecl, Frame frame)
        {
            try {
                var lexer = new CLexer(new StringReader(fnDecl + ";"));
                var symbols = program.CreateSymbolTable();
                var oldProcs = symbols.Procedures.Count;
                var cstate = new ParserState(symbols.NamedTypes.Keys);
                var cParser = new CParser(cstate, lexer);
                var decl = cParser.Parse_ExternalDecl();
                if (decl == null)
                    return null;

                //$HACK: Relying on a side effect here to
                // get both the procedure name and the signature. Ew.
                symbols.AddDeclaration(decl);
                if (symbols.Procedures.Count == oldProcs)
                    return null;
                return symbols.Procedures.Last();
            }
            catch (Exception ex)
            {
                //$TODO: if user has supplied a signature that can't parse,
                // we must notify them in the diagnostics window with a 
                // WARNING.
                Debug.Print("{0}\r\n{1}", ex.Message, ex.StackTrace);
                return null;
            }
        }
    }
}
