using UnityEngine;
using System.Collections;

public class CardController : MonoBehaviour {
	
	public string cardBody;
	
	public TextMesh CardText;
	
	GameController game;
	
	// Use this for initialization
	void Start () {
		game = GameObject.FindWithTag("GameController").GetComponent<GameController>();
		
		CardText.text = cardBody != null && cardBody.Length > 0 ? cardBody : "-";
		
		Cards.Initialize(this); // hacky
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void OnMouseDown () {
		game.AddRandos(1);
	}
}
