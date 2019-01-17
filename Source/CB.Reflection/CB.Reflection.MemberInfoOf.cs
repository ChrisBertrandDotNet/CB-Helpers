
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/* Example:
	public class TestClass //Test_MemberInfoOf
	{
		int Field1;
		int Property1 { get; }
		static int StaticField1;

		void test()
		{
			var fieldName = MemberInfoOf.InstanceField<TestClass, int>(a => a.Field1).Name;
			var fieldType = MemberInfoOf.InstanceField<TestClass, int>(a => a.Field1).FieldType;

			var propertyName = MemberInfoOf.InstanceProperty<TestClass, int>(a => a.Property1).Name;
			var propertyType = MemberInfoOf.InstanceProperty<TestClass, int>(a => a.Property1).PropertyType;

			var instanceFieldName = MemberInfoOf.InstanceFieldOrProperty<TestClass, int> (u => u.Field1).Name;

			var thisInstanceFunctionName = MemberInfoOf.Function((Action)this.InstanceFunction).Name;
			var staticFunctionName = MemberInfoOf.Function((Func<int, double>)TestClass.StaticFunction).Name;

			var instanceFunctionName = MemberInfoOf.InstanceFunction<TestClass> (u => (Action)u.InstanceFunction);

			var staticFieldName = MemberInfoOf.StaticFieldOrProperty(() => TestClass.StaticField1).Name;
		}

		static void Main(string[] args)
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
*/

