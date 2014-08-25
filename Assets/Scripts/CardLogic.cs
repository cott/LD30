using UnityEngine;
using System.Collections;

public class CardLogic {

	public int CardNum;
	public string Description;
	
	public delegate void InvokeFunc(GameController game, CardController target);
	
	private int TroopCost = 0;
	private int RandoCost = 0;
	private InvokeFunc invokeFunc;
	private GameController.CardValidator cardValidator;
	public bool SelfDestruct = false; // handled within GameController
	private GameController Game;

	// Use this for initialization
	public static CardLogic[] AllCards = {
		// HOME
		new CardLogic (0,0, "+1 troop", (g, _) => { g.AddTroops(1); }), // 0
		new CardLogic (0,0, "+2 cards", (g, _) => { g.DrawCards(2); }), // 1
		new CardLogic (2,0, "mobilize 2", (g, _) => { g.AddTroops(2); }),
		new CardLogic (1,0, "-1 civilian, +2 actions", (g, _) => { g.AddAp(2); }),
		
		// WAR
		new CardLogic (0,2, "-2 troops, 2 dmg", (g, t) => { t.AddHp(-2); }, true, EnemyValidator), // 4
		new CardLogic (0,3, "-3 troops, 3 dmg", (g, t) => { t.AddHp(-3); }, true, EnemyValidator),
		new CardLogic (0,4, "-4 troop, 6 dmg", (g, t) => { t.AddHp(-6); }, true, EnemyValidator),
		new CardLogic (0,1, "-1 troop, 2 dmg", (g, t) => { t.AddHp(-2); }, true, EnemyValidator),
		new CardLogic (0,0, "-1/2 of all troops, all Titans lose 1/2 of their health", (g, _) => {
			g.AddTroops (-g.Troops / 2);
			foreach(CardController card in g.EnemyCards) {
				card.AddHp(- card.Hp / 2);
			}
		}, true, EnemyValidator),
		
		// ENEMIES
		new CardLogic (0,2, "-2 troops", (g, _) => {}), // 9
		new CardLogic (0,3, "-3 troops", (g, _) => {}),
		new CardLogic (0,0, "+3 health for all enemies", (g, _) => {
			foreach(CardController card in g.EnemyCards) {
				card.AddHp(3);
			}
		}),
		new CardLogic (0,1, "-1 action, -1 troop", (g, _) => { g.AddAp(-1); }),
		new CardLogic (0,0, "-1/2 of all troops", (g, _) => {
			g.AddTroops (-g.Troops / 2);
		}, true, EnemyValidator), // 13
	};
	
	public CardLogic(int randos, int troops, string description, InvokeFunc invokeFunc, bool selfDestruct = false, GameController.CardValidator cardValidator = null) {
		TroopCost = troops;
		RandoCost = randos;
		Description = description;
		
		this.SelfDestruct = selfDestruct;
		this.invokeFunc = invokeFunc;
		this.cardValidator = cardValidator;
	}
	
	public void InvokeCard(GameController game) {
		
		if (cardValidator == null) {
			game.AddRandos(-RandoCost);
			game.AddTroops(-TroopCost);
			// doesn't need a target
			invokeFunc(game, null);
			game.AddAp(-1);
		}
		else {
			game.BeginSelection(cardValidator, (c) => {
				// subtract any cost after the selection has been made
				game.AddRandos(-RandoCost);
				game.AddTroops(-TroopCost);
				invokeFunc(game, c);
				game.AddAp(-1);
			}, false); // not cancelable!
		}
	}
	
	// selection callbacks
	public static bool EverythingValidator(CardController card, string typeLabel){
		return card.Logic != null;
	}
	public static bool EnemyValidator(CardController card, string typeLabel){
		return card.Logic != null && "ENEMY".Equals(typeLabel);
	}
	public static bool WarriorValidator(CardController card, string typeLabel) {
		return card.Logic != null && "WAR".Equals(typeLabel);
	}
}
