using System;
using System.Runtime.InteropServices;

namespace TouchOSC
{
	internal class Win32
	{
		[DllImport("iphlpapi.dll", ExactSpelling = true)]
		public static extern SendARPReturnValues SendARP(uint DestIP, uint SrcIP, byte[] pMacAddr, ref uint PhyAddrLen);

		public const uint INADDR_ANY = 0;

		public enum SendARPReturnValues : uint
		{
			ERROR_BAD_NET_NAME        = 67,
			ERROR_BUFFER_OVERFLOW     = 111,
			ERROR_GEN_FAILURE         = 31,
			ERROR_INVALID_PARAMETER   = 87,
			ERROR_INVALID_USER_BUFFER = 1784,
			ERROR_NOT_FOUND           = 0x80070490,
			ERROR_NOT_SUPPORTED       = 0x80070032,
			NO_ERROR                  = 0
		}
	}
}