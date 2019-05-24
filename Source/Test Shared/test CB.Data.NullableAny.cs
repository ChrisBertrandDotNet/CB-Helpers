
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net


using CB.Data;

public partial class TestHelpers
{
	public void Test_CB_Data_NullableAny_cs()
	{
		Release.Assert(GetStringRepresentation<object>(null) == "! undefined !");
		Release.Assert(GetStringRepresentation<int>(123) == "123");

		Release.Assert(IfStringIsHello(123).HasNoValue);
		Release.Assert(IfStringIsHello("Hello").HasValue);

		Release.Assert(AsAStringOrADouble(System.TimeSpan.Zero).HasNoValue);
		Release.Assert(AsAStringOrADouble("hello").HasValue);
		Release.Assert(AsAStringOrADouble(1.0).HasValue);
	}

	string GetStringRepresentation<T>(NullableAny<T> obj)
	{
		if (obj.HasValue) return obj.Value.ToString();
		else return "! undefined !";
	}

	NullableAny<T> IfStringIsHello<T>(T obj)
	{
		if (obj == null || obj.ToString() != "Hello") return default(NullableAny<T>);
		else return obj;
	}

	NullableAny<T> AsAStringOrADouble<T>(T obj)
	{
		if (obj is string || obj is double) return obj;
		else return NullableAny<T>.Undefined;
	}
}