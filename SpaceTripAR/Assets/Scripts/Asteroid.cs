using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{

	public float speedRotate;
	private float speedR;
	float disToRealScale;
	float needSize;
	[HideInInspector] public bool startScale = false;
	float timeToDie;
	
	private Vector3 rotateDir;
	private float speedGame;
	private MapGenerator mG;
	float i = 0;
	
	void Start () {
		rotateDir = Random.insideUnitSphere;
		GameObject mapG = GameObject.FindWithTag("MG");
		mG = mapG.GetComponent<MapGenerator>();
		speedGame = mG.speedGame_MetSec;
		timeToDie = mG.dieTimeForObstacle;
		disToRealScale = mG.disToRealScale;
		needSize = Random.Range(mG.minSize, mG.maxSize);
		speedR = Random.Range(10.0f, speedRotate);
	}
	
	
	void Update () {
		transform.Rotate(rotateDir * Time.deltaTime * speedR);
		if (startScale && transform.localScale.x <= needSize)
		{
			transform.localScale += ((Vector3.one * Time.deltaTime * needSize)
			                         * speedGame) / disToRealScale;
		}
		else if (startScale && transform.localScale.x >= needSize)
		{
			if (i < timeToDie)
				i += Time.deltaTime;
			else
				Destroy(gameObject);
		}
		
	}

}
