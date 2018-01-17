using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Rescuee: MonoBehaviour
{
//	new private Rigidbody2D rigidbody2D;
	[SerializeField] private float speed = 5f;
	[SerializeField] private float goToPortalDistance = 10f;

	new private SpriteRenderer renderer;

	private Transform target;

	private SpriteMotor2D motor;

	Vector2 [] path = null;
	int targetWaypoint = 0;

	float timer;
	float updatePathDelay = 0.25f;
	bool timeToUpdatePath { get { return timer <= 0f; }	}

	private void Awake ()
	{
		motor = new SpriteMotor2D (
			GetComponent<Rigidbody2D> (),
			GetComponent<SpriteRenderer> (),
			transform
		);
	}

	void Update ()
	{
		if (timer >= 0) {
			timer -= Clock.deltaTime;
		}
	}
	Vector2 requestPosition;
	Vector2 debugTargetPosition;

	bool pathRequested = false;

	private void FixedUpdate ()
	{
		if (!target) {
			target = FindObjectOfType<Player> ().transform;
		} else {

			if (path == null || timeToUpdatePath) {
				float distanceToPortal = (GameManager.instance.PortalPosition - transform.position).magnitude;
				Vector3 targetPosition = 
					distanceToPortal < goToPortalDistance ?
					GameManager.instance.PortalPosition :
					target.position;

				if (!pathRequested) {
					requestPosition = motor.position;
					debugTargetPosition = (Vector2)targetPosition;
					Pathfinder.RequestPath (motor.position, (Vector2)targetPosition, GetPath, null);
					pathRequested = true;
				}

			} else if (path != null) {
				if (path.Length == 0) {
					path = null;
					return;
				}


				if (motor.position == path [targetWaypoint]) {
					targetWaypoint++;
					if (targetWaypoint >= path.Length) {
						path = null;
						return;
					}
				}
					
				motor.Move (
					Vector2.MoveTowards (
						motor.position,
						path [targetWaypoint],
						speed * Clock.deltaTime
					)
				);
			}
		}
	}

	private void GetPath (Vector2[] path, System.EventArgs e)
	{
		this.path = path;
		timer = updatePathDelay;
		targetWaypoint = 0;
		pathRequested = false;
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		if (other.gameObject == GameManager.instance.Portal) {
			GameManager.instance.RescueeEscaped ();
			gameObject.SetActive (false);
		}
	}

	void OnDrawGizmos ()
	{
		if (path != null) {
			Gizmos.color = Color.yellow;
			foreach (Vector2 point in path) {
				Gizmos.DrawSphere (point, 0.5f);
			}
		}

		Gizmos.color = Color.red;
		Gizmos.DrawSphere (motor.position, 0.5f);

		Gizmos.color = Color.green;
		Gizmos.DrawSphere (debugTargetPosition, 0.5f);

	}
}
