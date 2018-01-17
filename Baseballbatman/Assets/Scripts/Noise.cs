public static class Noise
{
	private const int hashMask = 255;
	private static readonly int[] hash = {
		117, 102, 157, 218, 159, 237, 4, 227, 64, 156, 200, 0, 23, 144, 32, 96,
		99, 239, 149, 58, 188, 190, 40, 22, 134, 155, 124, 185, 62, 162, 245, 8,
		39, 217, 181, 122, 17, 37, 35, 25, 47, 126, 66, 174, 166, 172, 133, 137,
		187, 103, 69, 83, 89, 236, 180, 242, 177, 93, 113, 163, 194, 10, 45, 80,
		79, 28, 253, 146, 249, 38, 148, 212, 5, 21, 192, 140, 116, 29, 2, 254,
		101, 11, 206, 49, 136, 73, 108, 252, 24, 173, 30, 109, 141, 224, 169, 202,
		41, 165, 18, 204, 167, 215, 16, 171, 182, 234, 88, 34, 153, 130, 9, 95,
		55, 223, 75, 110, 196, 193, 197, 67, 105, 43, 12, 230, 6, 48, 135, 52,
		63, 61, 222, 42, 119, 82, 154, 14, 226, 164, 125, 86, 247, 219, 56, 87,
		151, 46, 161, 186, 20, 106, 179, 53, 139, 214, 131, 112, 128, 97, 27, 71,
		13, 94, 81, 201, 160, 199, 233, 15, 70, 216, 213, 120, 59, 198, 84, 68,
		115, 123, 142, 211, 246, 241, 210, 145, 98, 1, 189, 220, 114, 33, 57, 170,
		150, 250, 231, 243, 100, 168, 138, 118, 91, 255, 152, 65, 31, 74, 240, 248,
		195, 221, 244, 26, 203, 129, 85, 229, 77, 132, 78, 184, 178, 90, 72, 175,
		111, 7, 50, 183, 60, 228, 225, 235, 127, 147, 205, 251, 207, 208, 54, 19,
		191, 92, 143, 3, 209, 121, 44, 107, 36, 232, 76, 51, 238, 158, 176, 104,

		117, 102, 157, 218, 159, 237, 4, 227, 64, 156, 200, 0, 23, 144, 32, 96,
		99, 239, 149, 58, 188, 190, 40, 22, 134, 155, 124, 185, 62, 162, 245, 8,
		39, 217, 181, 122, 17, 37, 35, 25, 47, 126, 66, 174, 166, 172, 133, 137,
		187, 103, 69, 83, 89, 236, 180, 242, 177, 93, 113, 163, 194, 10, 45, 80,
		79, 28, 253, 146, 249, 38, 148, 212, 5, 21, 192, 140, 116, 29, 2, 254,
		101, 11, 206, 49, 136, 73, 108, 252, 24, 173, 30, 109, 141, 224, 169, 202,
		41, 165, 18, 204, 167, 215, 16, 171, 182, 234, 88, 34, 153, 130, 9, 95,
		55, 223, 75, 110, 196, 193, 197, 67, 105, 43, 12, 230, 6, 48, 135, 52,
		63, 61, 222, 42, 119, 82, 154, 14, 226, 164, 125, 86, 247, 219, 56, 87,
		151, 46, 161, 186, 20, 106, 179, 53, 139, 214, 131, 112, 128, 97, 27, 71,
		13, 94, 81, 201, 160, 199, 233, 15, 70, 216, 213, 120, 59, 198, 84, 68,
		115, 123, 142, 211, 246, 241, 210, 145, 98, 1, 189, 220, 114, 33, 57, 170,
		150, 250, 231, 243, 100, 168, 138, 118, 91, 255, 152, 65, 31, 74, 240, 248,
		195, 221, 244, 26, 203, 129, 85, 229, 77, 132, 78, 184, 178, 90, 72, 175,
		111, 7, 50, 183, 60, 228, 225, 235, 127, 147, 205, 251, 207, 208, 54, 19,
		191, 92, 143, 3, 209, 121, 44, 107, 36, 232, 76, 51, 238, 158, 176, 104
	};

	public static float Value2D (float x, float y)
	{
		// Since x and y are alwas integers, this is sufficent
		// Also, there is no need for interpolated values
		return hash [hash[(int)x & hashMask] + ((int)y & hashMask)] * (1f / hashMask);

//		int ix0 = (int)x;
//		int ix1 = ix0 + 1;
//		float tx = x - ix0;
//
//
//		int iy0 = (int)y;
//		int iy1 = iy0 + 1;
//		float ty = y - iy0;
//
//		int h0 = hash [ix0 & hashMask];
//		int h1 = hash [ix1 & hashMask];
//		float h00 = hash [h0 + (iy0 & hashMask)];
//		float h10 = hash [h1 + (iy0 & hashMask)];
//		float h01 = hash [h0 + (iy1 & hashMask)];
//		float h11 = hash [h1 + (iy1 & hashMask)];
// 
//		float value = 
//			((1 - ty) * ((1 - tx) * h00 + tx * h10) 
//			+ ty * ((1 - tx) * h01 + tx * h11)) 
//			/ hashMask;
//
//		return value;
	}
		
	private delegate bool BoolMethod (float x, float y, float treshold);
	private static readonly BoolMethod [] boolMethods = {
		SimpleClip,
		Neighbours4,
		Neighbours8
	};

	public static bool Bool (NoiseBool type, float x, float y, float treshold)
	{
		return boolMethods [(int)type] (x, y, treshold);
	}

	public static bool SimpleClip (float x, float y, float treshold)
	{
//		float value = hash [hash[(int)x & hashMask] + ((int)y & hashMask)] * (1f / hashMask);
		float value = Value2D (x, y);
		return value < treshold;
	}

	public static bool Neighbours4 (float x, float y, float treshold)
	{
		int value = 0;
		value += Value2D (x - 1, y) < treshold ? 1 : 0;
		value += Value2D (x + 1, y) < treshold ? 1 : 0;
		value += Value2D (x, y - 1) < treshold ? 1 : 0;
		value += Value2D (x, y + 1) < treshold ? 1 : 0;

		return Value2D (x, y) < treshold || value >= 2;
	}

	public static bool Neighbours8(float x, float y, float treshold)
	{
		int value = 0;
		value += Value2D (x - 1, y) < treshold ? 1 : 0;
		value += Value2D (x + 1, y) < treshold ? 1 : 0;
		value += Value2D (x, y - 1) < treshold ? 1 : 0;
		value += Value2D (x, y + 1) < treshold ? 1 : 0;

		value += Value2D (x - 1, y - 1) < treshold ? 1 : 0;
		value += Value2D (x + 1, y - 1) < treshold ? 1 : 0;
		value += Value2D (x - 1, y + 1) < treshold ? 1 : 0;
		value += Value2D (x + 1, y + 1) < treshold ? 1 : 0;

		return Value2D (x, y) < treshold || value > 4;
	}
}

public enum NoiseBool {
	SimpleClip, Neighbours4, Neighbours8
};