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

#if !EDDIENET2
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace Eddie.Common
{
    public static class Utils
    {
        public static bool CompareStrings(string s1, string s2, int StopCompareAt)
        {
            int l1 = Empty(s1) ? 0 : s1.Length;
            int l2 = Empty(s2) ? 0 : s2.Length;
            if (l1 > StopCompareAt)
            {
                l1 = StopCompareAt;
                s1 = s1.Substring(0, l1);
            }
            if (l2 > StopCompareAt)
            {
                l2 = StopCompareAt;
                s2 = s2.Substring(0, l2);
            }
            if (l1 != l2)
                return false;
            return (s1 == s2);
        }

		public static string GetTempFile(string extension = null)
		{
			return GetTempFile(Path.GetTempPath(), extension);
		}

		public static string GetTempFile(string rootPath, string extension = null)
		{
			string filename = Guid.NewGuid().ToString();
			if(Utils.Empty(extension) == false)
				filename += "." + extension;

			return Path.Combine(rootPath, filename);
		}

		public static bool SafeDeleteFile(string filePath)
		{
			try
			{
				File.Delete(filePath);
				return true;
			}
			catch
			{

			}

			return false;
		}

		public static byte[] SafeReadAllBytes(string filename)
		{
			try
			{
				return File.ReadAllBytes(filename);
			}
			catch
			{

			}

			return null;
		}

		public static bool SafeWriteAllBytes(string filename, byte[] data)
		{
			try
			{
				File.WriteAllBytes(filename, data);
				return true;
			}
			catch
			{

			}

			return false;
		}

		public static byte[] SafeGetFileContent(string filename, bool allowCopy = true)
		{
			try
			{
				if(File.Exists(filename) == false)
					return null;

				return File.ReadAllBytes(filename);
			}
			catch
			{

			}

			if(allowCopy == false)
				return null;

			string tempFile = null;

			try
			{
				string tempFileName = GetTempFile();
				File.Copy(filename, tempFileName);
				tempFile = tempFileName;

				return File.ReadAllBytes(tempFile);
			}
			catch
			{

			}
			finally
			{
				if(tempFile != null)
					SafeDeleteFile(tempFile);
			}

			return null;
		}

		public static bool Empty(string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static string GetSafeString(string value)
        {
            return value != null ? value : "";
        }

        public static void CopyStream(Stream input, Stream output, int bufferLen = 4096)
        {
            byte[] buffer = new byte[bufferLen];
            int len;
            while((len = input.Read(buffer, 0, bufferLen)) > 0)
            {
                output.Write(buffer, 0, len);
            }

            output.Flush();
        }           

        public static bool Equals(byte[] b1, byte[] b2)
        {
            if(b1 == b2)
                return true;

            if((b1 != null) && (b2 != null))
            {
                if(b1.Length != b2.Length)
                    return false;
                
                for(int i = 0; i < b1.Length; i++)
                {
                    if(b1[i] != b2[i])
                        return false;
                }

                return true;
            }

            return false;
        }

        public static void Dispose(params IDisposable[] list)
        {
            for(int i = 0 ; i < list.Length ; i++)
            {
                if(list[i] != null)
                    list[i].Dispose();                
            }
        }

        public static string ToHex(byte[] data)
        {
            if(data == null)
                return null;
            
            return BitConverter.ToString(data).Replace("-", string.Empty);
        }

        public static byte[] FromHex(string data)
        {
            if(data == null)
                return null;
            
            return Enumerable.Range(0, data.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(data.Substring(x, 2), 16)).ToArray();
        }

        public static string ToBase64(byte[] data)
        {
            if(data == null)
                return null;
            
            return Convert.ToBase64String(data);
        }        

        public static byte[] FromBase64(string data)
        {
            if(data == null)
                return null;
            
            return Convert.FromBase64String(data);
        }        

        public static byte[] GetUTF8Bytes(string value)
        {
            if(value == null)
                return null;

            return System.Text.Encoding.UTF8.GetBytes(value);
        }

        public static T Min<T>(T left, T right)
        {
             return (Comparer<T>.Default.Compare(left, right) > 0) ? right : left;
        }

        public static T Max<T>(T left, T right)
        {
             return (Comparer<T>.Default.Compare(left, right) > 0) ? left : right;
        }

        public static T Clamp<T>(T value, T lowerBound, T upperBound)
        {
            return Max<T>(Min<T>(value, upperBound), lowerBound);
        }

		public static int RandomInt(int min, int max, int? seed = null)
		{
			Random r = seed != null ? new Random(seed.Value) : new Random();
			return r.Next(min, max);
		}

		public static double RandomDouble(double min, double max, int? seed = null)
		{
			Random r = seed != null ? new Random(seed.Value) : new Random();
			return r.NextDouble() * (max - min) + min;
		}

        public static bool ReferenceEquals<T, Y>(T first, T second) where T : IList<Y>, ICollection<Y>
        {
            if(first == null)
                return second == null;

            if(second == null)
                return false;

            int count = first.Count;
            if(count != second.Count)
                return false;

            for(int i = 0; i < count; i++)
            {
                if(object.ReferenceEquals(first[i], second[i]) == false)
                    return false;
            }

            return true;
        }

        public static bool FillStream(Stream stream, string s)
        {
			if(stream == null || s == null)
                return false;

			using(StreamWriter writer = new StreamWriter(stream))
			{
				writer.Write(s);
				writer.Flush();
			}

			stream.Position = 0;

            return true;
        }
		
        public static string LimitStringLength(string str, int length)
        {
            return (length >= 0) && (str != null) && (str.Length > length) ? str.Substring(0, length) : str;
        }

        public static string CapitalizeString(string str)
        {
            if(Empty(str))
                return str;
            
            char first = str[0];
            string tmp = "";
            tmp += first;
            return tmp.ToUpper() + str.Substring(1);            
        }

        public static string CombineStrings(string first, string second, string separator = " ")
        {
	        if(Empty(first))
		        return second;

	        if(Empty(second))
		        return first;

	        string result = first;
	        if(Empty(separator) == false)
	        {               
                if(result.EndsWith(separator) == false)
			        result += separator;
	        }

            result += second;
	        return result;
        }

		public static void Sleep(int milliseconds)
		{
			System.Threading.Thread.Sleep(milliseconds);
		}

		public static String GetExceptionDetails(Exception e)
		{
			String details = e.Message;

			StackTrace stack = new StackTrace(e);
			if(stack.FrameCount > 0)
			{
				StackFrame frame = stack.GetFrame(stack.FrameCount - 1);
				if(frame != null)
				{
					MethodBase method = frame.GetMethod();
					if(method != null)
					{                       
						string methodName = method.Name;
						if(Utils.Empty(methodName) == false)
						{
							string className = method.ReflectedType != null ? method.ReflectedType.FullName : null;
							if(Utils.Empty(className) == false)
								methodName = className + "." + methodName;

							if(Utils.Empty(details) == false)
								details += " ";

							string fileName = frame.GetFileName();
							int fileLine = frame.GetFileLineNumber();
							int fileColumn = frame.GetFileColumnNumber();

							string fileInfo = "";
							if((Utils.Empty(fileName) == false) || (fileLine != 0) || (fileColumn != 0))
								fileInfo = " at file:line:column " + String.Format("{0}:{1}:{2}", Utils.Empty(fileName) ? "?" : fileName, fileLine != 0 ? Convert.ToString(fileLine) : "?", fileColumn != 0 ? Convert.ToString(fileColumn) : "?");

							details += "(" + methodName + fileInfo + ")";
						}
					}
				}
			}

			return details;
		}

		public static string[] GetAssemblyResources(Assembly assembly = null)
		{
			if(assembly == null)
				assembly = Assembly.GetExecutingAssembly();

			return assembly.GetManifestResourceNames();
		}

		public static string GetResourceText(string name, Assembly assembly = null)
		{
			if(assembly == null)
				assembly = Assembly.GetExecutingAssembly();

			// This code could cause a double stream.Dispose() call
			/*
			using(Stream stream = assembly.GetManifestResourceStream(name))
			{
				using(StreamReader reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
			*/

			Stream stream = null;
			StreamReader reader = null;

			try
			{
				stream = assembly.GetManifestResourceStream(name);
				reader = new StreamReader(stream);

				return reader.ReadToEnd();
			}
			finally
			{
				if(reader != null)
					reader.Dispose();       // stream.Dispose() is called here
				else if(stream != null)
					stream.Dispose();       // StreamReader raised an exception and stream.Dispose() won't be called here
			}
		}
	}
}
#endif
