using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Player : MonoBehaviour, IHurtable
{
	[SerializeField] private InputControl input;
	
	private Transform cameraTransform;
	[SerializeField] private float cameraMoveDamping = 2f;

	[SerializeField] private Collider2D attackCollider;
	public SpriteRenderer attackCircle;

	[SerializeField] private float speed = 10f;
	[SerializeField] private float dodgeDistance = 2f;
	[SerializeField] private float batPower = 10f;

	private SpriteMotor2D motor;

	[SerializeField] private int maxHitpoints;
	private int hitpoints;

	private PlayerUI ui;
	[SerializeField] private AudioClip batSwingClip;

	private void Awake ()
	{
		attackCircle.enabled = false;

		cameraTransform = Camera.main.transform;

		motor = new SpriteMotor2D (
			GetComponent<Rigidbody2D> (),
			GetComponent<SpriteRenderer> (),
			transform
		);
		hitpoints = maxHitpoints;
		ui = GameManager.instance.playerUI;
		ui.maxHitpoints = maxHitpoints;
	}

	private void Update ()
	{
		input.Update ();

		Vector3 movement = input.Move * speed * Clock.deltaTime;
		motor.Move(movement + transform.position);

		if (input.Dodge) {
			StartCoroutine (Dodge (movement));
		}

		if (input.Attack) {
			Strike ();
		}

		// Move Camera
		Vector3 cameraTargetPosition = new Vector3(transform.position.x, transform.position.y, cameraTransform.position.z);
		cameraTransform.position = Vector3.Lerp (cameraTransform.position, cameraTargetPosition, 1f/cameraMoveDamping * Clock.deltaTime);
	}
		
	public void Hurt (int damage)
	{
		hitpoints -= damage;
		if (hitpoints <= 0) {
			Die ();
		} else {
			motor.Flash (Color.red);
		}
		ui.SetHealth (hitpoints);
	}

	public void Heal (int amount)
	{
		hitpoints = Mathf.Min (maxHitpoints, hitpoints + amount);
		ui.SetHealth (hitpoints);
	}

	private void Die ()
	{
		gameObject.SetActive (false);
	}

	private IEnumerator Dodge (Vector2 direction)
	{
		direction = direction.normalized;

		float duration = 0.1f;
		float timer = 0;
		while (timer < duration) {
			motor.Move (direction * (dodgeDistance / duration) * Clock.deltaTime + (Vector2)transform.position);
			timer += Clock.deltaTime;
			yield return null;
		}

	}


	private void Strike ()
	{
		Collider2D [] hit = new Collider2D[10];
		ContactFilter2D contactFilter = new ContactFilter2D ();
		int hits = attackCollider.OverlapCollider (contactFilter, hit);
		if (hits > 0) {
			for (int i = 0; i < hits; i++) {
				IBattable battable = hit [i].GetComponent<IBattable> ();
				if (battable != null) {
					Vector2 force = (hit[i].attachedRigidbody.position - (Vector2)transform.position).normalized * batPower;
					battable.Push (force);

					if (battable is Enemy) {
						int pointsGotten = GameManager.instance.enemyController.GetPoints ((battable as Enemy).index);
						if (pointsGotten > 0) {
							ui.Points += pointsGotten;
						}
					}

				}
			}
		}
		SFX.Play (batSwingClip);
		StartCoroutine (displayAttack ());
	}

	private IEnumerator displayAttack ()
	{
		attackCircle.enabled = true;
		yield return new WaitForSeconds (0.15f);
		attackCircle.enabled = false;
	}

	private void OnTriggerEnter2D (Collider2D other)
	{
		if (other.gameObject == GameManager.instance.Portal) {
			if (GameManager.instance.RescueeSaved) {
				GameManager.instance.HeroEscaped ();
				gameObject.SetActive (false);
			} else {
				Heal (maxHitpoints);
			}
		}
	}
}

