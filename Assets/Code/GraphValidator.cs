using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides validation functionality for graph structures.
/// </summary>
public static class GraphValidator
{
    public static bool Validate(GraphData graph, out List<string> errors)
    {
        errors = new List<string>();

        if (graph == null) {
            errors.Add("GraphData is null.");
            return false;
        }

        if (graph.blocks == null || graph.blocks.Count == 0) {
            errors.Add("Graph contains no blocks.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(graph.startBlockId)) {
            errors.Add("StartBlockId is missing.");
        }

        Dictionary<string, BlockData> blockMap = new Dictionary<string, BlockData>();

        foreach (BlockData block in graph.blocks) {
            if (block == null) {
                errors.Add("Graph contains a null block.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(block.id)) {
                errors.Add("A block has no id.");
                continue;
            }

            if (blockMap.ContainsKey(block.id)) {
                errors.Add($"Duplicate block id found: '{block.id}'.");
            }
            else {
                blockMap.Add(block.id, block);
            }
        }

        if (!string.IsNullOrWhiteSpace(graph.startBlockId) && !blockMap.ContainsKey(graph.startBlockId)) {
            errors.Add($"Start block '{graph.startBlockId}' does not exist.");
        }

        foreach (BlockData block in graph.blocks) {
            if (block != null) {
                ValidateBlock(block, blockMap, errors);
            }
        }

        return errors.Count == 0;
    }

    private static void ValidateBlock(BlockData block, Dictionary<string, BlockData> blockMap, List<string> errors)
    {
        switch (block.type) {
            case BlockType.Spawn:
                if (string.IsNullOrWhiteSpace(block.objectId))
                    errors.Add($"Spawn block '{block.id}' requires an objectId.");
                break;

            case BlockType.Move:
            case BlockType.Rotate:
            case BlockType.Scale:
                if (string.IsNullOrWhiteSpace(block.objectId))
                    errors.Add($"{block.type} block '{block.id}' requires an objectId.");
                if (block.vectorValue == null)
                    errors.Add($"{block.type} block '{block.id}' requires a vectorValue.");
                break;

            case BlockType.SetValue:
            case BlockType.AddValue:
                if (string.IsNullOrWhiteSpace(block.variableName))
                    errors.Add($"{block.type} block '{block.id}' requires a variableName.");
                break;

            case BlockType.Compare:
                if (string.IsNullOrWhiteSpace(block.variableName))
                    errors.Add($"Compare block '{block.id}' requires a variableName.");
                break;

            case BlockType.Branch:
                ValidateBlockReference(block.trueBlockId, blockMap, $"Branch block '{block.id}' trueBlockId", errors);
                ValidateBlockReference(block.falseBlockId, blockMap, $"Branch block '{block.id}' falseBlockId", errors);
                return;
        }

        if (!string.IsNullOrWhiteSpace(block.nextBlockId)) {
            ValidateBlockReference(block.nextBlockId, blockMap, $"Block '{block.id}' nextBlockId", errors);
        }
    }

    private static void ValidateBlockReference(string blockId, Dictionary<string, BlockData> blockMap, string label, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(blockId)) {
            errors.Add($"{label} is missing.");
            return;
        }

        if (!blockMap.ContainsKey(blockId)) {
            errors.Add($"{label} references missing block '{blockId}'.");
        }
    }
}
