using UnityEngine;
using System.Collections;

public static class Cards : MonoBehaviour {

	public static GameController Game;

	public static void Initialize(GameController gameController) {
		Game = gameController;
	}
	
	static Card[] allCards = {
		new Card(0,0) {
			// Game.drawCard();
		}
	};
}

public abstract class Card {
	public int TroopCost = 0;
	public int RandoCost = 0;
	public string Description;
	
	public Card(int TroopCost, int RandoCost, string Description) {
		this.TroopCost = TroopCost;
		this.RandoCost = RandoCost;
		this.Description = Description;
	}

	public bool CanInvoke() {
		return Cards.Game.RandoCounter >= RandoCost && Cards.Game.TroopCounter >= TroopCost;
	}
	public abstract void Invoke(CardController self);
}
