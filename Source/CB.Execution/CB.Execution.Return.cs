
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/* Requires:
. CB.Data\CB.Data.EnumHelper.cs
. CB.Validation\CB.Validation.ContractConditions.cs
. CB_DOTNET\_FICHIERS\File for Windows 8 Store.cs -> uniquement pour les projets Windows 8 (universel ou Store ou Phone).
. CB.Reflection\CB.Reflection.TypeEx.cs
*/

/*

Here, the philosophy is to return codes on errors, not Exceptions.

## Why ?

First problem with Exceptions, they have inconvenients:
- Uncatched exceptions crash applications in the wild.
- Catched exceptions are very slow and resource-hungry.

Second problem:  
Normally, programmers should catch separatly all exceptions a function (or procedure) can throw.  
For example, ``System.IO.File.Open()`` can throw 9 different exceptions. Every call should be followed by 9 catches.  
In practice, I never see any programmer doing that.  
At best they just catch the all 9 exceptions at once using a global ``catch { }``, which can be seen as a bad practice (not discriminent enough).

All well-designed programming languages should enforce programmers to manage all errors a function can produce.  
Of course, that is so annoying and repetitive that in practice programming languages do not enforce anything at all.  
In short, that practice is highly error-prone and probably causes a lot of problems ("bugs" for entemologists), but it takes into account the lazyness that affects all human beings (including programmers).

## The proposal
- Functions (methods) return error codes on expected errors. For example when a needed file does not exist.
	- Every function declares, using a specific attribute, a list of error codes it can return.
- Every function call manages *all* the errors that function declares (using a switch-case code block).
- Exceptions are thrown only on unexpected errors, in fact on programming errors. Ideally, they should happen only when debugging ("insect-tracking" ?).

Of course, that affects only your own source code. So you have to catch Exceptions from external librairies, such as the dotnet and Core frameworks, that applies the regular exception scheme.

### Main structures
- ``Return<T,E>`` is a return structure that contains both a return value and an error code. The value can be null.
- ``Ret<T>`` is a shortcut for ``Return<T, ReturnSuccess>``. ``ReturnSuccess`` is a common error code set.
- ``ReturnNonNull<T,E>`` is similar to ``Return<T,E>``, but the value can't be null (except if there is an error, of course).
- ``RetNonNull<T>`` is a shortcut for ``ReturnNonNull<T, ReturnSuccess>``.

### Simple example
```C#
[ReturnSuccessCodes(ReturnSuccess.ArgumentOutOfRange)]
Ret<double> Sqrt(double d)
{
  if (d < 0.0) return ReturnSuccess.ArgumentOutOfRange;
  return Math.Sqrt(d);
}
...
var a = this.Sqrt(0.0);
switch(a.ErrorCode)
{
  case ReturnSuccess.ArgumentOutOfRange:
    a = -1; break;
}
```
### Example with a customized error code set
```C#
enum MyErrorCodes
{ NotInitialized, Success, StringIsEmpty, StringIsNull }

[ReturnCodes((int)MyErrorCodes.StringIsEmpty, (int)MyErrorCodes.StringIsNull)]
static ReturnNonNull<String, MyErrorCodes> CertifiesStringIsDefined(string p)
{
  if (p == null)
    return new ReturnNonNull<string, MyErrorCodes>MyErrorCodes.StringIsNull);
  if (p == string.Empty)
    return new ReturnNonNull<string, MyErrorCodes>MyErrorCodes.StringIsEmpty);
  return new ReturnNonNull<string, MyErrorCodes>(p);
}
...
var s = CertifiesStringIsDefined("good");
switch (s.ErrorCode)
{
  case MyErrorCodes.StringIsEmpty:
    s = "!Empty!"; break;
  case MyErrorCodes.StringIsNull:
    s = "!Null!"; break;
}
```
### Additional structures
- ``ReturnError`` contains both a ReturnSuccess and an Exception. Can be used when a function returns a detailed exception message: for example "Write access to file **x.y** is denied because it is open by process **z**".
 
 */

/* EXEMPLE COMPLET:
static void Main(string[] args)
{
	var r1 = SimpleReturnCodeFunction(null);
	var r2 = SimpleReturnCodeFunction(1234);
	switch (r2.ErrorCode)
	{
		case ReturnSuccess.Success:
			break;
		case ReturnSuccess.ArgumentIsNull:
			throw new Exception("a");
		default:
			throw new NotSupportedException();
	}

	var r3 = CustomReturnCodeFunction("good");
	switch(r3.ErrorCode)
	{
		case MyErrorCodes.Success:
			break;
		case MyErrorCodes.StringIsEmpty:
			throw new Exception("a");
		case MyErrorCodes.StringIsNull:
			throw new Exception("b");
		default:
			throw new NotSupportedException();
	}

}

[ReturnSuccessCodes(ReturnSuccess.ArgumentIsNull)]
static Ret<int> SimpleReturnCodeFunction(int? p)
{
	if (p.HasValue)
		return new Ret<int>(p.Value);
	return new Ret<int>(ReturnSuccess.ArgumentIsNull);
}

enum MyErrorCodes
{
	NotInitialized,
	Success,
	StringIsEmpty,
	StringIsNull
}

[ReturnCodes((int)MyErrorCodes.StringIsEmpty, (int)MyErrorCodes.StringIsNull)]
static ReturnNonNull<String, MyErrorCodes> CustomReturnCodeFunction(string p)
{
	if (p == null)
		return new ReturnNonNull<string, MyErrorCodes>(MyErrorCodes.StringIsNull);
	if (p == string.Empty)
		return new ReturnNonNull<string, MyErrorCodes>(MyErrorCodes.StringIsEmpty);
	return new ReturnNonNull<string, MyErrorCodes>(p);
}
*/

/* rappel des symboles de projet:
 * Console : (rien)
 * WPF : (rien)
 * Windows 8.1 pour Windows Store : NETFX_CORE;WINDOWS_APP
 * Windows 8 universel pour Windows (Store) : NETFX_CORE;WINDOWS_APP
 * Windows 8 universel pour Windows Phone (Store) : NETFX_CORE;WINDOWS_PHONE_APP
 * Windows 10 UWP (Store) : NETFX_CORE;WINDOWS_UWP
*/

#if WINDOWS_APP || WINDOWS_PHONE_APP
#define WINDOWS_8_STORE // Windows 8, mais non Windows 10.
#endif

