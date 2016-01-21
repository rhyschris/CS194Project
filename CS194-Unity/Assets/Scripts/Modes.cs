using System;

/**
 * Utility class to describe finite state automata modes
 * to communicate between player objects
 */
namespace AssemblyCSharp {

	public class Modes {
		public enum Stance {Stand, Crouch};
		public enum Attack {HiPunch, LoPunch, HiKick, LoKick, HiBlock, LoBlock, None};
	}
}

