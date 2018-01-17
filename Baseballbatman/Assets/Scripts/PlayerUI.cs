using UnityEngine;
using UnityEngine.UI;

/*
	This class Handles player UI.
*/

public class PlayerUI : MonoBehaviour
{	
	[SerializeField] private Slider healthBar;
	public int maxHitpoints { 
		get {
			return (int)healthBar.maxValue;
		}

		set { 	
			healthBar.maxValue = value; 
			healthBar.value = value;
		} 
	}

	public void SetHealth (int hitpoints)
	{
		healthBar.value = hitpoints;
	}

	[SerializeField] private Text pointsText;
	private int points;
	public int Points {
		get {
			return points;
		}

		set {
			points = value;
			pointsText.text = value.ToString ();
		}
	}

	public void Reset ()
	{
		Points = 0;
		maxHitpoints = maxHitpoints;
	}
}
