using System.Collections;
using System.Collections.Generic;
using GoogleARCore.Examples.CloudAnchor;
using UnityEngine;
using UnityEngine.Analytics;

public class MapGenerator : MonoBehaviour {

	public int mapWidth;
	public int mapHeight;
	public float noiseScale;

	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	private int numberLevel;
	private Transform[,] gridForward;
	private Transform[,] gridBack;
	public List<GameObject> levels = new List<GameObject>();
	int seedMoment = 2;
	
	[Header("For Grid")] 
	public Transform startTrans;
	public Transform playerTrans;
	
	public GameObject[] obstaclePrefabs;
	
	public float scaleGrid;
	public float scaleGridZ;
	
	
	public float speedGame_MetSec;
	[Range(0, 0.3f)]
	public float disForLevelGate;
	public float addSpeedGame;
	public int maxLevels;
	[HideInInspector]
	public int levelNum = 0;
	public float disToStart = 1.3f;

	[HideInInspector] public float dieTimeForObstacle = 0;
	[HideInInspector] public float timeOneLevel;

	[Header("Asteroid's settings")] 
	public float disToRealScale;

	public float disToDieSincePlayer;

	public float maxSize;
	public float minSize;

	private GameController gC;
	private Player player;

	void Start()
	{
		gC = GameObject.FindWithTag("GameController").GetComponent<GameController>();
		player = GameObject.FindWithTag("Player").GetComponent<Player>();
	}

	void Update ()
	{
		foreach (var level in levels)
		{
			level.transform.Translate(Vector3.forward * Time.deltaTime * speedGame_MetSec);
		}

		timeOneLevel = ((2.0f - (disForLevelGate * 4.0f)) * scaleGridZ) / speedGame_MetSec;
	}

	IEnumerator TimerDieLevel(GameObject mustDie)
	{
		float i = 0;
		while (i < (16.6f / speedGame_MetSec))
		{
			i += Time.deltaTime;
			yield return null;
		}
		levels.Remove(mustDie);
		Destroy(mustDie);
		yield return null;
	}

	IEnumerator TimerNewLevel(float speed)
	{
		float i = 0;
		while  (i < ((((2.0f - (disForLevelGate * 4.0f))  * scaleGridZ)) / speed))
		{
			i += Time.deltaTime;
			yield return null;
		} 
		if(maxLevels != levelNum)
			GenerateMap();
		else
		{
			player.PlayerWin();
		}
		yield return null;
	}

	IEnumerator TimerToInstantiateObstacleForward(int x, int y, float z, Transform gridObj)
	{
		float i = 0;
		while (i < (((1.0f - z - disForLevelGate) * scaleGridZ) / speedGame_MetSec))
		{
			i += Time.deltaTime;
			yield return null;
		}
		gridObj.GetComponent<Collider>().enabled = true;
		gridObj.GetComponent<Renderer>().enabled = true;
		gridObj.GetComponent<Asteroid>().enabled = true;
		gridObj.GetComponent<Asteroid>().startScale = true;
		yield return null;
	}
	
	IEnumerator TimerToInstantiateObstacleBack(int x, int y, float z, Transform gridObj)
	{
		float i = 0;
		while (i < (((1.0f + z - (disForLevelGate * 3.0f)) * scaleGridZ) / speedGame_MetSec))
		{
			i += Time.deltaTime;
			yield return null;
		}
		gridObj.GetComponent<Collider>().enabled = true;
		gridObj.GetComponent<Renderer>().enabled = true;
		gridObj.GetComponent<Asteroid>().enabled = true;
		gridObj.GetComponent<Asteroid>().startScale = true;
		yield return null;
	}
	
	public void GenerateMap()
	{
		float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed,
			noiseScale, octaves, persistance, lacunarity, offset);

		GameObject level = new GameObject("Level " + numberLevel);
		numberLevel++;
		