#if TEST
using CBdotnet.Test;
#endif
using CB.Data;
using CB.Validation;
using CB.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace CB.Execution
{
	/// <summary>
	/// Return value state.
	/// For <see cref="CB.Execution.Return{T, E}"/> and for <see cref="CB.Execution.ReturnNonNull{T, E}"/>.
	/// </summary>
	public enum ReturnState {
		/// <summary>
		/// The returned structure is not initialized at all.
		/// </summary>
		NotInitialized,
		/// <summary>
		/// The returned structure contains a valid value.
		/// </summary>
		Value,
		/// <summary>
		/// The returned structure contains an error code.
		/// </summary>
		Error
	}

	/// <summary>
	/// Gives a correpondence between the error code and an Exception type.
	/// </summary>
	public class CorrespondingExceptionAttribute : Attribute
	{
		/// <summary>
		/// The Exception type that corresponds to the error code.
		/// </summary>
		public Type ExceptionType;

		/// <summary>
		/// Gives a correpondence between the error code and an Exception type.
		/// </summary>
		/// <param name="exceptionType">The Exception type that corresponds to the error code.</param>
		public CorrespondingExceptionAttribute(Type exceptionType)
		{
			this.ExceptionType = exceptionType;
		}
	}

	/// <summary>
	/// Common error code set for <see cref="CB.Execution.Return{T, E}"/> and for <see cref="CB.Execution.ReturnNonNull{T, E}"/>.
	/// <para>Please note: if you need an Exception, your function should return a <see cref="ReturnError"/>, not a <see cref="ReturnSuccess"/>.</para>
	/// </summary>
	public enum ReturnSuccess : int
	{
		/// <summary>
		/// The function has a defect, it returns an uninitialized return entity.
		/// <para>Please correct the function, or contact its author.</para>
		/// </summary>
		Reserved_NotInitialized = 0,
		/// <summary>
		/// The function is a success, a valid value is defined.
		/// </summary>
		Success = 1,
		/// <summary>
		/// General failure.
		/// <para>If you authored the function, please consider creating an error enumeration that will let the function return more specific error codes.</para>
		/// </summary>
		[CorrespondingException(typeof(System.Exception))]
		Fail = 2,
		/// <summary>
		/// The thing to be created already exists.
		/// Example: a file can not be overwritten.
		/// </summary>
		AlreadyExists,
		/// <summary>
		/// One of the arguments provided to a method is not valid.
		/// </summary>
		[CorrespondingException(typeof(System.ArgumentException))]
		ArgumentError,
		/// <summary>
		/// The format of an argument is invalid, or a composite format string is not well formed.
		/// </summary>
		[CorrespondingException(typeof(System.FormatException))]
		ArgumentFormatError,
		/// <summary>
		/// A null reference (Nothing in Visual Basic) is passed to a method that does not accept it as a valid argument.
		/// </summary>
		[CorrespondingException(typeof(System.ArgumentNullException))]
		ArgumentIsNull,
		/// <summary>
		/// The value of an argument is outside the allowable range of values as defined by the invoked method.
		/// </summary>
		[CorrespondingException(typeof(System.ArgumentOutOfRangeException))]
		ArgumentOutOfRange,
		/// <summary>
		/// The parameter, a string, is null or is String.Empty.
		/// </summary>
		ArgumentStringIsNullOrEmpty,
		/// <summary>
		/// Part of a file or directory cannot be found.
		/// </summary>
		[CorrespondingException(typeof(System.IO.DirectoryNotFoundException))]
		DirectoryNotFound,
		/// <summary>
		/// An attempt to access a file that does not exist on disk failed.
		/// </summary>
		[CorrespondingException(typeof(System.IO.FileNotFoundException))]
		FileNotFound,
		/// <summary>
		/// The format of an argument is invalid, or a composite format string is not well formed.
		/// </summary>
		[CorrespondingException(typeof(FormatException))]
		FormatError,
		/// <summary>
		/// The arguments are inconsistent. They no not match each other.
		/// </summary>
		InconsistentArgumentsError,
		/// <summary>
		/// An I/O error occured.
		/// </summary>
		[CorrespondingException(typeof(System.IO.IOException))]
		IOError,
		/// <summary>
		/// The requested value was not found.
		/// </summary>
		NotFound,
		/// <summary>
		/// An invoked method is not supported, or there is an attempt to read, seek, or write to a stream that does not support the invoked functionality.
		/// </summary>
		[CorrespondingException(typeof(NotSupportedException))]
		NotSupported,
		/// <summary>
		/// An arithmetic, casting, or conversion operation in a checked context resulted in an overflow.
		/// </summary>
		[CorrespondingException(typeof(System.OverflowException))]
		Overflow,
		/// <summary>
		/// A path or file name is longer than the system-defined maximum length.
		/// </summary>
		[CorrespondingException(typeof(System.IO.PathTooLongException))]
		PathTooLong,
		/// <summary>
		/// A security error is detected.
		/// </summary>
		[CorrespondingException(typeof(System.Security.SecurityException))]
		SecurityError,
		/// <summary>
		/// The operating system denied access because of an I/O error or a specific type of security error.
		/// </summary>
		[CorrespondingException(typeof(System.UnauthorizedAccessException))]
		UnauthorizedAccess,
	}

	/// <summary>
	/// All the enumeration error codes that a specific method can return.
	/// <para>Codes need to be transcoded to Int32. The error enumeration itself must be based on Int32.</para>
	/// <para>Success code (default(error enumeration)) is implicit, no use to add it to the list.</para>
	/// <para>Please consult <see cref="CB.Execution.Return{T, E}"/>, <see cref="CB.Execution.ReturnNonNull{T, E}"/> or <see cref="CB.Execution.IReturn{T, E}"/>.</para>
	/// <example>
	/// <code>
	/// [ReturnCodesAttribute((int)MyReturnCode.NotHappy, (int)MyReturnCode.ArgumentIsUgly)]
	/// </code>
	/// </example>
	/// Example:
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Delegate)]
	public class ReturnCodesAttribute : Attribute
	{
		/// <summary>
		/// The errors this method can cause.
		/// <para>Every call should manage all these potential errors.</para>
		/// </summary>
		public int[] Codes;

		/// <summary>
		/// Possible error codes, transcoded to Int32.
		/// <para>Success code (default(error enumeration)) is implicit, no use to add it to the list.</para>
		/// </summary>
		/// <param name="codes"></param>
		public ReturnCodesAttribute(params int[] codes)
		{
		}
	}

	/// <summary>
	/// All ReturnSuccess codes that a specific method can return.
	/// <para>Success code (<see cref="ReturnSuccess.Success"/>) is implicit, no use to add it to the list.</para>
	/// <para>Please consult <see cref="CB.Execution.Ret{T}"/>, <see cref="CB.Execution.RetNonNull{T}"/>, or <see cref="CB.Execution.IReturn{T}"/>.</para>
	/// <example>
	/// Example:
	/// <code>
	/// [ReturnSuccessCodes(ReturnSuccess.FormatError, ReturnSuccess.ArgumentIsNull)]
	/// </code>
	/// </example>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Delegate)]
	public class ReturnSuccessCodesAttribute : Attribute
	{
		/// <summary>
		/// The errors this method can cause.
		/// <para>Success code (<see cref="ReturnSuccess.Success"/>) is implicit.</para>
		/// <para>Every call should manage all these potential errors.</para>
		/// </summary>
		public ReturnSuccess[] Codes;

		/// <summary>
		/// Possible error codes.
		/// <para>Success code (<see cref="ReturnSuccess.Success"/>) is implicit, no use to add it to the list.</para>
		/// </summary>
		/// <param name="codes"></param>
		public ReturnSuccessCodesAttribute(params ReturnSuccess[] codes)
		{
			this.Codes = codes.Length > 0 ? codes : null;
		}
	}

	/// <summary>
	/// The base interface for the return structures.
	/// </summary>
	public interface IReturn
	{
	}

	/// <summary>
	/// Return type interface to let return a value and an error code at the same time.
	/// <para>Note: a function that returns a IReturn type should set <see cref="ReturnCodesAttribute"/>.</para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="E">E must be an enumeration based on Int32.</typeparam>
	public interface IReturn<T, E> : IReturn
		where E : struct
#if !WINDOWS_8_STORE
        , IConvertible