#if TEST
using CBdotnet.Test;
#endif
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace CB.Reflection
{
	/// <summary>
	/// Retreives a <see cref="System.Reflection.MemberInfo"/>.
	/// Note: These functions use reflection, therefore can be slow.
	/// </summary>
	public static class MemberInfoOf
	{

		/// <summary>
		/// Gets the <see cref="System.Reflection.MemberInfo"/> of a function parameter.
		/// Example in Main() : var argsName = MemberInfoOf.Parameter(()=>args);
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="f"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static MemberInfo Parameter<T>(Expression<Func<T>> f)
		{
			return ((MemberExpression)(f.Body)).Member;
		}
#if TEST
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), Test]
		static void Parameter_test()
		{
			MemberInfoOf.Parameter_testPart2(0);
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		static void Parameter_testPart2(params int[] args)
		{
			var miParamètre = MemberInfoOf.Parameter(() => args);
			Testeur.TesteÉgalité(miParamètre.Name, "args");
		}
#endif

		/// <summary>
		/// Gets the <see cref="System.Reflection.MemberInfo"/> of a static data member (field or property).
		/// Example with a static class field: <code>var staticFieldName = MemberInfoOf.StaticFieldOrProperty(() => Program.staticField).Name;</code>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="f"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static MemberInfo StaticFieldOrProperty<T>(Expression<Func<T>> f)
		{
			return ((MemberExpression)(f.Body)).Member;
		}
#if TEST
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), Test]
		static void StaticFieldOrProperty_test()
		{
			var miChampStatique = MemberInfoOf.StaticFieldOrProperty(() => test.champStatique);
			Testeur.TesteÉgalité(miChampStatique.Name, "champStatique");
			Testeur.TesteÉgalité(miChampStatique, typeof(test).GetField("champStatique", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
		}
		class test
		{
			internal static int champStatique=0;
			internal int champ=1;
			internal int propriété=2;
			internal static int staticFunction(string t)
			{ return t.Length; }
			internal int InstanceFunction(string t)
			{ return t.Length; }
		}
#endif

		/// <summary>
		/// Gets the <see cref="System.Reflection.MemberInfo"/> of an instance field.
		/// <para>
		/// Example: <code>var fieldName = MemberInfoOf.InstanceField&lt;TestClass, int&gt;(a =&gt; a.Field1).Name;</code>
		/// Example: <code>var fieldType = MemberInfoOf.InstanceField&lt;TestClass, int&gt;(a =&gt; a.Field1).FieldType;</code>
		/// </para>
		/// </summary>
		/// <typeparam name="TC">Main type</typeparam>
		/// <typeparam name="TM">Member type</typeparam>
		/// <param name="f"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static FieldInfo InstanceField<TC, TM>(Expression<Func<TC, TM>> f)
		{
			return (FieldInfo)(((MemberExpression)(f.Body)).Member);
		}

		/// <summary>
		/// Gets the <see cref="System.Reflection.MemberInfo"/> of an instance property.
		/// <para>
		/// Example: <code>var propertyName = MemberInfoOf.InstanceProperty&lt;TestClass, int&gt;(a =&gt; a.Property1).Name;</code>
		/// Example: <code>var propertyType = MemberInfoOf.InstanceProperty&lt;TestClass, int&gt;(a =&gt; a.Property1).PropertyType;</code>
		/// </para>
		/// </summary>
		/// <typeparam name="TC">Main type</typeparam>
		/// <typeparam name="TM">Member type</typeparam>
		/// <param name="f"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static PropertyInfo InstanceProperty<TC, TM>(Expression<Func<TC, TM>> f)
		{
			return (PropertyInfo)(((MemberExpression)(f.Body)).Member);
		}

		/// <summary>
		/// Gets the <see cref="System.Reflection.MemberInfo"/> of an instance member (field or property).
		/// Note: you do not need an actual instance.
		/// Example with an instance class field: <code>var instanceFieldName = MemberInfoOf.InstanceFieldOrProperty&lt;Program, int&gt;(u =&gt; u.instanceField).Name;</code>
		/// </summary>
		/// <typeparam name="TC">Main type</typeparam>
		/// <typeparam name="TM">Member type</typeparam>
		/// <param name="f"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static MemberInfo InstanceFieldOrProperty<TC, TM>(Expression<Func<TC, TM>> f)
		{
			return ((MemberExpression)(f.Body)).Member;
		}
#if TEST
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), Test]
		static void InstanceFieldOrProperty_test()
		{
			var miChamp = MemberInfoOf.InstanceFieldOrProperty<test, int>(u => u.champ);
			Testeur.TesteÉgalité(miChamp.Name, "champ");
			Testeur.TesteÉgalité(miChamp, typeof(test).GetField("champ", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));

			var miPropriété = MemberInfoOf.InstanceFieldOrProperty<test, int>(u => u.propriété);
			Testeur.TesteÉgalité(miPropriété.Name, "propriété");
			Testeur.TesteÉgalité(miPropriété, typeof(test).GetField("propriété", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
		}
#endif

		/// <summary>
		/// Gets the <see cref="System.Reflection.MemberInfo"/> of an instance or static function.
		/// Note: for an instance function on a class type, you need an actual instance (null is not valid).
		/// Example: <code>var thisInstanceFunctionName = MemberInfoOf.Function((Action)this.InstanceFunction).Name;</code>
		/// Example: <code>var staticFunctionName = MemberInfoOf.Function((Func&lt;MemberInfo, int&gt;)test.staticFunction).Name;</code>
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public static MemberInfo Function(Delegate f)
		{
			return f.Method;
		}
#if TEST
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), Test]
		static void Function_test()
		{
			var staticFunctionMI = MemberInfoOf.Function((Func<string, int>)test.staticFunction);
			Testeur.TesteÉgalité(staticFunctionMI.Name, "staticFunction");
			Testeur.TesteÉgalité(staticFunctionMI, typeof(test).GetMethod("staticFunction", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));

			var thisInstanceFunctionMI = MemberInfoOf.Function((Func<string, int>)new test().InstanceFunction);
			Testeur.TesteÉgalité(thisInstanceFunctionMI.Name, "InstanceFunction");
			Testeur.TesteÉgalité(thisInstanceFunctionMI, typeof(test).GetMethod("InstanceFunction", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
		}
#endif

#if false // not very practicle. Example: var instanceFunctionName4 = MemberInfoOf4((Expression<Action<Program>>)(u => u.InstanceFunction("r")));
		public static MemberInfo MemberInfoOf4(LambdaExpression f)
		{
			return ((System.Linq.Expressions.MethodCallExpression)f.Body).Method.Name;
		}
#endif

		/// <summary>
		/// Gets the <see cref="System.Reflection.MemberInfo"/> of an instance (not static) function.
		/// Note: you do not need an actual instance.
		/// Example: var instanceFunctionName = MemberInfoOf.InstanceFunction&lt;Program&gt;(u => (Func&lt;MemberInfo, int&gt;)u.InstanceFunction);
		/// </summary>
		/// <typeparam name="TC">Main type</typeparam>
		/// <param name="f"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static MemberInfo InstanceFunction<TC>(Expression<Func<TC, Delegate>> f)
		{
			return ((System.Reflection.MethodInfo)((ConstantExpression)((MethodCallExpression)((System.Linq.Expressions.UnaryExpression)f.Body).Operand).Object).Value);

			/*var o=((System.Linq.Expressions.UnaryExpression)f.Body).Operand;
			var o2 = (MethodCallExpression)o;
			var ce = (ConstantExpression) o2.Object;
			var mi = (System.Reflection.MethodInfo) ce.Value;
			return mi;*/
		}
#if TEST
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), Test]
		static void InstanceFunction_test()
		{
			var instanceFunctionMI = MemberInfoOf.InstanceFunction<test>(u => (Func<string, int>)u.InstanceFunction);
			Testeur.TesteÉgalité(instanceFunctionMI.Name, "InstanceFunction");
			Testeur.TesteÉgalité(instanceFunctionMI, typeof(test).GetMethod("InstanceFunction", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
		}
#endif


	}
}