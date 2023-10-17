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
	public int superElipseN = 2;

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

		// formerly
		// baseOffset = Vector2.Scale(diff.normalized, camExtent/2);
		// TODO this is the right direction but needs to be rounded off and more continuous
		// what we really want is a super elipse https://en.wikipedia.org/wiki/Superellipse
		baseOffset = projectToSuperElipse(diff.normalized, camExtent, superElipseN) / 2;
		if (rotateOffset) {
			splitOffsetWorld = Vector2.Scale(camExtent, rot * splitOffsetS);
		} else {
			splitOffsetWorld = Vector2.Scale(camExtent, splitOffsetS);
		}
		alignedOffset = baseOffset.normalized * splitOffsetWorld.magnitude * Mathf.Cos(Mathf.Deg2Rad * Vector3.Angle(baseOffset, splitOffsetWorld));

		Vector3 unsplitPosition = getUnsplitPosition(Player1.transform.position, Player2.transform.position, Cam1.transform.position, margin*camExtent);


		Vector3 splitCamPos = Player1.transform.position + baseOffset - alignedOffset;
		Vector3 pos = Vector3.Lerp(unsplitPosition, splitCamPos, splitness);
		pos.z = Cam1.transform.position.z;
		Cam1.transform.position = pos;

		splitCamPos = Player2.transform.position -baseOffset - alignedOffset;
		pos = Vector3.Lerp(unsplitPosition, splitCamPos, splitness);
		pos.z = Cam2.transform.position.z;
		Cam2.transform.position = pos;
	}

	Vector3 getUnsplitPosition(Vector3 pos1, Vector3 pos2, Vector3 curr, Vector3 extent) {
		// return Player1.transform.position  + diff/2;
		Vector3 center = (pos2 - pos1)/ 2 + pos1;
		float xLb = Mathf.Min(Mathf.Max(pos1.x, pos2.x) - extent.x, center.x + extent.x);
		float xUb = Mathf.Max(Mathf.Min(pos1.x, pos2.x) + extent.x, center.x - extent.x);
		curr.x = Mathf.Clamp(curr.x, xLb, xUb);
		float yLb = Mathf.Min(Mathf.Max(pos1.y, pos2.y) - extent.y, center.y + extent.y);
		float yUb = Mathf.Max(Mathf.Min(pos1.y, pos2.y) + extent.y, center.y - extent.y);
		curr.y = Mathf.Clamp(curr.y, yLb, yUb);
		return curr;
	}

	Vector2 projectToSuperElipse(Vector2 norm, Vector2 extents, int power) {
		// y/x = (b/a)(tan t) ^ (2/n)
		float t;
		if (Mathf.Approximately(norm.x, 0)) {
			t = Mathf.PI / 2;
		} else if (Mathf.Approximately(norm.y, 0)) {
			t = 0;
		} else {
			t = Mathf.Atan(Mathf.Pow((extents.x / extents.y) * Mathf.Abs(norm.y / norm.x), power/2f));
			// = Mathf.Atan2(norm.y, norm.x);
		}
		float cos_t = Mathf.Cos(t);
		float sin_t = Mathf.Sin(t);
		return new Vector2(
			Mathf.Pow(cos_t, 2.0f/power) * extents.x * Mathf.Sign(norm.x),
			Mathf.Pow(sin_t, 2.0f/power) * extents.y * Mathf.Sign(norm.y)
		);
	}

	Vector2 projectToExtents(Vector2 norm, Vector2 extents) {
		if (Mathf.Approximately(norm.x, 0)) {
			return Vector2.up * extents.y * Mathf.Sign(norm.y);
		} else {
			if (Mathf.Abs(norm.y) < Mathf.Abs(norm.x) * extents.y / extents.x) {
				return new Vector2(
					extents.x * Mathf.Sign(norm.x),
					norm.y * extents.x / Mathf.Abs(norm.x)
				);
			} else {
				return new Vector2(
					norm.x * extents.y / Mathf.Abs(norm.y),
					extents.y * Mathf.Sign(norm.y)
				);
			}
		}
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
