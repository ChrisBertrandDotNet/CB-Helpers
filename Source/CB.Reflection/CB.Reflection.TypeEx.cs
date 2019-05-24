
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

// Portable reflection helpers for Type.

// Adds missing functions to Windows Store (Windows 8 and 10), as well as allowing multi-platforms work (.NET and Store in the same solution).

/* Requires:
 * cb.data\CB.Data.EnumHelper.cs
 * cb.validation\CB.Validation.ContractConditions.cs
 * CB_DOTNET\_TESTS\Test.cs			! SEULEMENT SI ON DÉCLARE LE SYMBOLE 'TEST' DANS LA CONFIG DE PROJET EN COMPILATION DEBUG.
 */

/* rappel des symboles de projet:
 * Console : (rien)
 * WPF : (rien)
 * Windows 8.1 pour Windows Store : NETFX_CORE;WINDOWS_APP
 * Windows 8 universel pour Windows (Store) : NETFX_CORE;WINDOWS_APP
 * Windows 8 universel pour Windows Phone (Store) : NETFX_CORE;WINDOWS_PHONE_APP
 * Windows 10 UWP (Store) : NETFX_CORE;WINDOWS_UWP
*/

#if WINDOWS_APP || WINDOWS_PHONE_APP
#define WINDOWS_8_STORE // Windows 8, mais non Windows 10.
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace CB.Reflection
{

	/*internal static class AttributeEx
	{

		internal static Attribute GetCustomAttribute(MemberInfo element, Type attributeType)
		{
#if NETFX_CORE

#endif
		}

	}*/

	/// <summary>
	/// Portable reflection helpers for <see cref="System.Type"/>.
	/// </summary>
	public static partial class TypeEx
	{
		/// <summary>
		/// An empty Type array.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static readonly Type[] EmptyTypes = new Type[0];


		/// <summary>
		/// Returns the meeting SearchedType in the inheritance path of ObjectType.
		/// Returns null of none.
		/// Example: FindDerivedOrEqualToThisType(typeof(List&lt;int&gt;),typeof(List&lt;&gt;)) returns typeof(List&lt;int&gt;).
		/// </summary>
		/// <param name="ObjectType">The type to be analysed.</param>
		/// <param name="SearchedType">Searched type. Can be generic with a type (as List&lt;int&gt;) of nothing (as List&lt;&gt;).</param>
		/// <returns></returns>
		public static Type FindDerivedOrEqualToThisType(this Type ObjectType, Type SearchedType)
		{
			if (ObjectType == SearchedType)
				return ObjectType;
			return ObjectType.FindDerivedFromThisType(SearchedType);
		}

#if TEST
		static class Test_FindDerivedOrEqualToThisType
		{
			[CBdotnet.Test.Test]
			static void teste()
			{
				var t7 = typeof(F).FindDerivedOrEqualToThisType(typeof(D));
				CBdotnet.Test.Testeur.TesteÉgalité(t7, typeof(D));

				var t6 = typeof(A<>).FindDerivedOrEqualToThisType(typeof(B));
				CBdotnet.Test.Testeur.TesteInstanceNulle(t6);

				var t5 = typeof(A<>).FindDerivedOrEqualToThisType(typeof(A<>));
				CBdotnet.Test.Testeur.TesteÉgalité(t5, typeof(A<>));

				var t0 = typeof(A<int>).FindDerivedOrEqualToThisType(typeof(A<>));
				CBdotnet.Test.Testeur.TesteÉgalité(t0, typeof(A<int>));

				var t1 = typeof(List<int>).FindDerivedOrEqualToThisType(typeof(List<>));
				CBdotnet.Test.Testeur.TesteÉgalité(t1, typeof(List<int>));

				var t2 = typeof(D).FindDerivedOrEqualToThisType(typeof(C));
				CBdotnet.Test.Testeur.TesteÉgalité(t2, typeof(C));

				var t3 = typeof(B).FindDerivedOrEqualToThisType(typeof(A<>));
				CBdotnet.Test.Testeur.TesteÉgalité(t3, typeof(A<int>));

				var t4 = typeof(Int32Value).FindDerivedOrEqualToThisType(typeof(Value<>));
				CBdotnet.Test.Testeur.TesteÉgalité(t4, typeof(Value<int>));
			}
			class A<T> { }
			class B : A<int> { }
			class C { }
			class D : C { }
			class E<T> : D { }
			class F : E<int> { }
			public abstract class Info { }
			public abstract class Value<T> : Info { }
			public abstract class ValueInATextBox<T> : Value<T> { }
			public abstract class DigitalValue<T> : ValueInATextBox<T> { }
			public class Int32Value : DigitalValue<System.Int32> { }
		}
#endif

		/// <summary>
		/// Finds if <paramref name="SearchedType"/> is one of the descendent of <paramref name="TheObjectType"/>. If true, returns <paramref name="SearchedType"/>. If false, returns null.
		/// <para>This function is cached.</para>
		/// </summary>
		/// <param name="TheObjectType"></param>
		/// <param name="SearchedType"></param>
		/// <returns></returns>
		public static Type FindDerivedFromThisType(this Type TheObjectType, Type SearchedType)
		{
			if (SearchedType == typeof(object))
				return SearchedType; // optimisation: All types are objects anyway.

			lock (lockForFindDerivedFromThisType)
			{
				Type ret;
				KeyValuePair<Type, Type> searched = new KeyValuePair<Type, Type>(TheObjectType, SearchedType);
				if (!_FindDerivedToThisTypeCache.TryGetValue(searched, out ret))
				{
					ret = _FindDerivedToThisType(TheObjectType, SearchedType);
					_FindDerivedToThisTypeCache.Add(searched, ret);
				}
				return ret;
			}
		}
		static object lockForFindDerivedFromThisType = new object();
		internal static Type _FindDerivedToThisType(Type ObjectType, Type SearchedType)
		{
			if (SearchedType.IsInterface())
			{
				if (ObjectType.IsGenericType() && !ObjectType.IsGenericTypeDefinition() && (ObjectType.GetGenericTypeDefinition() == SearchedType))
					return SearchedType;
				var ints = ObjectType.GetInterfaces();
				return ContainsThisType(ints, SearchedType);
			}
			else // NOT an interface: --------------------------
			{
				bool lookForAPureGenericWithoutDefinedParameter =
					SearchedType.IsGenericType() && (SearchedType.ContainsGenericParameters());

				if (!lookForAPureGenericWithoutDefinedParameter)
				{
					Type tparent = ObjectType.BaseType();
					while (tparent != null)
					{
						if (tparent == SearchedType)
							return tparent;
						tparent = tparent.BaseType();
					}
				}
				else
				{
#if true // new algo
					Type t = ObjectType;
					while(t!=null)
					{
						if (t.IsGenericType() && t.GetGenericTypeDefinition() == SearchedType)
							return t;
						t = t.BaseType();
					}
#else // old algo
					Type previous = ObjectType;
					Type tparent = ObjectType.IsGenericType ? ObjectType.GetGenericTypeDefinition() : ObjectType;
					while (tparent != null)
					{
						if (tparent == SearchedType)
							return previous;// tparent;
						// next:
						previous = tparent;
						tparent = tparent.BaseType;
						if (tparent != null)
						{
							if (tparent.IsGenericType)
								tparent = tparent.GetGenericTypeDefinition();
						}
					}
#endif
				}
			}
			return null;
		}
		static Dictionary<KeyValuePair<Type, Type>, Type> _FindDerivedToThisTypeCache
			= new Dictionary<KeyValuePair<Type, Type>, Type>();

		static Type ContainsThisType(Type[] array, Type value)
		{
			int length = array.Length;
			for (int i = 0; i < length; i++)
			{
				Type item = array[i];
				Type comparableItem =
					(value.IsGenericTypeDefinition() && item.IsGenericType() && !item.IsGenericTypeDefinition()) ?
					item.GetGenericTypeDefinition()
					: item;

				if (
					(comparableItem == value)
					||
					((comparableItem.FullName == value.FullName)
#if !PORTABLE
 && (comparableItem.GUID() == value.GUID())
#endif
))
					return item;
			}
			return null;
		}

		/// <summary>
		/// (Chris)
		/// Donne l'assemblage contenant ce type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Assembly GetAssembly(this Type type)
		{
#if NETFX_CORE
			return type.GetTypeInfo().Assembly; // à vérifier
#else
			return type.Assembly;
#endif
		}

#if !NETFX_CORE
		/// <summary>
		/// Finds the right constructor of this <paramref name="type"/>.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="bindingAttr"></param>
		/// <param name="binder"></param>
		/// <param name="types"></param>
		/// <param name="modifiers"></param>
		/// <returns></returns>
		public static ConstructorInfo GetConstructor(this Type type, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
		{
			return type.GetConstructor(bindingAttr, binder, types, modifiers);
		}
#endif

#if NETFX_CORE

		static bool valideConstructeur(ConstructorInfo ci, Type[] parameterTypes)
		{
			var ps = ci.GetParameters();
			if (parameterTypes == null)
				return (ps.Length == 0);
			if (ps.Length == parameterTypes.Length)
			{
				for (int i = 0; i < ps.Length; i++)
					if (ps[i].ParameterType != parameterTypes[i])
						break;
				return true;
			}
			return false;
		}

		public static ConstructorInfo GetConstructor(this Type type, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
		{
			DebugCheck.ParameterIsDefined(type);
			DebugCheck.ParameterEnumIsDefined(bindingAttr);
			DebugCheck.Parameter(binder == null);
			DebugCheck.Parameter(modifiers == null);

			var cis = type.GetTypeInfo().DeclaredConstructors;
			foreach (var ci in cis)
			{
				if (bindingFlagsEstValide(ci, bindingAttr))
					if (valideConstructeur(ci, types))
						return ci;
			}
			return null;
		}

		static bool bindingFlagsEstValide(MethodBase m, BindingFlags bindingFlags)
		{
			if (bindingFlags.HasFlag(BindingFlags.Instance) && m.IsStatic)
				return false;
			if (bindingFlags.HasFlag(BindingFlags.Static) && !m.IsStatic)
				return false;
			if (bindingFlags.HasFlag(BindingFlags.Public) && !m.IsPublic)
				return false;
			if (bindingFlags.HasFlag(BindingFlags.NonPublic) && m.IsPublic)
				return false;
			return true;
		}
#endif

#if WINDOWS_8_STORE

		public static ConstructorInfo GetConstructor(this Type type, Type[] parameterTypes)
		{
			var cis = type.GetTypeInfo().DeclaredConstructors;
			foreach (var ci in cis)
			{
				if (valideConstructeur(ci, parameterTypes))
					return ci;
			}
			return null;
		}

		public static MethodInfo[] GetMethods(this Type type, System.Reflection.BindingFlags bindingFlags)
		{
			var ms = type.GetTypeInfo().DeclaredMethods.ToArray();
			var r2 = new List<MethodInfo>(ms.Length);
			for (int i = 0; i < ms.Length; i++)
			{
				if (bindingFlagsEstValide(ms[i], bindingFlags))
					r2.Add(ms[i]);
			}
			return r2.ToArray();
		}
#endif

#if NETFX_CORE
		public static Type GetEnumUnderlyingType(this Type type)
        {
            return Enum.GetUnderlyingType(type);
        }

        public static Attribute[] GetCustomAttributes(this Type element, Type attributeType, bool inherit)
        {
            return CustomAttributeExtensions.GetCustomAttributes(element.GetTypeInfo(), attributeType, inherit).ToArray();
        }
#endif

#if NETFX_CORE
		public static Attribute GetCustomAttribute(this Type type, Type attributeType)
		{
			return type.GetTypeInfo().GetCustomAttribute(attributeType);
		}
#endif

		/*/// <summary>
		/// (Chris)
		/// Équivalent à (T)type.GetCustomAttribute(typeof(T));
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <param name="inherit"></param>
		/// <returns></returns>
		internal static T GetCustomAttributeByType<T>(this Type type, bool inherit = true)
			where T : Attribute
		{
			//return (T)type.GetCustomAttribute(typeof(T));
			return type.GetCustomAttributes(typeof(T), inherit).FirstOrDefault() as T;
		}*/

		/// <summary>
		/// (Chris)
		/// Équivalent à (T)memberInfo.GetCustomAttribute(typeof(T));
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="info"></param>
		/// <param name="inherit"></param>
		/// <returns></returns>
		public static T GetCustomAttributeByType<T>(this MemberInfo info, bool inherit = true)
			where T : Attribute
		{
			return info.GetCustomAttributes(typeof(T), inherit).FirstOrDefault() as T;
		}

		/// <summary>
		/// Gets the <see cref="System.Reflection.Assembly"/> in which the type is declared. For generic types, gets the <see cref="System.Reflection.Assembly"/> in which the generic type is defined.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Assembly Assembly(this Type type)
		{
#if NETFX_CORE
			return type.GetTypeInfo().Assembly;
#else
			return type.Assembly;
#endif
		}
		/// <summary>
		/// Gets the type from which the current <see cref="System.Type"/> directly inherits.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type BaseType(this Type type)
		{
#if NETFX_CORE
			return type.GetTypeInfo().BaseType;
#else
			return type.BaseType;
#endif
		}

		/// <summary>
		/// Renvoie toute la parentée de ce type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static List<Type> BaseTypes(this Type type)
		{
			List<Type> baseTypes = new List<Type>();
			var bt = type;
			while(true)
			{
				bt = bt.BaseType();
				if (bt == null)
					break;
				baseTypes.Add(bt);				
			}
			return baseTypes;
		}

		/// <summary>
		/// Gets a value indicating whether the current <see cref="System.Type"/> object has type parameters that have not been replaced by specific types.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool ContainsGenericParameters(this Type type)
		{
#if NETFX_CORE
			return type.GetTypeInfo().ContainsGenericParameters;
#else
			return type.ContainsGenericParameters;
#endif
		}
		/// <summary>
		/// Gets the GUID associated with the <see cref="System.Type"/>.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Guid GUID(this Type type)
		{
#if NETFX_CORE
			return type.GetTypeInfo().GUID;
#else
			return type.GUID;
#endif
		}
		/// <summary>
		/// Gets a value indicating whether the <see cref="System.Type"/> is a class or a delegate; that is, not a value type or interface.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsClass(this Type type)
		{
#if NETFX_CORE
			return type.GetTypeInfo().IsClass;
#else
			return type.IsClass;
#endif
		}

		/// <summary>
		/// (Chris)
		/// Donne la valeur d'une propriété non-indexée.
		/// </summary>
		/// <param name="pi"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static object GetValueOfNonIndexed(this PropertyInfo pi, object obj)
		{
			return pi.GetValue(obj, null);
		}
		/// <summary>
		/// Gets the value of a static and non-indexed property.
		/// </summary>
		/// <param name="pi"></param>
		/// <returns></returns>
		public static object GetValueOfStaticNonIndexed(this PropertyInfo pi)
		{
			return pi.GetValue(null, null);
		}
		/// <summary>
		/// Gets a value indicating whether the current <see cref="System.Type"/> represents an enumeration.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsEnum(this Type type)
		{
#if NETFX_CORE
            return type.GetTypeInfo().IsEnum;
#else
			return type.IsEnum;
#endif
		}
		/// <summary>
		/// Gets a value indicating whether the current type is a generic type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsGenericType(this Type type)
		{
#if NETFX_CORE
			return type.GetTypeInfo().IsGenericType;
#else
			return type.IsGenericType;
#endif
		}
		/// <summary>
		/// Gets a value indicating whether the current <see cref="System.Type"/> represents a generic type definition, from which other generic types can be constructed.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsGenericTypeDefinition(this Type type)
		{
#if NETFX_CORE
			return type.GetTypeInfo().IsGenericTypeDefinition;
#else
			return type.IsGenericTypeDefinition;
#endif
		}
		/// <summary>
		/// Gets a value indicating whether the <see cref="System.Type"/> is an interface; that is, not a class or a value type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsInterface(this Type type)
		{
#if NETFX_CORE
			return type.GetTypeInfo().IsInterface;
#else
			return type.IsInterface;
#endif
		}
		/// <summary>
		/// Gets a value indicating whether the <see cref="System.Type"/> is a value type (that is, a structure).
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsValueType(this Type type)
		{
#if NETFX_CORE
            return type.GetTypeInfo().IsValueType;
#else
			return type.IsValueType;
#endif
		}

#if WINDOWS_8_STORE
        public static FieldInfo[] GetFields(this Type type)
        {
            return type.GetTypeInfo().DeclaredFields.ToArray();
        }
#endif

#if PORTABLE || WINDOWS_PHONE7_1 || NETFX_CORE
        // WP7.1 has Enum.GetRawConstantValue(), but it is wrong and causes an exception.
        //internal static object GetRawConstantValueOfFieldInfo(FieldInfo fieldInfo)
        public static object GetRawConstantValue(this FieldInfo fieldInfo)
        {
            object value = fieldInfo.GetValue(null);

            return value;
        }
#endif

		/// <summary>
		/// Finds if <paramref name="SearchedType"/> is one of the descendent of <paramref name="ObjectType"/>.
		/// <para>This function is cached.</para>
		/// </summary>
		/// <param name="ObjectType"></param>
		/// <param name="SearchedType"></param>
		/// <returns></returns>
		public static bool IsDerivedFromThisType(this Type ObjectType, Type SearchedType)
		{
			return ObjectType.FindDerivedFromThisType(SearchedType) != null;
		}
		/// <summary>
		/// Finds if <paramref name="SearchedType"/> is one of the descendent of <paramref name="ObjectType"/>, or is the same type.
		/// <para>This function is cached.</para>
		/// </summary>
		/// <param name="ObjectType"></param>
		/// <param name="SearchedType"></param>
		/// <returns></returns>
		public static bool IsDerivedOrEqualToThisType(this Type ObjectType, Type SearchedType)
		{
			return ObjectType.FindDerivedOrEqualToThisType(SearchedType) != null;
		}
	}
}

namespace System.Reflection
{

#if NETFX_CORE
	/// <summary>
	/// chris.
	/// Remplace le type manquant tiré de .NET classique.
	/// </summary>
	public class Binder{}

	/// <summary>
	/// chris.
	/// Remplace le type manquant tiré de .NET classique.
	/// </summary>
	public class ParameterModifier{}
#endif

#if WINDOWS_8_STORE
	/// <summary>
	/// chris.
	/// Remplace le type manquant tiré de .NET classique.
	/// </summary>
	public enum BindingFlags
	{
		//
		// Résumé :
		//     Specifies no binding flag.
		Default = 0,
		//
		// Résumé :
		//     Specifies that the case of the member name should not be considered when binding.
		IgnoreCase = 1,
		//
		// Résumé :
		//     Specifies that only members declared at the level of the supplied type's hierarchy
		//     should be considered. Inherited members are not considered.
		DeclaredOnly = 2,
		//
		// Résumé :
		//     Specifies that instance members are to be included in the search.
		Instance = 4,
		//
		// Résumé :
		//     Specifies that static members are to be included in the search.
		Static = 8,
		//
		// Résumé :
		//     Specifies that public members are to be included in the search.
		Public = 16,
		//
		// Résumé :
		//     Specifies that non-public members are to be included in the search.
		NonPublic = 32,
		//
		// Résumé :
		//     Specifies that public and protected static members up the hierarchy should be
		//     returned. Private static members in inherited classes are not returned. Static
		//     members include fields, methods, events, and properties. Nested types are not
		//     returned.
		FlattenHierarchy = 64,
		//
		// Résumé :
		//     Specifies that a method is to be invoked. This must not be a constructor or a
		//     type initializer.
		InvokeMethod = 256,
		//
		// Résumé :
		//     Specifies that Reflection should create an instance of the specified type. Calls
		//     the constructor that matches the given arguments. The supplied member name is
		//     ignored. If the type of lookup is not specified, (Instance | Public) will apply.
		//     It is not possible to call a type initializer.
		CreateInstance = 512,
		//
		// Résumé :
		//     Specifies that the value of the specified field should be returned.
		GetField = 1024,
		//
		// Résumé :
		//     Specifies that the value of the specified field should be set.
		SetField = 2048,
		//
		// Résumé :
		//     Specifies that the value of the specified property should be returned.
		GetProperty = 4096,
		//
		// Résumé :
		//     Specifies that the value of the specified property should be set. For COM properties,
		//     specifying this binding flag is equivalent to specifying PutDispProperty and
		//     PutRefDispProperty.
		SetProperty = 8192,
		//
		// Résumé :
		//     Specifies that the PROPPUT member on a COM object should be invoked. PROPPUT
		//     specifies a property-setting function that uses a value. Use PutDispProperty
		//     if a property has both PROPPUT and PROPPUTREF and you need to distinguish which
		//     one is called.
		PutDispProperty = 16384,
		//
		// Résumé :
		//     Specifies that the PROPPUTREF member on a COM object should be invoked. PROPPUTREF
		//     specifies a property-setting function that uses a reference instead of a value.
		//     Use PutRefDispProperty if a property has both PROPPUT and PROPPUTREF and you
		//     need to distinguish which one is called.
		PutRefDispProperty = 32768,
		//
		// Résumé :
		//     Specifies that types of the supplied arguments must exactly match the types of
		//     the corresponding formal parameters. Reflection throws an exception if the caller
		//     supplies a non-null Binder object, since that implies that the caller is supplying
		//     BindToXXX implementations that will pick the appropriate method.
		ExactBinding = 65536,
		//
		// Résumé :
		//     Not implemented.
		SuppressChangeType = 131072,
		//
		// Résumé :
		//     Returns the set of members whose parameter count matches the number of supplied
		//     arguments. This binding flag is used for methods with parameters that have default
		//     values and methods with variable arguments (varargs). This flag should only be
		//     used with System.Type.InvokeMember(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Object,System.Object[],System.Reflection.ParameterModifier[],System.Globalization.CultureInfo,System.String[]).
		OptionalParamBinding = 262144,
		//
		// Résumé :
		//     Used in COM interop to specify that the return value of the member can be ignored.
		IgnoreReturn = 16777216
	}
#endif

#if NETFX_CORE
	//
	// Résumé :
	//     Instructs obfuscation tools to take the specified actions for an assembly, type,
	//     or member.
	/// <summary>
	/// chris.
	/// Remplace le type manquant tiré de .NET classique.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.Delegate, AllowMultiple = true, Inherited = false)]
    public sealed class ObfuscationAttribute : Attribute
    {
        //
        // Résumé :
        //     Initializes a new instance of the System.Reflection.ObfuscationAttribute class.
        public ObfuscationAttribute() { }

		//
		// Résumé :
		//     Gets or sets a System.Boolean value indicating whether the attribute of a type
		//     is to apply to the members of the type.
		//
		// Retourne :
		//     true if the attribute is to apply to the members of the type; otherwise, false.
		//     The default is true.
		public bool ApplyToMembers { get; set; }
		//
		// Résumé :
		//     Gets or sets a System.Boolean value indicating whether the obfuscation tool should
		//     exclude the type or member from obfuscation.
		//
		// Retourne :
		//     true if the type or member to which this attribute is applied should be excluded
		//     from obfuscation; otherwise, false. The default is true.
		public bool Exclude { get; set; }
		//
		// Résumé :
		//     Gets or sets a string value that is recognized by the obfuscation tool, and which
		//     specifies processing options.
		//
		// Retourne :
		//     A string value that is recognized by the obfuscation tool, and which specifies
		//     processing options. The default is "all".
		public string Feature { get; set; }
		//
		// Résumé :
		//     Gets or sets a System.Boolean value indicating whether the obfuscation tool should
		//     remove this attribute after processing.
		//
		// Retourne :
		//     true if an obfuscation tool should remove the attribute after processing; otherwise,
		//     false. The default is true.
		public bool StripAfterObfuscation { get; set; }
	}
#endif

}

namespace System
{

#if NETFX_CORE
	//
	// Résumé :
	//     Indicates that a class can be serialized. This class cannot be inherited.
	/// <summary>
	/// chris.
	/// Remplace le type manquant tiré de .NET classique.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate, Inherited = false)]
    public sealed class SerializableAttribute : Attribute
    {
        //
        // Résumé :
        //     Initializes a new instance of the System.SerializableAttribute class.
        public SerializableAttribute(){}
    }
#endif
}