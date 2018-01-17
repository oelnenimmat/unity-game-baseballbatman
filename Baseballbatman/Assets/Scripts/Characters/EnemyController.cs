using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
	[SerializeField] EnemyType defaultType;
	[SerializeField] Enemy containerPrefab;

	/*
		Charge means move to player, attack means do attacking
	*/
	private enum EnemyState { Contained, Idle, Charge, Attack, Batted, Stunned, Dead }

	[SerializeField] LayerMask goodGuysLayerMask;
	[SerializeField] ParticleSystem stunnedEffectPrefab;
	[SerializeField] ParticleSystem hitEffectPrefab;
	[SerializeField] AudioClip pushedSFX;
	[SerializeField] ParticleSystem barrelBreakEffect;

	int length;

	private Enemy[] enemies;
	private Enemy[] containers;

	private SpriteMotor2D[] motors;
	private EnemyState[] states;

	private Vector2[][] paths;
	private int[] pathTargetPositions;
	private CallBackIndex[] callBackIndices;
	private float[] pathUpdateTimes;
	private bool[] hasRequestedPath;

	private EnemyType[] types;
	private int[] hitpoints;
	private float[] stunnedTimers;
	private ParticleSystem[] stunnedEffects;
	private ParticleSystem[] hitEffects;


	private int[] pointsLeft;

	private Transform target;
	private Vector2 targetPosition { get { return (Vector2)target.position; } }

	float attackHitRadius = 0.1f;
	float pathUpdateDelay = 0.25f;
	private bool initialized = false;

	public void Initialize (Vector3 [] positions, Transform target)
	{
		if (initialized) {
			UnInitialize ();
		}
		initialized = true;

		this.enabled = true;
		this.target = target;

		length 	= positions.Length;

		enemies		= new Enemy[length];
		containers	= new Enemy[length];

		motors 		= new SpriteMotor2D[length];
		states 		= new EnemyState[length];

		paths		= new Vector2[length][];
		pathTargetPositions =	new int[length];
		callBackIndices = new CallBackIndex[length];
		pathUpdateTimes = new float[length];
		hasRequestedPath = new bool[length];

		types 			= new EnemyType[length];
		hitpoints		= new int[length];
		stunnedTimers 	= new float[length];
		stunnedEffects 	= new ParticleSystem[length];
		hitEffects		= new ParticleSystem[length];

		pointsLeft = new int[length];

		for (int i = 0; i < length; i++) {
			enemies [i] = Instantiate<Enemy> (defaultType.prefab, positions [i], Quaternion.identity);
			enemies [i].index = i;
			motors [i] = new SpriteMotor2D (
				enemies [i].gameObject.GetComponent<Rigidbody2D> (),
				enemies [i].gameObject.GetComponent<SpriteRenderer> (),
				enemies [i].transform
			);
			enemies [i].gameObject.SetActive (false);

			containers [i] = Instantiate<Enemy> (containerPrefab, positions [i], Quaternion.identity);
			containers [i].index = i;

			callBackIndices [i] = new CallBackIndex (i);

			types [i] = defaultType;
			hitpoints [i] = types [i].maxHitpoints;

			stunnedEffects [i] = Instantiate<ParticleSystem> (stunnedEffectPrefab);
			stunnedEffects [i].gameObject.SetActive (false);
			stunnedEffects [i].transform.position = motors [i].position;
			stunnedEffects [i].transform.parent = motors [i].transform;


			hitEffects [i] = Instantiate<ParticleSystem> (hitEffectPrefab);
			hitEffects [i].transform.position = motors [i].position;

			pointsLeft [i] = types [i].pointsValue;

		}

		Enemy.enemyController = this;
	}

	public void UnInitialize ()
	{
		this.enabled = false;
		for (int i = 0; i < length; i++) {
			Destroy (enemies [i]);
			Destroy (containers [i]);
		}

		enemies 	= null;
		containers	= null;

		motors				= null;
		states				= null;
		paths 				= null;
		pathTargetPositions = null;
		callBackIndices 	= null;
		hasRequestedPath 	= null;

		types			= null;
		hitpoints		= null;
		stunnedTimers 	= null;
		stunnedEffects 	= null;
		hitEffects		= null;


		initialized = false;
	}

	int updatePathIndex = 0;
	private void FixedUpdate ()
	{
		// Loop all enemies this controller controls
		for (int i = 0; i < length; i++) {

			// Check if path should be updated
			if (paths [i] != null && pathUpdateTimes [i] >= 0f) {
				pathUpdateTimes [i] -= Clock.fixedDeltaTime;
				if (pathUpdateTimes [i] < 0) {
					RequestPath (i);
				}
			}
				

			switch (states [i]) {

			// Move towards player
			case EnemyState.Charge:

				// If enemy os close enough, attack
				float distanceToTarget = (targetPosition - motors [i].position).magnitude;
				if (distanceToTarget <= types [i].attackDistance) {
					StartCoroutine (Attack (i));

					// Clear path, so that new will be get after attack
					paths [i] = null;
					break;	
				}

				if (paths [i] == null) {
					RequestPath (i);

				} else {
					if (motors [i].position == paths [i] [pathTargetPositions [i]]) {
						pathTargetPositions [i]++;
						if (pathTargetPositions [i] >= paths [i].Length) {
							paths [i] = null;
							break;
						}
					}

					motors[i].Move (
						Vector2.MoveTowards(
							motors[i].position,
							paths [i] [pathTargetPositions [i]],
							types [i].speed * Clock.deltaTime
						)
					);
				}
				break;

			case EnemyState.Batted:
				if (motors[i].velocity.magnitude < types[i].speed) {
					states[i] = EnemyState.Stunned;
					stunnedTimers [i] = types [i].stunRecoveryTime;
					stunnedEffects [i].gameObject.SetActive (true);
				}
				break;

			case EnemyState.Stunned:
				stunnedTimers [i] -= Clock.fixedDeltaTime;
				if (stunnedTimers [i] < 0) {
					states [i] = EnemyState.Charge;
					stunnedEffects [i].gameObject.SetActive (false);
				}
				break;
			}
			
		}
	}

	private class CallBackIndex : System.EventArgs
	{
		public readonly int index;
		public CallBackIndex (int index) { this.index = index; }
	}

	private void RequestPath (int index)
	{
		if (!hasRequestedPath [index]) {
			Pathfinder.RequestPath (
				motors [index].position, 
				target.position, 
				GetPath, 
				callBackIndices [index]
			);
			hasRequestedPath [index] = true;
		}
	}

	private void GetPath(Vector2 [] path, System.EventArgs index)
	{
		if (!initialized) {
			return;
		}

		paths [(index as CallBackIndex).index] = path;
		pathTargetPositions [(index as CallBackIndex).index] = 0;
		pathUpdateTimes [(index as CallBackIndex).index] = pathUpdateDelay;
		hasRequestedPath [(index as CallBackIndex).index] = false;
	}

	public void Push (int index, Vector2 force)
	{
		if (states [index] == EnemyState.Contained) {

			states [index] = EnemyState.Charge;
			enemies [index].gameObject.SetActive (true);
			containers [index].gameObject.SetActive (false);
			Instantiate<ParticleSystem> (barrelBreakEffect, motors [index].position, Quaternion.identity);


		} else {
			paths [index] = null;
			states [index] = EnemyState.Batted;
			motors [index].rigidbody2D.AddForce (force, ForceMode2D.Impulse);
		}
		SFX.Play (pushedSFX);
	}

	public void Hurt (int index, int damage)
	{
		hitpoints [index] -= damage;
		if (hitpoints [index] <= 0) {
			states [index] = EnemyState.Dead;
			enemies [index].gameObject.SetActive (false);
		} else {
			motors [index].Flash (Color.red);
		}
	}

	private IEnumerator Attack (int index)
	{
		states [index] = EnemyState.Attack;
		Vector2 attackPoint = targetPosition;
		Vector2 startPosition = motors [index].position;
	
		float duration = types[index].attackDuration;
		float timer = 0;
		while (timer < duration) {
			if (states [index] == EnemyState.Attack) {
				motors [index].position = Vector2.Lerp (startPosition, attackPoint, timer / duration);
				timer += Clock.deltaTime;
				yield return null;
			} else {
				yield break;
			}
		}

		foreach (Collider2D other in Physics2D.OverlapCircleAll (attackPoint, attackHitRadius, goodGuysLayerMask)) {
			IHurtable hurtableThing = other.GetComponent<IHurtable> ();
			if (hurtableThing != null) {
				hurtableThing.Hurt (types [index].power);
			}
		}

		yield return new WaitForSeconds (types [index].attackCooldown);
		states [index] = EnemyState.Charge;
	}

	// Player uses this method to get points
	public int GetPoints (int index)
	{
		if (pointsLeft [index] > 0) {
			pointsLeft [index]--;
			hitEffects [index].transform.position = motors [index].position;

			hitEffects [index].Play ();
			SFX.PointCollect ();

			return 1;
		}
		return -1;
	}
}
