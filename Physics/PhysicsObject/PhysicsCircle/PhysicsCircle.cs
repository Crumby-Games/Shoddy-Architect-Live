using Godot;
using Geometry;

[Tool]
// Basic PhysicsObject with the ability to be treated as a circle collider in editor yet be instanced as a polygon.
public partial class PhysicsCircle : PhysicsObject
{
    float radius = 50f;

    [Export]
    public float Radius
    {
        get { return radius; }

        set
        {
            if (value != radius)
            {
                radius = value;

                // Checking if node is ready prevents errors while in editor.
                if (IsNodeReady()) SetPolygon(Polygon.GeneratePointsForCircle(radius));
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();
        SetPolygon(Polygon.GeneratePointsForCircle(radius));
    }

}