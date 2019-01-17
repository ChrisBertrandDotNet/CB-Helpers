
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/* Requires:
 */

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CB.Reflection
{
	/// <summary>
	/// Helpers for reflection classes such as <see cref="System.Reflection.MemberInfo"/>.
	/// </summary>
	public static partial class Reflection
	{
		/// <summary>
		/// Functional comparer (unlike FieldInfo.Equals or FieldInfo.== that do not work correctly).
		/// </summary>
		/// <param name="f1"></param>
		/// <param name="f2"></param>
		/// <returns></returns>
		public static bool IsEqualTo(this FieldInfo f1, FieldInfo f2)
		{
			return FieldInfoComparer.StaticComparer.Equals(f1, f2);
		}

		/// <summary>
		/// Functional comparer (unlike PropertyInfo.Equals or PropertyInfo.== that do not work correctly).
		/// </summary>
		/// <param name="f1"></param>
		/// <param name="f2"></param>
		/// <returns></returns>
		public static bool IsEqualTo(this PropertyInfo f1, PropertyInfo f2)
		{
			return PropertyInfoComparer.StaticComparer.Equals(f1, f2);
		}

		/// <summary>
		/// Functional comparer (unlike MemberInfo.Equals or MemberInfo.== that do not work correctly).
		/// </summary>
		/// <param name="f1"></param>
		/// <param name="f2"></param>
		/// <returns></returns>
		public static bool IsEqualTo(this MemberInfo f1, MemberInfo f2)
		{
			return MemberInfoComparer.StaticComparer.Equals(f1, f2);
		}

		/// <summary>
		/// Functional comparer (unlike MethodInfo.Equals or MethodInfo.== that do not work correctly).
		/// </summary>
		/// <param name="f1"></param>
		/// <param name="f2"></param>
		/// <returns></returns>
		public static bool IsEqualTo(this MethodInfo f1, MethodInfo f2)
		{
			return MethodInfoComparer.StaticComparer.Equals(f1, f2);
		}

		/// <summary>
		/// Functional comparer (unlike ParameterInfo.Equals or ParameterInfo.== that do not work correctly).
		/// </summary>
		/// <param name="f1"></param>
		/// <param name="f2"></param>
		/// <returns></returns>
		public static bool IsEqualTo(this ParameterInfo f1, ParameterInfo f2)
		{
			return ParameterInfoComparer.StaticComparer.Equals(f1, f2);
		}
	}

	/// <summary>
	/// The standard comparison of FieldInfo is incorrect. This comparer fills the gap.
	/// <para>It can be used in the constructor of various collection types, as HashSet and Dictionary.</para>
	/// </summary>
	public class FieldInfoComparer : IEqualityComparer<FieldInfo>
	{
		/// <summary>
		/// The standard comparison of FieldInfo is incorrect. This comparer fills the gap.
		/// </summary>
		public static FieldInfoComparer StaticComparer = new FieldInfoComparer();

		/// <summary>
		/// True if x really equals y.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool Equals(FieldInfo x, FieldInfo y)
		{
			return x.GetType() == y.GetType()
				&& x.DeclaringType == y.DeclaringType
				&& x.Name == y.Name; // a name usually can not be used for several fields, in the same type.
		}

		/// <summary>
		/// Returns a hash code.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int GetHashCode(FieldInfo obj)
		{
			return obj.DeclaringType.FullName.GetHashCode() ^ obj.Name.GetHashCode() ^ obj.MemberType.GetHashCode();
		}
	}


	/// <summary>
	/// The standard comparison of PropertyInfo is incorrect. This comparer fills the gap.
	/// <para>It can be used in the constructor of various collection types, as HashSet and Dictionary.</para>
	/// </summary>
	public class PropertyInfoComparer : IEqualityComparer<PropertyInfo>
	{
		/// <summary>
		/// The standard comparison of PropertyInfo is incorrect. This comparer fills the gap.
		/// </summary>
		public static PropertyInfoComparer StaticComparer = new PropertyInfoComparer();

		/// <summary>
		/// True if x really equals y.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool Equals(PropertyInfo x, PropertyInfo y)
		{
			return x.GetType() == y.GetType()
				&& x.DeclaringType == y.DeclaringType
				&& x.Name == y.Name; // a name usually can not be used for several properties, in the same type.
		}
		/// <summary>
		/// Returns a hash code.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int GetHashCode(PropertyInfo obj)
		{
			return obj.DeclaringType.FullName.GetHashCode() ^ obj.Name.GetHashCode() ^ obj.MemberType.GetHashCode();
		}
	}

	/// <summary>
	/// The standard comparison of MemberInfo is incorrect. This comparer fills the gap.
	/// <para>It can be used in the constructor of various collection types, as HashSet and Dictionary.</para>
	/// </summary>
	public class MemberInfoComparer : IEqualityComparer<MemberInfo>
	{
		/// <summary>
		/// The standard comparison of MemberInfo is incorrect. This comparer fills the gap.
		/// </summary>
		public static MemberInfoComparer StaticComparer = new MemberInfoComparer();

		/// <summary>
		/// True if x really equals y.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool Equals(MemberInfo x, MemberInfo y)
		{
			var t = x.GetType();
			if (t != y.GetType())
				return false;
			if (t == typeof(FieldInfo))
				return ((FieldInfo)x).IsEqualTo((FieldInfo)y);
			if (t == typeof(MethodInfo))
				return ((MethodInfo)x).IsEqualTo((MethodInfo)y);
			if (t == typeof(PropertyInfo))
				return ((PropertyInfo)x).IsEqualTo((PropertyInfo)y);
			return object.ReferenceEquals(x, y);
		}
		/// <summary>
		/// Returns a hash code.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int GetHashCode(MemberInfo obj)
		{
			return obj.DeclaringType.FullName.GetHashCode() ^ obj.Name.GetHashCode() ^ obj.MemberType.GetHashCode();
		}
	}

	/// <summary>
	/// The standard comparison of MethodInfo is incorrect. This comparer fills the gap.
	/// <para>It can be used in the constructor of various collection types, as HashSet and Dictionary.</para>
	/// </summary>
	public class MethodInfoComparer : IEqualityComparer<MethodInfo>
	{
		/// <summary>
		/// The standard comparison of MethodInfo is incorrect. This comparer fills the gap.
		/// </summary>
		public static MethodInfoComparer StaticComparer = new MethodInfoComparer();

		/// <summary>
		/// True if x really equals y.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool Equals(MethodInfo x, MethodInfo y)
		{
			var xPars = x.GetParameters();
			var yPars = y.GetParameters();

			return x.GetType() == y.GetType()
				&& x.DeclaringType == y.DeclaringType
				&& x.Name == y.Name
				&& xPars.Length == yPars.Length
				&& xPars.All(mi => yPars.Contains(mi, ParameterInfoComparer.StaticComparer));// usually, a name can't be used in several methods that have the parameters, in the same type.
		}
		/// <summary>
		/// Returns a hash code.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int GetHashCode(MethodInfo obj)
		{
			return obj.DeclaringType.FullName.GetHashCode() ^ obj.Name.GetHashCode() ^ obj.MemberType.GetHashCode();
		}
	}

	/// <summary>
	/// The standard comparison of ParameterInfo is incorrect. This comparer fills the gap.
	/// <para>It can be used in the constructor of various collection types, as HashSet and Dictionary.</para>
	/// </summary>
	public class ParameterInfoComparer : IEqualityComparer<ParameterInfo>
	{
		/// <summary>
		/// The standard comparison of ParameterInfo is incorrect. This comparer fills the gap.
		/// </summary>
		public static ParameterInfoComparer StaticComparer = new ParameterInfoComparer();

		/// <summary>
		/// True if x really equals y.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool Equals(ParameterInfo x, ParameterInfo y)
		{
			return x.GetType() == y.GetType()
				&& x.Member.IsEqualTo(y.Member)
				&& x.Name == y.Name; // a name usually can not be used for several members, in the same type.
		}
		/// <summary>
		/// Returns a hash code.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int GetHashCode(ParameterInfo obj)
		{
			return obj.Member.Name.GetHashCode() ^ obj.Name.GetHashCode() ^ obj.ParameterType.Name.GetHashCode();
		}
	}

}