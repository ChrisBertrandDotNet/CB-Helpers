
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net


using CB.Data;

public partial class TestHelpers
{
	public void Test_CB_Data_NullableAny_cs()
	{
		var r1 = GetStringRepresentation<object>(null);
		var r2 = GetStringRepresentation<int>(123);

		var faux = IfStringIsHello(123);
		var vrai = IfStringIsHello("Hello");

		var asObject = AsAStringOrADouble(System.TimeSpan.Zero);
		var asString = AsAStringOrADouble("hello");
		var asDouble = AsAStringOrADouble(1.0);
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