#endif
	{
		/// <summary>
		/// True if contains a valid value.
		/// </summary>
		bool HasValue { get; }
		/// <summary>
		/// True if contains an error code.
		/// </summary>
		bool HasErrorCode { get; }
		/// <summary>
		/// The structure can be uninitialized, can has a value, or can contain an error code.
		/// </summary>
		ReturnState State { get; }
		/// <summary>
		/// The error code.
		/// </summary>
		E ErrorCode { get; }
		/// <summary>
		/// The error code, transtyped as a Int32.
		/// </summary>
		int ErrorCodeAsInt32 { get; }
		/// <summary>
		/// The value.
		/// </summary>
		T Value { get; }
		/// <summary>
		/// Returns the valid value or default(T).
		/// </summary>
		/// <returns></returns>
		T GetValueOrDefault();
		/// <summary>
		/// Returns the valid value or <paramref name="defaultValue"/>.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		T GetValueOrDefault(T defaultValue);
	}

	/// <summary>
	/// Return type interface to let return a value and a <see cref="ReturnSuccess"/> code at the same time.
	/// <para>Note: a function that returns a IReturn type should set <see cref="ReturnSuccessCodesAttribute"/>.</para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IReturn<T> : IReturn<T, ReturnSuccess>
	{
	}

	/// <summary>
	/// An error built from a <see cref="CB.Execution.ReturnSuccess"/> and an <see cref="System.Exception"/>.
	/// <para>Please note: if you do not need an Exception, your function should return a simple <see cref="ReturnSuccess"/>, not a <see cref="ReturnError"/>.</para>
	/// </summary>
	public struct ReturnError
	{
		/// <summary>
		/// The error code.
		/// </summary>
		public readonly ReturnSuccess ErrorCode;
		/// <summary>
		/// The exception.
		/// </summary>
		public readonly Exception Exception;

		/// <summary>
		/// A dictionary that matches error codes and exceptions.
		/// </summary>
		public static readonly Dictionary<ReturnSuccess, Type> CorrespondenceErrorToException = _GetCorrespondenceErrorToException();
		/// <summary>
		/// A dictionary that matches exceptions and error codes.
		/// </summary>
		public static readonly Dictionary<Type, ReturnSuccess> CorrespondenceExceptionToError = _GetCorrespondenceExceptionToError(CorrespondenceErrorToException);
		/// <summary>
		/// No error, just a success.
		/// </summary>
		public static readonly ReturnError Success = new ReturnError(ReturnSuccess.Success);

		static Dictionary<ReturnSuccess, Type> _GetCorrespondenceErrorToException()
		{
			// initializes correspondences between ReturnSuccess and Exception:
			var t = typeof(ReturnSuccess);
			var valeurs = t.GetFields().Where(champ => champ.FieldType == t).ToArray();
			var c = new Dictionary<ReturnSuccess, Type>(valeurs.Length);
			foreach (var v in valeurs)
			{
				var at = v.GetCustomAttributeByType<CorrespondingExceptionAttribute>();
				if (at != null)
				{
					var a = (CorrespondingExceptionAttribute)at;
					var et = a.ExceptionType;
					var valeur = (ReturnSuccess)v.GetRawConstantValue();
					c.Add(valeur, et);
				}
			}
			return c;
		}

		static Dictionary<Type, ReturnSuccess> _GetCorrespondenceExceptionToError(Dictionary<ReturnSuccess, Type> c)//correspondenceErrorToException)
		{
			var c2 = new Dictionary<Type, ReturnSuccess>(c.Count);
			foreach (var ec in c)
			{
				if (!c2.ContainsKey(ec.Value))
					c2.Add(ec.Value, ec.Key); // reverses.
			}
			return c2;
		}

#if false // effacer après tests
		static ReturnError()
		{
			// initialise les correspondances entre ReturnSuccess et Exception, dans les deux sens:
			{
				var t = typeof(ReturnSuccess);
				var valeurs = t.GetFields().Where(champ => champ.FieldType == t).ToArray();
				var c = CorrespondenceErrorToException = new Dictionary<ReturnSuccess, Type>(valeurs.Length);
				var c2 = CorrespondenceExceptionToError = new Dictionary<Type, ReturnSuccess>(valeurs.Length);
				foreach (var v in valeurs)
				{
					var at = v.GetCustomAttribute(typeof(CorrespondingExceptionAttribute));
					if (at != null)
					{
						var a = (CorrespondingExceptionAttribute)at;
						var et = a.ExceptionType;
						var valeur = (ReturnSuccess)v.GetRawConstantValue();
						c.Add(valeur, et);
						if (!c2.ContainsKey(et))
							c2.Add(et, valeur);
					}
				}
			}
		}
#endif

		/// <summary>
		/// <para>Only a framework exception should be passed here, you may not instanciate an exception.</para>
		/// </summary>
		/// <param name="errorCode"></param>
		/// <param name="exception"></param>
		public ReturnError(ReturnSuccess errorCode, Exception exception)
		{
			ReleaseCheck.ParameterEnumIsDefined(errorCode);

			this.ErrorCode = errorCode;
			this.Exception = exception;
		}

		/// <summary>
		/// Returns a simple ReturnSuccess error code, with no Exceotion.
		/// </summary>
		/// <param name="errorCode"></param>
		public ReturnError(ReturnSuccess errorCode)
			: this(errorCode, null)
		{ }

		/// <summary>
		/// Tries to find the meeting ReturnSuccess error code.
		/// <para>If no error code corresponds, returns ReturnSuccess.Fail.</para>
		/// <para>Only a framework exception should be passed here, you may not instanciate an exception.</para>
		/// </summary>
		/// <param name="exception"></param>
		public ReturnError(Exception exception)
			: this(exception, false)
		{
			this.ErrorCode = ErrorOfException(exception);
			this.Exception = exception;
		}

		/// <summary>
		/// Tries to find the meeting ReturnSuccess error code.
		/// <para>If no error code corresponds, throws the exception, or returns ReturnSuccess.Fail, depending on <paramref name="throwsIfNoCorrespondence"/>.</para>
		/// <para>Only a framework exception should be passed here, you may not instanciate an exception.</para>
		/// </summary>
		/// <param name="exception"></param>
		/// <param name="throwsIfNoCorrespondence">If true and if no error code corresponds, throws the exception.</param>
		public ReturnError(Exception exception, bool throwsIfNoCorrespondence)
		{
			if (throwsIfNoCorrespondence)
				this.ErrorCode = ErrorOfException(exception);
			else
				this.ErrorCode = ErrorOfExceptionOrDefault(exception);
			this.Exception = exception;
		}

		/// <summary>
		/// Returns the Exception error code that correpond to the given ReturnSuccess.
		/// <para>If no exception corresponds, returns null.</para>
		/// </summary>
		/// <param name="error"></param>
		/// <returns></returns>
		public static Type ExceptionTypeOfError(ReturnSuccess error)
		{
			if (CorrespondenceErrorToException.TryGetValue(error, out Type e))
				return e;
			return null;
		}

		/// <summary>
		/// Returns the ReturnSuccess error code that correpond to the given Exception.
		/// <para>If no error corresponds, throws the Exception.</para>
		/// </summary>
		/// <param name="exception"></param>
		/// <returns></returns>
		public static ReturnSuccess ErrorOfException(Exception exception)
		{
			if (CorrespondenceExceptionToError.TryGetValue(exception.GetType(), out ReturnSuccess r))
				return r;
			throw exception;
		}

		/// <summary>
		/// Returns the ReturnSuccess error code that correpond to the given Exception.
		/// <para>If no error corresponds, returns ReturnSuccess.Fail.</para>
		/// </summary>
		/// <param name="exception"></param>
		/// <returns></returns>
		public static ReturnSuccess ErrorOfExceptionOrDefault(Exception exception)
		{
			if (CorrespondenceExceptionToError.TryGetValue(exception.GetType(), out ReturnSuccess r))
				return r;
			return ReturnSuccess.Fail;
		}

		/// <summary>
		/// True if the function call was a success.
		/// </summary>
		public bool IsASuccess
		{ get { return this.ErrorCode == ReturnSuccess.Success; } }
		/// <summary>
		/// True if the function call was a failure.
		/// </summary>
		public bool IsAnError
		{ get { return this.ErrorCode != ReturnSuccess.Success; } }

		/// <summary>
		/// Writes the error to the (IDE's) errors log.
		/// </summary>
		public void LogsIfError()
		{
			if (this.IsAnError)
				if (System.Diagnostics.Debugger.IsAttached)
#if NETFX_CORE || WIN_8_UNIVERSAL
                    System.Diagnostics.Debug.WriteLine(
#else
					System.Diagnostics.Debugger.Log(0, typeof(ReturnError).Name,
#endif
 this.Exception != null ? Exception.Message : this.ErrorCode.ToString());
		}

		/// <summary>
		/// The exception message, or the error code name.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.Exception != null ? Exception.Message : this.ErrorCode.ToString();
		}

	}

	[Obfuscation(Exclude = true)]
	static class ErrorCodesEnumHelper<E>
		where E : struct
