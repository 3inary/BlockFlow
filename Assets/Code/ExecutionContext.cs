using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the execution context for a graph, storing runtime data such as objects, variables, and comparison results.
/// </summary>
public class ExecutionContext
{
    public Dictionary<string, GameObject> Objects { get; } = new();
    public Dictionary<string, float> FloatVariables { get; } = new();
    public bool LastComparisonResult { get; set; }
}