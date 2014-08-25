using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public CardController[] HomeCards;
	public CardController[] WarCards;
	public CardController[] EnemyCards;
	
	public int Troops;
	public int Randos;
	public int Enemies;
	
	public int Ap;
	
	// STATE
	public bool myTurn = true;
	public bool invokeMode = true; // versus "selectMode"
	public CardSelectCallback SelectionCallback;

	// display logic
	public GUIText TroopCounter;
	public GUIText RandoCounter;
	public GUIText EnemyCounter;
	public GUIText ActionBannerText;
	public GUIText ApCounter;
	
	// Delegate types
	public delegate bool CardValidator(CardController cardController, string stackLabel);
	public delegate void CardSelectCallback(CardController cardController);

	// Use this for initialization
	void Start () {
	
		Screen.SetResolution(720, 480, false); // TODO does this do anything?
	
		// TODO always start w/ the same 2 home & 2 war cards
		
		AddRandos(0);
		AddTroops(0);
		AddEnemies(0);
		ActionBannerText.text = "";
	}
	
	public bool AddRandos(int diff) {
		if (Randos + diff < 0) { return false; } // TODO wut
		Randos += diff;
		RandoCounter.text = "Randos: " + Randos;
		return true;
	}
	
	public bool AddTroops(int diff, bool canTerminateGame=false) {
		if (!canTerminateGame && Troops + diff < 0) { return false; } // TODO wut
		Troops += diff;
		if (Troops < 0) { Troops = 0; }
		TroopCounter.text = "Troops: " + Troops;
		
		if (Troops == 0) { GameOver(); }
		return true;
	}
	
	public bool AddEnemies(int diff) {
		if (Enemies + diff < 0) { return false; }
		Enemies += diff;
		EnemyCounter.text = "Enemies: " + Enemies;
		return true;
	}
	
	public void DrawCard() {
		var emptySlot = false;
//		for (var i=0; i<3; i++) {
//			emptySlot = emptySlot || HomeCards[i].card == null;
//		}
		if (!emptySlot) {
			AddRandos(1);
		}
	}
	public void DrawCards(int numCards) { // lulz
		for (var i=0; i<numCards; i++) {
			DrawCard();
		}
	}
	
	public void GameOver() {
		ActionBannerText.text = "GAME OVER, SUCKER";
		ActionBannerText.enabled = true;
	}
	public void Victory() {
		ActionBannerText.text = "YAY YOU WON";
		ActionBannerText.enabled = true;
	}
	
	// enter selection mode
	public void BeginSelection(CardValidator validator, CardSelectCallback callback, bool cancelable = false) {
		// TODO cancelable behavior
	
		Debug.Log("begin Selection phase");
		// display the "select a card" banner
		ActionBannerText.text = "Select a card";
		
		// mark which cards can be selected
		for(var i=0; i<3; i++) {
			validateCard(validator, HomeCards[i], "HOME");
			validateCard(validator, WarCards[i], "WAR");
			validateCard(validator, EnemyCards[i], "ENEMY");
		}
		SelectionCallback = callback;
		invokeMode = false;
	}
	void validateCard(CardValidator validator, CardController card, string label) {
		if (card.Logic != null && validator(card, label)) {
			card.enableButton();
		} else {
			card.disableButton();
		}
	} // exit selection mode
	void ExitSelectMode() {
		ActionBannerText.text = "";
		ActionBannerText.enabled = false;
		CardValidator validator = (card, label) => { return true; }; // always valid
		for(var i=0; i<3; i++) {
			validateCard(validator, HomeCards[i], "HOME");
			validateCard(validator, WarCards[i], "WAR");
			validateCard(validator, EnemyCards[i], "ENEMY");
		}
		SelectionCallback = null;
		invokeMode = true;
	}
	
	public void CardClicked(CardController card) {
		Debug.Log("Card " + (invokeMode ? "invoked" : "selected") + ": " + (card.Logic == null ? "-" : card.Logic.Description));
		// PATH 1: invoke
		if (myTurn && invokeMode) {
			if (card.Logic == null) {
				Debug.Log("tried invoking a null card");
			} else {
				card.Logic.InvokeCard(this); // pass the game object BECAUSE I'M JANKY
				if (card.Logic.SelfDestruct) {
					card.SetLogic(null); // this should really happen AFTER animations & things are done...
				}
			}
		}
		
		// PATH 2: select
		else if (!invokeMode) {
			if (SelectionCallback == null) {
				Debug.Log("ERROR: selected a card, but there's no selectionCallback!");
				return;
			}
			
			Debug.Log("doing the selectionCallback!");
			SelectionCallback(card);
			Debug.Log("finished the selectionCallback!");
			ExitSelectMode();
		}
	}
	
	public void AddAp(int delta) {
		if (Ap + delta < 0) {
			Ap = 0;
		} else {
			Ap += delta;
		}
		ApCounter.text = "Actions: " + Ap;
	}
	
	public void KillEnemy(CardController enemyCard) {
		Debug.Log("YOU KILLED AN ENEMY!");
		
		// remove card
		enemyCard.SetLogic(null);
		enemyCard.RefreshEnemyStats();
		
		// TODO add skull head avatar!
		
		// if all enemies have been defeated, rejoice!
		if (Enemies == 0) {
			// check that all 3 enemies on the board have been defeated as well
			var isVictory = true;
			foreach(var card in EnemyCards) {
				isVictory = isVictory && card.Logic == null; 
			}
			if (isVictory){
				Victory();
			}
		}
	}
	
	public void newHero() {
//		
//		// draw warrior & worker cards
//		CardLogic warrior = drawWarrior();
//		CardLogic worker = drawWorker();
//		
//		bool foundEmptyWarSlot = false;
//		// check for empty war slots
//		foreach (CardController card in WarCards) {
//			if (card.Logic == null) {
//				card.ShowAddButton(warrior);
//				foundEmptyWarSlot = true;
//			}
//		}
//		
//		ActionBannerText.text = "A new HERO! Send them to WORK" + (foundEmptyWarSlot ? " or to WAR!" : "!");
//		
//		// add "boot" option to occupied worker slots, "add" option for empty ones
//		foreach (CardController card in HomeCards) {
//			if (card.Logic == null) {
//				card.ShowAddButton(worker);
//			} else {
//				card.ShowReplaceButton(worker);
//			}
//		}
	}
	
	public CardLogic drawWorker(){
		var i = (int) Random.Range(0, 4);
		return CardLogic.AllCards[i];
	}
	public CardLogic drawWarrior(){
		var i = (int) Random.Range(4, 9);
		return CardLogic.AllCards[i];
	}
	public CardLogic drawEnemy(){
		var i = (int) Random.Range (9, 14);
		return CardLogic.AllCards[i];
	}
}
