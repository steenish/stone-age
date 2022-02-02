using UnityEngine;
using StoneAge;

namespace Utility {

	public class FunctionTester : MonoBehaviour {

		public BlitTesting tester;

		public void PerformTest() {
			tester.TestBlit();
		}
	}
}