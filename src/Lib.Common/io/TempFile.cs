// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2019 AirVPN (support@airvpn.org) / https://airvpn.org
//
// Eddie is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Eddie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Eddie. If not, see <http://www.gnu.org/licenses/>.
// </eddie_source_header>

using System;
using System.IO;

namespace Eddie.Common.IO
{    
    public sealed class TempFile : IDisposable
    {
        private string m_filename;

        public TempFile() : this(Path.GetTempFileName())
        {

        }

        public TempFile(string filename)
        {
            if(string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");

            m_filename = filename;
        }

        ~TempFile()
        {
            Dispose(false);
        }

        public string Filename
        {
            get
            {
                return m_filename;
            }
        }
    
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if(disposing)
                GC.SuppressFinalize(this);                
            
            if(m_filename != null)
            {
                try
                {
                    File.Delete(m_filename);
                }
                catch
                {

                }
                finally
                {
                    m_filename = null;
                }
            }
        }
    }
}
