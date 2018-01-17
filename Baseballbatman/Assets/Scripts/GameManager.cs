using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour 
{
	public static GameManager instance { get; private set; }

	public enum GameState { Menu, Playing }
	public GameState gameState { get; private set; }

	[SerializeField] private Player _playerPrefab;
	[SerializeField] private GameObject rescueePrefab;
	[SerializeField] private GeneratedLevel level;
	public string seed;
	[SerializeField] private GameObject _portalPrefab;

	public Player player { get; private set; }
	private GameObject rescuee;
	public GameObject Portal { get; private set; }
	public Vector3 PortalPosition { get; private set; }
	public bool RescueeSaved { get; private set; }


	[SerializeField] private SFX sfx;
	public EnemyController enemyController { get; set; }

	public bool useUI = true;
	[Header ("UI Items")]
	[SerializeField] private GameObject _startMenu;
	[SerializeField] private PlayerUI _playerUI;
	public PlayerUI playerUI { get { return _playerUI; } }

	int [,] walkableMap = null;

	void Awake ()
	{
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
		sfx.Initialize ();
		enemyController = GetComponent<EnemyController> ();
	}

	void Start ()
	{
		_startMenu.SetActive (true);
	}

	public void GenerateLevel ()
	{
		if (seed != "") {
			RandomUtility.Initialize (seed);
		} else {
			RandomUtility.Initialize (Random.ColorHSV());
		}

		level.Generate ();
	}

	public void StartGame ()
	{
		RandomUtility.Initialize (Random.rotation);

		StartCoordinates startCoords = level.Generate ();
		PortalPosition = startCoords.playerPosition;
		player = Instantiate<Player> (_playerPrefab, PortalPosition, Quaternion.identity);
		Portal = Instantiate (_portalPrefab, PortalPosition, Quaternion.identity);

		rescuee = Instantiate (rescueePrefab, startCoords.rescueePosition, Quaternion.identity);
		enemyController.Initialize (startCoords.enemyPositions, player.transform);

		walkableMap = startCoords.walkableMap;
		Pathfinder.Create(walkableMap, (Vector2)level.grid.cellSize);

		_playerUI.gameObject.SetActive (true);
		_startMenu.SetActive (false);
	}

	void Update ()
	{
		Pathfinder.Update ();
	}

	private void Clear ()
	{
		Destroy (player.gameObject);
		Destroy (rescuee);
		Destroy (Portal);
		enemyController.UnInitialize ();

		_playerUI.gameObject.SetActive (false);
		_startMenu.SetActive (true);

		RescueeSaved = false;
	}

	public void RescueeEscaped ()
	{
		RescueeSaved = true;
	}

	public void HeroEscaped ()
	{
		// Restart
		Clear ();
	}

}
