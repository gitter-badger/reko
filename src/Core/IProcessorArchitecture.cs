#region License
/* 
 * Copyright (C) 1999-2015 John K�ll�n.
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

using Reko.Core.Expressions;
using Reko.Core.Machine;
using Reko.Core.Rtl;
using Reko.Core.Serialization;
using Reko.Core.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using BitSet = Reko.Core.Lib.BitSet;

namespace Reko.Core
{
    /// <summary>
    /// Abstraction of a CPU architecture.
    /// </summary>
	public interface IProcessorArchitecture
	{
        /// <summary>
        /// Creates an IEnumerable of disassembled MachineInstructions which consumes 
        /// its input from the provided <paramref name="imageReader"/>.
        /// </summary>
        /// <remarks>This was previously an IEnumerator, but making it IEnumerable lets us use Linq expressions
        /// like Take().</remarks>
        /// <param name="imageReader"></param>
        /// <returns></returns>
        IEnumerable<MachineInstruction> CreateDisassembler(ImageReader imageReader);

        /// <summary>
        /// Creates an instance of a ProcessorState appropriate for this processor.
        /// </summary>
        /// <returns></returns>
		ProcessorState CreateProcessorState();

        /// <summary>
        /// Creates a BitSet large enough to fit all the registers.
        /// </summary>
        /// <returns></returns>
		BitSet CreateRegisterBitset();

        /// <summary>
        /// Returns a stream of machine-independent instructions, which it generates by successively disassembling
        /// machine-specific instructions and rewriting them into one or more machine-independent RtlInstructions codes. These are then 
        /// returned as clusters of RtlInstructions.
        /// </summary>
        IEnumerable<RtlInstructionCluster> CreateRewriter(ImageReader rdr, ProcessorState state, Frame frame, IRewriterHost host);

        /// <summary>
        /// Given a set of addresses, returns a set of address where something
        /// is referring to one of those addresses. The referent may be a
        /// machine instruction calling or jumping to the address, or a 
        /// reference to the address stored in memory.
        /// reference
        /// </summary>
        /// <param name="rdr"></param>
        /// <param name="knownAddresses"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        IEnumerable<Address> CreatePointerScanner(ImageMap map, ImageReader rdr, IEnumerable<Address> knownAddresses, PointerScannerFlags flags);

        /// <summary>
        /// Creates a Frame instance appropriate for this architecture type.
        /// </summary>
        /// <returns></returns>
        Frame CreateFrame();

        /// <summary>
        /// Creates an <see cref="ImageReader" /> with the preferred endianness of the processor.
        /// </summary>
        /// <param name="img">Program image to read</param>
        /// <param name="addr">Address at which to start</param>
        /// <returns>An imagereader of the appropriate endianness</returns>
        ImageReader CreateImageReader(LoadedImage img, Address addr);

        /// <summary>
        /// Creates an <see cref="ImageReader" /> with the preferred endianness of the processor.
        /// </summary>
        /// <param name="img">Program image to read</param>
        /// <param name="addr">offset from the start of the image</param>
        /// <returns>An imagereader of the appropriate endianness</returns>
        ImageReader CreateImageReader(LoadedImage img, ulong off);

        /// <summary>
        /// Creates a comparer that compares instructions for equality. Normalization means
        /// some attributes of the instruction are trated as wildcards.
        /// </summary>
        /// <param name="norm"></param>
        /// <returns></returns>
        IEqualityComparer<MachineInstruction> CreateInstructionComparer(Normalize norm);

        /// <summary>
        /// Creates a procedure serializer that understands the calling conventions used on this
        /// processor.
        /// </summary>
        /// <param name="typeLoader">Used to resolve data types</param>
        /// <param name="defaultConvention">Default calling convention, if none specified.</param>
        ProcedureSerializer CreateProcedureSerializer(ISerializedTypeVisitor<DataType> typeLoader, string defaultConvention);

		RegisterStorage GetRegister(int i);			        // Returns register corresponding to number i.
		RegisterStorage GetRegister(string name);	        // Returns register whose name is 'name'
        RegisterStorage[] GetRegisters();                   // Returns all registers of this architecture.
        bool TryGetRegister(string name, out RegisterStorage reg); // Attempts to find a register with name <paramref>name</paramref>
        FlagGroupStorage GetFlagGroup(uint grf);		    // Returns flag group matching the bitflags.
		FlagGroupStorage GetFlagGroup(string name);
        Expression CreateStackAccess(Frame frame, int cbOffset, DataType dataType);
        Address ReadCodeAddress(int size, ImageReader rdr, ProcessorState state);

        string GrfToString(uint grf);                       // Converts a union of processor flag bits to its string representation

        PrimitiveType FramePointerType { get; }             // Size of a pointer into the stack frame (near pointer in x86 real mode)
        PrimitiveType PointerType { get; }                  // Pointer size that reaches anywhere in the address space (far pointer in x86 real mode )
		PrimitiveType WordWidth { get; }					// Processor's native word size
        int InstructionBitSize { get; }                     // Instruction "granularity" or alignment.
        RegisterStorage StackRegister { get; }              // Stack pointer used by this machine.
        uint CarryFlagMask { get; }                         // Used when building large adds/subs when carry flag is used.
        string Description { get; set; }                    // Typically loaded from app.config

        /// <summary>
        /// Parses an address according to the preferred base of the architecture.
        /// </summary>
        /// <param name="txtAddr"></param>
        /// <param name="addr"></param>
        /// <returns></returns>
        bool TryParseAddress(string txtAddr, out Address addr);

        Address MakeAddressFromConstant(Constant c);

    }

    /// <summary>
    /// Normalize enumeration controls the operation of instruction comparer. 
    /// </summary>
    [Flags]
    public enum Normalize
    {
        Nothing,        // Match identically
        Constants,      // all constants treated as wildcards
        Registers,      // all registers treated as wildcards.
    }

    [Designer("Reko.Gui.Design.ArchitectureDesigner,Reko.Gui")]
    public abstract class ProcessorArchitecture : IProcessorArchitecture
    {
        public string Description {get; set; }
        public PrimitiveType FramePointerType { get; protected set; }
        public PrimitiveType PointerType { get; protected set; }
        public PrimitiveType WordWidth { get; protected set; }
        public int InstructionBitSize { get; protected set; }
        public RegisterStorage StackRegister { get; protected set; }
        public uint CarryFlagMask { get; protected set; }

        public abstract IEnumerable<MachineInstruction> CreateDisassembler(ImageReader imageReader);
        public Frame CreateFrame() { return new Frame(FramePointerType); }
        public abstract ImageReader CreateImageReader(LoadedImage img, Address addr);
        public abstract ImageReader CreateImageReader(LoadedImage img, ulong off);
        public abstract IEqualityComparer<MachineInstruction> CreateInstructionComparer(Normalize norm);
        public abstract ProcedureSerializer CreateProcedureSerializer(ISerializedTypeVisitor<DataType> typeLoader, string defaultConvention);
        public abstract ProcessorState CreateProcessorState();
        public abstract IEnumerable<Address> CreatePointerScanner(ImageMap map, ImageReader rdr, IEnumerable<Address> knownAddresses, PointerScannerFlags flags);
        public abstract BitSet CreateRegisterBitset();
        public abstract IEnumerable<RtlInstructionCluster> CreateRewriter(ImageReader rdr, ProcessorState state, Frame frame, IRewriterHost host);
        public abstract Expression CreateStackAccess(Frame frame, int cbOffset, DataType dataType);

        public abstract RegisterStorage GetRegister(int i);
        public abstract RegisterStorage GetRegister(string name);
        public abstract RegisterStorage[] GetRegisters();
        public abstract bool TryGetRegister(string name, out RegisterStorage reg);
        public abstract FlagGroupStorage GetFlagGroup(uint grf);
        public abstract FlagGroupStorage GetFlagGroup(string name);
        public abstract string GrfToString(uint grf);
        public abstract Address MakeAddressFromConstant(Constant c);
        public abstract Address ReadCodeAddress(int size, ImageReader rdr, ProcessorState state);
        public abstract bool TryParseAddress(string txtAddr, out Address addr);
    }
}
