using UnityEngine;
using System.Collections;
using System.Collections.Generic; //Enables List<>s
using System.Linq; //Enables LING queries

//The MPhase enum is used to track the phase of mouse interaction
public enum MPhase {
	idle,
	down,
	drag
}

//The ElementType enum
public enum ElementType {
	earth,
	water,
	air,
	fire,
	aether,
	none
}

//MouseInfo stores information about the mouse in each frame of interaction
[System.Serializable]
public class MouseInfo {
	public Vector3 loc; //3D loc of the mouse near z=0
	public Vector3 screenLoc; //screen pos of mouse
	public Ray ray; //Ray frmo the mouse into 3D space
	public float time; //Time this mouse info was recorded
	public RaycastHit hitInfo; //info abt what was hit by the ray
	public bool hit; //Wheter the mouse was over any collider

	//These methods see if the mouseRay hits nything
	public RaycastHit Raycast() {
		hit = Physics.Raycast (ray, out hitInfo);
		return(hitInfo);
	}

	public RaycastHit Raycast(int mask) {
		hit = Physics.Raycast (ray, out hitInfo, mask);
		return(hitInfo);
	}
}

//Mage is a sublass of PT_MonoBehavior
public class Mage : PT_MonoBehaviour {
	static public Mage S;
	static public bool DEBUG = true;
	public float mTapTime = 0.1f; //How long is considered a tap
	public GameObject tapIndicatorPrefab; //prefab to the tap indicator
	public float mDragDist = 5; //Min dist in pixels to be a drag

	public float activeScreenWidth = 1; //% of the screen to use

	public float speed = 2; //mage walk speed

	public GameObject[] elementPrefabs; //The lement_sphere prefabs
	public float elementRotDist = 0.5f; //Radihus of rotation
	public float elementRotSpeed = 0.5f; //Period of rotation
	public int maxNumSelectedElements = 1;

	public bool _________________;

	public MPhase mPhase = MPhase.idle;
	public List<MouseInfo> mouseInfos = new List<MouseInfo>();

	public bool walking = false;

	public Vector3 walkTarget;
	public Transform characterTrans;

	public List<Element> selectedElements = new List<Element>();

	void Awake() {
		S = this; //Set the Mage Singleton
		mPhase = MPhase.idle;

		//Find the characterTrans to rotate with Face()
		characterTrans = transform.Find ("CharacterTrans");
	}

	void Update () {
		//Find whether the mouse button - was pressed or released this frame
		bool b0Down = Input.GetMouseButtonDown (0);
		bool b0Up = Input.GetMouseButtonUp(0);

		//Handle all input here except for inventory buttons
		/*
		 * There are only a few possible actions
		 * 1. tap on ground to move to that point
		 * 2. Drag on ground with no spell selected to move the Mage
		 * 3. Drag on ground with spell to cast along ground
		 * 4. Tap on an enemy to atk (or force-push away without an element)
		 */
		 
		 //An example of using <to return a bool value
		bool inActiveArea = (float) Input.mousePosition.x/Screen.width<activeScreenWidth;

		//This is handled as an if statement instead of siwtch beacsue a tap
		//can sometimes happen within a single frame
		if (mPhase == MPhase.idle) { //if the mouse is idle
			if(b0Down && inActiveArea) {
				mouseInfos.Clear(); //clear the mouseinfos
				AddMouseInfo(); //And add a first MouseInfo

				//IF the mouse was clicked on stomething, its a valid mousedown
				if (mouseInfos[0].hit) { //Something was hit!
					MouseDown(); //Call MouseDown()
					mPhase = MPhase.down; //and set the phase
				}
			}
		}

		if (mPhase == MPhase.down) { //if the mouse is down
			AddMouseInfo(); //Add a mouseinfo for this frame
			if (b0Up) { //The mouse button was released
				MouseTap(); //this was a tap
				mPhase = MPhase.idle;
			} else if(Time.time - mouseInfos[0].time > mTapTime) {
				//If its been down longer than a tap, this may be a drag but
				//to be a drag, it must also have moved a certain number of pixels on screen
				float dragDist = (lastMouseInfo.screenLoc - mouseInfos[0].screenLoc).magnitude;
				if(dragDist >= mDragDist) {
					mPhase = MPhase.drag;
				}

				//However, drag will immediately start after mTapTime if there are no elements selected
				if (selectedElements.Count == 0) {
					mPhase = MPhase.drag;
				}
			}
		}
		if (mPhase == MPhase.drag) {//if the mouse is being dragged
			AddMouseInfo();
			if(b0Up){
				//The mouse button was released
				MouseDragUp();
				mPhase = MPhase.idle;
			} else {
				MouseDrag(); //stil dragging
			}
		}

		OrbitSelectedElements();
	}

	//Pulls info about the Mouse, adds it to mouseinfos, and return it
	MouseInfo AddMouseInfo() {
		MouseInfo mInfo = new MouseInfo();
		mInfo.screenLoc = Input.mousePosition;
		mInfo.loc = Utils.mouseLoc; //Get pos of mouse at z=0
		mInfo.ray = Utils.mouseRay; //Gets the ray frmo the Main Camera through the mosue pointer
		mInfo.time = Time.time;
		mInfo.Raycast (); //Default is to raycast with no mask

		if(mouseInfos.Count == 0) {
			//IF this is the first mouseInfo
			mouseInfos.Add(mInfo); //Add mInfo to mouseinfos
		} else {
			float lastTime = mouseInfos[mouseInfos.Count-1].time;
			if(mInfo.time != lastTime) {
				//if time has passed since the last mouseInfo
				mouseInfos.Add(mInfo); //Add mInfo to mouseInfos
			}
			//This time test is necessary because ADdMouseInfo() could be called twice in one frame
		}
		return(mInfo); // Return mInfo as well
	}

