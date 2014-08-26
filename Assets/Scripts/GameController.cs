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
	public int NextEnemyIndex = 0;
	
	// STATE
	public bool myTurn = true;
	public bool invokeMode = true; // versus "selectMode"
	public bool NewHeroMode = false; // overrides invokeMode OR selectMode
	public CardSelectCallback SelectionCallback;
	public ButtonController.Callback NewHeroCallback;

	// display logic
	public GUIText TroopCounter;
	public GUIText RandoCounter;
	public GUIText EnemyCounter;
	public GUIText ActionBannerText;
	public GUIText ApCounter;
	public GameObject NewHeroButton;
	public GameObject EndTurnButton;
	
	// Delegate types
	public delegate bool CardValidator(CardController cardController, string stackLabel);
	public delegate void CardSelectCallback(CardController cardController);

	// Use this for initialization
	void Start () {
	
		Screen.SetResolution(720, 540, false); // TODO does this do anything?
	
		// TODO always start w/ the same 2 home & 2 war cards
		
		AddRandos(0);
		AddTroops(0);
		AddEnemies(0);
		ActionBannerText.text = "";
		
		// init interface stuff
		initGui ();
		initCards ();
		EnterInvokeMode(); // hacky way to enter Invoke mode
	}
	
	public bool AddRandos(int diff) {
		if (Randos + diff < 0) { return false; } // TODO wut
		Randos += diff;
		RandoCounter.text = "Randos: " + Randos;
		return true;
	}
	
	public bool AddTroops(int diff) {
		Troops += diff;
		if (Troops < 0) { Troops = 0; }
		TroopCounter.text = "Troops: " + Troops;
		
		if (Troops == 0)
			GameOver();

		return true;
	}
	
	public bool AddEnemies(int diff) {
		if (Enemies + diff < 0) { return false; }
		Enemies += diff;
		EnemyCounter.text = "Enemies: " + Enemies;
		return true;
	}
	
	private void initGui(){
		NewHeroButton.GetComponent<ButtonController>().ClickCallback = () => { newHero(true); };
		EndTurnButton.GetComponent<ButtonController>().ClickCallback = EndTurn;
	}
	
	private void initCards(){
		// janky place to do this but WHATEVER
		for(var i=0; i<CardLogic.AllCards.Length; i++){
			CardLogic.AllCards[i].CardNum = i; // EW WHATEVER EW
		}
		
		// init home cards
		HomeCards[0].SetLogic(CardLogic.AllCards[0]);
		HomeCards[1].SetLogic(CardLogic.AllCards[1]);
		HomeCards[2].SetLogic(drawWorker());
		
		WarCards[0].SetLogic(CardLogic.AllCards[CardLogic.FirstWarCard + 2]);
		WarCards[1].SetLogic(drawWarrior());
		WarCards[2].SetLogic(drawWarrior());
		
		EnemyCards[0].SetLogic(CardLogic.AllCards[CardLogic.FirstEnemyCard + 2]);
		EnemyCards[1].SetLogic(CardLogic.AllCards[CardLogic.FirstEnemyCard + 1]);
		EnemyCards[2].SetLogic(drawEnemy());
	}

	
	public void GameOver() {
		ActionBannerText.text = "GAME OVER, SUCKER";
	}
	public void Victory() {
		ActionBannerText.text = "YAY YOU WON";
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
	}
	
	// exit selection mode & enter invokeMode
	void EnterInvokeMode() {
		Debug.Log("!!!!!!!! entering InvokeMode !!!!!!!!!!!!!!");
		ActionBannerText.text = "";
		for(var i=0; i<3; i++) {
			validateCard(InvokeValidator, HomeCards[i], "HOME");
			validateCard(InvokeValidator, WarCards[i], "WAR");
			validateCard(InvokeValidator, EnemyCards[i], "ENEMY");
		}
		SelectionCallback = null;
		invokeMode = true;
		if (myTurn) {
			NewHeroButton.SetActive(true);
		}
	}
	
	public void CardClicked(CardController card) {
		Debug.Log("Card " + (invokeMode ? "invoked" : "selected") + ": " + (card.Logic == null ? "-" : card.Logic.Description));
		
		if (NewHeroMode) {
			Debug.Log("ignore this click--we're in NewHeroMode");
		}
		
		// PATH 1: invoke
		if (myTurn && invokeMode) {
			if (card.Logic == null) {
				Debug.Log("tried invoking a null card");
			} else {
				var logic = card.Logic;
				if (card.Logic.SelfDestruct) {
					card.SetLogic(null); // this should really happen AFTER animations & things are done...
				}
				card.InvokedAlready = true;
				// invoke the card AFTER wiping its logic
				logic.InvokeCard(this); // pass the game object BECAUSE I'M JANKY
				
				// if still in invokeMode, refresh
				if (!NewHeroMode && invokeMode) {
					Debug.Log("still in InvokeMode. refreshing InvokeMode");
					EnterInvokeMode();
				}
			}
		}
		
		// PATH 2: select mode
		else if (!invokeMode) {
			if (SelectionCallback == null) {
				Debug.Log("ERROR: selected a card, but there's no selectionCallback!");
			} else {
				Debug.Log("doing the selectionCallback!");
				SelectionCallback(card);
				Debug.Log("finished the selectionCallback!");
			}
			EnterInvokeMode();
		}
	}
	
	public void AddAp(int delta) {
		if (Ap + delta <= 0) {
			Ap = 0;
			if (myTurn) {
				EndTurn();
			}
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
	
	
	
	public void newHero(bool costAp, ButtonController.Callback callback = null) {
	
		// hide button during HeroSelect phase
		NewHeroButton.SetActive(false);
		NewHeroMode = true;
		Debug.Log("!!!!!!! enter NewHeroMode !!!!!!!");
		
		if (costAp || callback != null) {
			NewHeroCallback = () => {
				if (callback != null) {
					callback();
				}
				if (costAp) {
					AddAp(-1);
				}
			};
		}
		
		// draw warrior & worker cards
		CardLogic warrior = drawWarrior();
		CardLogic worker = drawWorker();
		
		ActionBannerText.text = "go to work: " + worker.Description + "\ngo to war: " + warrior.Description;
		
		// check for empty war slots
		foreach (CardController card in WarCards) {
			if (card.Logic == null) {
				card.ShowAddButton(warrior);
				card.enableButton();
			} else {
				card.disableButton();
			}
		}
		// add "replace" option to occupied worker slots, "add" option for empty ones
		foreach (CardController card in HomeCards) {
			card.enableButton();
			if (card.Logic == null) {
				card.ShowAddButton(worker);
			} else {
				card.ShowReplaceButton(worker);
			}
		}
		foreach (CardController card in EnemyCards) {
			card.disableButton();
		}
	}
	public void ExitNewHeroMode(){
		NewHeroMode = false;
		foreach(var card in HomeCards) {
			card.HideNewHeroButtons();
		}
		foreach(var card in WarCards) {
			card.HideNewHeroButtons();
		}
		if (NewHeroCallback != null) {
			var callback = NewHeroCallback;
			NewHeroCallback = null;
			callback();
			Debug.Log("SHOULD BE FIRING MY NEW_HERO CALLBACK RN");
		}
		// if it's invokeMode, re-enter invokeMode. so janky.
		if (invokeMode) {
			EnterInvokeMode();
		}
	}
	
	public bool InvokeValidator(CardController card, string stackLabel){
		return ("WAR".Equals (stackLabel) || "HOME".Equals(stackLabel))
			&& !card.InvokedAlready
			&& Troops >= card.Logic.TroopCost
			&& Randos >= card.Logic.RandoCost;
	}
	
	public CardLogic drawWorker(){
		var i = (int) Random.Range(0, CardLogic.FirstWarCard);
		return CardLogic.AllCards[i];
	}
	public CardLogic drawWarrior(){
		var i = (int) Random.Range(CardLogic.FirstWarCard, CardLogic.FirstEnemyCard);
		return CardLogic.AllCards[i];
	}
	public CardLogic drawEnemy(){
		var i = (int) Random.Range (CardLogic.FirstEnemyCard, CardLogic.AllCards.Length);
		return CardLogic.AllCards[i];
	}
	
	public void EndTurn(){
		Debug.Log("!!!!!!!!!! END TURN !!!!!!!!!!!! AP:" + Ap);
		myTurn = false;
		
		// restore my AP before it's my enemy's turn
		AddAp(3);
		
		// LET ENEMIES TAKE THEIR TURN
		
		// if there's no card to execute, draw a new one
		if(EnemyCards[NextEnemyIndex].Logic == null) {
			EnemyCards[NextEnemyIndex].SetLogic(drawEnemy());
			EnemyCards[NextEnemyIndex].AddHp(EnemyCards[NextEnemyIndex].BaseHp);
			EnterInvokeMode();
		} else if (!EnemyCards[NextEnemyIndex].InvokedAlready){
			// OTHERWISE execute the current card
			EnemyCards[NextEnemyIndex].Logic.InvokeCard(this);
			// TODO if we added enemy cards w/ choice, we'd have to extend invokeCard to take a callback!
			// this will set up "EnterInvokeMode" for us anyways
		}
		EnemyCards[NextEnemyIndex].InvokedAlready = false; // how freezing works
		
		// switch to the next card for "next turn" & refresh visuals
		EnemyCards[NextEnemyIndex].NextTurnMarker.SetActive(false);
		NextEnemyIndex = (NextEnemyIndex + 1) % 3;
		EnemyCards[NextEnemyIndex].NextTurnMarker.SetActive(true);
		
		// SWICH BACK TO YOUR TURN
		
		foreach (var card in HomeCards) {
			card.InvokedAlready = false;
		}
		foreach (var card in WarCards) {
			card.InvokedAlready = false;
		}
		myTurn = true;
	}
}
