
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using CB.Execution;
using System;

public partial class TestHelpers
{

	public void Test_CB_Execution_Return_cs()
	{
		Release.Assert(this.Sqrt(-1.0).ErrorCode == ReturnSuccess.ArgumentOutOfRange);

		Release.Assert(this.DisplayLine(100) == ReturnSuccess.Success);

		Release.Assert(CertifiesStringIsDefined(null).ErrorCode == MyErrorCodes.StringIsNull);
		Release.Assert(CertifiesStringIsDefined(string.Empty).ErrorCode == MyErrorCodes.StringIsEmpty);
		Release.Assert(CertifiesStringIsDefined("good").ErrorCode == MyErrorCodes.Success);

	}

	[ReturnSuccessCodes(ReturnSuccess.ArgumentOutOfRange)]
	Ret<double> Sqrt(double d)
	{
		if (d < 0.0) return ReturnSuccess.ArgumentOutOfRange;
		return Math.Sqrt(d);
	}

	[ReturnSuccessCodes(ReturnSuccess.ArgumentOutOfRange)]
	ReturnSuccess DisplayLine(int lineNumber)
	{
		if (lineNumber < 0) return ReturnSuccess.ArgumentOutOfRange;
		// do some work
		return ReturnSuccess.Success;
	}


	enum MyErrorCodes
	{ NotInitialized, Success, StringIsEmpty, StringIsNull }

	[ReturnCodes((int)MyErrorCodes.StringIsEmpty, (int)MyErrorCodes.StringIsNull)]
	ReturnNonNull<String, MyErrorCodes> CertifiesStringIsDefined(string p)
	{
		if (p == null)
			return new ReturnNonNull<string, MyErrorCodes>(MyErrorCodes.StringIsNull);
		if (p == string.Empty)
			return new ReturnNonNull<string, MyErrorCodes>(MyErrorCodes.StringIsEmpty);
		return new ReturnNonNull<string, MyErrorCodes>(p);
	}

}