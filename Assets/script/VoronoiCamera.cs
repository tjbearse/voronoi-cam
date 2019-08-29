using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

	public Vector2 splitOffsetS = Vector2.zero; // screen space offset
	public bool rotateOffset = true;

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

	// stored for drawing
	private Vector3 splitOffsetWorld;
	private Vector3 baseOffset;
	private Vector3 alignedOffset;

    void Update() {
		Vector3 diff = (Player2.position - Player1.position);
		float mag = diff.magnitude;
		float splitness = Mathf.InverseLerp(2*extent*margin, 2*extent*margin + smoothDistance, mag);

		Vector2 rotSplit = splitOffsetS;
		Quaternion rot = Quaternion.FromToRotation(Vector3.up, diff);
		if (rotateOffset) {
			rotSplit = rot * rotSplit;
		}
		rotSplit += new Vector2(.5f, .5f);
		CamTexture.material.SetVector("_Split", Vector2.Perpendicular(diff.normalized));
		CamTexture.material.SetFloat("_SplitDist", splitness);
		CamTexture.material.SetVector("_Origin", rotSplit);

		baseOffset = Vector2.Scale(diff.normalized, camExtent/2);
		if (rotateOffset) {
			splitOffsetWorld = Vector2.Scale(camExtent, rot * splitOffsetS);
		} else {
			splitOffsetWorld = Vector2.Scale(camExtent, splitOffsetS);
		}
		alignedOffset = baseOffset.normalized * splitOffsetWorld.magnitude * Mathf.Cos(Mathf.Deg2Rad * Vector3.Angle(baseOffset, splitOffsetWorld));

		Vector3 splitCamPos = baseOffset - alignedOffset;
		Vector3 offset = Vector3.Lerp(diff/2, splitCamPos, splitness);
		Vector3 pos = Player1.transform.position + offset;
		pos.z = Cam1.transform.position.z;
		Cam1.transform.position = pos;

		splitCamPos = -baseOffset - alignedOffset;
		offset = Vector2.Lerp(-diff/2, splitCamPos, splitness);
		pos = Player2.transform.position + offset;
		pos.z = Cam2.transform.position.z;
		Cam2.transform.position = pos;
	}

	void OnDrawGizmosSelected() {
		DrawVector(Player2.position - Player1.position, Player1.position, Color.red, "diff");

		// player 1
		DrawVector(baseOffset, Player1.position, Color.blue, "base");
		DrawVector(-splitOffsetWorld, baseOffset+Player1.position, Color.yellow, "split");
		DrawVector(-alignedOffset, baseOffset+Player1.position, Color.green, "");

		// player 2
		DrawVector(-baseOffset, Player2.position, Color.blue, "base");
		DrawVector(-splitOffsetWorld, -baseOffset+Player2.position, Color.yellow, "split");
		DrawVector(-alignedOffset, -baseOffset+Player2.position, Color.green, "");
	}
	void DrawVector(Vector2 vector, Vector2 origin, Color c, string name) {
		Gizmos.color = c;
		Gizmos.DrawLine(origin, vector+origin);
		if (name.Length > 0) {
			Handles.Label(origin + vector/2, name);
		}
	}
}
