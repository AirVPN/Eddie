using System;
using System.Collections.Generic;
using System.Text;

namespace AirVPN.Core
{
	public class Process : System.Diagnostics.Process
	{	
		public bool ReallyExited
		{
			get
			{
				bool check1 = HasExited;

				bool check2 = true;
				System.Diagnostics.Process[] processlist = Process.GetProcesses();
				foreach (System.Diagnostics.Process theprocess in processlist)
				{
					try
					{
						if (theprocess.Id == this.Id)
							check2 = false;
					}
					catch (System.InvalidOperationException)
					{
					}
				}

				if ((check1 == true) && (check2 == false))
				{
					Console.WriteLine("Unexpected: process marked as exited but still running.");
				}

				return check2;
			}
		}
	}
}
