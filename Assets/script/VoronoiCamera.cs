using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VoronoiCamera : MonoBehaviour {
	public Transform Player1, Player2;
	public Camera Cam1, Cam2;
	public RawImage CamTexture;
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

		var res = Screen.currentResolution;
		var rt = new RenderTexture(res.width, res.height, res.refreshRate);
		Cam1.targetTexture = rt;
		CamTexture.material.SetTexture("_Cam1", rt);
		rt = new RenderTexture(res.width, res.height, res.refreshRate);
		CamTexture.material.SetTexture("_Cam2", rt);
		Cam2.targetTexture = rt;

    }

    void Update() {
		Vector3 diff = (Player2.position - Player1.position);
		float mag = diff.magnitude;
		CamTexture.material.SetVector("_Split", Vector2.Perpendicular(diff.normalized));
		float lerp = Mathf.InverseLerp(2*extent*margin, 2*extent*margin + smoothDistance, mag);
		CamTexture.material.SetFloat("_SplitDist", lerp);
		Vector3 offset = Vector2.Lerp(diff/2, Vector2.Scale(diff.normalized, camExtent / 2), lerp);
		Vector3 pos1 = Player1.transform.position + offset;
		pos1.z = Cam1.transform.position.z;
		Cam1.transform.position = pos1;

		Vector3 pos2 = Player2.transform.position - offset;
		pos2.z = Cam2.transform.position.z;
		Cam2.transform.position = pos2;
	}
}
