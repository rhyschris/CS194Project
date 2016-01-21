using System;

/* Mono doesn't support Tuple for some reason
 * To avoid clashing with Tuple when compiled
 * under other C# compilers, we call this "Pair".
 * 
 * Solution adapted from 
 * http://answers.unity3d.com/questions/381993/does-unity-4-mono-support-tuples.html
 */
namespace AssemblyCSharp {
	
	public class Pair<T1, T2>
	{
		public T1 First { get; private set; }
		public T2 Second { get; private set; }
	
		internal Pair(T1 first, T2 second)
		{
			First = first;
			Second = second;
		}
	}
	/* Static factory to create new tuples */
	public static class Pair {
		public static Pair<T1, T2> New<T1, T2> (T1 first, T2 second){
			return new Pair<T1, T2> (first, second);
		}
	}
}

