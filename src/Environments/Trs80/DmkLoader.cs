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

using Reko.Arch.Z80;
using Reko.Core;
using Reko.Environments.Trs80.Dmk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reko.Environments.Trs80
{
    public class DmkLoader : ImageLoader
    {
        private int TrackLength;
        private bool m_singleDensityOnly;
        private bool m_mixedDensity;

        public DmkLoader(IServiceProvider services, string filename, byte[] rawBytes)
            : base(services, filename, rawBytes)
        {
        }

        public override Address PreferredBaseAddress
        {
            get
            {
                return Address.Ptr16(0x4000);
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override Program Load(Address addrLoad)
        {
            if (!ParseDMKHeader())
                return null;

            var tracks = BuildTrackList(TrackLength);
            var bytes = tracks.SelectMany(t => t.Sectors)
                .SelectMany(s => s.GetData())
                .ToArray();
            var image = new LoadedImage(addrLoad, bytes);
            return new Program
            {
                Architecture = new Z80ProcessorArchitecture(),
                Image = image,
                ImageMap = image.CreateImageMap(),
            };
        }

        private List<Track> BuildTrackList(int trackLength)
        {
            List<Track> listToProcess = new List<Track>();
            var rawDMK = RawImage;
            listToProcess.Clear();
            if (trackLength == 0)
            {
                return listToProcess;
            }
            int i = 16;
            int num = rawDMK.Length;
            while (i < num)
            {
                if (num - i <= 128)
                {
                    return listToProcess;
                }
                Track track = new Track();
                if (this.m_singleDensityOnly || this.m_mixedDensity)
                {
                    track.twoByteSingleDensity = false;
                }
                track.setHeader(rawDMK, i);
                i += 128;
                int num2 = num - i;
                if (num2 > trackLength - 128)
                {
                    num2 = trackLength - 128;
                }
                track.setData(rawDMK, i, num2);
                track.ParseSectors();
                i += num2;
                listToProcess.Add(track);
            }
            return listToProcess;
        }

        private bool ParseDMKHeader()
        {
            if (this.RawImage == null)
            {
                Debug.Print("Empty file!");
                return false;
            }
            this.TrackLength = 0;
            if (this.RawImage.Length < 16)
            {
                return false;
            }
            byte b = this.RawImage[0];
            if (b != 0)
            {
                if (b != 255)
                {
                    Debug.Print(" Unknown read/write flag:" + this.RawImage[0].ToString("X"));
                }
                else
                {
                    Debug.Print(" Read Only");
                }
            }
            else
            {
                Debug.Print( " Read/Write");
            }
            this.TrackLength = Convert.ToInt32(this.RawImage[2]) + (Convert.ToInt32(this.RawImage[3]) << 8);
            Debug.Print(" Track Length: {0}", TrackLength);
            if (this.TrackLength > 10560 || this.TrackLength < 3264)
            {
                Debug.Print(" Invalid track length:" + this.TrackLength.ToString());
                return false;
            }
            int trackCount = (this.RawImage.Length - 16) / this.TrackLength;
            if ((this.RawImage[4] & 16) == 16)
            {
                Debug.Print("Single sided only ");
            }
            if ((this.RawImage[4] & 64) == 64)
            {
                Debug.Print("Single Density only ");
                this.m_singleDensityOnly = true;
            }
            if ((this.RawImage[4] & 128) == 128)
            {
                Debug.Print(" Mixed Density (older format)");
                this.m_mixedDensity = true;
            }
            return true;
        }

        public override RelocationResults Relocate(Program program, Address addrLoad)
        {
            return new RelocationResults(new List<EntryPoint>(), new RelocationDictionary(), new List<Address>());
        }
    }
}
