using Godot;
using Geometry;

[Tool]
// Basic PhysicsObject with the ability to be treated as a rectangle collider in editor yet be instanced as a polygon.
public partial class PhysicsRectangle : PhysicsObject
{
    Vector2 dimensions = Vector2.One * 100;

    [Export]
    public Vector2 Dimensions
    {
        get { return dimensions; }

        set
        {
            if (value != dimensions)
            {
                dimensions = value;

                // Checking if node is ready prevents errors while in editor.
                if (IsNodeReady()) SetPolygon(Polygon.GeneratePointsForRectangle(dimensions));
            }
        }
    }


    public override void _Ready()
    {
        base._Ready();
        SetPolygon(Polygon.GeneratePointsForRectangle(dimensions));
    }


}