using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the execution context for a graph, storing runtime data such as objects, variables, and comparison results.
/// </summary>
public class ExecutionContext
{
    public readonly Dictionary<string, GameObject> objects = new();
    public readonly Dictionary<string, float> floatVariables = new();
    public bool lastComparisonResult = false;
}