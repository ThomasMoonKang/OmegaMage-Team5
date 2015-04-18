using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * TapIndicator makes use of the PT_Move class from ProtoTools. This allows it to
 * use a Bezizer curve to alter position, rotation ,scale etc.
 * 
 * You'll also notice that htis adds several public fields to the insepctor*/

public class TapIndicator : PT_Mover {

	public float lifeTime = 0.4f;//how long will it last
	public float[] scales; //the scales i tinterpolates
	public Color[] colors; //the coors it interpolates

	void Awake() {
		scale = Vector3.zero; //This intially hides the indicator
	}

	// Use this for initialization
	void Start () {
		//PT_Mover works base don the PT_Loc class, which contains informatio
		//About position, rot and scale. It's similar to a transfomr but
		//simpler and unity wontl et us creat transform at will

		PT_Loc pLoc;
		List<PT_Loc> locs = new List<PT_Loc>();

		//The position is always the same and always at z=-0.1f

		Vector3 tPos = pos;
		tPos.z = -0.1f;

		//You must have an equal numebr of scales and colors in the Inspector
		for(int i=0; i<scales.Length; i++){
			pLoc = new PT_Loc();
			pLoc.scale = Vector3.one * scales[i]; //EAch scale
			pLoc.pos = tPos;
			pLoc.color = colors[i]; //and each color

			locs.Add (pLoc); //is added to locs
		}

		//callback is a function delegate that can call a void function() when the move is done
		callback = CallbackMethod; //Call callbackMethod() when finidhsed

		//Initiate the move by passing in a series of PT_Locs and duration for
		//the Beier curve.
		PT_StartMove(locs, lifeTime);
	}

	void CallbackMethod(){
		Destroy(gameObject); //When the move is done, Destroy(gameObject)
	}
}
