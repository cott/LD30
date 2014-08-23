using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public CardController[] HomeCards;
	public CardController[] WarCards;
	public CardController[] EnemyCards;
	
	public int Troops;
	public int Randos;

	// display logic
	public GUIText TroopCounter;
	public GUIText RandoCounter;

	// Use this for initialization
	void Start () {
		// TODO always start w/ the same 2 home & 2 war cards
		
		AddRandos(0);
		AddTroops(0);
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	public bool AddRandos(int diff) {
		if (Randos + diff < 0) { return false; }
		Randos += diff;
		RandoCounter.text = "Randos: " + Randos;
		return true;
	}
	
	public bool AddTroops(int diff, bool canTerminateGame=false) {
		if (!canTerminateGame && Troops + diff < 0) { return false; }
		Troops += diff;
		if (Troops < 0) { Troops = 0; }
		TroopCounter.text = "Troops: " + Troops;
		
		if (Troops == 0) { GameOver(); }
		return true;
	}
	
	public void DrawCard() {
		var emptySlot = false;
//		for (var i=0; i<3; i++) {
//			emptySlot = emptySlot || HomeCards[i].card == null;
//		}
		if (!emptySlot) {
			AddTroops(1);
		}
	}
	public void DrawCards(int numCards) { // lulz
		for (var i=0; i<numCards; i++) {
			DrawCard(numCards);
		}
	}
	
	public void GameOver() {
		Debug.Log("GAME OVER, SUCKER");
	}
}
