
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

#if TEST
using CBdotnet.Test;
#endif
using System.Diagnostics;
using System.IO;

namespace CB.Processus
{
	/// <summary>
	/// Helpers for <see cref="System.Diagnostics.Process"/>.
	/// </summary>
	public static partial class ProcessEx
	{

		/// <summary>
		/// Gets the shell process executable name.
		/// Example: "explorer.exe".
		/// </summary>
		public static string ShellName
		{
			get
			{
				return (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "Shell", "explorer.exe");
			}
		}

		/// <summary>
		/// Gets all running shells (usually "explorer.exe").
		/// </summary>
		public static Process[] ShellProcesses
		{
			get { return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ShellName)); }
		}

		/// <summary>
		/// Starts the process as elevated, that is as an administrator.
		/// <para>On Windows XP and previous, the started process must check it is really elevated (because it can be started as any user).</para>
		/// </summary>
		/// <param name="psi"></param>
		/// <returns></returns>
		/// <exception cref="System.ComponentModel.Win32Exception">User has canceled the UAC prompt. Or an error occurred when opening the associated file. -or-The sum of the length of the arguments and the length of the full path to the process exceeds 2080. The error message associated with this exception can be one of the following: "The data area passed to a system call is too small." or "Access is denied."</exception>
		/// <exception cref="System.InvalidOperationException">No file name was specified in the startInfo parameter's System.Diagnostics.ProcessStartInfo.FileName property.-or- The System.Diagnostics.ProcessStartInfo.UseShellExecute property of the startInfo parameter is true and the System.Diagnostics.ProcessStartInfo.RedirectStandardInput, System.Diagnostics.ProcessStartInfo.RedirectStandardOutput, or System.Diagnostics.ProcessStartInfo.RedirectStandardError property is also true.-or-The System.Diagnostics.ProcessStartInfo.UseShellExecute property of the startInfo parameter is true and the System.Diagnostics.ProcessStartInfo.UserName property is not null or empty or the System.Diagnostics.ProcessStartInfo.Password property is not null.</exception>
		/// <exception cref="System.ArgumentNullException">The <paramref name="psi"/> parameter is null.</exception>
		/// <exception cref="System.ObjectDisposedException">The process object has already been disposed.</exception>
		/// <exception cref="System.IO.FileNotFoundException">The file specified in the startInfo parameter's System.Diagnostics.ProcessStartInfo.FileName property could not be found.</exception>
		public static Process StartsAsElevated(this ProcessStartInfo psi)
		{
			if (psi == null)
				throw new System.ArgumentNullException();
			psi.UseShellExecute = true;
			psi.Verb = "runas";
			return Process.Start(psi);
		}
	}
}