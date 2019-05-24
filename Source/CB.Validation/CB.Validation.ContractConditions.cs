
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/*
 * 
 * Do conditional checks, similarly to the contracts (see  System.Diagnostics.Contracts.Contract), but with different (and more comprehensible) names and a few other differences.
* With the Release class, and unlike Contract, checks are always done, whatever we compile as release or as debug.
* With the Debug class, unlike Contract we don't need the code refactorer "ccrewrite".
 */

/* Requires:
 * cb.data\CB.Data.EnumHelper.cs
 * CB.Reflection\CB.Reflection.TypeEx.cs
 */

using CB.Data;
using CB.Reflection;
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace CB.Validation
{
	
	/// <summary>
	/// Something went wrong during the execution of a method.
	/// </summary>
	[Serializable]
	public sealed class InCodeConditionException : Exception
	{
		/// <summary>
		/// Something went wrong during the execution of a method.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public InCodeConditionException(string message, Exception innerException = null)
			: base(string.IsNullOrEmpty(message) ? "In-code condition (method assertion) exception." : message, innerException)
		{ }
		/// <summary>
		/// Something went wrong during the execution of a method.
		/// </summary>
		/// <param name="innerException"></param>
		public InCodeConditionException(Exception innerException = null)
			: this(null, innerException)
		{ }
	}

	/// <summary>
	/// Something went wrong during the initialization of a type.
	/// </summary>
	[Serializable]
	public sealed class InitializationConditionException : Exception
	{
		/// <summary>
		/// Something went wrong during the initialization of a type.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public InitializationConditionException(string message, Exception innerException = null)
			: base(string.IsNullOrEmpty(message) ? "The initialization of the type has failed." : message, innerException)
		{ }
		/// <summary>
		/// Something went wrong during the initialization of a type.
		/// </summary>
		/// <param name="initializingType"></param>
		/// <param name="innerException"></param>
		public InitializationConditionException(Type initializingType, Exception innerException = null)
			: this(string.Format("The initialization of the type {0} has failed.", initializingType.Name), innerException)
		{ }
	}

	/// <summary>
	/// A method requirement is not adequate.
	/// <para>It can be due to a parameter, or a more global reason. Please see the inner exception, if any.</para>
	/// </summary>
	[Serializable]
	public sealed class BeforeCodeConditionException : Exception
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		const string msg = "Pre-code condition (method requirement) exception.";
		/// <summary>
		/// A method requirement is not adequate.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public BeforeCodeConditionException(string message = null, Exception innerException = null)
			: base(string.IsNullOrEmpty(message) ? msg : message, innerException)
		{ }
		/// <summary>
		/// A method requirement is not adequate.
		/// </summary>
		/// <param name="innerException"></param>
		public BeforeCodeConditionException(Exception innerException)
			: base(msg, innerException)
		{ }
	}

	/// <summary>
	/// The final state is wrong.
	/// </summary>
	[Serializable]
	public sealed class AfterCodeConditionException : Exception
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		const string msg = "Post-code condition (method ensures) exception.";
		/// <summary>
		/// The final state is wrong.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public AfterCodeConditionException(string message = null, Exception innerException = null)
			: base(string.IsNullOrEmpty(message) ? msg : message, innerException)
		{ }
		/// <summary>
		/// The final state is wrong.
		/// </summary>
		/// <param name="innerException"></param>
		public AfterCodeConditionException(Exception innerException)
			: base(msg, innerException)
		{ }
	}

	/// <summary>
	/// Checks Contract-like conditions, both in release and in debug compilation mode.
	/// <para>In the context of <see cref="CB.Execution.Ret{T}">the error codes philosophy</see>, this class should be applied to non-planned errors (that is, programming errors) only.</para>
	/// </summary>
	public static class ReleaseCheck
	{
		static string _Formate(string formattedMessage, object[] messageParameters)
		{
			if (string.IsNullOrEmpty(formattedMessage))
				return null;
			return messageParameters.Length == 0 ?
					formattedMessage
					: string.Format(CultureInfo.InvariantCulture, formattedMessage, messageParameters);
		}

		/// <summary>
		/// Checks an assertion, even in release compilation mode.
		/// Message parameters are formatted using CultureInfo.InvariantCulture.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="formattedMessage"></param>
		/// <param name="messageParameters"></param>
		/// <example>Release.InCode(a == b, "{0} ≠ {1}", a, b);</example>
		public static void InCode(bool condition, string formattedMessage = null, params object[] messageParameters)
		{
			if (!condition)
				throw new InCodeConditionException(_Formate(formattedMessage, messageParameters));
		}

		/// <summary>
		/// Checks an assertion during a type initialization, even in release compilation mode.
		/// Message parameters are formatted using CultureInfo.InvariantCulture.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="formattedMessage"></param>
		/// <param name="messageParameters"></param>
		/// <example>Release.InCode(a == b, "{0} ≠ {1}", a, b);</example>
		public static void Initialization(bool condition, string formattedMessage, params object[] messageParameters)
		{
			if (!condition)
				throw new InitializationConditionException(_Formate(formattedMessage, messageParameters));
		}

		/// <summary>
		/// Checks an assertion during a type initialization, even in release compilation mode.
		/// Message parameters are formatted using CultureInfo.InvariantCulture.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="initializingType"></param>
		/// <example>Release.InCode(a == b, "{0} ≠ {1}", a, b);</example>
		public static void Initialization(bool condition, Type initializingType)
		{
			if (!condition)
				throw new InitializationConditionException(initializingType);
		}
		/// <summary>
		/// Throws an <see cref="CB.Validation.InCodeConditionException"/>.
		/// </summary>
		/// <param name="formattedMessage"></param>
		/// <param name="messageParameters"></param>
		public static void ThrowsInCode(string formattedMessage = null, params object[] messageParameters)
		{
			throw new InCodeConditionException(ReleaseCheck._Formate(formattedMessage, messageParameters));
		}

		/// <summary>
		/// Checks the string is not null nor empty.
		/// </summary>
		/// <param name="text"></param>
		public static void InCodeStringIsDefined(string text)
		{
			if (text == null)
				throw new InCodeConditionException(new ArgumentNullException());
			if (text == string.Empty)
				throw new InCodeConditionException(new ArgumentException());
		}

		/// <summary>
		/// Checks the reference is not null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		public static void ReferenceIsDefined<T>(T reference)
		{
			if (reference == null)
				throw new BeforeCodeConditionException(new ArgumentNullException());
		}

		/// <summary>
		/// Checks the action does not throw an exception.
		/// </summary>
		/// <param name="a"></param>
		public static void InCodeShouldNotThrowException(Action a)
		{
			if (a == null)
				throw new InCodeConditionException(new ArgumentNullException());
			try
			{
				a();
			}
			catch (Exception e)
			{
				throw new InCodeConditionException(e);
			}
		}
		/// <summary>
		/// Checks the function does not throw an exception.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <returns></returns>
		public static T InCodeShouldNotThrowException<T>(Func<T> a)
		{
			if (a == null)
				throw new InCodeConditionException(new ArgumentNullException());
			try
			{
				return a();
			}
			catch (Exception e)
			{
				throw new InCodeConditionException(e);
			}
		}

		/// <summary>
		/// Checks a condition at method ending, usually on the return value, even in release compilation mode.
		/// Message parameters are formatted using CultureInfo.InvariantCulture.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="formattedMessage"></param>
		/// <param name="messageParameters"></param>
		/// <example>Release.AfterCode(a == b, "{0} ≠ {1}", a, b);</example>
		public static void AfterCode(bool condition, string formattedMessage = null, params object[] messageParameters)
		{
			if (!condition)
				throw new AfterCodeConditionException(ReleaseCheck._Formate(formattedMessage, messageParameters));
		}
		/// <summary>
		/// Throws an <see cref="CB.Validation.AfterCodeConditionException"/>.
		/// </summary>
		/// <param name="formattedMessage"></param>
		/// <param name="messageParameters"></param>
		public static void ThrowsAfterCode(string formattedMessage = null, params object[] messageParameters)
		{
			throw new AfterCodeConditionException(ReleaseCheck._Formate(formattedMessage, messageParameters));
		}

		/// <summary>
		/// Checks a requirement at method beggining, usually on a parameter, even in release compilation mode.
		/// Message parameters are formatted using CultureInfo.InvariantCulture.
		/// 
		/// Note: if you check a method parameter, please call ParameterXXX() instead.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="formattedMessage"></param>
		/// <param name="messageParameters"></param>
		/// <example>Release.BeforeCode(a == b, "{0} ≠ {1}", a, b);</example>
		public static void BeforeCode(bool condition, string formattedMessage = null, params object[] messageParameters)
		{
			if (!condition)
				throw new BeforeCodeConditionException(ReleaseCheck._Formate(formattedMessage, messageParameters));
		}
		/// <summary>
		/// Throws a <see cref="CB.Validation.BeforeCodeConditionException"/>.
		/// </summary>
		/// <param name="formattedMessage"></param>
		/// <param name="messageParameters"></param>
		public static void ThrowsBeforeCode(string formattedMessage = null, params object[] messageParameters)
		{
			throw new BeforeCodeConditionException(ReleaseCheck._Formate(formattedMessage, messageParameters));
		}

#if WPF
		/// <summary>
		/// Checks the current thread is the same as the Dispatcher's thread.
		/// </summary>
		/// <param name="dispatcher">The Dispatcher we should be executing in its thread.</param>
		public static void DispatcherIsInUse(System.Windows.Threading.Dispatcher dispatcher)
		{
			ReleaseCheck.ParameterIsDefined(dispatcher);
			ReleaseCheck.ParameterIsDefined(dispatcher.Thread);
			if ((dispatcher.Thread != System.Threading.Thread.CurrentThread))
				throw new BeforeCodeConditionException(
					string.Format("The current Thread is not the Dispatcher's Thread. (Current's name={0} , Dispatcher's thread name={1}", System.Threading.Thread.CurrentThread.Name, dispatcher.Thread.Name));
		}
#endif

		/// <summary>
		/// Checks the parameter fullfills the condition.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="message">Optional error message.</param>
		/// <exception cref="CB.Validation.BeforeCodeConditionException"/>
		public static void Parameter(bool condition, string message=null)
		{
			if (!condition)
				throw new BeforeCodeConditionException(message, new ArgumentException());
		}

		/// <summary>
		/// Checks the parameter index is in the range of the collection.
		/// </summary>
		public static void ParameterIndexIsInRange(int index, ICollection collection)
		{
			if (index < 0 || index >= collection.Count)
				throw new BeforeCodeConditionException(new ArgumentOutOfRangeException());
		}

		/// <summary>
		/// Checks the parameter index is in the range of the array.
		/// </summary>
		public static void ParameterIndexIsInRange(int index, Array array)
		{
			if (index < array.GetLowerBound(0) || index > array.GetUpperBound(0))
				throw new BeforeCodeConditionException(new ArgumentOutOfRangeException());
		}

		/// <summary>
		/// Checks the parameters fullfill the condition.
		/// </summary>
		/// <param name="condition"></param>
		public static void Parameters(bool condition)
		{
			if (!condition)
				throw new BeforeCodeConditionException(new ArgumentException());
		}

		/// <summary>
		/// Checks the parameter is in the range.
		/// </summary>
		/// <typeparam name="T">An IComparable&lt;T&gt;</typeparam>
		/// <param name="value"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="minIncluded"></param>
		/// <param name="maxIncluded"></param>
		public static void ParameterIsInRange<T>(T value, T min, T max, bool minIncluded = true, bool maxIncluded = true)
			where T : IComparable<T>
		{
			var diffMin = value.CompareTo(min);
			var diffMax = value.CompareTo(max);
			if ((!minIncluded && diffMin <= 0) || (minIncluded && diffMin < 0) || (!maxIncluded && diffMax >= 0) || (maxIncluded && diffMax > 0))
				throw new BeforeCodeConditionException(new ArgumentOutOfRangeException());
		}

		/// <summary>
		/// Checks the reference is not null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		public static void InCodeReferenceIsDefined<T>(T reference)
			where T : class
		{
			if (reference == null)
				throw new InCodeConditionException(new NullReferenceException());
		}

		/// <summary>
		/// Checks the reference is not null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		public static void AfterCodeReferenceIsDefined<T>(T reference)
			where T : class
		{
			if (reference == null)
				throw new AfterCodeConditionException(new NullReferenceException());
		}

		/// <summary>
		/// Checks the reference is not null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		public static void ParameterIsDefined<T>(T reference)
			where T : class
		{
			if (reference == null)
				throw new BeforeCodeConditionException(new ArgumentNullException());
		}

		/// <summary>
		/// Checks the enumeration value is defined in its enumeration type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerationValue"></param>
		public static void ParameterEnumIsDefined<T>(T enumerationValue)
			where T : struct
		{
			if (!typeof(T).IsEnum())
				throw new BeforeCodeConditionException(new ArgumentException(string.Format("Type {0} is not an Enum.", typeof(T).Name)));
			if (!EnumHelper<T>.IsDefinedValue(enumerationValue))
				throw new BeforeCodeConditionException(new ArgumentOutOfRangeException("enumerationValue", enumerationValue, string.Format("Value {0} is not part of the enumeration type {1}", enumerationValue, typeof(T).FullName)));
		}

		/// <summary>
		/// Checks the enumeration value is one of the defined values in its enumeration type.
		/// <para>The value can not be a multiple values combination, even if <typeparamref name="T"/> has FlagsAttribute.</para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerationValue"></param>
		public static void ParameterEnumIsDefinedAsOneValue<T>(T enumerationValue)
			where T : struct
		{
			if (!typeof(T).IsEnum())
				throw new BeforeCodeConditionException(new ArgumentException(string.Format("Type {0} is not an Enum.", typeof(T).Name)));
			if (!EnumHelper<T>.IsDeclaredAsAUniqueValue(enumerationValue))
				throw new BeforeCodeConditionException(new ArgumentOutOfRangeException());
		}

		/// <summary>
		/// Checks the parameter is not null nor empty.
		/// </summary>
		/// <param name="text"></param>
		public static void ParameterStringIsDefined(string text)
		{
			if (text == null)
				throw new BeforeCodeConditionException(new ArgumentNullException());
			if (text == string.Empty)
				throw new BeforeCodeConditionException(new ArgumentException());
		}

	}

	/// <summary>
	/// Checks Contract-like conditions, but only in debug compilation mode.
	/// </summary>
	public static class DebugCheck
	{

		/// <summary>
		/// Checks an assertion in code, only in debug compilation mode.
		/// Message parameters are formatted using CultureInfo.InvariantCulture.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="formattedMessage"></param>
		/// <param name="messageParameters"></param>
		/// <example>Debug.InCode(a == b, "{0} ≠ {1}", a, b);</example>
		[Conditional("DEBUG")]
		public static void InCode(bool condition, string formattedMessage = null, params object[] messageParameters)
		{
#if DEBUG
#if CONTRACTS_FULL
			System.Diagnostics.Contracts.Contract.Assert(condition, Release.formate(formattedMessage, messageParameters));
#else
			ReleaseCheck.InCode(condition, formattedMessage, messageParameters);
#endif
#endif
		}

		/// <summary>
		/// Checks the string is not null nor empty.
		/// </summary>
		/// <param name="text"></param>
		[Conditional("DEBUG")]
		public static void InCodeStringIsDefined(string text)
		{
#if DEBUG
			ReleaseCheck.InCodeStringIsDefined(text);
#endif
		}

		/// <summary>
		/// Checks the reference is not null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		[Conditional("DEBUG")]
		public static void InCodeReferenceIsDefined<T>(T reference)
			where T : class
		{
#if DEBUG
			ReleaseCheck.InCodeReferenceIsDefined(reference);
#endif
		}

		/// <summary>
		/// Checks an assertion during a type initialization, even in release compilation mode.
		/// Message parameters are formatted using CultureInfo.InvariantCulture.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="formattedMessage"></param>
		/// <param name="messageParameters"></param>
		/// <example>Release.InCode(a == b, "{0} ≠ {1}", a, b);</example>
		[Conditional("DEBUG")]
		public static void Initialization(bool condition, string formattedMessage, params object[] messageParameters)
		{
#if DEBUG
			ReleaseCheck.Initialization(condition, formattedMessage, messageParameters);
#endif
		}

		/// <summary>
		/// Checks an assertion during a type initialization, even in release compilation mode.
		/// Message parameters are formatted using CultureInfo.InvariantCulture.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="initializingType"></param>
		/// <example>Release.InCode(a == b, "{0} ≠ {1}", a, b);</example>
		[Conditional("DEBUG")]
		public static void Initialization(bool condition, Type initializingType)
		{
#if DEBUG
			ReleaseCheck.Initialization(condition, initializingType);
#endif
		}

		/// <summary>
		/// Checks the reference is not null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		[Conditional("DEBUG")]
		public static void AfterCodeReferenceIsDefined<T>(T reference)
			where T : class
		{
#if DEBUG
			ReleaseCheck.AfterCodeReferenceIsDefined(reference);
#endif
		}
		/// <summary>
		/// Checks the action does not throw an exception.
		/// </summary>
		/// <param name="a"></param>
		[Conditional("DEBUG")]
		public static void InCodeShouldNotThrowException(Action a)
		{
#if DEBUG
			ReleaseCheck.InCodeShouldNotThrowException(a);
#endif
		}

#if false
		[Conditional("DEBUG")]
		public static void InCodeShouldNotThrowException<T>(Func<T> a, ref T r)
		{
#if DEBUG
			r = ReleaseCheck.InCodeShouldNotThrowException(a);
#endif
		}
#endif

		/// <summary>
		/// Checks a condition at method ending, usually on the return value, only in debug compilation mode.
		/// Message parameters are formatted using CultureInfo.InvariantCulture.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="formattedMessage"></param>
		/// <param name="messageParameters"></param>
		/// <example>Debug.AfterCode(a == b, "{0} ≠ {1}", a, b);</example>
		[Conditional("DEBUG")]
		public static void AfterCode(bool condition, string formattedMessage = null, params object[] messageParameters)
		{
#if DEBUG
#if CONTRACTS_FULL
			System.Diagnostics.Contracts.Contract.Ensures(condition, Release.formate(formattedMessage, messageParameters));
#else
			ReleaseCheck.AfterCode(condition, formattedMessage, messageParameters);
#endif
#endif
		}
		/// <summary>
		/// Throws an <see cref="CB.Validation.AfterCodeConditionException"/>.
		/// </summary>
		/// <param name="formattedMessage"></param>
		/// <param name="messageParameters"></param>
		[Conditional("DEBUG")]
		public static void ThrowsAfterCode(string formattedMessage = null, params object[] messageParameters)
		{
#if DEBUG
			ReleaseCheck.ThrowsAfterCode(formattedMessage, messageParameters);
#endif
		}
		/// <summary>
		/// Throws a <see cref="CB.Validation.BeforeCodeConditionException"/>.
		/// </summary>
		/// <param name="formattedMessage"></param>
		/// <param name="messageParameters"></param>
		[Conditional("DEBUG")]
		public static void ThrowsBeforeCode(string formattedMessage = null, params object[] messageParameters)
		{
#if DEBUG
			ReleaseCheck.ThrowsBeforeCode(formattedMessage, messageParameters);
#endif
		}
		/// <summary>
		/// Throws an <see cref="CB.Validation.InCodeConditionException"/>.
		/// </summary>
		/// <param name="formattedMessage"></param>
		/// <param name="messageParameters"></param>
		[Conditional("DEBUG")]
		public static void ThrowsInCode(string formattedMessage = null, params object[] messageParameters)
		{
#if DEBUG
			ReleaseCheck.ThrowsInCode(formattedMessage, messageParameters);
#endif
		}

		/// <summary>
		/// Checks a requirement at method beggining, usually on a parameter, only in debug compilation mode.
		/// Message parameters are formatted using CultureInfo.InvariantCulture.
		/// 
		/// Note: if you check a method parameter, please call ParameterXXX() instead.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="formattedMessage"></param>
		/// <param name="messageParameters"></param>
		/// <example>Debug.BeforeCode(a == b, "{0} ≠ {1}", a, b);</example>
		[Conditional("DEBUG")]
		public static void BeforeCode(bool condition, string formattedMessage = null, params object[] messageParameters)
		{
#if DEBUG
#if CONTRACTS_FULL
			System.Diagnostics.Contracts.Contract.Requires(condition, Release.formate(formattedMessage, messageParameters));
#else
			ReleaseCheck.BeforeCode(condition, formattedMessage, messageParameters);
#endif
#endif
		}

#if WPF
		/// <summary>
		/// Checks the current thread is the same as the Dispatcher's thread.
		/// </summary>
		/// <param name="dispatcher">The Dispatcher we should be executing in its thread.</param>
		public static void DispatcherIsInUse(System.Windows.Threading.Dispatcher dispatcher)
		{
			DebugCheck.ParameterIsDefined(dispatcher);
			DebugCheck.ParameterIsDefined(dispatcher.Thread);
			if ((dispatcher.Thread != System.Threading.Thread.CurrentThread))
				throw new BeforeCodeConditionException(
					string.Format("The current Thread is not the Dispatcher's Thread. (Current's name={0} , Dispatcher's thread name={1}", System.Threading.Thread.CurrentThread.Name, dispatcher.Thread.Name));
		}
#endif

		/// <summary>
		/// Checks the parameter fullfills the condition.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="message">Optional error messag.</param>
		/// <exception cref="CB.Validation.BeforeCodeConditionException"/>
		[Conditional("DEBUG")]
		public static void Parameter(bool condition, string message = null)
		{
#if DEBUG
			ReleaseCheck.Parameter(condition, message);
#endif
		}

		/// <summary>
		/// Checks the parameter index is in the range of the collection.
		/// </summary>
		[Conditional("DEBUG")]
		public static void ParameterIndexIsInRange(int index, ICollection collection)
		{
#if DEBUG
			ReleaseCheck.ParameterIndexIsInRange(index, collection);
#endif
		}

		/// <summary>
		/// Checks the parameter index is in the range of the array.
		/// </summary>
		[Conditional("DEBUG")]
		public static void ParameterIndexIsInRange(int index, Array array)
		{
#if DEBUG
			ReleaseCheck.ParameterIndexIsInRange(index, array);
#endif
		}

		/// <summary>
		/// Checks the parameters fullfill the condition.
		/// </summary>
		/// <param name="condition"></param>
		[Conditional("DEBUG")]
		public static void Parameters(bool condition)
		{
#if DEBUG
			ReleaseCheck.Parameters(condition);
#endif
		}

		/// <summary>
		/// Checks the parameter is in the range.
		/// </summary>
		/// <typeparam name="T">An IComparable&lt;T&gt;</typeparam>
		/// <param name="value"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="minIncluded"></param>
		/// <param name="maxIncluded"></param>
		[Conditional("DEBUG")]
		public static void ParameterIsInRange<T>(T value, T min, T max, bool minIncluded = true, bool maxIncluded = true)
			where T : IComparable<T>
		{
#if DEBUG
			ReleaseCheck.ParameterIsInRange(value, min, max, minIncluded, maxIncluded);
#endif
		}

		/// <summary>
		/// Checks the parameter is not null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		[Conditional("DEBUG")]
		public static void ParameterIsDefined<T>(T reference)
			where T : class
		{
#if DEBUG
			ReleaseCheck.ParameterIsDefined(reference);
#endif
		}

		/// <summary>
		/// Checks the enumeration value is defined in its enumeration type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumeration"></param>
		[Conditional("DEBUG")]
		public static void ParameterEnumIsDefined<T>(T enumeration)
			where T : struct
		{
#if DEBUG
			ReleaseCheck.ParameterEnumIsDefined(enumeration);
#endif
		}

		/// <summary>
		/// Checks the enumeration value is one of the defined values in its enumeration type.
		/// <para>The value can not be a multiple values combination, even if <typeparamref name="T"/> has FlagsAttribute.</para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumeration"></param>
		[Conditional("DEBUG")]
		public static void ParameterEnumIsDefinedAsOneValue<T>(T enumeration)
			where T : struct
		{
#if DEBUG
			ReleaseCheck.ParameterEnumIsDefinedAsOneValue(enumeration);
#endif
		}


		/// <summary>
		/// Checks the parameter is not null nor empty.
		/// </summary>
		/// <param name="text"></param>
		[Conditional("DEBUG")]
		public static void ParameterStringIsDefined(string text)
		{
#if DEBUG
			ReleaseCheck.ParameterStringIsDefined(text);
#endif
		}

	}

}