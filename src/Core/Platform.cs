#region License
/* 
 * Copyright (C) 1999-2016 John K�ll�n.
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

using Reko.Core.CLanguage;
using Reko.Core.Configuration;
using Reko.Core.Expressions;
using Reko.Core.Lib;
using Reko.Core.Rtl;
using Reko.Core.Serialization;
using Reko.Core.Services;
using Reko.Core.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Reko.Core
{
    /// <summary>
    /// A Platform is an abstraction of the operating environment,
    /// say MS-DOS, Win32, or Posix.
    /// </summary>
    public interface IPlatform
    {
        IProcessorArchitecture Architecture { get; }
        string DefaultCallingConvention { get; }
        Encoding DefaultTextEncoding { get; }
        string Description { get; }
        PrimitiveType FramePointerType { get; }
        PlatformHeuristics Heuristics { get; }
        string Name { get; }
        string PlatformIdentifier { get; }
        PrimitiveType PointerType { get; }

        Address AdjustProcedureAddress(Address addrCode);
        HashSet<RegisterStorage> CreateImplicitArgumentRegisters();
        IEnumerable<Address> CreatePointerScanner(ImageMap imageMap, ImageReader rdr, IEnumerable<Address> address, PointerScannerFlags pointerScannerFlags);
        ProcedureSerializer CreateProcedureSerializer(ISerializedTypeVisitor<DataType> typeLoader, string defaultConvention);
        TypeLibrary CreateMetadata();

        /// <summary>
        /// Given a C basic type, returns the number of bytes that type is
        /// represented with on this platform.
        /// </summary>
        /// <param name="cb">A C Basic type, like int, float etc.</param>
        /// <returns>Number of bytes used by this platform.
        /// </returns>
        int GetByteSizeFromCBasicType(CBasicType cb);

        ProcedureBase GetTrampolineDestination(ImageReader imageReader, IRewriterHost host);
        SystemService FindService(int vector, ProcessorState state);
        SystemService FindService(RtlInstruction call, ProcessorState state);
        void LoadUserOptions(Dictionary<string, object> options);
        ExternalProcedure LookupProcedureByName(string moduleName, string procName);
        ExternalProcedure LookupProcedureByOrdinal(string moduleName, int ordinal);
        Address MakeAddressFromConstant(Constant c);
        Address MakeAddressFromLinear(ulong uAddr);
        Dictionary<string, object> SaveUserOptions();
        ProcedureSignature SignatureFromName(string importName);
    }

    /// <summary>
    /// Implementation of functionality common to most platforms.
    /// </summary>
    [Designer("Reko.Gui.Design.PlatformDesigner,Reko.Gui")]
	public abstract class Platform : IPlatform
	{ 
        /// <summary>
        /// Initializes a Platform instance
        /// </summary>
        /// <param name="arch"></param>
        protected Platform(IServiceProvider services, IProcessorArchitecture arch, string platformId) 
        {
            this.Services = services;
            this.Architecture = arch;
            this.PlatformIdentifier = platformId;
            this.Heuristics = new PlatformHeuristics();
        }

        public IProcessorArchitecture Architecture { get; private set; }
        public IServiceProvider Services { get; private set; }
        public virtual TypeLibrary Metadata { get; protected set; }
        public CharacteristicsLibrary[] CharacteristicsLibs { get; protected set; }
        public string Description { get; set; }
        public PlatformHeuristics Heuristics { get; private set; }
        public string Name { get; set; }
        public virtual PrimitiveType FramePointerType { get { return Architecture.FramePointerType; } }
        public virtual PrimitiveType PointerType { get { return Architecture.PointerType; } }

        /// <summary>
        /// String identifier used by Reko to locate platform-specfic information from the 
        /// app.config file.
        /// </summary>
        public string PlatformIdentifier { get; private set; }

        /// <summary>
        /// The default encoding for byte-encoded text.
        /// </summary>
        /// <remarks>
        /// We use ASCII as the lowest common denominator here, but some arcane platforms (e.g.
        /// ZX-81) don't use ASCII.
        /// </remarks>
        public virtual Encoding DefaultTextEncoding { get { return Encoding.ASCII; } }

        public abstract string DefaultCallingConvention { get; }

        /// <summary>
        /// Some architectures platforms (I'm looking at you ARM Thumb) will use addresses
        /// that are offset by 1. Most don't.
        /// </summary>
        /// <param name="addr"></param>
        /// <returns>Adjusted address</returns>
        public virtual Address AdjustProcedureAddress(Address addr)
        {
            return addr;
        }

        /// <summary>
        /// Creates a bitset that represents those registers that are never used as arguments to a 
        /// procedure. 
        /// </summary>
        /// <remarks>
        /// Typically, the stack pointer register is one of these registers. Some architectures define
        /// global registers that are preserved across calls; these should also be present in this set.
        /// </remarks>
        public abstract HashSet<RegisterStorage> CreateImplicitArgumentRegisters();

        public IEnumerable<Address> CreatePointerScanner(
            ImageMap imageMap,
            ImageReader rdr,
            IEnumerable<Address> address,
            PointerScannerFlags pointerScannerFlags)
        {
            return Architecture.CreatePointerScanner(imageMap, rdr, address, pointerScannerFlags);
        }

        public TypeLibrary CreateMetadata()
        {
            EnsureTypeLibraries(PlatformIdentifier);
            return Metadata.Clone();
        }

        /// <summary>
        /// Creates a procedure serializer that understands the calling conventions used on this
        /// processor and environment
        /// </summary>
        /// <param name="typeLoader">Used to resolve data types</param>
        /// <param name="defaultConvention">Default calling convention, if none specified.</param>
        public abstract ProcedureSerializer CreateProcedureSerializer(ISerializedTypeVisitor<DataType> typeLoader, string defaultConvention);

        /// <summary>
        /// Utility function for subclasses that loads all type libraries and characteristics libraries 
        /// </summary>
        /// <param name="envName"></param>
        public virtual void EnsureTypeLibraries(string envName)
        {
            if (Metadata == null)
            {
                var cfgSvc = Services.RequireService<IConfigurationService>();
                var envCfg = cfgSvc.GetEnvironment(envName);
                if (envCfg == null)
                    throw new ApplicationException(string.Format(
                        "Environment '{0}' doesn't appear in the configuration file. Your installation may be out-of-date.",
                        envName));
                var tlSvc = Services.RequireService<ITypeLibraryLoaderService>();
                this.Metadata = new TypeLibrary();
                foreach (var tl in envCfg.TypeLibraries
                    .OfType<ITypeLibraryElement>())
                {
                    Debug.Print("Loading {0}", tl.Name);
                    Metadata = tlSvc.LoadLibrary(this, cfgSvc.GetInstallationRelativePath(tl.Name), Metadata);
                }
                this.CharacteristicsLibs = ((System.Collections.IEnumerable)envCfg.CharacteristicsLibraries)
                    .OfType<ITypeLibraryElement>()
                    .Select(cl => tlSvc.LoadCharacteristics(cl.Name))
                    .Where(cl => cl != null).ToArray();
            }
        }

        public abstract int GetByteSizeFromCBasicType(CBasicType cb);

        public abstract SystemService FindService(int vector, ProcessorState state);

        public virtual SystemService FindService(RtlInstruction rtl, ProcessorState state)
        {
            return null;
        }

        public virtual SystemService FindService(string name)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// If the instructions located at the address the image reader is reading are a 
        /// trampoline, returns the procedure where the destination is located, otherwise
        /// returns null.
        /// </summary>
        /// <param name="imageReader"></param>
        /// <returns></returns>
        public abstract ProcedureBase GetTrampolineDestination(ImageReader imageReader, IRewriterHost host);

        public virtual Address MakeAddressFromConstant(Constant c)
        {
            return Architecture.MakeAddressFromConstant(c);
        }

        /// <summary>
        /// Given a linear address, converts it to an Address instance. By default,
        /// use the architecture pointer size for the address.
        /// </summary>
        /// <remarks>
        /// The method is virtual to allow a platform to override the pointer size. For instance
        /// although the PowerPC 64 has 64-bit addresses, the Playstation3 implementation 
        /// uses 32-bit addresses.
        /// </remarks>
        /// <param name="uAddr"></param>
        /// <returns></returns>
        public virtual Address MakeAddressFromLinear(ulong uAddr)
        {
            return Address.Create(Architecture.PointerType, uAddr);
        }

        public abstract ExternalProcedure LookupProcedureByName(string moduleName, string procName);

        /// <summary>
        /// If the platform can be customized by user, load those customizations here.
        /// </summary>
        /// <returns>The options serialized as strings, or lists of objects, or dictionaries of objects.</returns>
        public virtual void LoadUserOptions(Dictionary<string, object> options) { }

        /// <summary>
        /// If the platform can be customized by user, save those customizations here.
        /// </summary>
        /// <returns>The options serialized as strings, or lists of objects, or dictionaries of objects.</returns>
        public virtual Dictionary<string, object> SaveUserOptions() { return null; }
        
        /// <summary>
        /// Guess signature from the name of the procedure.
        /// </summary>
        /// <param name="fnName"></param>
        /// <returns>null if there is no way to guess a ProcedureSignature from the name.</returns>
        public virtual ProcedureSignature SignatureFromName(string fnName)
        {
            return null;
        }

        public virtual ExternalProcedure LookupProcedureByOrdinal(string moduleName, int ordinal)
        {
            return null;
        }
    }

    /// <summary>
    /// The default platform is used when a specific platform cannot be determined.
    /// </summary>
    /// <remarks>
    /// "All the world's a VAX"  -- not Henry Spencer
    /// </remarks>
    public class DefaultPlatform : Platform
    {
        public DefaultPlatform(IServiceProvider services, IProcessorArchitecture arch) : base(services, arch, "default")
        {
            this.TypeLibraries = new List<TypeLibrary>();
            this.Description = "(Unknown operating environment)";
        }

        public List<TypeLibrary> TypeLibraries { get; private set; }

        public override string DefaultCallingConvention
        {
            get { return ""; }
        }

        public override HashSet<RegisterStorage> CreateImplicitArgumentRegisters()
        {
            return new HashSet<RegisterStorage>();
        }

        public override ProcedureSerializer CreateProcedureSerializer(ISerializedTypeVisitor<DataType> typeLoader, string defaultConvention)
        {
            throw new NotSupportedException();
        }

        public override SystemService FindService(int vector, ProcessorState state)
        {
            throw new NotSupportedException();
        }

        public override int GetByteSizeFromCBasicType(CBasicType cb)
        {
            switch (cb)
            {
            case CBasicType.Char: return 1;
            case CBasicType.WChar_t: return 2;
            case CBasicType.Short: return 2;
            case CBasicType.Int: return 4;      // Assume 32-bit int.
            case CBasicType.Long: return 4;
            case CBasicType.LongLong: return 8;
            case CBasicType.Float: return 4;
            case CBasicType.Double: return 8;
            case CBasicType.LongDouble: return 8;
            case CBasicType.Int64: return 8;
            default: throw new NotImplementedException(string.Format("C basic type {0} not supported.", cb));
            }
        }
        public override ProcedureBase GetTrampolineDestination(ImageReader imageReader, IRewriterHost host)
        {
            // No trampolines are supported.
            return null;
        }

        public override ExternalProcedure LookupProcedureByName(string moduleName, string procName)
        {
            //$Identical to Win32, move into base class?
            return TypeLibraries
                .Select(t => t.Lookup(procName))
                .Where(sig => sig != null)
                .Select(sig => new ExternalProcedure(procName, sig))
                .FirstOrDefault();
        }
    }
}