#if !WINDOWS_8_STORE 
        , IConvertible
#endif
	{
		[Obfuscation(Exclude = true)]
		public static readonly E SuccessCode = CheckTypeE(CreateVal((int)ReturnSuccess.Success));
		[Obfuscation(Exclude = true)]
		public static readonly E NotInitializedCode = CreateVal((int)ReturnSuccess.Reserved_NotInitialized);

		/// <summary>
		/// Effectue des vérifications sur le type E.
		/// Le paramètre est juste renvoyé en sortie, de façon à être une fonction transparente.
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		static E CheckTypeE(E code)
		{
			if (typeof(E) != typeof(int))
			{
				if (!typeof(E).IsEnum())
					throw new Exception(string.Format(
						"The error type '{0}' is not an enumeration.", typeof(E).ToString()));
				if (typeof(E).GetEnumUnderlyingType() != typeof(int))
					throw new Exception(string.Format(
						"The error enumeration type '{0}' is not based on Int32, but on '{1}'.", typeof(E).ToString(), typeof(E).GetEnumUnderlyingType().ToString()));
				var valeurs = (E[])Enum.GetValues(typeof(E));
				var valeursInt32 = valeurs.Select(e => ToInt32(e));
				if (!valeursInt32.Contains((int)ReturnSuccess.Reserved_NotInitialized))
					throw new Exception(string.Format(
						"The error enumeration type '{0}' does not contain a value {1} (NotInitialized).", typeof(E).ToString(), (int)ReturnSuccess.Reserved_NotInitialized));
				if (!valeursInt32.Contains((int)ReturnSuccess.Success))
					throw new Exception(string.Format(
						"The error enumeration type '{0}' does not contain a value {1} (Success).", typeof(E).ToString(), (int)ReturnSuccess.Success));
			}
			return code;
		}

		public static int ToInt32(E valeur)
		{
#if !WINDOWS_8_STORE
			return valeur.ToInt32(null);
#else
			return valeur.GetHashCode(); // vu que l'énumération est obligatoirement basée sur un Int32, GetHashCode est équivalent à ToInt32.
#endif
		}

		static E CreateVal(int c)
		{
			var t = typeof(E);
			if (!t.IsEnum())
				throw new Exception("The type is not an enumeration (Enum) type.");
			if (t.GetEnumUnderlyingType() != typeof(int))
				throw new Exception("The enumeration type's underlying type  is not an Int32.");

			var r = (E)(object)c;
			if (!EnumHelper<E>.IsDeclaredAsAUniqueValue(r))
				throw new ArgumentException("The value is not a declared value of the enumeration.");
			return r;
		}
	}

	/// <summary>
	/// A return value or an-error-code/enumeration.
	/// <para>The function should set <see cref="ReturnCodesAttribute"/>.</para>
	/// </summary>
	/// <typeparam name="T">Value type.</typeparam>
	/// <typeparam name="E">Error code enumeration type, where NotInitialized = 0 and Success = 1.</typeparam>
	[Serializable]
	[Obfuscation(Exclude = true)]
	public struct Return<T, E> : IEquatable<Return<T, E>>, IReturn<T, E>
		where E : struct
#if !WINDOWS_8_STORE
        , IConvertible
#endif
	{
#if !DEBUG
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
		readonly T _Value;

#if !DEBUG
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
		readonly E _ErrorCode;
		/// <summary>
		/// The error code.
		/// </summary>
		public E ErrorCode
		{ get { return this._ErrorCode; } }
		/// <summary>
		/// The state: NotInitialized, Success or Error.
		/// </summary>
		public ReturnState State
		{
			get
			{
				switch (this.ErrorCodeAsInt32)
				{
					case (int)ReturnSuccess.Reserved_NotInitialized:
						return ReturnState.NotInitialized;
					case (int)ReturnSuccess.Success:
						return ReturnState.Value;
					default:
						return ReturnState.Error;
				}
			}
		}

		/// <summary>
		/// Initialises with a valid return value.
		/// Null is seen as a valid value, not an error.
		/// </summary>
		/// <param name="value"></param>
		public Return(T value)
		{
			this._Value = value;
			this._ErrorCode = ErrorCodesEnumHelper<E>.SuccessCode;
		}

		/// <summary>
		/// Initialises with an error code.
		/// </summary>
		/// <param name="errorCode"></param>
		public Return(E errorCode)
		{
			if (
#if !WINDOWS_8_STORE
errorCode.ToInt32(null)
#else
				ErrorCodesEnumHelper<E>.ToInt32(errorCode)
#endif
 == (int)ReturnSuccess.Success)
				throw new ArgumentException(
					string.Format("Error code {0} is a success code. This constructor needs an error code.", errorCode.ToString()));
			this._Value = default(T);
			this._ErrorCode = errorCode;
		}

		/// <summary>
		/// True if there is a valid value.
		/// </summary>
		public bool HasValue
		{
			get { return this.ErrorCodeAsInt32 == (int)ReturnSuccess.Success; }
		}

		/// <summary>
		/// True if there is a code error.
		/// </summary>
		public bool HasErrorCode
		{
			get
			{
				var ec = this.ErrorCodeAsInt32;
				return ec != (int)ReturnSuccess.Success && ec != (int)ReturnSuccess.Reserved_NotInitialized;
			}
		}

		/// <summary>
		/// Returns a valid value, or throws an exception
		/// </summary>
		/// <exception cref="System.InvalidOperationException">The structure is not initialized, or there is an error code.</exception>
		public T Value
		{
			get
			{
				switch (this.ErrorCodeAsInt32)
				{
					case (int)ReturnSuccess.Reserved_NotInitialized:
						throw new InvalidOperationException(typeof(Return<T, E>).Name + " not initialized. No parameter constructor was called.");
					case (int)ReturnSuccess.Success:
						return _Value;
					default:
						throw new InvalidOperationException(this.ErrorCode.ToString());
				}
			}
		}

		/// <summary>
		/// True if a == b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(Return<T, E> a, Return<T, E> b)
		{
			if (!a.HasValue && !b.HasValue)
				return true;
			if (!a.HasValue || !b.HasValue)
				return false;

			if (!typeof(T).IsValueType())
			{
				if (object.ReferenceEquals(a._Value, b._Value))
					return true;
				if (object.ReferenceEquals(a._Value, null) || object.ReferenceEquals(b._Value, null))
					return false;
			}

			{
				if (a._Value is IEquatable<T>)
					return ((IEquatable<T>)a._Value).Equals(b._Value);
				else
					if (a._Value is IEqualityComparer<T>)
						return ((IEqualityComparer<T>)a._Value).Equals(a._Value, b._Value);
				return object.Equals(a._Value, b._Value);
			}
		}

		/// <summary>
		/// True if a  != b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(Return<T, E> a, Return<T, E> b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Returns the valid value, or default(T).
		/// </summary>
		/// <returns></returns>
		public T GetValueOrDefault()
		{
			return _Value;
		}

		/// <summary>
		/// Returns the valid value, or <paramref name="defaultValue"/>.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public T GetValueOrDefault(T defaultValue)
		{
			return HasValue ? _Value : defaultValue;
		}
		/// <summary>
		/// True if this equals the <paramref name="other"/> object.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			return this == (Return<T, E>)other;
		}
		/// <summary>
		/// Returns the hash code.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return HasValue ? _Value.GetHashCode() : 0;
		}

		/// <summary>
		/// Returns either "&lt;not initialized&gt;", or the valid value.ToString(), or "&lt;error: xxxx&gt;".
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			switch (this.ErrorCodeAsInt32)
			{
				case (int)ReturnSuccess.Reserved_NotInitialized:
					return "<not initialized>";
				case (int)ReturnSuccess.Success:
					return _Value.ToString();
				default:
					return "<error: " + this.ErrorCode.ToString() + ">";
			}
		}

		/// <summary>
		/// Transtypes an original value to a return structure.
		/// </summary>
		/// <param name="value"></param>
		public static implicit operator Return<T, E>(T value)
		{
			return new Return<T, E>(value);
		}

		/// <summary>
		/// Transtypes a return structure to a value.
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="System.InvalidOperationException">The structure is not initialized, or there is an error code.</exception>
		public static explicit operator T(Return<T, E> value)
		{
			return value.Value;
		}

		/// <summary>
		/// True if this == <paramref name="other"/>.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(Return<T, E> other)
		{
			return this == other;
		}

		/// <summary>
		/// Gets the error code transtyped to a Int32.
		/// </summary>
		public int ErrorCodeAsInt32
		{
			get
			{
#if !WINDOWS_8_STORE
				return this.ErrorCode.ToInt32(null);
#else
				return ErrorCodesEnumHelper<E>.ToInt32(this.ErrorCode);
#endif
			}
		}
	}

