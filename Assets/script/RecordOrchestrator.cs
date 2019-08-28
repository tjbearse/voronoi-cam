using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Moments;

public class RecordOrchestrator : MonoBehaviour {
	private Recorder recorder;
    void Start() {
		recorder = GetComponent<Recorder>();
		recorder.OnFileSaved += (i, name) => {
			Debug.Log(string.Format("file ({0}) saved", name));
		};
    }

	public void Record() {
		recorder.Record();
	}

	public void Save() {
		recorder.Save();
	}
}

[CustomEditor(typeof(RecordOrchestrator))]
public class RecordOrchestratorEditor : Editor {
    public override void OnInspectorGUI() {
		DrawDefaultInspector();
		RecordOrchestrator rec = target as RecordOrchestrator;

		if(GUILayout.Button("Record")) {
			rec.Record();
		}
		if(GUILayout.Button("Save")) {
			rec.Save();
		}
    }
}
