using UnityEngine;
using System.Collections;

public class CardController : MonoBehaviour {
	public bool buttonDisabled = false;
	public CardLogic Logic;
	public int StartingCardLogicNumber = -1; // default to no starting card
	public bool IsEnemy = false;
	public int Hp = 0;
	public int BaseHp = 10;
	
	public TextMesh CardText;
	public TextMesh HpText;
	public SpriteRenderer Outline;
	
	GameController game;
	
	// Use this for initialization
	void Start () {
		game = GameObject.FindWithTag("GameController").GetComponent<GameController>();
		
		// populate the card logic
		if (StartingCardLogicNumber >= 0 && StartingCardLogicNumber < CardLogic.AllCards.Length) {
			SetLogic(CardLogic.AllCards[StartingCardLogicNumber]);
		} else {
			SetLogic(null);
		}
		RefreshEnemyStats();
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void OnMouseUpAsButton () {
		if (buttonDisabled) {
			Debug.Log("button is disabled!");
			return;
		}

		game.CardClicked(this);
	}
	
	public void disableButton(){
		buttonDisabled = true;
		Outline.color = new Color(0, 0, 0);
		Outline.renderer.enabled = true;
	}
	
	public void enableButton(){
		buttonDisabled = false;
		Outline.color = new Color(255, 255, 0);
		Outline.renderer.enabled = true;
	}
	
	public void SetLogic(CardLogic logic){
		Logic = logic;
		if (logic == null) {
			CardText.text = "[NOTHING]";
		} else {
			CardText.text = Logic.Description;
		}
	}
	public void RefreshEnemyStats(){
		if (IsEnemy && Logic != null) {
			Hp = BaseHp;
			HpText.text = "HP: " + Hp;
		} else {
			HpText.text = "";
		}
	}
	public void AddHp(int delta) {
		if (!IsEnemy) {
			Debug.Log("NOT AN ENEMY");
			return;
		}
		
		Hp += delta;
		if (Hp <= 0) {
			Hp = 0;
			game.KillEnemy(this);
		}
		if (Hp > BaseHp) {
			Hp = BaseHp;
		}
		HpText.text = "HP: " + Hp;
	}
}
