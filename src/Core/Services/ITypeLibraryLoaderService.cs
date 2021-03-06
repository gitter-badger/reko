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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Reko.Core.Services
{
    public interface ITypeLibraryLoaderService
    {
        TypeLibrary LoadLibrary(IPlatform platform, string name, TypeLibrary libDst);

        string InstalledFileLocation(string name);

        /// <summary>
        /// Loads the characteristics library for the dynamic library
        /// named 'name'.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        CharacteristicsLibrary LoadCharacteristics(string name);
    }

    public class TypeLibraryLoaderServiceImpl : ITypeLibraryLoaderService
    {
        private IServiceProvider services;

        public TypeLibraryLoaderServiceImpl(IServiceProvider services)
        {
            this.services = services;
        }
        
        //$REFACTOR: needs a better name.
        public TypeLibrary LoadLibrary(IPlatform platform, string name, TypeLibrary dstLib)
        {
            try
            {
                string libFileName = ImportFileLocation(name);
                if (!File.Exists(libFileName))
                    return dstLib;

                byte[] bytes;
                var fsSvc = services.RequireService<IFileSystemService>();
                using (var stm = fsSvc.CreateFileStream(libFileName, FileMode.Open, FileAccess.Read))
                {
                    bytes = new Byte[stm.Length];
                    stm.Read(bytes, 0, (int)stm.Length);
                }
                var tlldr = new TypeLibraryLoader(services, libFileName, bytes);
                var lib = tlldr.Load(platform, dstLib);
                return lib;
            }
            catch
            {
                return dstLib;
            }
        }

        public CharacteristicsLibrary LoadCharacteristics(string name)
        {
            var filename = InstalledFileLocation(name);
            if (!File.Exists(filename))
                return new CharacteristicsLibrary();
            var fsSvc = services.RequireService<IFileSystemService>();
            var lib = CharacteristicsLibrary.Load(filename, fsSvc);
            return lib;
        }

        public string InstalledFileLocation(string name)
        {
            string assemblyDir = Path.GetDirectoryName(GetType().Assembly.Location);
            return Path.Combine(assemblyDir, name);
        }

        public string ImportFileLocation(string dllName)
        {
            string assemblyDir = Path.GetDirectoryName(GetType().Assembly.Location);
            return Path.Combine(assemblyDir, Path.ChangeExtension(dllName, ".xml"));
        }
    }
}
