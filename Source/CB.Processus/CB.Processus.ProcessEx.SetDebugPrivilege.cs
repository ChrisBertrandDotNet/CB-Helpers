
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;

namespace CB.Processus
{
	public static partial class ProcessEx
	{
		/// <summary>
		/// Obtains Debug privilege for the current process.
		/// </summary>
		/// <returns></returns>
		public static bool SetDebugPrivilege()
		{
			IntPtr hToken = IntPtr.Zero;
			try
			{
				CB.Processus.NativeStructures.LUID luidSEDebugNameValue = new CB.Processus.NativeStructures.LUID();
				CB.Processus.NativeStructures.TOKEN_PRIVILEGES tkpPrivileges;

				if (!NativeMethods.OpenProcessToken(Process.GetCurrentProcess().Handle,
					TokenAccessLevels.AdjustPrivileges | TokenAccessLevels.Query, out hToken))
				{
					Console.WriteLine("OpenProcessToken() failed, error = {0} . SeDebugPrivilege is not available", Marshal.GetLastWin32Error());
					return false;
				}
				else
				{
					Console.WriteLine("OpenProcessToken() successfully");
				}

				if (!NativeMethods.LookupPrivilegeValue(string.Empty, NativeStructures.SE.SE_DEBUG_NAME, ref luidSEDebugNameValue))
				{
					Console.WriteLine("LookupPrivilegeValue() failed, error = {0} .SeDebugPrivilege is not available", Marshal.GetLastWin32Error());
					NativeMethods.CloseHandle(hToken);
					return false;
				}
				else
				{
					Console.WriteLine("LookupPrivilegeValue() successfully");
				}

				tkpPrivileges.PrivilegeCount = 1;
				tkpPrivileges.Privileges.Luid = luidSEDebugNameValue;
				tkpPrivileges.Privileges.Attributes = CB.Processus.NativeStructures.PrivilegeAttributes.Enabled;

				bool ret = NativeMethods.AdjustTokenPrivileges(hToken, false, ref tkpPrivileges, 0, IntPtr.Zero, IntPtr.Zero);
				if (!ret)
				{
					Console.WriteLine("LookupPrivilegeValue() failed, error = {0} .SeDebugPrivilege is not available", Marshal.GetLastWin32Error());
				}
				else
				{
					Console.WriteLine("SeDebugPrivilege is now available");
				}
				NativeMethods.CloseHandle(hToken);
				return ret;
			}
			catch
			{
				if (hToken != IntPtr.Zero)
					NativeMethods.CloseHandle(hToken);
				return false;
			}
		}
	}

	static class NativeStructures
	{
		[StructLayout(LayoutKind.Sequential)]
		internal struct LUID
		{
			internal uint HighPart;
			internal uint LowPart;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct LUID_AND_ATTRIBUTES
		{
			internal LUID Luid;
			internal PrivilegeAttributes Attributes;
		}

		[Flags,
		SuppressMessage(
			"Microsoft.Design",
			"CA1008:EnumsShouldHaveZeroValue",
			Justification = "Native enum."),
		SuppressMessage(
			"Microsoft.Usage",
			"CA2217:DoNotMarkEnumsWithFlags",
			Justification = "Native enum.")]
		public enum PrivilegeAttributes
		{
			/// <summary>Privilege is disabled.</summary>
			Disabled = 0,

			/// <summary>Privilege is enabled by default.</summary>
			EnabledByDefault = 1,

			/// <summary>Privilege is enabled.</summary>
			Enabled = 2,

			/// <summary>Privilege is removed.</summary>
			Removed = 4,

			/// <summary>Privilege used to gain access to an object or service.</summary>
			UsedForAccess = -2147483648
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct TOKEN_PRIVILEGES
		{
			internal UInt32 PrivilegeCount;
			internal LUID_AND_ATTRIBUTES Privileges;
		}

