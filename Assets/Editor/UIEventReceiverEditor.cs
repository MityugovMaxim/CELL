using UnityEditor;

[CustomEditor(typeof(UIEventReceiver), true)]
public class UIEventReceiverEditor : Editor
{
	public override void OnInspectorGUI()
	{
		SerializedProperty property = serializedObject.GetIterator();
		for (bool enterChildren = true; property.NextVisible(enterChildren); enterChildren = false)
		{
			if (property.propertyPath == "m_Material")
				continue;
			
			if (property.propertyPath == "m_Color")
				continue;
			
			using (new EditorGUI.DisabledScope(property.propertyPath == "m_Script"))
				EditorGUILayout.PropertyField(property, true);
		}
		serializedObject.ApplyModifiedProperties();
	}
}