using UnityEngine;
using System.Collections;

public class HeartBar : MonoBehaviour {
	
	private int maxHealth;
	public int startingHealth;
	public int healthPerHeart;

	private int currentHealth;

	public GUITexture heartGUI;
	public Texture[] images;

	private ArrayList hearts = new ArrayList();

	public int maxHeartsPerRow = 1;
	private float spacingX;
	private float spacingY;


	// Use this for initialization
	void Start () {
		currentHealth = startingHealth;
		spacingX = -heartGUI.pixelInset.width * 1 * Screen.width/960;
		spacingY = -heartGUI.pixelInset.height * 1 * Screen.height/540;

		AddHearts (startingHealth/healthPerHeart);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void AddHearts(int n){
		for (int i = 0; i < n; i ++) {
			Debug.Log ("HA");
			Transform newHeart = ((GameObject)Instantiate(heartGUI.gameObject,transform.position,Quaternion.identity)).transform; //gets transform of instantiated gameobject
			newHeart.parent = this.transform; //makes the hearts children of game object
			newHeart.gameObject.layer = 8;

			//FloorToInt returns lowest integer of float (ie. if 5.8, returns 5)
			int y = Mathf.FloorToInt(hearts.Count / maxHeartsPerRow); // until hearts.count/maxhearts equals 1, int y will be equal to 0
			int x = hearts.Count - y * maxHeartsPerRow; //resets x position for new row of hearts

			newHeart.GetComponent<GUITexture>().pixelInset = new Rect(x * spacingX + 0.25f*Screen.width, y * spacingY + 0.9f*Screen.height, -spacingX, -spacingY); //size and position of heartbar

			hearts.Add (newHeart);
		}
		maxHealth += n * healthPerHeart; //so maxHealth increases with added hearts
		currentHealth = maxHealth; //Health gets refilled when gaining a new heart
		UpdateHearts();

	}

	public void ModifyHealth(int amount){
		currentHealth += amount;
		currentHealth = Mathf.Clamp(currentHealth,0,maxHealth);//restricts the variable to a min and max (0 and maxHealth in this case)
		Debug.Log (currentHealth);
		UpdateHearts();
	}

	private void UpdateHearts () {
		bool restAreEmpty = false; //if we find one of the hearts is empty or partially empty, all the hearts after that will be empty
		int i = 0;
		foreach (Transform heart in  hearts) {
			if(restAreEmpty) {
				heart.GetComponent<GUITexture>().texture = images[0];
			} 
			else{
				i += 1;
				if (currentHealth >= i * healthPerHeart){ //detects if heart is full 
					heart.GetComponent<GUITexture>().texture = images[images.Length-1];
				}
				else{
					int currentHeartHealth = (int)(healthPerHeart - (healthPerHeart * i - currentHealth)); //calculates health in current heart
					int healthPerImage = healthPerHeart / images.Length; //how much health each image represents
					int imageIndex = currentHeartHealth / healthPerImage;

					if(imageIndex == 0 && currentHeartHealth > 0) {
						imageIndex = 1;
					}

					heart.GetComponent<GUITexture>().texture = images[imageIndex];
					restAreEmpty = true;
				}
			}
		}
	}
}
