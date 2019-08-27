using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VoronoiCamera : MonoBehaviour {
	public Transform Player1, Player2;
	public Camera Cam1, Cam2;
	public RawImage Cam2Texture;
	public Image Cam2Mask;
	[Range(.1f, 1)]
	public float margin;
	public float smoothDistance = 1f;

	private float extent;
	private Vector2 camExtent;

	private Material camMask;

    void Start() {
		float z = -Cam1.transform.position.z; // inflexible
		camExtent =  Cam1.ViewportToWorldPoint(new Vector3(.5f, .5f, z)) - Cam1.ViewportToWorldPoint(new Vector3(0,0,z));
		extent = Mathf.Min(camExtent.x, camExtent.y);
		// var res = Screen.currentResolution;
		// (Cam2Texture.texture as Texture2D).Resize(res.width, res.height);
		// camMask = Instantiate(Cam2Mask.material);
		// Cam2Mask.material = camMask;

    }

    void Update() {
		Vector3 diff = (Player2.position - Player1.position);
		// base on diff size but translated to camera pixels
		float mag = diff.magnitude;
		if (mag > 2 * extent * margin) {
			// split up
			// extnet/2 // center to edge
			// extent*margin/2 // center to margin	
			float lerp = Mathf.InverseLerp(2*extent*margin, 2*extent*margin + smoothDistance, mag);
			Vector3 offset = Vector2.Lerp(diff/2, Vector2.Scale(diff.normalized, camExtent / 2), lerp);
			Vector3 pos1 = Player1.transform.position + offset;
			pos1.z = Cam1.transform.position.z;
			Cam1.transform.position = pos1;

			Vector3 pos2 = Player2.transform.position - offset;
			pos2.z = Cam2.transform.position.z;
			Cam2.transform.position = pos2;

			Cam2Mask.materialForRendering.SetVector("_Split", Vector2.Perpendicular(diff));
			// camMask.SetVector("_Split", diff);
			// Debug.Log(camMask.GetVector("_Split"));
			
			Cam2Texture.enabled = true;
		} else {
			// align to average position
			Vector3 avg = Player1.position + (diff)/2f;
			Vector3 pos = Cam1.transform.position;
			pos.x = avg.x;
			pos.y = avg.y;
			Cam1.transform.position = pos;
			Cam2.transform.position = pos;
			Cam2Texture.enabled = false;
		}
	}
}
