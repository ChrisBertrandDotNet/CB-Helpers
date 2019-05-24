
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using System;

[Serializable]
public class AssertionFailureException : Exception
{
	public AssertionFailureException()
		:base("Assertion failed.")
	{ }
}

public static class Release
{
	public static void Assert(bool condition)
	{
		if (!condition) throw new AssertionFailureException();
	}

	public static E ExceptionIsExpected<E>(Action action)
		where E:Exception
	{
		try
		{
			action();
		}
		catch(E ex)
		{
			return ex;
		}
		throw new AssertionFailureException();
	}
}