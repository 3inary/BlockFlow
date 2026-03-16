using System;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType
{
    Spawn,
    Move,
    Rotate,
    Scale,
    SetValue,
    AddValue,
    Compare,
    Branch
}

public enum ShapeType
{
    Cube,
    Sphere,
    Cylinder
}

public enum ComparisonOperator
{
    Greater,
    Less,
    GreaterOrEqual,
    LessOrEqual,
    Equal,
    NotEqual
}

[Serializable]
public class GraphData
{
    public string startBlockId;
    public List<BlockData> blocks = new();
}

// TODO: Refactor BlockData into dedicated block-specific data classes and use Newtonsoft.Json for polymorphic serialization.
[Serializable]
public class BlockData
{
    public string id;
    public BlockType type;
    public ShapeType shapeType;
    public string nextBlockId;
    public string trueBlockId;
    public string falseBlockId;
    public string objectId;
    public string variableName;
    public float floatValue;
    public Vector3Data vectorValue;
    public ComparisonOperator comparisonOperator;
}

[Serializable]
public class Vector3Data
{
    public float x;
    public float y;
    public float z;

    public Vector3 ToVector3() => new Vector3(x, y, z);
}