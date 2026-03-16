using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for the GraphController component, providing a user interface in Unity's inspector
/// to manage graph operations such as creation, validation, execution, and JSON serialization.
/// </summary>
[CustomEditor(typeof(GraphController))]
public class GraphControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Graph Actions", EditorStyles.boldLabel);

        GraphController controller = (GraphController)target;

        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button("Create Example Graph", GUILayout.Height(30))) {
            Undo.RecordObject(controller, "Create Example Graph");
            controller.CreateExampleGraph();
            EditorUtility.SetDirty(controller);
        }

        if (GUILayout.Button("Validate Graph", GUILayout.Height(30))) {
            controller.ValidateGraph();
        }

        if (GUILayout.Button("Execute Graph", GUILayout.Height(30))) {
            controller.ExecuteGraph();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Export JSON", GUILayout.Height(30))) {
            controller.ExportJson();
        }

        if (GUILayout.Button("Import JSON", GUILayout.Height(30))) {
            Undo.RecordObject(controller, "Import Graph JSON");
            controller.ImportJson();
            EditorUtility.SetDirty(controller);
        }

        EditorGUILayout.EndVertical();
    }
}