using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IBattable, IHurtable
{
	public static EnemyController enemyController;
	public int index { get; set; }

	public void Push (Vector2 force)
	{
		enemyController.Push(index, force);
	}

	public void Hurt (int damage)
	{
		enemyController.Hurt (index, damage);
	}
}