		internal static class SE
		{
			internal const string SE_ASSIGNPRIMARYTOKEN_NAME = "SeAssignPrimaryTokenPrivilege";
			internal const string SE_AUDIT_NAME = "SeAuditPrivilege";
			internal const string SE_BACKUP_NAME = "SeBackupPrivilege";
			internal const string SE_CHANGE_NOTIFY_NAME = "SeChangeNotifyPrivilege";
			internal const string SE_CREATE_GLOBAL_NAME = "SeCreateGlobalPrivilege";
			internal const string SE_CREATE_PAGEFILE_NAME = "SeCreatePagefilePrivilege";
			internal const string SE_CREATE_PERMANENT_NAME = "SeCreatePermanentPrivilege";
			internal const string SE_CREATE_SYMBOLIC_LINK_NAME = "SeCreateSymbolicLinkPrivilege";
			internal const string SE_CREATE_TOKEN_NAME = "SeCreateTokenPrivilege";
			internal const string SE_DEBUG_NAME = "SeDebugPrivilege";
			internal const string SE_ENABLE_DELEGATION_NAME = "SeEnableDelegationPrivilege";
			internal const string SE_IMPERSONATE_NAME = "SeImpersonatePrivilege";
			internal const string SE_INC_BASE_PRIORITY_NAME = "SeIncreaseBasePriorityPrivilege";
			internal const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";
			internal const string SE_INC_WORKING_SET_NAME = "SeIncreaseWorkingSetPrivilege";
			internal const string SE_LOAD_DRIVER_NAME = "SeLoadDriverPrivilege";
			internal const string SE_LOCK_MEMORY_NAME = "SeLockMemoryPrivilege";
			internal const string SE_MACHINE_ACCOUNT_NAME = "SeMachineAccountPrivilege";
			internal const string SE_MANAGE_VOLUME_NAME = "SeManageVolumePrivilege";
			internal const string SE_PROF_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";
			internal const string SE_RELABEL_NAME = "SeRelabelPrivilege";
			internal const string SE_REMOTE_SHUTDOWN_NAME = "SeRemoteShutdownPrivilege";
			internal const string SE_RESTORE_NAME = "SeRestorePrivilege";
			internal const string SE_SECURITY_NAME = "SeSecurityPrivilege";
			internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
			internal const string SE_SYNC_AGENT_NAME = "SeSyncAgentPrivilege";
			internal const string SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";
			internal const string SE_SYSTEM_PROFILE_NAME = "SeSystemProfilePrivilege";
			internal const string SE_SYSTEMTIME_NAME = "SeSystemtimePrivilege";
			internal const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";
			internal const string SE_TCB_NAME = "SeTcbPrivilege";
			internal const string SE_TIME_ZONE_NAME = "SeTimeZonePrivilege";
			internal const string SE_TRUSTED_CREDMAN_ACCESS_NAME = "SeTrustedCredManAccessPrivilege";
			internal const string SE_UNDOCK_NAME = "SeUndockPrivilege";
			internal const string SE_UNSOLICITED_INPUT_NAME = "SeUnsolicitedInputPrivilege";
		}


	}

	static class NativeMethods
	{
		// Use this signature if you do not want the previous state
		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool AdjustTokenPrivileges(
			IntPtr TokenHandle,
		   [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
		   ref CB.Processus.NativeStructures.TOKEN_PRIVILEGES NewState,
		   UInt32 Zero,
		   IntPtr Null1,
		   IntPtr Null2);

		[DllImport("kernel32", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CloseHandle([In] IntPtr hObject);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true),
		SuppressUnmanagedCodeSecurity]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool LookupPrivilegeValue(string lpSystemName, string lpName,
			ref NativeStructures.LUID lpLuid);

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool OpenProcessToken(IntPtr ProcessHandle,
			System.Security.Principal.TokenAccessLevels DesiredAccess, out IntPtr TokenHandle);

	}
}