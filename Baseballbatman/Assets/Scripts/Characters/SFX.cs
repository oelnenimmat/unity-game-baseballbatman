using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SFX
{
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AudioClip pointSound;

	private static SFX instance;
	public void Initialize ()
	{
		// Confirm that all requred objects are set.
		if (
			audioSource == null ||
			pointSound == null 
		) {
			Debug.LogError ("Not all Effect components are set!");
			return;
		}

		instance = this;
	}

	public static void Play (AudioClip clip)
	{
		instance.audioSource.PlayOneShot (clip);
	}

	public static void PointCollect () 
	{ 
		instance.audioSource.PlayOneShot (instance.pointSound);
	}
}
