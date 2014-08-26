using UnityEngine;
using System.Collections;

public class ButtonController : MonoBehaviour {

	public delegate void Callback();
	
	public Callback ClickCallback;
	
	void OnMouseUpAsButton() {
		if (ClickCallback != null) {
			ClickCallback();
		}
	}
}
