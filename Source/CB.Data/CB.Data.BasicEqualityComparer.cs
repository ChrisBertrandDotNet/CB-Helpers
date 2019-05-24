
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net


/* Requires:
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CB.Data
{
	/// <summary>
	/// This comparer uses <see cref="System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode()"/> rather than potentially overriden <see cref="System.Object.GetHashCode()"/>.
	/// <para>That way, a dictionary or a hash set can find an instance item in its collection even when the value of this instance's fields changed.</para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class BasicEqualityComparer<T> : IEqualityComparer<T>
	{
		public static readonly BasicEqualityComparer<T> Default = new BasicEqualityComparer<T>();
		readonly Func<T, T, bool> GenericEqualityComparison;

		public BasicEqualityComparer()
			: this(EqualityComparer<T>.Default.Equals)
		{ }

		public BasicEqualityComparer(Func<T, T, bool> genericEqualityComparison)
		{
			Debug.Assert(genericEqualityComparison != null);
			this.GenericEqualityComparison = genericEqualityComparison;
		}

		public bool Equals(T x, T y)
		{
			return x != null ? this.GenericEqualityComparison(x, y) : false;
		}

		/// <summary>
		///  Uses <see cref="System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode()"/> rather than potentially overriden <see cref="System.Object.GetHashCode()"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int GetHashCode(T obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}
}