#if TEST
	class Return_Test
	{
		enum MonÉnumérationDErreurs { NonInitialisé, OK, ErrZéro, ErrUne, ErrDeux }

		enum WrongErrorEnumeration1 : ulong { a, b, c }
		enum WrongErrorEnumeration2 { a = 2, b, c }
		enum WrongErrorEnumeration3 { a = 0, b = 2, c }

		[Test]
		static void test_ReturnEnumT()
		{
			Testeur.TesteÉchecPrévu(() => new Return<int, double>(0));
			Testeur.TesteÉchecPrévu(() => new Return<int, WrongErrorEnumeration1>(0));
			Testeur.TesteÉchecPrévu(() => new Return<int, WrongErrorEnumeration2>(0));
			Testeur.TesteÉchecPrévu(() => new Return<int, WrongErrorEnumeration3>(0));

			{
				Testeur.TesteÉchecPrévu(() => new Return<int, MonÉnumérationDErreurs>(MonÉnumérationDErreurs.OK));
				var a = new Return<Classe, MonÉnumérationDErreurs>();
				Testeur.TesteÉchecPrévu(() => a.Value);
				var e = a == null;
				Testeur.TesteFausseté(() => a == (Return<Classe, MonÉnumérationDErreurs>)null);
				Return<Classe, MonÉnumérationDErreurs> NonDéfini = null;
				var b = new Return<Classe, MonÉnumérationDErreurs>();
				Testeur.TesteÉgalité(a, b);
				a = new Classe() { I = 1 };
				Testeur.TesteÉgalité(a.Value.I, 1);
				Testeur.TesteVérité(() => a != b);
				var c = a;
				Testeur.TesteVérité(() => a == c);
			}
			{
				var a = new Return<int, MonÉnumérationDErreurs>();
				Testeur.TesteÉchecPrévu(() => a.Value);
				//Testeur.TesteFausseté(() => a == (ReturnEnum<int>)null);
				var b = new Return<int, MonÉnumérationDErreurs>();
				Testeur.TesteÉgalité(a, b);
				a = 1;
				Testeur.TesteÉgalité(a.Value, 1);
				Testeur.TesteVérité(() => a != b);
				b = 1;
				Testeur.TesteVérité(() => a == b);
				var c = a;
				Testeur.TesteVérité(() => a == c);
			}
			{
				var a = new Return<Structure, MonÉnumérationDErreurs>();
				Testeur.TesteÉchecPrévu(() => a.Value);
				//Testeur.TesteFausseté(() => a == (ReturnEnum<Structure>)null);
				var b = new Return<Structure, MonÉnumérationDErreurs>();
				Testeur.TesteÉgalité(a, b);
				a = new Structure() { I = 1 };
				Testeur.TesteÉgalité(a.Value.I, 1);
				Testeur.TesteVérité(() => a != b);
				var c = a;
				Testeur.TesteVérité(() => a == c);
			}
		}
		static Return<int, ReturnSuccess> exemple(bool valide)
		{
			if (valide)
				return new Return<int, ReturnSuccess>(1);
			return new Return<int, ReturnSuccess>(ReturnSuccess.Fail);
		}
		internal class Classe
		{
			internal int I;
		}
		internal struct Structure
		{
			internal int I;
		}
	}
#endif

	/// <summary>
	/// A return value or an-error-code/enumeration.
	/// <para>The function should set <see cref="ReturnCodesAttribute"/>.</para>
	/// <example>
	/// Example:
	/// <code>[ReturnCodes((int)MyErrorCodes.StringIsEmpty, (int)MyErrorCodes.StringIsNull)]</code>
	/// </example>
	/// </summary>
	/// <typeparam name="T">Value type.</typeparam>
	/// <typeparam name="E">Error code enumeration type, where NotInitialized = 0 and Success = 1.</typeparam>
	[Serializable]
	[Obfuscation(Exclude = true)]
	public struct ReturnNonNull<T, E> : IEquatable<ReturnNonNull<T, E>>, IReturn<T, E>
		where E : struct
#if !WINDOWS_8_STORE
        , IConvertible
