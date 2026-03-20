using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Executes a graph of blocks defined in GraphData.
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
        blockMap = graph.blocks.ToDictionary(block => block.id, block => block);

        string currentBlockId = graph.startBlockId;

        while (!string.IsNullOrEmpty(currentBlockId)) {

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
        PrimitiveType primitiveType = block.shapeType switch
        {
            ShapeType.Cube => PrimitiveType.Cube,
            ShapeType.Sphere => PrimitiveType.Sphere,
            ShapeType.Cylinder => PrimitiveType.Cylinder,
            _ => PrimitiveType.Cube
        };

        string objectId = string.IsNullOrWhiteSpace(block.objectId)
            ? $"Spawned_{block.id}"
            : block.objectId;

        GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
        gameObject.name = objectId;

        if (block.vectorValue != null) {
            gameObject.transform.position = block.vectorValue.ToVector3();
        }

        context.Objects[objectId] = gameObject;

        Debug.Log($"Spawned {block.shapeType} as '{objectId}'.");
        return block.nextBlockId;
    }

    private string ExecuteMove(BlockData block)
    {
        if (TryGetObject(block.objectId, out GameObject gameObject)) {
            gameObject.transform.position = block.vectorValue != null
                ? block.vectorValue.ToVector3()
                : Vector3.zero;

            Debug.Log($"Moved '{block.objectId}' to {gameObject.transform.position}.");
        }

        return block.nextBlockId;
    }

    private string ExecuteRotate(BlockData block)
    {
        if (TryGetObject(block.objectId, out GameObject gameObject)) {
            Vector3 rotation = block.vectorValue != null
                ? block.vectorValue.ToVector3()
                : Vector3.zero;

            gameObject.transform.rotation = Quaternion.Euler(rotation);
            Debug.Log($"Rotated '{block.objectId}' to {rotation}.");
        }

        return block.nextBlockId;
    }

    private string ExecuteScale(BlockData block)
    {
        if (TryGetObject(block.objectId, out GameObject gameObject)) {
            gameObject.transform.localScale = block.vectorValue != null
                ? block.vectorValue.ToVector3()
                : Vector3.one;

            Debug.Log($"Scaled '{block.objectId}' to {gameObject.transform.localScale}.");
        }

        return block.nextBlockId;
    }

    private string ExecuteSetValue(BlockData block)
    {
        if (string.IsNullOrWhiteSpace(block.variableName)) {
            Debug.LogWarning("SetValue block has no variableName.");
            return block.nextBlockId;
        }

        context.FloatVariables[block.variableName] = block.floatValue;
        Debug.Log($"Set variable '{block.variableName}' = {block.floatValue}.");

        return block.nextBlockId;
    }

    private string ExecuteAddValue(BlockData block)
    {
        if (string.IsNullOrWhiteSpace(block.variableName)) {
            Debug.LogWarning("AddValue block has no variableName.");
            return block.nextBlockId;
        }

        if (!context.FloatVariables.ContainsKey(block.variableName)) {
            context.FloatVariables[block.variableName] = 0f;
        }

        context.FloatVariables[block.variableName] += block.floatValue;

        Debug.Log(
            $"Added {block.floatValue} to '{block.variableName}'. New value: {context.FloatVariables[block.variableName]}"
        );

        return block.nextBlockId;
    }

    private string ExecuteCompare(BlockData block)
    {
        if (string.IsNullOrWhiteSpace(block.variableName)) {
            Debug.LogWarning("Compare block has no variableName.");
            context.LastComparisonResult = false;
            return block.nextBlockId;
        }

        if (!context.FloatVariables.TryGetValue(block.variableName, out float currentValue)) {
            Debug.LogWarning($"Variable '{block.variableName}' not found. Using 0.");
            currentValue = 0f;
        }

        context.LastComparisonResult = block.comparisonOperator switch
        {
            ComparisonOperator.Greater => currentValue > block.floatValue,
            ComparisonOperator.Less => currentValue < block.floatValue,
            ComparisonOperator.GreaterOrEqual => currentValue >= block.floatValue,
            ComparisonOperator.LessOrEqual => currentValue <= block.floatValue,
            ComparisonOperator.Equal => Mathf.Approximately(currentValue, block.floatValue),
            ComparisonOperator.NotEqual => !Mathf.Approximately(currentValue, block.floatValue),
            _ => false
        };

        Debug.Log(
            $"Compare: {block.variableName} ({currentValue}) {block.comparisonOperator} {block.floatValue} => {context.LastComparisonResult}"
        );

        return block.nextBlockId;
    }

    private string ExecuteBranch(BlockData block)
    {
        string nextBlockId = context.LastComparisonResult
            ? block.trueBlockId
            : block.falseBlockId;

        Debug.Log($"Branch result: {context.LastComparisonResult} -> Next block: {nextBlockId}");
        return nextBlockId;
    }

    private bool TryGetObject(string objectId, out GameObject gameObject)
    {
        if (string.IsNullOrWhiteSpace(objectId)) {
            Debug.LogWarning("Block has no objectId.");
            gameObject = null;
            return false;
        }

        if (!context.Objects.TryGetValue(objectId, out gameObject)) {
            Debug.LogWarning($"Object not found: {objectId}");
            return false;
        }

        return true;
    }
}