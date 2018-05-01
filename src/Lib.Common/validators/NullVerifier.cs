using System;

namespace Eddie.Common.Validators
{
	public class NullVerifier
	{
		public static void CantBeNull(object argument, string name)
		{
			if(argument == null)
				throw new NullReferenceException(name);
		}
	}
}
