
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/* Requires:
 * CB.Execution\CB.Execution.Return.cs
. CB.Data\CB.Data.EnumHelper.cs
. CB.Validation\CB.Validation.ContractConditions.cs
. CB_DOTNET\_FICHIERS\File for Windows 8 Store.cs -> uniquement pour les projets Windows 8 (universel ou Store ou Phone).
. CB.Reflection\CB.Reflection.TypeEx.cs
*/

/*

This is a way to manage data concurrently without any lock, thanks to consistency.

Here consistency means every class contains data sets we can't modify individually.
For example, you want to brighten a color. The color components (red, green and blue) can't be modified one after the other because a parallel task could see inconstancies. We have to set all components at once.

The solution here is to let data store immutable (invariable) value sets. The only way to modify such a data is to copy, modify the copy then replace the set as a whole in the data.

Please note this concept works fine on small data sets but can be slow and resource-hungry on big sets (such as a big document in an instance).

*/

/* [ internal notes in French: ]
C'est une façon de gérer des données à accès parallèle sans utiliser de verrouillage (lock).
Chaque classe contient un ensemble de données qu'on ne peut pas modifier individuellement.
Cette méthode marche très bien pour de petits ensembles. Par contre, je ne la conseille pas pour de gros documents, car elle nécessite la copie de tout un ensemble avant sa modification.

Au 2017-04-04, je pense que même les petites structures en arbre peuvent fonctionner en parallèle si chaque étage a un ensemble cohérent gérant les accès parallèles.

PAR CONTRE, les gros arbres ne sont pas utilisables ainsi, car trop lents.
Si un arbre cohérent est énorme, par exemple un document de 10 Mo, alors la modification d’un seul octet nécessite la copie de tout l’arbre.
Donc, je dois concevoir un système qui permette de modifier une petite partie tout en conservant la cohérence de l’ensemble mais sans tout recopier à chaque fois.
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

#if TEST
using CBdotnet.Test;
#endif
using CB.Data;
using System;
using System.Diagnostics;

namespace CB.Parallelism
{

	/*class ParallelAccessByConsistence
	{	}*/

	/// <summary>
	/// "The arguments are inconsistent. They no not match each other."
	/// </summary>
	[Serializable]
	public class InconsistentArgumentsException : ArgumentException
	{
		const string error = "The arguments are inconsistent. They no not match each other.";
		/// <summary>
		/// "The arguments are inconsistent. They no not match each other."
		/// </summary>
		public InconsistentArgumentsException()
			: base(error) { }
	}

	/// <summary>
	/// "Direct access to the original data is prohibited. You have to claim an immutable copy of this instance first."
	/// </summary>
	[Serializable]
	public class OriginalAccessViolationException : InvalidOperationException
	{
		const string erreur = "Direct access to the original data is prohibited. You have to claim an immutable copy of this instance first, calling GetCopy().";
		/// <summary>
		/// "Direct access to the original data is prohibited. You have to claim an immutable copy of this instance first."
		/// </summary>
		public OriginalAccessViolationException()
			: base(erreur) { }
	}

	/// <summary>
	/// "Copied data can not be modified. You have to modify data of the original class instance class."
	/// </summary>
	[Serializable]
	public class CopyAccessViolationException : AccessViolationException
	{
		const string erreur = "Copied data can not be modified. You have to modify data of the original class instance class.";
		/// <summary>
		/// "Copied data can not be modified. You have to modify data of the original class instance class."
		/// </summary>
		public CopyAccessViolationException()
			: base(erreur) { }
	}

	/*
	/// <summary>
	/// A structure/class where its data is consistent.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IConsistent<T>
	{
		/// <summary>
		/// Gets a consistent copy of this instance.
		/// </summary>
		/// <returns></returns>
		T GetAnImmutableCopy();
	}*/

	// ParallelClass offre de nombreuses garanties qui évitent des erreurs de programmation. Toutefois elle est assez complexe à utiliser.
	// Trop lourde pour un programmeur qui veut créer des classes à accès parallèle.
	// Mais très pratique pour un programmeur qui se contente d'utiliser des classes à accès parallèle existantes.
	/// <summary>
	/// A class where its data provides parallel access almost transparently.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal abstract class ParallelClass<T>
		where T : ParallelClass<T>
	{
		public bool IsAnImmutableCopy { get; protected set; }
		public bool IsTheOriginal { get { return !this.IsAnImmutableCopy; } }

		/// <summary>
		/// Returns a immutable copy.
		/// This is the only way to read the data.
		/// </summary>
		/// <returns></returns>
		public abstract T GetAnImmutableCopy();
	}

	#region Méthode simple mais un peu risquée

	/// <summary>
	/// A consistent class.
	/// Its data can be modified, but all at once.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Consistent<T>
		where T : class
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		volatile T _Data;
		/// <summary>
		/// A consistent state of the data.
		/// </summary>
		public T Data
		{
			get { return this._Data; }
			set
			{
				if (value == null)
					throw new ArgumentNullException();
				this._Data = value;
			}
		}

		/// <summary>
		/// Initializes a consistent class.
		/// </summary>
		/// <param name="data"></param>
		public Consistent(T data)
		{
			if (data == null)
				throw new ArgumentNullException();
			this._Data = new NonNull<T>(data);
		}

		/// <summary>
		/// Initializes a consistent class with a non-null data.
		/// </summary>
		/// <param name="data"></param>
		public Consistent(NonNull<T> data)
		{
			this._Data = data.Value;
		}
	}

	#endregion Méthode simple mais un peu risquée


