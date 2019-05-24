
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using CB.Reflection;
using System;

public partial class TestHelpers
{

	public void Test_CB_Reflection_MemberInfoOf_cs()
	{
		var t1=new Test_MemberInfoOf();
		t1.Test();
	}

	#region test 1

	public class Test_MemberInfoOf  //Test_MemberInfoOf
	{
		int Field1=1;
		int Property1 { get; }
		static int StaticField1=2;

		public void Test()
		{
			var fieldName = MemberInfoOf.InstanceField<Test_MemberInfoOf, int>(a => a.Field1).Name;
			var fieldType = MemberInfoOf.InstanceField<Test_MemberInfoOf, int>(a => a.Field1).FieldType;

			var propertyName = MemberInfoOf.InstanceProperty<Test_MemberInfoOf, int>(a => a.Property1).Name;
			var propertyType = MemberInfoOf.InstanceProperty<Test_MemberInfoOf, int>(a => a.Property1).PropertyType;

			var instanceFieldName = MemberInfoOf.InstanceFieldOrProperty<Test_MemberInfoOf, int> (u => u.Field1).Name;

			var thisInstanceFunctionName = MemberInfoOf.Function((Action)this.InstanceFunction).Name;
			var staticFunctionName = MemberInfoOf.Function((Func<int, double>)Test_MemberInfoOf.StaticFunction).Name;

			var instanceFunctionName = MemberInfoOf.InstanceFunction<Test_MemberInfoOf> (u => (Action)u.InstanceFunction);

			var staticFieldName = MemberInfoOf.StaticFieldOrProperty(() => Test_MemberInfoOf.StaticField1).Name;
		}

		static void Main2(string[] args)
		{
			var argsName = MemberInfoOf.Parameter(() => args);
		}

		void InstanceFunction()
		{
			
		}

		static double StaticFunction(int a)
		{
			return a;
		}

	}

	#endregion test 1

}