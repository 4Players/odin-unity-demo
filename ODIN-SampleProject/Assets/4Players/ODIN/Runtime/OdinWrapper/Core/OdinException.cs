using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdinNative.Core
{
    /// <summary>
    /// Exception type for the native ODIN runtime
    /// </summary>
    public class OdinException : Exception
    {
        public int ErrorCode;

        public OdinException(int error, string message)
            : base(message)
        {
            ErrorCode = error;
        }

        public OdinException(int error, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = error;
        }
    }
}
