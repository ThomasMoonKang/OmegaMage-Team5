using UnityEngine;
using System.Collections;

public class HeartTest : MonoBehaviour {

	public HeartBar heartBar;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI() {
		if (GUI.Button(new Rect(Screen.width/1.5f, Screen.height/4 + Screen.width/10 * 0, 100, 25), "Add Heart")){
			heartBar.AddHearts(1);
		}
		if (GUI.Button(new Rect(Screen.width/1.5f,Screen.height/4 + Screen.width / 10 * 1, 100, 25), "TakeDamage")){
			heartBar.ModifyHealth(-5);
		}
		if (GUI.Button(new Rect(Screen.width/1.5f,Screen.height/4 + Screen.width / 10 * 2, 100, 25), "Heal")){
			heartBar.ModifyHealth(5);
		}
	}
}
