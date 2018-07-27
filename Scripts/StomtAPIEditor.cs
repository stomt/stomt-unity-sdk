#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Stomt
{
	[CustomEditor(typeof(StomtAPI))]
	public class StomtAPIEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			StomtAPI myScript = (StomtAPI)target;
			if (GUILayout.Button("Delete local Session"))
			{
				myScript.cleanConfig();
			}
		}
	}
}

#endif
