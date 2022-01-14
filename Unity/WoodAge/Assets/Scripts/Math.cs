namespace Utility {
    public class Math {

        public static bool IsPowerOfTwo(int x) {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        public static float[] LinearVector(float start, float end, int n) {
            float distance = (end - start) / n;
            float[] result = new float[n];

            for (int i = 0; i < n; ++i) {
                result[i] = start + distance * i;
            }

            return result;
        }

        public static double Pythagoras(double a, double b) {
            return System.Math.Sqrt(a * a + b * b);
		}
    }
}
