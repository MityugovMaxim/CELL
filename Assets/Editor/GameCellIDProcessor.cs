using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
public class GameCellIDProcessor : IPreprocessBuildWithReport
{
	static GameCellIDProcessor()
	{
		EditorApplication.playModeStateChanged += PlayModeChanged;
	}

	static void PlayModeChanged(PlayModeStateChange _PlayMode)
	{
		if (_PlayMode == PlayModeStateChange.ExitingEditMode)
			ProcessID();
	}

	public int callbackOrder { get; }

	public void OnPreprocessBuild(BuildReport _Report)
	{
		ProcessID();
	}

	static void ProcessID()
	{
		int id = 0;
		
		IEnumerable<GameCell> cells = AssetDatabase.FindAssets("t:Prefab")
			.Select(AssetDatabase.GUIDToAssetPath)
			.Select(AssetDatabase.LoadAssetAtPath<GameCell>)
			.Where(_Cell => _Cell != null);
		
		foreach (GameCell cell in cells)
		{
			EditorUtility.DisplayProgressBar("Resolving cell", cell.name, 0.1f);
			
			SerializedObject   cellObject = new SerializedObject(cell);
			SerializedProperty idProperty = cellObject.FindProperty("m_ID");
			idProperty.intValue = id++;
			cellObject.ApplyModifiedProperties();
		}
		EditorUtility.ClearProgressBar();
	}
}