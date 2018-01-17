using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
	[SerializeField] private int damage;

	private void OnCollisionEnter2D(Collision2D collision)
	{
		IHurtable hurtableEntity = collision.collider.GetComponent<IHurtable> ();
		if (hurtableEntity != null) {
			hurtableEntity.Hurt (damage);
		}
	}
}
