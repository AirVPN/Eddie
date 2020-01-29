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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Lib.Platform.Windows.Elevated
{
	public static class Utils
	{
		public static void LogDebug(string msg)
		{			
			try
			{
				if (Constants.Debug)
				{
					string log = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", System.Globalization.CultureInfo.InvariantCulture) + " - " + msg;
					string pathLog = System.IO.Path.GetTempPath() + "eddie-elevated.log";
					System.IO.File.AppendAllText(pathLog, log + "\n");
					Console.WriteLine(log);
				}
			}
			catch
			{

			}
		}

		public static bool IsAdministrator()
		{
			WindowsIdentity identity = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new WindowsPrincipal(identity);
			return principal.IsInRole(WindowsBuiltInRole.Administrator);
		}

		public static Process GetParentProcess()
		{
			int iParentPid = 0;
			int iCurrentPid = Process.GetCurrentProcess().Id;

			IntPtr oHnd = NativeMethods.CreateToolhelp32Snapshot(NativeMethods.TH32CS_SNAPPROCESS, 0);

			if (oHnd == IntPtr.Zero)
				return null;

			NativeMethods.PROCESSENTRY32 oProcInfo = new NativeMethods.PROCESSENTRY32();

			oProcInfo.dwSize =
			(uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMethods.PROCESSENTRY32));

			if (NativeMethods.Process32First(oHnd, ref oProcInfo) == false)
				return null;

			do
			{
				if (iCurrentPid == oProcInfo.th32ProcessID)
					iParentPid = (int)oProcInfo.th32ParentProcessID;
			}
			while (iParentPid == 0 && NativeMethods.Process32Next(oHnd, ref oProcInfo));

			if (iParentPid > 0)
				return Process.GetProcessById(iParentPid);
			else
				return null;
		}

		public static Process GetProcessOfMatchingIPEndPoint(System.Net.IPEndPoint localEndpoint, System.Net.IPEndPoint remoteEndpoint)
		{
			int bufferSize = 0;
			
			// Getting the size of TCP table, that is returned in 'bufferSize' variable. 
			uint result = NativeMethods.GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, NativeMethods.AF_INET, NativeMethods.TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);

			// Allocating memory from the unmanaged memory of the process by using the 
			// specified number of bytes in 'bufferSize' variable. 
			IntPtr tcpTableRecordsPtr = Marshal.AllocHGlobal(bufferSize);

			try
			{
				// The size of the table returned in 'bufferSize' variable in previous 
				// call must be used in this subsequent call to 'GetExtendedTcpTable' 
				// function in order to successfully retrieve the table. 
				result = NativeMethods.GetExtendedTcpTable(tcpTableRecordsPtr, ref bufferSize, true, NativeMethods.AF_INET, NativeMethods.TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);

				// Non-zero value represent the function 'GetExtendedTcpTable' failed, 
				// hence empty list is returned to the caller function. 
				if (result != 0)
					return null;

				// Marshals data from an unmanaged block of memory to a newly allocated 
				// managed object 'tcpRecordsTable' of type 'MIB_TCPTABLE_OWNER_PID' 
				// to get number of entries of the specified TCP table structure. 
				NativeMethods.MIB_TCPTABLE_OWNER_PID tcpRecordsTable = (NativeMethods.MIB_TCPTABLE_OWNER_PID)
										Marshal.PtrToStructure(tcpTableRecordsPtr,
										typeof(NativeMethods.MIB_TCPTABLE_OWNER_PID));
				IntPtr tableRowPtr = (IntPtr)((long)tcpTableRecordsPtr +
										Marshal.SizeOf(tcpRecordsTable.dwNumEntries));

				for (int row = 0; row < tcpRecordsTable.dwNumEntries; row++)
				{
					NativeMethods.MIB_TCPROW_OWNER_PID tcpRow = (NativeMethods.MIB_TCPROW_OWNER_PID)Marshal.
						PtrToStructure(tableRowPtr, typeof(NativeMethods.MIB_TCPROW_OWNER_PID));

					System.Net.IPAddress localAddr = new System.Net.IPAddress(tcpRow.localAddr);
					System.Net.IPAddress remoteAddr = new System.Net.IPAddress(tcpRow.remoteAddr);
					UInt16 localPort = BitConverter.ToUInt16(new byte[2] { tcpRow.localPort[1], tcpRow.localPort[0] }, 0);
					UInt16 remotePort = BitConverter.ToUInt16(new byte[2] { tcpRow.remotePort[1], tcpRow.remotePort[0] }, 0);
				
					if ((localEndpoint.Address.ToString() == localAddr.ToString()) &&
						(localEndpoint.Port == localPort) &&
						(remoteEndpoint.Address.ToString() == remoteAddr.ToString()) &&
						(remoteEndpoint.Port == remotePort))
					{
						int pid = tcpRow.owningPid;
						return Process.GetProcessById(pid);
					}
					
					tableRowPtr = (IntPtr)((long)tableRowPtr + Marshal.SizeOf(tcpRow));
				}
			}
			catch (OutOfMemoryException)
			{
				return null;
			}
			catch (Exception)
			{
				return null;
			}
			finally
			{
				Marshal.FreeHGlobal(tcpTableRecordsPtr);
			}

			return null;
		}

		public static string LocateExecutable(string name)
		{
			return name;
		}

		public static string Shell(string path, string args)
		{
			string stdout = "";
			string stderr = "";
			int result = Shell(path, args, "", out stdout, out stderr);
			string output = stdout + "\n" + stderr;
			return output.Trim();
		}

		public static int Shell(string path, string[] args, string autoWriteStdin, out string stdout, out string stderr)
		{
			return Shell(path, String.Join(" ", args), autoWriteStdin, out stdout, out stderr);
		}

		public static int Shell(string path, string args, string autoWriteStdin, out string stdout, out string stderr)
		{
			Process process = new Process();
			process.StartInfo.FileName = path;
			process.StartInfo.Arguments = args;
			process.StartInfo.WorkingDirectory = "";
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			if (autoWriteStdin != "")
				process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.StandardErrorEncoding = Encoding.UTF8; // 2.11.10
			process.StartInfo.StandardOutputEncoding = Encoding.UTF8; // 2.11.10

			process.Start();

			if (autoWriteStdin != "")
			{
				process.StandardInput.Write(autoWriteStdin);
				process.StandardInput.Close();
			}

			stdout = process.StandardOutput.ReadToEnd();
			stderr = process.StandardError.ReadToEnd();

			process.WaitForExit();

			int code = process.ExitCode;

			process.Close();

			process.Dispose();

			return code;
		}

		public static string StringPruneCharsNotIn(string value, string chars)
		{
			// Only paranoic about bug/design, because we presume the arguments already come from a trusted source.
			string result = "";
			foreach (char c in value.ToCharArray())
				if (chars.IndexOf(c) != -1)
					result += c;
			return result;
		}

		public static string EnsureStringCidr(string value)
		{
			return StringPruneCharsNotIn(value.ToLowerInvariant(), "0123456789abcdef.:/");
		}

		public static string EnsureStringIpAddress(string value)
		{
			return StringPruneCharsNotIn(value.ToLowerInvariant(), "0123456789abcdef.:");
		}

		public static string EnsureStringNumericInt(string value)
		{
			return StringPruneCharsNotIn(value.ToLowerInvariant(), "-0123456789");
		}

		public static string EscapeStringForInsideQuote(string value)
		{
			// Note: Used only in already-quoted (double-quote).

			// Note: Must be improved, but at least Windows it's a mess.
			// For the moment, simply remove. Will be an issue only in rare cases.
			// Look for reference https://blogs.msdn.microsoft.com/twistylittlepassagesallalike/2011/04/23/everyone-quotes-command-line-arguments-the-wrong-way/
			// Advise /check these chars where it's used. Look EscapeInsideQuoteAcceptable
			foreach (char c in Path.GetInvalidPathChars())
				value = value.Replace(c, '-');
			value = value.Replace("'", "-");
			value = value.Replace("`", "-");
			value = value.Replace("\"", "-");
			value = value.Replace("%", "-");
			value = value.Replace("!", "-"); // Delayed variable substitution
			value = value.Replace("$", "-");
			return value;
		}

		// Not yet used
		/*
		public static string HashSHA256(string password)
		{
			using (System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed())
			{
				System.Text.StringBuilder hash = new System.Text.StringBuilder();
				byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
				foreach (byte theByte in crypto)
				{
					hash.Append(theByte.ToString("x2"));
				}
				return hash.ToString();
			}
		}
		*/

		public static string HashSHA512File(string path)
		{
			using (FileStream stream = File.OpenRead(path))
			{
				using (SHA512 sha = SHA512.Create())
				{
					byte[] hash = sha.ComputeHash(stream);

					string o = "";
					for (int i = 0; i < hash.Length; i++)
					{
						o += String.Format("{0:X2}", hash[i]);
					}
					return o.ToLowerInvariant();
				}
			}
		}

		/*
		public static void CreateEddieSignature(string path)
		{
			RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
			string privateKey = File.ReadAllText("r:\\teste\\eddie-file-private-unencrypted.pem.xml");
			RSA.FromXmlString(privateKey);

			FileStream SignFile = new FileStream(path, FileMode.Open, FileAccess.Read);
			BinaryReader SignFileReader = new BinaryReader(SignFile);
			byte[] SignFileData = SignFileReader.ReadBytes((int)SignFile.Length);

			File.WriteAllBytes(path + ".eddie-sig-net", RSA.SignData(SignFileData, "SHA512"));
		}

		public static bool CheckEddieSignature(string path)
		{
			RSACryptoServiceProvider RSAVerifier = new RSACryptoServiceProvider();			
			string publicKey = Properties.Resources.eddie_file_public_xml;
			RSAVerifier.FromXmlString(publicKey);

			FileStream Signature = new FileStream(path + ".eddie-sig-net", FileMode.Open, FileAccess.Read);
			BinaryReader SignatureReader = new BinaryReader(Signature);
			byte[] SignatureData = SignatureReader.ReadBytes((int)Signature.Length);

			FileStream Verifyfile = new FileStream(path, FileMode.Open, FileAccess.Read);
			BinaryReader VerifyFileReader = new BinaryReader(Verifyfile);
			byte[] VerifyFileData = VerifyFileReader.ReadBytes((int)Verifyfile.Length);

			bool isValidsignature = RSAVerifier.VerifyData(VerifyFileData, "SHA512", SignatureData);

			Signature.Close();
			Verifyfile.Close();

			return isValidsignature;
		}
		*/
	}
}