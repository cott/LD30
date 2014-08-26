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
	
	public SpriteRenderer Avatar;
	
	public GameObject AddButton;
	public GameObject ReplaceButton;
	public GameObject NextTurnMarker; // TODO make show up in the beginning
	
	public bool InvokedAlready = false;
	
	public SpriteRenderer InfoBox;
	public SpriteRenderer InfoBoxDisabled;
	public SpriteRenderer Shadow;
	public SpriteRenderer ShadowHighlight;
	
	private CardLogic newHeroCard;
	
	GameController game;
	GameGraphics graphics;
	
	// Use this for initialization
	void Start () {
		game = GameObject.FindWithTag("GameController").GetComponent<GameController>();
		graphics = GameObject.FindWithTag("GameController").GetComponent<GameGraphics>();
		
		// populate the card logic
		if (StartingCardLogicNumber >= 0 && StartingCardLogicNumber < CardLogic.AllCards.Length) {
			SetLogic(CardLogic.AllCards[StartingCardLogicNumber]);
		} else {
			SetLogic(null);
		}
		RefreshEnemyStats();
		
		AddButton.GetComponent<ButtonController>().ClickCallback = () => {
			SetLogic(newHeroCard);
			game.ExitNewHeroMode();
		};
		ReplaceButton.GetComponent<ButtonController>().ClickCallback = () => {
			SetLogic(newHeroCard);
			game.AddRandos(1);
			game.ExitNewHeroMode();
		};
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
		InfoBox.enabled = false;
		InfoBoxDisabled.enabled = true;
	}
	
	public void enableButton(){
		buttonDisabled = false;
		InfoBox.enabled = true;
		InfoBoxDisabled.enabled = false;
	}

	public void ShowAddButton (CardLogic warrior)
	{
		AddButton.SetActive(true);
		newHeroCard = warrior;
	}
	public void ShowReplaceButton (CardLogic warrior)
	{
		ReplaceButton.SetActive(true);
		newHeroCard = warrior;
	}
	public void HideNewHeroButtons (){
		AddButton.SetActive(false);
		ReplaceButton.SetActive(false);
		newHeroCard = null;
	}
	
	public void SetLogic(CardLogic logic){
		Logic = logic;
		if (logic == null) {
			CardText.text = "[NOTHING]";
		} else {
			CardText.text = Logic.Description;
		}
		RefreshGraphics();
	}
	public void RefreshEnemyStats(){
		if (IsEnemy && Logic != null) {
			Hp = BaseHp;
			HpText.text = "HP: " + Hp;
		} else {
			HpText.text = "";
		}
	}
	public void RefreshGraphics(){
		// render monster (if monster)
		Debug.Log("HOME SPRITES LENGTH ********* " + graphics.WarSprites.Length);
		Avatar.enabled = true;
		if (Logic == null) {
			Avatar.enabled = false;
		}
		else if (Logic.CardNum >= CardLogic.FirstEnemyCard) {
			int i = (Logic.CardNum - CardLogic.FirstEnemyCard) % graphics.EnemySprites.Length;
			Avatar.sprite = graphics.EnemySprites[i];
		} else if (Logic.CardNum >= CardLogic.FirstWarCard) {
			int i = (Logic.CardNum - CardLogic.FirstWarCard) % graphics.WarSprites.Length;
			Avatar.sprite = graphics.WarSprites[i];
		} else if (Logic.CardNum >= 0) {
			int i = Logic.CardNum % graphics.HomeSprites.Length;
			Avatar.sprite = graphics.HomeSprites[i]; // TODO  home sprites
		} else {
			Avatar.enabled = false;
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
