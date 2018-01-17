using System;

public static class RandomUtility
{
	private static string seed = "My Unexpected Seed";
	private static Random generator = new Random(seed.GetHashCode());

	public static void Initialize (object seed)
	{
		generator = new Random (seed.GetHashCode ());
	}

	public static int Range(int min, int max)
	{
		return generator.Next (min, max);
	}

	public static float Percent
	{ get { return (float)generator.NextDouble (); }
	}

	public static T RandomItem <T>(this T [] array) 
	{
		return array [Range (0, array.Length)];
	}
}