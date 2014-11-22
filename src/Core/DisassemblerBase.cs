#region License
/* 
 * Copyright (C) 1999-2014 John K�ll�n.
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

using Decompiler.Core.Machine;
using System;
using System.Collections.Generic;

namespace Decompiler.Core
{
    /// <summary>
    /// A disassembler can be considered an enumerator of disassembled instructions.
    /// </summary>
    /// <typeparam name="TInstr"></typeparam>
    public abstract class DisassemblerBase<TInstr> : IEnumerator<TInstr>
    {
        public abstract TInstr Current { get; }

        object System.Collections.IEnumerator.Current { get { return Current; } }

        public void Dispose() { }

        public abstract bool MoveNext();

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
}