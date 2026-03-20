using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Controls the creation, validation, execution, and serialization of a graph-based workflow.
/// </summary>
public class GraphController : MonoBehaviour
{
    public GraphData graphData = new();
    
    [Header("Optional file name")]
    public string fileName = "GraphImportExport.json";
    
    public void CreateExampleGraph()
    {
        graphData = new GraphData
        {
            startBlockId = "1"
        };

        graphData.blocks.Add(new BlockData
        {
            id = "1",
            type = BlockType.Spawn,
            shapeType = ShapeType.Cube,
            objectId = "cube1",
            vectorValue = new Vector3Data { x = 0, y = 0, z = 0 },
            nextBlockId = "2"
        });

        graphData.blocks.Add(new BlockData
        {
            id = "2",
            type = BlockType.Move,
            objectId = "cube1",
            vectorValue = new Vector3Data { x = 2, y = 0, z = 0 },
            nextBlockId = "3"
        });

        graphData.blocks.Add(new BlockData
        {
            id = "3",
            type = BlockType.Rotate,
            objectId = "cube1",
            vectorValue = new Vector3Data { x = 0, y = 45, z = 0 },
            nextBlockId = "4"
        });

        graphData.blocks.Add(new BlockData
        {
            id = "4",
            type = BlockType.Scale,
            objectId = "cube1",
            vectorValue = new Vector3Data { x = 2, y = 2, z = 2 },
            nextBlockId = "5"
        });

        graphData.blocks.Add(new BlockData
        {
            id = "5",
            type = BlockType.SetValue,
            variableName = "counter",
            floatValue = 5,
            nextBlockId = "6"
        });

        graphData.blocks.Add(new BlockData
        {
            id = "6",
            type = BlockType.Compare,
            variableName = "counter",
            comparisonOperator = ComparisonOperator.Greater,
            floatValue = 3,
            nextBlockId = "7"
        });

        graphData.blocks.Add(new BlockData
        {
            id = "7",
            type = BlockType.Branch,
            trueBlockId = "8",
            falseBlockId = "9"
        });

        graphData.blocks.Add(new BlockData
        {
            id = "8",
            type = BlockType.Spawn,
            shapeType = ShapeType.Sphere,
            objectId = "sphere1",
            vectorValue = new Vector3Data { x = 0, y = 0, z = 3 }
        });

        graphData.blocks.Add(new BlockData
        {
            id = "9",
            type = BlockType.Spawn,
            shapeType = ShapeType.Cylinder,
            objectId = "cylinder1",
            vectorValue = new Vector3Data { x = -2, y = 0, z = 0 }
        });

        Debug.Log("Example graph created.");
    }

    public void ValidateGraph()
    {
        List<string> errors;
        bool valid = GraphValidator.Validate(graphData, out errors);

        if (valid) {
            Debug.Log("Graph validation successful.");
            return;
        }

        StringBuilder sb = new();
        sb.AppendLine("Graph validation failed:");

        foreach (string error in errors)
            sb.AppendLine($"- {error}");

        Debug.LogError(sb.ToString());
    }

    public void ExecuteGraph()
    {
        GraphExecutor executor = new GraphExecutor();
        executor.Execute(graphData);
    }
    
    public void ExportJson()
    {
        string path = GetFilePath();
        string json = JsonUtility.ToJson(graphData, true);
        File.WriteAllText(path, json);
        Debug.Log($"Graph exported to: {path}");
    }
    
    public void ImportJson()
    {
        string path = GetFilePath();

        if (!File.Exists(path)) {
            Debug.LogWarning($"JSON file not found: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        graphData = JsonUtility.FromJson<GraphData>(json);

        Debug.Log($"Graph imported from: {path}");
    }

    private string GetFilePath()
    {
        return Path.Combine(Application.dataPath, fileName);
    }
}