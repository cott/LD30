using UnityEngine;
using System.Collections;

public class CardLogic {

	public int CardNum;
	public string Description;
	
	public delegate void InvokeFunc(GameController game, CardController target);
	
	public int TroopCost = 0;
	public int RandoCost = 0;
	private InvokeFunc invokeFunc;
	private GameController.CardValidator cardValidator;
	public bool SelfDestruct = false; // handled within GameController
	private GameController Game;
	private bool usesApImmediately;

	public static int FirstWarCard = 7;
	public static int FirstEnemyCard = 13;
	
	// Use this for initialization
	public static CardLogic[] AllCards = {
		// HOME
		new CardLogic (0,0, "+3 Civilians", (g, _) => { g.AddRandos(5); }), // 0
		new CardLogic (2,0, "Deploy 2", (g, _) => { g.AddTroops(2); }),
		
		new CardLogic (0,0, "+5 Civilians", (g, _) => { g.AddRandos(5); }), // 0
		new CardLogic (0,0, "+2 Civilians\n+1 Hero", (g, _) => { g.AddRandos(2); g.newHero(true); }, false), // 1
		new CardLogic (0,0, "+1 Hero\n+1 Action", (g, _) => { g.newHero(true); }, false, false), // 1
		new CardLogic (0,0, "+1 Civilian\n+2 Actions", (g, _) => { g.AddRandos(1); g.AddAp(2); }),
		new CardLogic (0,0, "Deploy All Civilians\n(Single Use)", (g, _) => {
			g.AddTroops (g.Randos);
			g.AddRandos (-g.Randos);
		}, true, true),
		
		// WAR		
		new CardLogic (0,4, "-4 Soldiers\n6 dmg", (g, t) => { t.AddHp(-6); }, true, true, EnemyValidator),
		new CardLogic (0,2, "-2 Soldiers\n4 dmg", (g, t) => { t.AddHp(-4); }, true, true, EnemyValidator),
		new CardLogic (0,0, "2 dmg", (g, t) => { t.AddHp(-2); }, true, true, EnemyValidator), // ** SPECIAL ONE **
		new CardLogic (0,4, "-4 Soldiers\n2 dmg (All Enemies)", (g, t) => {
			foreach(CardController card in g.EnemyCards) {
				card.AddHp(-2);
			}
		}, true, true),
		new CardLogic (0,0, "-50% Soldiers\n50% dmg (All)", (g, _) => {
			g.AddTroops (-g.Troops / 2);
			foreach(CardController card in g.EnemyCards) {
				card.AddHp(-card.Hp / 2);
			}
		}, true, true),
		new CardLogic (0,1, "-1 Soldier\n1 dmg\nFreezes Enemy", (g, t) => { t.AddHp(-1); t.InvokedAlready = true; }, true, true, EnemyValidator),
		
		// ENEMIES
		new CardLogic (0,0, "+2 health\n(All Enemies)", (g, _) => {
			foreach(CardController card in g.EnemyCards) {
				card.AddHp(2);
			}
		}, false),
		new CardLogic (0,4, "-3 Soldiers", (g, _) => {}, false),
		new CardLogic (0,2, "-2 Soldiers", (g, _) => {}, false),
		new CardLogic (0,0, "-All Civilians", (g, _) => { g.AddRandos(-g.Randos); }, false, true, EnemyValidator), // BOSS FIGHT
		new CardLogic (0,0, "-50% Soldiers", (g, _) => {
			g.AddTroops (-g.Troops / 2);
		}, false, true, EnemyValidator), // 13
		new CardLogic (0,1, "-1 Action\n-1 Soldier", (g, _) => { g.AddAp(-1); }, false),
		new CardLogic (0,0, "Freeze All Heroes\nin the Homeland", (g, _) => {
			foreach(CardController card in g.HomeCards) {
				card.InvokedAlready = true;
			}
		}, false, true, EnemyValidator), // 13
	};
	
	// uses AP = false IF Ap drains in the callback (newHero card)
	public CardLogic(int randos, int troops, string description, InvokeFunc invokeFunc,  bool usesAp = true, bool selfDestruct = false, GameController.CardValidator cardValidator = null) {
		TroopCost = troops;
		RandoCost = randos;
		Description = description;
		
		this.SelfDestruct = selfDestruct;
		this.invokeFunc = invokeFunc;
		this.cardValidator = cardValidator;
		this.usesApImmediately = usesAp;
	}
	
	public void InvokeCard(GameController game) {
		
		if (cardValidator == null) {
			game.AddRandos(-RandoCost);
			game.AddTroops(-TroopCost);
			// doesn't need a target
			invokeFunc(game, null);
			
			// otherwise this'll happen in the callback
			if (usesApImmediately) {
				game.AddAp(-1);
			}
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