#endif
	{
#if !DEBUG
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
		readonly T _Value;

#if !DEBUG
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
		readonly E _ErrorCode;
		/// <summary>
		/// The error code.
		/// </summary>
		public E ErrorCode
		{ get { return this._ErrorCode; } }

		/// <summary>
		/// The state: NotInitialized, Success or Error.
		/// </summary>
		public ReturnState State
		{
			get
			{
				switch (this.ErrorCodeAsInt32)
				{
					case (int)ReturnSuccess.Reserved_NotInitialized:
						return ReturnState.NotInitialized;
					case (int)ReturnSuccess.Success:
						return ReturnState.Value;
					default:
						return ReturnState.Error;
				}
			}
		}

		/// <summary>
		/// Initialises with a valid return value.
		/// </summary>
		/// <param name="value">Can not be null.</param>
		public ReturnNonNull(T value)
		{
			if (value == null)
				throw new ArgumentNullException("value");
			this._Value = value;
			this._ErrorCode = ErrorCodesEnumHelper<E>.SuccessCode;
		}

		/// <summary>
		/// Initialises with an error code.
		/// </summary>
		/// <param name="errorCode"></param>
		public ReturnNonNull(E errorCode)
		{
			if (
#if !WINDOWS_8_STORE
errorCode.ToInt32(null)
#else
				ErrorCodesEnumHelper<E>.ToInt32(errorCode)
#endif
 == (int)ReturnSuccess.Success)
				throw new ArgumentException(string.Format("Error code {0} is a success code. This constructor needs an error code.", errorCode.ToString()));
			this._Value = default(T);
			this._ErrorCode = errorCode;
		}

		/// <summary>
		/// True if there is a valid value.
		/// </summary>
		public bool HasValue
		{
			get { return this.ErrorCodeAsInt32 == (int)ReturnSuccess.Success; }
		}
		/// <summary>
		/// True if there is an error code.
		/// </summary>
		public bool HasErrorCode
		{
			get
			{
				var ec = this.ErrorCodeAsInt32;
				return ec != (int)ReturnSuccess.Success && ec != (int)ReturnSuccess.Reserved_NotInitialized;
			}
		}

		/// <summary>
		/// Returns a valid value, or throws an exception
		/// </summary>
		public T Value
		{
			get
			{
				switch (this.ErrorCodeAsInt32)
				{
					case (int)ReturnSuccess.Reserved_NotInitialized:
						throw new InvalidOperationException(typeof(ReturnNonNull<T, E>).Name + " not initialized. No parameter constructor was called.");
					case (int)ReturnSuccess.Success:
						return _Value;
					default:
						throw new InvalidOperationException(this.ErrorCode.ToString());
				}
			}
		}

		/// <summary>
		/// True if a == b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(ReturnNonNull<T, E> a, ReturnNonNull<T, E> b)
		{
			if (!a.HasValue && !b.HasValue)
				return true;
			if (!a.HasValue || !b.HasValue)
				return false;

			if (!typeof(T).IsValueType())
			{
				if (object.ReferenceEquals(a._Value, b._Value))
					return true;
				if (object.ReferenceEquals(a._Value, null) || object.ReferenceEquals(b._Value, null))
					return false;
			}

			{
				if (a._Value is IEquatable<T>)
					return ((IEquatable<T>)a._Value).Equals(b._Value);
				else
					if (a._Value is IEqualityComparer<T>)
						return ((IEqualityComparer<T>)a._Value).Equals(a._Value, b._Value);
				return object.Equals(a._Value, b._Value);
			}
		}

		/// <summary>
		/// True if a != b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(ReturnNonNull<T, E> a, ReturnNonNull<T, E> b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Returns the valid value, or default(T).
		/// </summary>
		/// <returns></returns>
		public T GetValueOrDefault()
		{
			return _Value;
		}

		/// <summary>
		/// Returns the valid value, or <paramref name="defaultValue"/>.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public T GetValueOrDefault(T defaultValue)
		{
			return HasValue ? _Value : defaultValue;
		}

		/// <summary>
		/// True if this equals the <paramref name="other"/> object.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			return this == (ReturnNonNull<T, E>)other;
		}
		/// <summary>
		/// Returns the hash code.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return HasValue ? _Value.GetHashCode() : 0;
		}

		/// <summary>
		/// Returns either "&lt;not initialized&gt;", or the valid value.ToString(), or "&lt;error: xxxx&gt;".
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			switch (this.ErrorCodeAsInt32)
			{
				case (int)ReturnSuccess.Reserved_NotInitialized:
					return "<not initialized>";
				case (int)ReturnSuccess.Success:
					return _Value.ToString();
				default:
					return "<error: " + this.ErrorCode.ToString() + ">";
			}
		}

		/// <summary>
		/// Transtypes an original value to a return structure.
		/// </summary>
		/// <param name="value"></param>
		public static implicit operator ReturnNonNull<T, E>(T value)
		{
			return new ReturnNonNull<T, E>(value);
		}

		/// <summary>
		/// Transtypes a return structure to a value.
		/// </summary>
		/// <param name="value"></param>
		public static implicit operator T(ReturnNonNull<T, E> value)
		{
			return value.Value;
		}

		/// <summary>
		/// True if this == <paramref name="other"/>.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(ReturnNonNull<T, E> other)
		{
			return this == other;
		}

		/// <summary>
		/// Gets the error code transtyped to a Int32.
		/// </summary>
		public int ErrorCodeAsInt32
		{
			get
			{
#if !WINDOWS_8_STORE
				return this.ErrorCode.ToInt32(null);
#else
				return ErrorCodesEnumHelper<E>.ToInt32(this.ErrorCode);
#endif
			}
		}
	}

#if TEST
	static class ReturnNonNull_test
	{
		[Test]
		static void test()
		{
			var r = new ReturnNonNull<int, ReturnSuccess>(123);
			Testeur.TesteRéussite(() => r == 123);

			Testeur.TesteÉchecPrévu(() => new ReturnNonNull<int?, ReturnSuccess>(new int?()));


		}
	}
#endif

#if false // ancien code, gérant les exceptions. Abandonné parce que les exceptions me semblent incompatibles avec une erreur rapide.
	/// <summary>
	/// A return value or {an error code/enumeration + exception}.
	/// </summary>
	/// <typeparam name="T">Value type.</typeparam>
	/// <typeparam name="E">Error code type. Code default(E) must indicate a success.</typeparam>
	[Serializable]
	[Obfuscation(Exclude = true)]
	public struct ReturnOrException<T, E> : IEquatable<Return<T, E>>
		where E : struct
	{
		readonly T _Value;

		public readonly ReturnState State;

		public readonly E ErrorCode;

		/// <summary>
		/// Inner Exception.
		/// </summary>
		public readonly Exception InnerException;

		/// <summary>
		/// Initialises with a valid return value.
		/// Null is seen as a valid value, not an error.
		/// </summary>
		/// <param name="value"></param>
		public ReturnOrException(T value)
		{
			this.State = ReturnState.Value;
			this._Value = value;
			this.InnerException = null;
			this.ErrorCode = default(E);
		}

		/// <summary>
		/// Initialises with an error code.
		/// </summary>
		/// <param name="value"></param>
		public ReturnOrException(E errorCode)
		{
			if (errorCode.Equals(default(E)))
				throw new ArgumentException("Error code 0 is a success code. This constructor needs an error code.");
			this.State = ReturnState.Error;
			this._Value = default(T);
			this.InnerException = null;
			this.ErrorCode = errorCode;
		}

		/// <summary>
		/// Initialises with an exception error and an error code.
		/// </summary>
		/// <param name="value"></param>
		public ReturnOrException(E errorCode, Exception innerException)
		{
			if (innerException == null)
				throw new ArgumentNullException("innerException");
			this.State = ReturnState.Error;
			this._Value = default(T);
			this.InnerException = innerException;
			this.ErrorCode = errorCode;
		}

		internal bool HasValue
		{
			get { return this.State == ReturnState.Value; }
		}

		internal bool IsAnError
		{
			get { return this.State == ReturnState.Error; }
		}

		public bool IsInitialized
		{
			get { return this.State != ReturnState.NotInitialized; }
		}

		/// <summary>
		/// Returns a valid value, or throws an exception
		/// </summary>
		internal T Value
		{
			get
			{
				if (!this.IsInitialized)
					throw new InvalidOperationException(typeof(Return<T, E>).Name + " not initialized. No parameter constructor was called.");
				if (this.InnerException != null)
					throw new InvalidOperationException(this.ErrorCode.ToString(), this.InnerException);
				if (!HasValue)
					throw new InvalidOperationException(this.ErrorCode.ToString());
				return _Value;
			}
		}

		public static bool operator ==(Return<T, E> a, Return<T, E> b)
		{
			if (!a.HasValue && !b.HasValue)
				return true;
			if (!a.HasValue || !b.HasValue)
				return false;

			if (!typeof(T).IsValueType)
			{
				if (object.ReferenceEquals(a._Value, b._Value))
					return true;
				if (object.ReferenceEquals(a._Value, null) || object.ReferenceEquals(b._Value, null))
					return false;
			}

			{
				if (a._Value is IEquatable<T>)
					return ((IEquatable<T>)a._Value).Equals(b._Value);
				else
					if (a._Value is IEqualityComparer<T>)
						return ((IEqualityComparer<T>)a._Value).Equals(a._Value, b._Value);
				return object.Equals(a._Value, b._Value);
			}
		}
		public static bool operator !=(Return<T, E> a, Return<T, E> b)
		{
			return !(a == b);
		}

		public T GetValueOrDefault()
		{
			return _Value;
		}

		public T GetValueOrDefault(T defaultValue)
		{
			return HasValue ? _Value : defaultValue;
		}

		public override bool Equals(object other)
		{
			return this == (Return<T, E>)other;
		}

		public override int GetHashCode()
		{
			return HasValue ? _Value.GetHashCode() : 0;
		}

		public override string ToString()
		{
			switch (this.State)
			{
				case ReturnState.NotInitialized:
					return "<not initialized>";
				case ReturnState.Value:
					return _Value.ToString();
				case ReturnState.Error:
				default:
					var e = this.InnerException;
					if (e == null)
						return "<error: " + this.ErrorCode.ToString() + ">";
					return "<error: " + this.ErrorCode.ToString() + " (inner exception: " + e.Message + ")>";
			}
		}

		public static implicit operator Return<T, E>(T value)
		{
			return new Return<T, E>(value);
		}

		public static explicit operator T(Return<T, E> value)
		{
			return value.Value;
		}

		public bool Equals(Return<T, E> other)
		{
			return this == other;
		}
	}
