
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

#if TEST
using CBdotnet.Test;
#endif
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
#if NETFX_CORE
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
#endif

namespace CB.OperatingSystem
{
	/// <summary>
	/// Helpers about the operating system's environment in general (similar to <see cref="System.Environment"/>).
	/// </summary>
	public static class EnvironmentEx
	{
		/// <summary>
		/// True if the operating system is 64-bits (at least).
		/// <para>Of course the process can be 32-bits even when the operating system is 64-bits.</para>
		/// </summary>
		public static bool Is64BitOperatingSystem
		{
			get
			{
				if (Is64BitProcess)
					return true; // if the process is 64-bits, then the operating system is 64-bits at least.
#if NETFX_CORE     
				bool retVal;
				var h = GetCurrentProcess().DangerousGetHandle();
				var e = GetLastError();
				if (e != 0)
					throw new Exception();
				IsWow64Process(h, out retVal);
                return retVal;
#else
				return Environment.Is64BitOperatingSystem;
				/* // .NET < 4.0
				bool retVal;
				IsWow64Process(Process.GetCurrentProcess().Handle, out retVal);
				return retVal;*/
#endif
			}
		}

#if NETFX_CORE
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool lpSystemInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        static extern Microsoft.Win32.SafeHandles.SafeWaitHandle GetCurrentProcess();

		[DllImport("kernel32.dll")]
		static extern uint GetLastError();
#endif

		/// <summary>
		/// True if the process is 64-bits.
		/// <para>True implies the operating system is 64-bits too.</para>
		/// </summary>
		public static bool Is64BitProcess
		{
			get
			{
#if NETFX_CORE
                return IntPtr.Size == 8;
                // voir aussi Windows.System.ProcessorArchitecture.X64
#else
				return Environment.Is64BitProcess;
#endif
			}
		}

		/// <summary>
		/// True if the current user account has an administrator role.
		/// <para>An administrator account is supposed to be able to obtain elevated rights.</para>
		/// </summary>
		public static bool UserIsAdmin
		{
			get
			{
				return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
			}
		}
	}

	/// <summary>
	/// Throws an exception with the Marshal.GetLastWin32Error() added at the end of the message.
	/// </summary>
	[Serializable]
	public class Win32ExceptionEx : Exception
	{
		/// <summary>
		/// Throws an exception with the Marshal.GetLastWin32Error() added at the end of the message.
		/// </summary>
		/// <param name="message">The first part of the exception message.</param>
		public Win32ExceptionEx(string message)
			: base(message + "\nError code = " + Marshal.GetLastWin32Error().ToString() + "\n" + new Win32Exception().Message)
		{ }
	}

}