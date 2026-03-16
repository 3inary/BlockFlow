using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Executes a graph of blocks defined in GraphData, following the flow from startBlockId to subsequent blocks.
/// </summary>
public class GraphExecutor
{
    private Dictionary<string, BlockData> blockMap;
    private ExecutionContext context;

    public bool Execute(GraphData graph)
    {
        if (!GraphValidator.Validate(graph, out List<string> errors)) {
            foreach (string error in errors) {
                Debug.LogError(error);
            }

            return false;
        }

        context = new ExecutionContext();
        blockMap = graph.blocks.ToDictionary(b => b.id, b => b);

        string currentBlockId = graph.startBlockId;
        int safetyCounter = 0;
        const int maxSteps = 1000;

        while (!string.IsNullOrEmpty(currentBlockId)) {
            safetyCounter++;
            if (safetyCounter > maxSteps) {
                Debug.LogError("Execution aborted: too many steps. Possible infinite loop.");
                return false;
            }

            if (!blockMap.TryGetValue(currentBlockId, out BlockData block)) {
                Debug.LogError($"Block not found: {currentBlockId}");
                return false;
            }

            currentBlockId = ExecuteBlock(block);
        }

        Debug.Log("Graph execution finished successfully.");
        return true;
    }

    private string ExecuteBlock(BlockData block)
    {
        switch (block.type) {
            case BlockType.Spawn:
                return ExecuteSpawn(block);

            case BlockType.Move:
                return ExecuteMove(block);

            case BlockType.Rotate:
                return ExecuteRotate(block);

            case BlockType.Scale:
                return ExecuteScale(block);

            case BlockType.SetValue:
                return ExecuteSetValue(block);

            case BlockType.AddValue:
                return ExecuteAddValue(block);

            case BlockType.Compare:
                return ExecuteCompare(block);

            case BlockType.Branch:
                return ExecuteBranch(block);

            default:
                Debug.LogError($"Unknown block type: {block.type}");
                return null;
        }
    }

    private string ExecuteSpawn(BlockData block)
    {
        PrimitiveType primitiveType = PrimitiveType.Cube;

        switch (block.shapeType) {
            case ShapeType.Cube:
                primitiveType = PrimitiveType.Cube;
                break;
            case ShapeType.Sphere:
                primitiveType = PrimitiveType.Sphere;
                break;
            case ShapeType.Cylinder:
                primitiveType = PrimitiveType.Cylinder;
                break;
            default:
                Debug.LogWarning($"Unknown shapeType '{block.shapeType}', defaulting to Cube.");
                break;
        }

        GameObject go = GameObject.CreatePrimitive(primitiveType);
        go.name = string.IsNullOrEmpty(block.objectId) ? $"Spawned_{block.id}" : block.objectId;

        if (block.vectorValue != null)
            go.transform.position = block.vectorValue.ToVector3();

        string objectKey = string.IsNullOrEmpty(block.objectId) ? go.name : block.objectId;
        context.objects[objectKey] = go;

        Debug.Log($"Spawned {block.shapeType} as '{objectKey}'.");
        return block.nextBlockId;
    }

    private string ExecuteMove(BlockData block)
    {
        if (TryGetObject(block.objectId, out GameObject go)) {
            go.transform.position = block.vectorValue != null ? block.vectorValue.ToVector3() : Vector3.zero;
            Debug.Log($"Moved '{block.objectId}' to {go.transform.position}.");
        }

        return block.nextBlockId;
    }

    private string ExecuteRotate(BlockData block)
    {
        if (TryGetObject(block.objectId, out GameObject go)) {
            Vector3 rotation = block.vectorValue != null ? block.vectorValue.ToVector3() : Vector3.zero;
            go.transform.rotation = Quaternion.Euler(rotation);
            Debug.Log($"Rotated '{block.objectId}' to {rotation}.");
        }

        return block.nextBlockId;
    }

    private string ExecuteScale(BlockData block)
    {
        if (TryGetObject(block.objectId, out GameObject go)) {
            go.transform.localScale = block.vectorValue != null ? block.vectorValue.ToVector3() : Vector3.one;
            Debug.Log($"Scaled '{block.objectId}' to {go.transform.localScale}.");
        }

        return block.nextBlockId;
    }

    private string ExecuteSetValue(BlockData block)
    {
        if (string.IsNullOrEmpty(block.variableName)) {
            Debug.LogWarning("SetValue block has no variableName.");
            return block.nextBlockId;
        }

        context.floatVariables[block.variableName] = block.floatValue;
        Debug.Log($"Set variable '{block.variableName}' = {block.floatValue}.");

        return block.nextBlockId;
    }

    private string ExecuteAddValue(BlockData block)
    {
        if (string.IsNullOrEmpty(block.variableName)) {
            Debug.LogWarning("AddValue block has no variableName.");
            return block.nextBlockId;
        }

        if (!context.floatVariables.ContainsKey(block.variableName))
            context.floatVariables[block.variableName] = 0f;

        context.floatVariables[block.variableName] += block.floatValue;

        Debug.Log($"Added {block.floatValue} to '{block.variableName}'. New value: {context.floatVariables[block.variableName]}");
        return block.nextBlockId;
    }

    private string ExecuteCompare(BlockData block)
    {
        if (string.IsNullOrEmpty(block.variableName)) {
            Debug.LogWarning("Compare block has no variableName.");
            context.lastComparisonResult = false;
            return block.nextBlockId;
        }

        if (!context.floatVariables.TryGetValue(block.variableName, out float currentValue)) {
            Debug.LogWarning($"Variable '{block.variableName}' not found. Using 0.");
            currentValue = 0f;
        }

        context.lastComparisonResult = block.comparisonOperator switch {
            ComparisonOperator.Greater => currentValue > block.floatValue,
            ComparisonOperator.Less => currentValue < block.floatValue,
            ComparisonOperator.GreaterOrEqual => currentValue >= block.floatValue,
            ComparisonOperator.LessOrEqual => currentValue <= block.floatValue,
            ComparisonOperator.Equal => Mathf.Approximately(currentValue, block.floatValue),
            ComparisonOperator.NotEqual => !Mathf.Approximately(currentValue, block.floatValue),
            _ => false
        };

        Debug.Log($"Compare: {block.variableName} ({currentValue}) {block.comparisonOperator} {block.floatValue} => {context.lastComparisonResult}");
        return block.nextBlockId;
    }

    private string ExecuteBranch(BlockData block)
    {
        string next = context.lastComparisonResult ? block.trueBlockId : block.falseBlockId;
        Debug.Log($"Branch result: {context.lastComparisonResult} -> Next block: {next}");
        return next;
    }

    private bool TryGetObject(string objectId, out GameObject go)
    {
        if (string.IsNullOrEmpty(objectId)) {
            Debug.LogWarning("Block has no objectId.");
            go = null;
            return false;
        }

        if (!context.objects.TryGetValue(objectId, out go)) {
            Debug.LogWarning($"Object not found: {objectId}");
            return false;
        }

        return true;
    }
}