#endif

	/// <summary>
	/// Ret&lt;T&gt; is equivalent to Return&lt;T, <see cref="T:ReturnSuccess"/>&gt;.
	/// <para>The function should set <see cref="ReturnSuccessCodesAttribute"/>.</para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Obfuscation(Exclude = true)]
	public struct Ret<T> : IEquatable<Ret<T>>, IReturn<T>
	{
#if !DEBUG
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
		readonly T _Value;

		/// <summary>
		/// Current state: NotInitialized, Success or Error.
		/// </summary>
		public ReturnState State
		{
			get
			{
				switch (this._ErrorCode)
				{
					case ReturnSuccess.Reserved_NotInitialized:
						return ReturnState.NotInitialized;
					case ReturnSuccess.Success:
						return ReturnState.Value;
					default:
						return ReturnState.Error;
				}
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly ReturnSuccess _ErrorCode;
		/// <summary>
		/// Error code.
		/// </summary>
		public ReturnSuccess ErrorCode
		{ get { return this._ErrorCode; } }

		/// <summary>
		/// Initialises with a valid return value.
		/// <para>Null is seen as a valid value, not an error.</para>
		/// </summary>
		/// <param name="value"></param>
		public Ret(T value)
		{
			this._Value = value;
			this._ErrorCode = ReturnSuccess.Success;
		}

		/// <summary>
		/// Initializes with an error code.
		/// </summary>
		/// <param name="errorCode"></param>
		/// <exception cref="System.ArgumentException">Given error code is a success code. This constructor needs an error code.</exception>
		public Ret(ReturnSuccess errorCode)
		{
			if (errorCode.Equals(ReturnSuccess.Success))
				throw new ArgumentException(
					string.Format("Error code '{0}' is a success code. This constructor needs an error code.", errorCode.ToString()));
			this._Value = default(T);
			this._ErrorCode = errorCode;
		}
		/// <summary>
		/// True if there is a valid value.
		/// </summary>
		public bool HasValue
		{			get { return this._ErrorCode == ReturnSuccess.Success; }		}
		/// <summary>
		/// True if there is a code error.
		/// </summary>
		public bool HasErrorCode
		{			get { return this._ErrorCode != ReturnSuccess.Success && this._ErrorCode != ReturnSuccess.Reserved_NotInitialized; }		}

		/// <summary>
		/// True if the structure is initialized.
		/// </summary>
		public bool IsInitialized
		{ get { return this.State != ReturnState.NotInitialized; } }

		/// <summary>
		/// Returns a valid value, or throws an exception.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">Not initialized. No parameter constructor was called.</exception>
		public T Value
		{
			get
			{
				switch (this._ErrorCode)
				{
					case ReturnSuccess.Reserved_NotInitialized:
						throw new InvalidOperationException(typeof(Ret<T>).Name + " not initialized. No parameter constructor was called.");
					case ReturnSuccess.Success:
						return _Value;
					default:
						throw new InvalidOperationException(this.ErrorCode.ToString());
				}
			}
		}
		/// <summary>
		/// True if a == b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(Ret<T> a, Ret<T> b)
		{
			if (!a.HasValue && !b.HasValue)
				return true;
			if (!a.HasValue || !b.HasValue)
				return false;

			if (!typeof(T).IsValueType())
			{
				if (object.ReferenceEquals(a._Value, b._Value))
					return true;
				if (object.ReferenceEquals(a._Value, null) || object.ReferenceEquals(b._Value, null))
					return false;
			}

			{
				if (a._Value is IEquatable<T>)
					return ((IEquatable<T>)a._Value).Equals(b._Value);
				else
					if (a._Value is IEqualityComparer<T>)
						return ((IEqualityComparer<T>)a._Value).Equals(a._Value, b._Value);
				return object.Equals(a._Value, b._Value);
			}
		}
		/// <summary>
		/// True if a != b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(Ret<T> a, Ret<T> b)
		{
			return !(a == b);
		}
		/// <summary>
		/// Returns the valid value, or default(T).
		/// </summary>
		/// <returns></returns>
		public T GetValueOrDefault()
		{
			return _Value;
		}
		/// <summary>
		/// Returns the valid value, or <paramref name="defaultValue"/>.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public T GetValueOrDefault(T defaultValue)
		{
			return HasValue ? _Value : defaultValue;
		}
		/// <summary>
		/// True if this equals the <paramref name="other"/> object.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			return this == (Ret<T>)other;
		}
		/// <summary>
		/// Returns the hash code.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return HasValue ? _Value.GetHashCode() : 0;
		}
		/// <summary>
		/// Returns either "&lt;not initialized&gt;", or the valid value.ToString(), or "&lt;error: xxxx&gt;".
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			switch (this._ErrorCode)
			{
				case ReturnSuccess.Reserved_NotInitialized:
					return "<not initialized>";
				case ReturnSuccess.Success:
					return _Value.ToString();
				default:
					return "<error: " + this.ErrorCode.ToString() + ">";
			}
		}

		/// <summary>
		/// Creates a Ret&lt;T&gt; from a value.
		/// </summary>
		/// <param name="value"></param>
		public static implicit operator Ret<T>(T value)
		{
			return new Ret<T>(value);
		}

		/// <summary>
		/// Transcodes to the value.
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="System.InvalidOperationException">Not initialized. No parameter constructor was called.</exception>
		public static explicit operator T(Ret<T> value)
		{
			return value.Value;
		}

		/// <summary>
		/// Creates a Ret&lt;T&gt; from a ReturnSuccess.
		/// </summary>
		/// <param name="errorCode"></param>
		public static implicit operator Ret<T>(ReturnSuccess errorCode)
		{
			return new Ret<T>(errorCode);
		}
		/// <summary>
		/// True if this == <paramref name="other"/>.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(Ret<T> other)
		{
			return this == other;
		}

		/// <summary>
		/// Gets the error code transtyped to a Int32.
		/// </summary>
		public int ErrorCodeAsInt32
		{
			get { return (int)this.ErrorCode; }
		}
	}

	/// <summary>
	/// The function returns a value or an error, not a null.
	/// <para>RetNonNull&lt;T&gt; is equivalent to ReturnNonNull&lt;T, ReturnSuccess&gt;.</para>
	/// <para>The function should set <see cref="ReturnSuccessCodesAttribute"/>.</para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Obfuscation(Exclude = true)]
	public struct RetNonNull<T> : IEquatable<RetNonNull<T>>, IReturn<T>
	{
#if !DEBUG
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
		readonly T _Value;

#if !DEBUG
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
		readonly ReturnSuccess _ErrorCode;
		/// <summary>
		/// The error code.
		/// </summary>
		public ReturnSuccess ErrorCode
		{ get { return this._ErrorCode; } }
		/// <summary>
		/// The state: NotInitialized, Success or Error.
		/// </summary>
		public ReturnState State
		{
			get
			{
				switch (this._ErrorCode)
				{
					case ReturnSuccess.Reserved_NotInitialized:
						return ReturnState.NotInitialized;
					case ReturnSuccess.Success:
						return ReturnState.Value;
					default:
						return ReturnState.Error;
				}
			}
		}

		/// <summary>
		/// Initialises with a valid return value.
		/// </summary>
		/// <param name="value">Can not be null.</param>
		/// <exception cref="System.ArgumentNullException">value is null</exception>
		public RetNonNull(T value)
		{
			if (value == null)
				throw new ArgumentNullException("value");
			this._Value = value;
			this._ErrorCode = ReturnSuccess.Success;
		}

		/// <summary>
		/// Initialises with an error code.
		/// </summary>
		/// <param name="errorCode"></param>
		public RetNonNull(ReturnSuccess errorCode)
		{
			if (errorCode.Equals(ReturnSuccess.Success))
				throw new ArgumentException(string.Format("Error code '{0}' is a success code. This constructor needs an error code.", errorCode.ToString()));
			this._Value = default(T);
			this._ErrorCode = errorCode;
		}
		/// <summary>
		/// True if there is a valid value.
		/// </summary>
		public bool HasValue
		{
			get { return this._ErrorCode == ReturnSuccess.Success; }
		}
		/// <summary>
		/// True if there is a code error.
		/// </summary>
		public bool HasErrorCode
		{
			get { return this._ErrorCode != ReturnSuccess.Success && _ErrorCode != ReturnSuccess.Reserved_NotInitialized; }
		}

		/// <summary>
		/// True if the structure is initialized.
		/// </summary>
		public bool IsInitialized
		{ get { return this.State != ReturnState.NotInitialized; } }

		/// <summary>
		/// Returns a valid value, or throws an exception
		/// </summary>
		/// <exception cref="System.InvalidOperationException">The structure is uninitialized, or it contains an error code.</exception>
		public T Value
		{
			get
			{
				switch (this._ErrorCode)
				{
					case ReturnSuccess.Reserved_NotInitialized:
						throw new InvalidOperationException(typeof(RetNonNull<T>).Name + " not initialized. No parameter constructor was called.");
					case ReturnSuccess.Success:
						return _Value;
					default:
						throw new InvalidOperationException(this.ErrorCode.ToString());
				}
			}
		}
		/// <summary>
		/// True if a == b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(RetNonNull<T> a, RetNonNull<T> b)
		{
			if (!a.HasValue && !b.HasValue)
				return true;
			if (!a.HasValue || !b.HasValue)
				return false;

			if (!typeof(T).IsValueType())
			{
				if (object.ReferenceEquals(a._Value, b._Value))
					return true;
				if (object.ReferenceEquals(a._Value, null) || object.ReferenceEquals(b._Value, null))
					return false;
			}

			{
				if (a._Value is IEquatable<T>)
					return ((IEquatable<T>)a._Value).Equals(b._Value);
				else
					if (a._Value is IEqualityComparer<T>)
						return ((IEqualityComparer<T>)a._Value).Equals(a._Value, b._Value);
				return object.Equals(a._Value, b._Value);
			}
		}
		/// <summary>
		/// True if a != b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(RetNonNull<T> a, RetNonNull<T> b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Creates a RetNonNull&lt;T&gt; from a value.
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="System.ArgumentNullException">value is null</exception>
		public static implicit operator RetNonNull<T>(T value)
		{
			return new RetNonNull<T>(value);
		}

		/// <summary>
		/// Transcodes to the value.
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="System.InvalidOperationException">Not initialized. No parameter constructor was called.</exception>
		public static explicit operator T(RetNonNull<T> value)
		{
			return value.Value;
		}

		/// <summary>
		/// Creates a RetNonNull&lt;T&gt; from a ReturnSuccess.
		/// </summary>
		/// <param name="errorCode"></param>
		public static implicit operator RetNonNull<T>(ReturnSuccess errorCode)
		{
			return new RetNonNull<T>(errorCode);
		}
		/// <summary>
		/// Returns the valid value, or default(T).
		/// </summary>
		/// <returns></returns>
		public T GetValueOrDefault()
		{
			return _Value;
		}
		/// <summary>
		/// Returns the valid value, or <paramref name="defaultValue"/>.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public T GetValueOrDefault(T defaultValue)
		{
			return HasValue ? _Value : defaultValue;
		}
		/// <summary>
		/// True if this equals the <paramref name="other"/> object.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			return this == (RetNonNull<T>)other;
		}
		/// <summary>
		/// Returns the hash code.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return HasValue ? _Value.GetHashCode() : 0;
		}
		/// <summary>
		/// Returns either "&lt;not initialized&gt;", or the valid value.ToString(), or "&lt;error: xxxx&gt;".
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			switch (this._ErrorCode)
			{
				case ReturnSuccess.Reserved_NotInitialized:
					return "<not initialized>";
				case ReturnSuccess.Success:
					return _Value.ToString();
				default:
					return "<error: " + this.ErrorCode.ToString() + ">";
			}
		}
		/// <summary>
		/// True if this == <paramref name="other"/>.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(RetNonNull<T> other)
		{
			return this == other;
		}

		/// <summary>
		/// Gets the error code transtyped to a Int32.
		/// </summary>
		public int ErrorCodeAsInt32
		{
			get { return (int)this.ErrorCode; }
		}
	}

	/// <summary>
	/// Returns a disposable value or an error, not a null.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class RetNonNullDisposable<T> : IDisposable, IReturn<T>
		where T : IDisposable
	{
#if !DEBUG
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
		readonly RetNonNull<T> r; // repose essentiellement sur ceci.

		/// <summary>
		/// Initialises with a valid return value.
		/// Null is an invalid value.
		/// </summary>
		/// <param name="value"></param>
		public RetNonNullDisposable(T value)
		{
			this.r = new RetNonNull<T>(value);
		}
		/// <summary>
		/// Initialises with an error code.
		/// </summary>
		/// <param name="errorCode"></param>
		public RetNonNullDisposable(ReturnSuccess errorCode)
		{
			this.r = new RetNonNull<T>(errorCode);
		}

		#region IReturn<T>

		/// <summary>
		/// True if there is a valid value.
		/// </summary>
		public bool HasValue
		{
			get { return r.HasValue; }
		}

		/// <summary>
		/// True if there is a code error.
		/// </summary>
		public bool HasErrorCode
		{
			get { return r.HasErrorCode; }
		}
		/// <summary>
		/// The state: NotInitialized, Success or Error.
		/// </summary>
		public ReturnState State
		{
			get { return r.State; }
		}
		/// <summary>
		/// The error code.
		/// </summary>
		public ReturnSuccess ErrorCode
		{
			get { return r.ErrorCode; }
		}
		/// <summary>
		/// Gets the error code transtyped to a Int32.
		/// </summary>
		public int ErrorCodeAsInt32
		{
			get { return r.ErrorCodeAsInt32; }
		}
		/// <summary>
		/// Returns a valid value, or throws an exception
		/// </summary>
		/// <exception cref="System.InvalidOperationException">The structure is uninitialized, or it contains an error code.</exception>
		public T Value
		{
			get { return r.Value; }
		}
		/// <summary>
		/// Returns the valid value, or default(T).
		/// </summary>
		/// <returns></returns>
		public T GetValueOrDefault()
		{
			return r.GetValueOrDefault();
		}
		/// <summary>
		/// Returns the valid value, or <paramref name="defaultValue"/>.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public T GetValueOrDefault(T defaultValue)
		{
			return r.GetValueOrDefault(defaultValue);
		}

		#endregion IReturn<T>

		#region IDisposable
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int m_isDisposed = 0;
		/// <summary>
		/// IDisposable.Dispose().
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}
		void Dispose(bool disposing)
		{
			if (Interlocked.CompareExchange(ref  m_isDisposed, 1, 0) == 1)
				return;
			if (disposing)
			{
				if (r.HasValue)
					r.Value.Dispose();
			}
		}
		#endregion

		/// <summary>
		/// Returns the hash code.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return r.GetHashCode();
		}

		/// <summary>
		/// True if this equals the <paramref name="obj"/> object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return r.Equals(obj);
		}
		/// <summary>
		/// Returns either "&lt;not initialized&gt;", or the valid value.ToString(), or "&lt;error: xxxx&gt;".
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return r.ToString();
		}

	}

}