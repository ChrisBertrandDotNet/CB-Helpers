
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

#if TEST
using CBdotnet.Test;
#endif
using System;
using System.Linq.Expressions;

namespace CB.Reflection
{
	/// <summary>	
	/// Similar to 'nameof' in C# ≤ 5 (and VS ≤ 2013).
	/// Note: These functions use reflection, therefore are much slower than 'nameof'.
	/// </summary>
	public static class NameOf
	{

		/// <summary>
		/// Gets the name of a function parameter.
		/// Example in Main() : var argsName = NameOf.Parameter(()=&gt;args);
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="f"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static string Parameter<T>(Expression<Func<T>> f)
		{
			return ((MemberExpression)(f.Body)).Member.Name;
		}
#if TEST
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), Test]
		static void Parameter_test()
		{
			NameOf.Parameter_testPart2(0);
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		static void Parameter_testPart2(params int[] args)
		{
			var nomParamètre = NameOf.Parameter(() => args);
			Testeur.TesteÉgalité(nomParamètre, "args");
		}
#endif

		/// <summary>
		/// Gets the name of a static data member (field or property).
		/// Example with a static class field: var staticFieldName = NameOf.StaticFieldOrProperty(() =&gt; Program.staticField);
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="f"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static string StaticFieldOrProperty<T>(Expression<Func<T>> f)
		{
			return ((MemberExpression)(f.Body)).Member.Name;
		}
#if TEST
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), Test]
		static void StaticFieldOrProperty_test()
		{
			var nomChampStatique = NameOf.StaticFieldOrProperty(() => test.champStatique);
			Testeur.TesteÉgalité(nomChampStatique, "champStatique");
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
		/// Gets the name of an instance member (field or property).
		/// Note: you do not need an actual instance.
		/// Example with an instance class field: var instanceFieldName = NameOf&lt;Program, int&gt;(u =&gt; u.instanceField);
		/// </summary>
		/// <typeparam name="TC">The type.</typeparam>
		/// <typeparam name="TM">The member type.</typeparam>
		/// <param name="f"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static string InstanceFieldOrProperty<TC, TM>(Expression<Func<TC, TM>> f)
		{
			return ((MemberExpression)(f.Body)).Member.Name;
		}
#if TEST
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), Test]
		static void InstanceFieldOrProperty_test()
		{
			var nChamp = NameOf.InstanceFieldOrProperty<test, int>(u => u.champ);
			Testeur.TesteÉgalité(nChamp, "champ");
			var nPropriété = NameOf.InstanceFieldOrProperty<test, int>(u => u.propriété);
			Testeur.TesteÉgalité(nPropriété, "propriété");
		}
#endif

		/// <summary>
		/// Gets the name of an instance or static function.
		/// Note: for an instance function on a class type, you need an actual instance (null is not valid).
		/// Example: var thisInstanceFunctionName = NameOf.Function((Action)this.InstanceFunction);
		/// Example: var staticFunctionName = NameOf.Function((Func&lt;string, int&gt;)test.staticFunction);
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public static string Function(Delegate f)
		{
			return f.Method.Name;
		}
#if TEST
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), Test]
		static void Function_test()
		{
			var staticFunctionName = NameOf.Function((Func<string, int>)test.staticFunction);
			Testeur.TesteÉgalité(staticFunctionName, "staticFunction");
			var thisInstanceFunctionName = NameOf.Function((Func<string, int>)new test().InstanceFunction);
			Testeur.TesteÉgalité(thisInstanceFunctionName, "InstanceFunction");
		}
#endif

#if false // not very practicle. Exemple: var instanceFunctionName4 = NameOf4((Expression<Action<Program>>)(u => u.InstanceFunction("r")));
		public static string NameOf4(LambdaExpression f)
		{
			return ((System.Linq.Expressions.MethodCallExpression)f.Body).Method.Name;
		}
#endif

		/// <summary>
		/// Gets the name of an instance (not static) function.
		/// Note: you do not need an actual instance.
		/// Example: var instanceFunctionName = NameOf.InstanceFunction&lt;Program&gt;(u =&gt; (Func&lt;string, int&gt;)u.InstanceFunction);
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static string InstanceFunction<TC>(Expression<Func<TC, Delegate>> f)
		{
			return ((System.Reflection.MethodInfo)((ConstantExpression)((MethodCallExpression)((System.Linq.Expressions.UnaryExpression)f.Body).Operand).Object).Value).Name;

			/*var o=((System.Linq.Expressions.UnaryExpression)f.Body).Operand;
			var o2 = (MethodCallExpression)o;
			var ce = (ConstantExpression) o2.Object;
			var mi = (System.Reflection.MethodInfo) ce.Value;
			return mi.Name;*/
		}
#if TEST
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), Test]
		static void InstanceFunction_test()
		{
			var instanceFunctionName = NameOf.InstanceFunction<test>(u => (Func<string, int>)u.InstanceFunction);
			Testeur.TesteÉgalité(instanceFunctionName, "InstanceFunction");
		}
#endif


	}
}