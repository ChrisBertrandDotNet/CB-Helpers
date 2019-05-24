
// Copyright (c) Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/* Requires:
 * (nothing)
*/

/*
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace CB.Data
{
	public static class EnumerableExtensions
	{

		public static bool NextItem<T>(this IList<T> list, T item, out T nextItem)
		{
			var i = list.IndexOf(item);
			if (i >= 0)
			{
				i++;
				if (i < list.Count)
				{
					nextItem = list[i];
					return true;
				}
			}
			nextItem = default(T);
			return false;
		}

		public static T NextItemOrDefault<T>(this IList<T> list, T item)
			where T : class
		{
			var i = list.IndexOf(item);
			if (i >= 0)
			{
				i++;
				if (i < list.Count)
					return list[i];
			}
			return null;
		}

		public static bool PreviousItem<T>(this IList<T> list, T item, out T previousItem)
		{
			var i = list.IndexOf(item);
			if (i >= 1)
			{
				i--;
				previousItem = list[i];
				return true;
			}
			previousItem = default(T);
			return false;
		}

		public static T PreviousItemOrDefault<T>(this IList<T> list, T item)
			where T : class
		{
			var i = list.IndexOf(item);
			i--;
			if (i >= 0)
				return list[i];
			return null;
		}

	}
}