#if TEST
	/// <summary>
	/// Exemple pour ParallelClass.
	/// C'est une classe qui vu de l'extérieur parait plutôt ordinaire, avec des propriétés pour lire les valeurs.
	/// Par contre, la modification des valeurs doit se faire par une fonction qui les change toutes en un appel.
	/// </summary>
	public class ClassePublique // Classe complexe mais d'uage assez facile pour le programmeur utilisateur de l'API.
		: ParallelClass<ClassePublique>
	{
		[System.ComponentModel.ImmutableObject(true)]
		class EnsembleCohérentInvariable
		{
			// toutes les variables de la classe principale
			internal readonly int Bas;
			internal readonly int Haut;
			internal EnsembleCohérentInvariable(int bas, int haut) // initialise l'ensemble cohérent et invariable
			{
				if (haut < bas)
					throw new InconsistentArgumentsException();
				this.Bas = bas;
				this.Haut = haut;
			}
		}

		/// <summary>
		/// This value can be read only on copies obtained through <see cref="GetAnImmutableCopy"/>().
		///This certifies you can not handle inconsistent Data that may have been modified by parallel threads.
		/// </summary>
		/// <exception cref="OriginalAccessViolationException">When the instance is the original.</exception>
		public int Bas
		{
			get
			{
				if (this.IsAnImmutableCopy)
					return this.ensembleCohérentInvariable.Bas;
				throw new OriginalAccessViolationException();
			}
		}

		/// <summary>
		/// This value can be read only on copies obtained through <see cref="GetAnImmutableCopy"/>().
		///This certifies you can not handle inconsistent Data that may have been modified by parallel threads.
		/// </summary>
		/// <exception cref="OriginalAccessViolationException">When the instance is the original.</exception>
		public int Haut
		{
			get
			{
				if (this.IsAnImmutableCopy)
					return this.ensembleCohérentInvariable.Haut;
				throw new OriginalAccessViolationException();
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		volatile EnsembleCohérentInvariable ensembleCohérentInvariable = new EnsembleCohérentInvariable(0, 0);

		/// <summary>
		/// Fournit les données sous forme d'un ensemble cohérent et invariable.
		/// </summary>
		/// <returns></returns>
		public override ClassePublique GetAnImmutableCopy()
		{ // Une copie de cette classe permet de ne pas risquer des modifications en parallèle.
			return new ClassePublique() { ensembleCohérentInvariable = this.ensembleCohérentInvariable, IsAnImmutableCopy = true };
		}

		/// <summary>
		/// This value can be written on the original only.
		///This certifies you can not handle inconsistent Data that may have been modified by parallel threads.
		/// </summary>
		/// <param name="bas"></param>
		/// <param name="haut"></param>
		public void Set(int bas, int haut)
		{
			if (this.IsTheOriginal)
			{
				var ensemble = new EnsembleCohérentInvariable(bas, haut);
				this.ensembleCohérentInvariable = ensemble;
				return;
			}
			throw new CopyAccessViolationException();
		}

		[Test]
		static void Teste_ClassePublique()
		{
			var cp = new ClassePublique();
			Testeur.TesteÉchecPrévu(() => cp.Bas); // car un tel accès permettrait de lire des valeurs incohérentes.
			Testeur.TesteÉchecPrévu(() => cp.Set(500, 200)); // car les valeurs sont incohérentes pour cette classe.
			cp.Set(100, 200);
			var copieInvariable = cp.GetAnImmutableCopy();
			cp.Set(3000, 4000); // simule une tâche parallèle.
			Testeur.TesteÉgalité(copieInvariable.Bas, 100);
			Testeur.TesteÉchecPrévu(() => copieInvariable.Set(1000, 2000)); // car une copie est invariable.
		}
	}

	/// <summary>
	/// Exemple des plus simples.
	/// </summary>
	[System.ComponentModel.ImmutableObject(true)]
	internal class ClasseParallèleInvariable
	{
		internal readonly int Bas;
		internal readonly int Haut;
		internal ClasseParallèleInvariable(int bas, int haut) // initialise l'ensemble cohérent et invariable
		{
			if (haut < bas)
				throw new CB.Parallelism.InconsistentArgumentsException();
			this.Bas = bas;
			this.Haut = haut;
		}

#if TEST
		[CBdotnet.Test.Test]
		static void Teste()
		{
			var cp = new Consistent<ClasseParallèleInvariable>(new ClasseParallèleInvariable(100, 200));
			var valeurs1 = cp.Data;
			cp.Data = new ClasseParallèleInvariable(1000, 2000);
			var valeurs2 = cp.Data;
		}
#endif
	}

	#region Exemple arbre cohérent

	/* 2017-04-04
	 * Exemple d'un arbre non seulement constitué de classes cohérentes, mais également cohérent en lui-même.
	 * Les deux classes qui constituent cet arbre sont cohérentes: on ne peut pas modifier le contenu individuel de l'une sans modifier le contenu de l'autre.
	 * Forme shématique:
 . C
	. Bas = 2
	. D
		. Haut = 3
	 * 
	*/

	public class C : CB.Parallelism.ParallelClass<C>
	{
		[System.ComponentModel.ImmutableObject(true)]
		class C_Valeurs
		{
			internal readonly int Bas;
			internal readonly D d1;
			internal C_Valeurs(int bas, D D1)
			{
				this.Bas = bas;
				this.d1 = D1;
			}
		}

		C_Valeurs cv;

		/// <summary>
		/// This value can be read only on copies obtained through <see cref="GetAnImmutableCopy"/>().
		///This certifies you can not handle inconsistent Data that may have been modified by parallel threads.
		/// </summary>
		/// <exception cref="OriginalAccessViolationException">When the instance is the original.</exception>
		[System.Diagnostics.DebuggerDisplay("{cv.Bas}")]
		public int Bas
		{
			get
			{
				if (this.IsAnImmutableCopy)
					return this.cv.Bas;
				throw new CB.Parallelism.OriginalAccessViolationException();
			}
		}

		/// <summary>
		/// This value can be read only on copies obtained through <see cref="GetAnImmutableCopy"/>().
		///This certifies you can not handle inconsistent Data that may have been modified by parallel threads.
		/// </summary>
		/// <exception cref="OriginalAccessViolationException">When the instance is the original.</exception>
		public D D1
		{
			get
			{
				if (this.IsAnImmutableCopy)
					return this.cv.d1;
				throw new CB.Parallelism.OriginalAccessViolationException();
			}
		}

		public C(int bas, D d1)
		{
			var d = d1.GetAnImmutableCopy();
			if (d.Haut < bas)
				throw new CB.Parallelism.InconsistentArgumentsException();
			this.Set(bas, d);
		}

		public void Set(int bas, D d1)
		{
			if (this.IsTheOriginal)
				this.cv = new C_Valeurs(bas, d1);
			else
				throw new CB.Parallelism.CopyAccessViolationException();
		}

		public override C GetAnImmutableCopy()
		{
			var cvActuel = this.cv;
			return new C(cvActuel.Bas, cvActuel.d1) { IsAnImmutableCopy = true };
		}
	}

	public class D : CB.Parallelism.ParallelClass<D>
	{
		[System.ComponentModel.ImmutableObject(true)]
		internal class D_Valeurs
		{
			internal readonly int Haut;
			internal D_Valeurs(int haut)
			{ this.Haut = haut; }
		}

		D_Valeurs dv;

		/// <summary>
		/// This value can be read only on copies obtained through <see cref="GetAnImmutableCopy"/>().
		///This certifies you can not handle inconsistent Data that may have been modified by parallel threads.
		/// </summary>
		/// <exception cref="OriginalAccessViolationException">When the instance is the original.</exception>
		[System.Diagnostics.DebuggerDisplay("{dv.Haut}")]
		public int Haut
		{
			get
			{
				if (this.IsAnImmutableCopy)
					return this.dv.Haut;
				throw new CB.Parallelism.OriginalAccessViolationException();
			}
		}

		public D(int haut)
		{
			this.Set(haut);
		}

		public void Set(int haut)
		{
			if (this.IsTheOriginal)
				this.dv = new D_Valeurs(haut);
			else
				throw new CB.Parallelism.CopyAccessViolationException();
		}

		public override D GetAnImmutableCopy()
		{
			var valActuelle = this.dv;
			return new D(valActuelle.Haut) { IsAnImmutableCopy = true };
		}
	}

	public static class Teste_Arbre_C_D
	{
		[CBdotnet.Test.Test]
		public static void Teste()
		{
			// tâche 1:
			var c = new C(100, new D(200));
			var c1 = c.GetAnImmutableCopy();
			//Console.WriteLine("bas={0} haut={1}", c1.Bas, c1.D1.Haut);
			// tâche 2:
			var c2 = c.GetAnImmutableCopy();
			c.Set(c2.Bas + 1000, new D(c2.D1.Haut + 1000).GetAnImmutableCopy());
			var c3 = c.GetAnImmutableCopy();
			//Console.WriteLine("bas={0} haut={1}", c3.Bas, c3.D1.Haut);
		}
	}

	#endregion Exemple arbre cohérent



	#region ancien exemple, qui se suffit à lui-même

	/// <summary>
	/// C'est une classe à usage interne, qui est rapide à écrire mais qui expose sa logique interne (sa gestion du parallélisme par des ensembles cohérents invariables).
	/// IMPORTANT: Sa structure est très différente des classes qui héritent de ParallelClass.
	/// NOTER: on n'a pas besoin de ce fichier source pour écrire de telles classes. Cet exemple est ici juste pour me le rappeler.
	/// NOTE: on peut écrire tout aussi bien des structures sur ce modèle. Seules les Valeurs doivent être placées dans une (sous)classe.
	/// </summary>
	internal class ClasseInterne // Structure/Classe simple, pour mon usage personnel.
	{
		[System.ComponentModel.ImmutableObject(true)]
		internal class Valeurs
		{
			// toutes les variables de la classe principale
			internal readonly int Bas;
			internal readonly int Haut;
			internal Valeurs(int bas, int haut) // initialise l'ensemble cohérent et invariable
			{
				this.Bas = bas;
				this.Haut = haut;
			}
		}

		internal volatile Valeurs ValeursInvariables = new Valeurs(0, 0); // On s'assure qu'il existe toujours un ensemble de valeurs.

		[ReturnSuccessCodes(ReturnSuccess.InconsistentArgumentsError)]
		internal RetNonNull<Valeurs> Set(int bas, int haut) // pas d'exception, plutôt une erreur possible.
		{
			if (haut < bas)
				return new RetNonNull<Valeurs>(ReturnSuccess.InconsistentArgumentsError);
			var ensemble = new Valeurs(bas, haut);
			this.ValeursInvariables = ensemble;
			return new RetNonNull<Valeurs>(ensemble);
		}

		[Test]
		static void Teste_ClasseInterne()
		{
			var ci = Testeur.TesteRéussite(() => new ClasseInterne());
			var ensemble = Testeur.TesteInstanceNonNulle(ci.Set(100, 200).Value);
			Testeur.TesteRéussite(() => ci.Set(3000, 4000)); // simule une tâche parallèle.
			Testeur.TesteÉgalité(ensemble.Bas, 100); // pas affecté par la tâche parallèle.
		}
	}

	#endregion ancien exemple, qui se suffit à lui-même

#endif

}