using UnityEngine;
using System.Collections;

public class NewHeroButton : MonoBehaviour {

	private GameController game;

	void Start () {
		game = GameObject.FindWithTag("GameController").GetComponent<GameController>();
	}
	
	void OnMouseUpAsButton() {
		game.newHero();
	}
}
