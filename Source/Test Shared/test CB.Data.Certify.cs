
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using CB.Data;
using System;
using System.Globalization;

public partial class TestHelpers
{
	public void Test_CB_Data_Certify_cs()
	{
		NonNull<CultureInfo> culture = CultureInfo.CurrentUICulture; // implicitly transcoded. 'culture' is guaranted not to be null.

		DefinedString name = CultureInfo.CurrentUICulture.Name; // implicitly transcoded. 'name' is guaranted not to be null or empty.

		DefinedEnum<System.PlatformID> Windows = System.PlatformID.Win32NT; // ok, this value is declared in its enumeration.
		Release.ExceptionIsExpected<ArgumentException>(() => { DefinedEnum<System.PlatformID> MyOwnSystem = (System.PlatformID)1000; }); // Wrong ! That will throw an ArgumentException.

		#region Bounded values thanks to a specific class

		var number1 = new Percentage(50); // ok, 50 is between 0 and 100.
		Release.ExceptionIsExpected<ArgumentOutOfRangeException>(() => { var number2 = new Percentage(-200); }); // Wrong ! That will throw an ArgumentOutOfRangeException();

		Minute m = 30;
		var m2 = Minute.TryCreate(100); // returns null because this value is not in the range.
		Release.ExceptionIsExpected<ArgumentOutOfRangeException>(() => { var m3 = new Minute(1000); });// Wrong ! That will throw an ArgumentOutOfRangeException();

		#endregion Bounded values thanks to a specific class

		#region Bounded value using a simple range generator

		var angleRange = Bounded<double>.CreateASimpleRange(0, 360, true, false); // Serves as a range for angles.
		var angle180 = angleRange.NewValue(180.0); // ok, that angle is in the range.
		var certifiedAngle270 = angleRange.NewValueAsNonNull(270);
		var wrongAngle = angleRange.TryCreateValue(1000); // Wrong, the value is not in the in the range. The result is null.
		var certifiedAngle = angleRange.NewValueAsNonNull(200); // returned type: NonNull<IBounded<double>>.

		#endregion Bounded value using a simple range generator
	}

	DefinedString ToUpper(DefinedString s)
	{
		// No use to check the parameter: it can't be null or String.Empty.
		return s.Value.ToUpper(); // We certify the return value is defined.
	}

	NonNull<TextInfo> GetTextInfo(NonNull<CultureInfo> culture)
	{
		// No use to check the parameter: it can't be null.
		return culture.Value.TextInfo; // We certify the return value is defined.
	}

	string UnintializedNonNull()
	{
		var nullRef = default(NonNull<CultureInfo>); // Never write that !
		return nullRef.Value.Name; // Will throw an InvalidOperationException.
	}

	DefinedEnum<UnicodeCategory> GetUnicodeCategory(char c)
	{
		return char.GetUnicodeCategory(c); // We certify the return value is a declared value of its enumeration.
	}

}

/// <summary>
/// A number bounded in [ 0 ; 60 [
/// </summary>
internal class Minute : Bounded<int>
{
	public override int Minimum => 0;

	public override int Maximum => 60;

	public override bool RangeIncludesTheMinimum => true;

	public override bool RangeIncludesTheMaximum => false;

	public Minute(int value)
		:base(value)
	{	}

	public static Minute TryCreate(int value)
	{
		if (value >= 0 && value < 60)
			return new Minute(value);
		return null;
	}

	public static implicit operator Minute(int value)
	{
		return new Minute(value);
	}
}

/// <summary>
/// A number bounded in [ 0 ; 100 [
/// </summary>
internal class Percentage : Bounded<int>
{
	public override int Minimum => 0;

	public override int Maximum => 100;

	public override bool RangeIncludesTheMinimum => true;

	public override bool RangeIncludesTheMaximum => false;

	public Percentage(int value)
		: base(value)
	{ }

	public static Percentage TryCreate(int value)
	{
		if (value >= 0 && value < 100)
			return new Percentage(value);
		return null;
	}

	public static implicit operator Percentage(int value)
	{
		return new Percentage(value);
	}
}