		// ----------------------Create obstacle's grid-------------------------
		Vector3 gridPos = startTrans.position;
		level.transform.position = gridPos;
		//level.transform.rotation = startTrans.rotation;
		gridForward = new Transform[mapWidth, mapHeight];
		gridBack = new Transform[mapWidth, mapHeight];
		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				if (disForLevelGate < noiseMap[x, y] && (1.0f - disForLevelGate) > noiseMap[x, y])
				{
					GameObject obstaclePrefab = obstaclePrefabs[Mathf.RoundToInt(Random.Range(0, obstaclePrefabs.Length))];
					gridForward[x, y] = Instantiate(obstaclePrefab.transform);
					gridForward[x, y].position = new Vector3((gridPos.x + ((x - mapWidth / 2) * scaleGrid))
					                           , (gridPos.y + ((y - mapHeight / 2) * scaleGrid))
					                           , gridPos.z + (scaleGridZ * noiseMap[x, y])
					                           - (scaleGridZ - disForLevelGate * 4) - disToStart - (disForLevelGate * scaleGridZ));//- scaleGridZ - disToStart - (disForLevelGate * scaleGridZ));
					gridForward[x, y].SetParent(level.transform);
					StartCoroutine(TimerToInstantiateObstacleForward(x, y, noiseMap[x, y], gridForward[x, y]));

					obstaclePrefab = obstaclePrefabs[Mathf.RoundToInt(Random.Range(0, obstaclePrefabs.Length))];
					gridBack[x, y] = Instantiate(obstaclePrefab.transform);
					gridBack[x, y].position = new Vector3((gridPos.x + ((x - mapWidth / 2) * scaleGrid))
					                          , (gridPos.y + ((y - mapHeight / 2) * scaleGrid))
					                           , gridPos.z - (scaleGridZ * noiseMap[x, y])
					                           - (scaleGridZ - disForLevelGate * 4) - disToStart + (disForLevelGate * scaleGridZ));//- scaleGridZ - disToStart + (disForLevelGate * scaleGridZ));
					gridBack[x, y].SetParent(level.transform);
					StartCoroutine(TimerToInstantiateObstacleBack(x, y, noiseMap[x, y], gridBack[x, y]));
				}
			}
		}

		// ----------- calculate die's time for obstacles ------------
			dieTimeForObstacle = (Vector3.Distance(startTrans.position, playerTrans.position)
			                      / speedGame_MetSec) + disToDieSincePlayer - disToRealScale;

		// ----------------------level---------------------
		level.transform.LookAt(playerTrans);
		StartCoroutine(TimerDieLevel(level));
		levels.Add(level);
		
		// ----------------------next level---------------------
		levelNum++;
		if(speedGame_MetSec <= 1.2f)
			speedGame_MetSec += addSpeedGame;
		NewLevelProperties();
		StartCoroutine(TimerNewLevel(speedGame_MetSec));
		
		
	}

	void NewLevelProperties()
	{
		//---------------------persistance----------------------
		persistance = Mathf.Sin(Mathf.PI / 4.0f * levelNum);
		if (persistance < 0.63f)
			persistance = 0.63f;
		
		//---------------------lacunarity---------------------
		lacunarity = Mathf.Sin(Mathf.PI / 10.0f * levelNum);
		if (lacunarity < 0)
		{
			lacunarity = -lacunarity;
			lacunarity = 1.0f - lacunarity;
			lacunarity *= 8.0f;
			lacunarity++;
		}
		lacunarity += 10.0f;
		
		//---------------------Scale---------------------
		noiseScale = 17.0f - (Mathf.Sin(Mathf.PI / 10.0f * levelNum) + levelNum * 0.09f);
		
		//---------------------Seed---------------------
		if(seedMoment == levelNum)
		{
			seedMoment += 2;
			seed ++;
		}
	}

	
	
	void OnValidate() {
		if (mapWidth < 1) {
			mapWidth = 1;
		}
		if (mapHeight < 1) {
			mapHeight = 1;
		}
		if (lacunarity < 1) {
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}
	}

}