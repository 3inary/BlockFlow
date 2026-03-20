# Block Flow

Block Flow is a lightweight data-driven block execution system built in Unity 6000.3.2f1.

It supports:
- spawning primitive objects
- moving, rotating and scaling them
- simple logic through variables, comparisons and branching
- JSON export and import of complete graph configurations

## Included Blocks

- Spawn
- Move
- Rotate
- Scale
- SetValue
- AddValue
- Compare
- Branch

Supported primitives:
- Cube
- Sphere
- Cylinder

## Architecture

- `GraphData` stores the serializable graph definition
- `GraphExecutor` executes the graph at runtime
- `ExecutionContext` stores runtime state
- `GraphValidator` validates the graph before execution
- `GraphController` provides Unity-facing orchestration
- `GraphControllerEditor` adds Inspector buttons for common actions

## Usage

1. Add `GraphController` to a GameObject
2. Use the Inspector buttons to create, validate, execute, export or import a graph
3. Adjust graph data in the Inspector as needed

## Notes

The project uses an Inspector-first workflow and keeps the implementation focused on data, execution, validation and serialization.

## Unity Version

Unity 6000.3.2f1

## Author

René Marcel Welland
