using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyType")]
public class EnemyType : ScriptableObject
{
	[SerializeField] private Enemy 	_prefab;
	[SerializeField] private float	_speed = 3f;
	[SerializeField] private int 	_maxHitpoints = 20;
	[SerializeField] private int 	_power = 2;
	[SerializeField] private float 	_attackDistance = 1f;
	[SerializeField] private float 	_attackDuration = 0.5f;
	[SerializeField] private float 	_attackCooldown = 1f;
	[SerializeField] private float 	_stunRecoveryTime = 5f;
	[SerializeField] private int 	_pointsValue = 5;

	public Enemy 	prefab 				{ get { return _prefab; } }
	public float 	speed 				{ get { return _speed; } }
	public int 		maxHitpoints 		{ get { return _maxHitpoints; } }
	public int 		power 				{ get { return _power; } }
	public float 	attackDistance		{ get { return _attackDistance; } }
	public float 	attackDuration		{ get { return _attackDuration; } }
	public float 	attackCooldown		{ get { return _attackCooldown; } }
	public float 	stunRecoveryTime	{ get { return _stunRecoveryTime; } }
	public int	 	pointsValue			{ get { return _pointsValue; } }
}