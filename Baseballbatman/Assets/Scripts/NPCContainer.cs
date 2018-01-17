using UnityEngine;

public class NPCContainer : MonoBehaviour, IBattable
{
	public GameObject containedEnemyPrefab;

	public void Push(Vector2 force)
	{
		gameObject.SetActive (false);
		if (containedEnemyPrefab) {
			Instantiate (containedEnemyPrefab, transform.position, Quaternion.identity);
		}
	}
}
