// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/* Requires:
 * (nothing)
*/

/*
 */

namespace CB.Data
{
	/// <summary>
	/// Gives an empty array of type T.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public static class EmptyArray<T>
    {
        /// <summary>
        /// An empty array of type T.
        /// <para>Which is a IList of T at the same time. Unlike a List of T, it cannot grow.</para>
        /// </summary>
        public readonly static T[] Empty = new T[0];
    }

    // NOTE: there is no EmptyList, since a list can grow.
}