	public MouseInfo lastMouseInfo {
		//Access to the latest mouseInfo 
		get {
			if (mouseInfos.Count == 0) return(null);
			return(mouseInfos[mouseInfos.Count-1]);
		}
	}

	void MouseDown() {
		//The mouse was pressed on something (it coudl be a drag or tap
		if (DEBUG) print("Mage.MouseDown()");
	}

	void MouseTap() {
		//Something was tapped like a button
		if(DEBUG) print("Mage.MouseTap()");

		WalkTo(lastMouseInfo.loc); //Walk tot he latest mouseInfo pos
		ShowTap(lastMouseInfo.loc); //show where the play ert tapped
	}

	void MouseDrag() {
		//The mosue is being dragged across osmething
		if(DEBUG) print("Mage.MouseDrag()");

		//Continuously walk to ward the current mouseinfo pos
		WalkTo(mouseInfos[mouseInfos.Count-1].loc);
	}

	void MouseDragUp() {
		//the mosue is released after being dragged
		if(DEBUG) print("Mage.Mouse.DragUp()");
		StopWalking();
	}


	//Walk to a specific position. The Position.z is always 0
	public void WalkTo(Vector3 xTarget) {
		walkTarget = xTarget; //Set th epoint to walk to
		walkTarget.z = 0; //Force z=0
		walking=true; //Now the mage is walkin
		Face(walkTarget); //Look in the direction of the wlakTarget
	}

	public void Face(Vector3 poi) {//face toward a point of intereste
		Vector3 delta = poi-pos; //Find vector the point of interest
		//Use atan2 to get the rotation around Z that points the X-axis of 
		// _Mage:CharacterTrans toward poi
		float rZ = Mathf.Rad2Deg * Mathf.Atan2 (delta.y,delta.x);
		//Set the rotation of characterTrans(doesn't actually roate _Mage)
		characterTrans.rotation = Quaternion.Euler (0,0,rZ);
	}

	public void StopWalking () { //STop the mage from walking
		walking = false;
		GetComponent<Rigidbody>().velocity = Vector3.zero;
	}

	void FixedUpdate() { //happens every physics step 50 times /sec
		if (walking) {///if mAge is walking
			if ((walkTarget-pos).magnitude < speed*Time.fixedDeltaTime) {
				//If mage is very close to walkTarget, just stop there
				pos = walkTarget;
				StopWalking();
			} else {
				//Otherwise, move toward walkTArget
				this.GetComponent<Rigidbody>().velocity = (walkTarget-pos).normalized*speed;
			}
		} else {
			//If not walking, velocity should be zero
			GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
	}

	void OnCollisionEnter (Collision coll) {
		GameObject otherGO = coll.gameObject;

		//Volliding with a wall can also stop walking
		Tile ti = otherGO.GetComponent<Tile>();
		if(ti != null) {
			if (ti.height > 0) {//IF ti.height is >0
				//The this ti is a wall, and mage should stop
				StopWalking();
				Debug.Log ("TileTooHIGH");
			}
		}
	}

	//Show where the player tapped
	public void ShowTap(Vector3 loc) {
		GameObject go = Instantiate(tapIndicatorPrefab) as GameObject;
		go.transform.position = loc;
	}

	//Chooses an Eelemnt_Sphere of elType and adds it to selectedElements
	public void SelectElement(ElementType elType){
		if(elType == ElementType.none) { //If its the none element...
			ClearElements(); //then clear all elements
			return; //and return
		}

		if(maxNumSelectedElements == 1){
			//if only one can be selected, clear the existing one..
			ClearElements(); //so it can be replaced
		}

		//Can't select more than max Num selected elmenets simultaneously
		if(selectedElements.Count >= maxNumSelectedElements) return;

		//It's okay to add this element
		GameObject go = Instantiate(elementPrefabs[(int) elType]) as GameObject;
		//^Note the typecast fromElementType to int in the line above
		Element el = go.GetComponent<Element>();
		el.transform.parent = this.transform;

		selectedElements.Add (el); //Add el to the list of selceted ELements
	}

	//Clears al lalememnets from selcted leements and destroys their GameObjects
	public void ClearElements(){
		foreach (Element el in selectedElements) {
			//Destroy each GameObject in the list
			Destroy(el.gameObject);
		}
		selectedElements.Clear (); //and clear the list
	}

	//Called every update() to orbit the elements around
	void OrbitSelectedElements(){
		//if none selected just return
		if(selectedElements.Count == 0)return;

		Element el;

		Vector3 vec;
		float theta0, theta;
		float tau = Mathf.PI*2; //Taue is 360 in radians (ie, 6.283..

		//Divid ethe circle into the number of elemetns that are orbiting
		float rotPerElement = tau / selectedElements.Count;

		//The base roation angle (theta0) is set based on time
		theta0 = elementRotSpeed*Time.time *tau;

		for(int i=0; i<selectedElements.Count; i++){
			//Determine the rotation angle for each elements
			theta = theta0 + i*rotPerElement;
			el = selectedElements[i];
			//Use simple trigonometry to turnt he angle into a unit vector
			vec = new Vector3(Mathf.Cos (theta),Mathf.Sin (theta),0);
			//Multiplty that unit vector by the elementRotDist
			vec *= elementRotDist;
			//raise the leemnt to waist height
			vec.z = -0.5f;
			el.lPos = vec; //Set the pos of Element_Sphere
		}
	}
}
