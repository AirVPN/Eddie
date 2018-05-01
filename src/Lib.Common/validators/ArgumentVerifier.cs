#if !EDDIENET2
using System;

namespace Eddie.Common.Validators
{
    public static class ArgumentVerifier
    {
        public static void CantBeNull(object argument, string name)
        {
            if(argument == null)
                throw new ArgumentNullException(name);            
        }

        public static void ShouldBeTrue<T>(Func<T, bool> checkFunc, T argument, string formattedError)
        {
            CantBeNull(checkFunc, "checkFunc");
            if(!checkFunc(argument))
                throw new ArgumentException(formattedError);
        }
    }
}
#endif