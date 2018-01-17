//using System;

//public interface IHealth
//{
//	Health Health { get; }
//}

public interface IHurtable
{
	void Hurt (int damage);
}
//
//[Serializable]
//public class Health
//{
//	public int maxHitPoints = 100;
//	public int hitPoints;
//
//	private Action hurtAction;
//	private Action hitPointsOverAction;
//
//	public void Initialize (Action hurtAction, Action hitPointsOverAction)
//	{
//		this.hurtAction = hurtAction;
//		this.hitPointsOverAction = hitPointsOverAction;
//		hitPoints = maxHitPoints;
//	}
//
//	public void Hurt (int damage)
//	{
//		hitPoints -= damage;
//		if (hitPoints <= 0) {
//			if (hitPointsOverAction != null) {
//				hitPointsOverAction ();
//			}
//		} else if (hurtAction != null) {
//			hurtAction ();
//		}
//	}
//}