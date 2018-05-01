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
