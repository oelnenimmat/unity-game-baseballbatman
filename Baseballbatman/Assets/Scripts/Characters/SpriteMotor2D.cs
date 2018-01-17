using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpriteMotor2D
{
	public Rigidbody2D rigidbody2D { get; private set; }
	public SpriteRenderer renderer { get; private set; }
	public Transform transform { get; private set; }

	public Vector2 position { 
		get { return (Vector2)transform.position; } 
		set { transform.position = value; }
	}
	public Vector2 velocity { get { return rigidbody2D.velocity; } }

	public SpriteMotor2D (
		Rigidbody2D rigidbody2D,
		SpriteRenderer renderer,
		Transform transform
	) {
		this.rigidbody2D = rigidbody2D;
		rigidbody2D.gravityScale = 0;
		rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;

		this.renderer = renderer;
		renderer.sortingLayerName = "Actors";

		this.transform = transform;
	}

	public void Move (Vector3 movePosition) {
		rigidbody2D.MovePosition (movePosition);
		if ((movePosition.x - position.x) != 0) {
			renderer.flipX = movePosition.x - position.x > 0;
		}
	}

	public void SetSprite (SpriteRenderInfo spriteInfo)
	{
		renderer.sprite = spriteInfo.sprite;
	}

	public void Flash (Color color)
	{
		GameManager.instance.StartCoroutine (FlashSprite (color));
	}

	private IEnumerator FlashSprite (Color color)
	{
		renderer.color = color;
		yield return new WaitForSeconds (0.1f);
		renderer.color = Color.white;
		yield return new WaitForSeconds (0.1f);
		renderer.color = color;
		yield return new WaitForSeconds (0.1f);
		renderer.color = Color.white;
	}
}

[System.Serializable]
public class SpriteRenderInfo
{
	public Sprite sprite;
}