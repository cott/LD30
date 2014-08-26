using UnityEngine;
using System.Collections;

public class GameGraphics : MonoBehaviour {

	public Sprite[] EnemySprites;
	public Sprite[] WarSprites;
	public Sprite[] HomeSprites;
	
	// Use this for initialization
	void Start () {
		EnemySprites = Resources.LoadAll<Sprite>("enemies 1");
		WarSprites = Resources.LoadAll<Sprite>("war");
		HomeSprites = Resources.LoadAll<Sprite>("home");
	}
}
