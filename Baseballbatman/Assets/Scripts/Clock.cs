using UnityEngine;

public class Clock : MonoBehaviour
{
	public static float deltaTime { get { return Time.deltaTime; } }
	public static float fixedDeltaTime { get { return Time.fixedDeltaTime; } }
}