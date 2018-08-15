using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using GoogleARCore.Examples.CloudAnchor;
using PSlicer;
using UnityEngine;

public class Player : MonoBehaviour
{

	public Vector3 normalPos;
	[HideInInspector]
	public Transform target;

	public Transform camera;
	public GameObject hitParticle;
	public GameObject dieParticle;

	public float timeToBack;
	[HideInInspector] 
	public float health;
	public float minusHealth;
	private Rigidbody rB;
	private GameController gC;
	public float minusScaleForWin;
	private float effectorOffset;
	public float smoothing;
	[Header("Advent Player")]
	public GameObject playerEffector;

	public float timeAdvent;
	public float offSetLength;
	Effector effectorPlayer;
	
	void Start ()
	{
		gC = GameObject.FindWithTag("GameController").GetComponent<GameController>();
	}
	
	void Update () {
			transform.LookAt(target);
	}

	
	
	public void NormalPosition(float returnPosTime, float moveTime)
	{
		StartCoroutine(NormalPos(returnPosTime, moveTime));
	}

	public void AdventPlayer()
	{
		effectorPlayer = playerEffector.GetComponent<Effector>();
		StartCoroutine(AdventPlayerIEnumerator());
	}

	IEnumerator AdventPlayerIEnumerator()
	{
		float i = 0;
		while (i < timeAdvent)
		{
			i += Time.deltaTime;
			float k = Time.deltaTime / timeAdvent;
			effectorPlayer._offset += offSetLength * k;
			yield return null;
		}
		yield return null;
	}
	
	public IEnumerator NormalPos(float returnPosTime, float moveTime)
	{
		float i = 0f;
		while (i < returnPosTime)
		{
			i += Time.deltaTime;
			yield return null;
		}

		float c = 0f;
		Vector3 startPos = transform.localPosition;
		while (c < 1.0f)
		{
			c += Time.deltaTime / moveTime;
			transform.localPosition = Vector3.Lerp(startPos, normalPos, c);
			yield return null;
		}

		transform.localPosition = normalPos;
		gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
		gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		
		yield return null;
	}

	private void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.tag == "Obstacles" || col.gameObject.tag == "Luna")
		{
			ContactPoint contact = col.contacts[0];
			GameObject pS = Instantiate(hitParticle, contact.point, Quaternion.identity);
			pS.transform.SetParent(transform);
			Destroy(pS, 0.7f);
			health -= minusHealth;
			if (health > 0)
			{
				StartCoroutine(NormalPos(timeToBack, 0.9f));
			}
			else
			{
				transform.SetParent(gC.gameObject.transform);
				GameObject pSDie = Instantiate(dieParticle, contact.point, Quaternion.identity);
				pSDie.transform.SetParent(transform);
				gC.GameOver();
			}
		}
	}

	IEnumerator Win()
	{
		float i = 0;
		while (i < 1.7f)
		{
			i += Time.deltaTime;
			yield return null;
		}
		gC.PlayerWin();
		while (transform.localScale.x > 0)
		{
			transform.localScale -= Vector3.one * minusScaleForWin;
			transform.Translate(Vector3.forward * Time.deltaTime * 0.2f);
			yield return null;
		}
		yield break;
	}
	
	public void PlayerWin()
	{
		Debug.Log("Player Win!");
		StartCoroutine(Win());
	}
}
