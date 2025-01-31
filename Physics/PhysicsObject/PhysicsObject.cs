using Godot;
using Geometry;

// Fundamental physics object that does not have a default shape
public partial class PhysicsObject : RigidBody2D
{
    // If a shape is instanced with less area than this, it is destroyed.
    public const float MINIMUM_AREA = 100;

    protected PolygonSpriteCollider collider;

    // PhysicsObject source loaded in advance so that new physics objeccts are created without loading
    PackedScene physicsObjectScene = GD.Load<PackedScene>("Physics/PhysicsObject/PhysicsObject.tscn");


    public override void _Ready()
    {
        collider = GetNode<PolygonSpriteCollider>("Collider");
    }


    public void Split(Vector2 from, Vector2 direction)
    {
        // Iterates through any number of polygons with the possibility of convex polygons in mind, even though this is not currently possible with current tools
        foreach (Vector2[] polygon in collider.Polygon.Split((from, direction), 5))
        {
            Vector2 relativeCenterOfMass = polygon.GetRelativeCenterOfMass();

            // Move origin of polygon to the center of mass, since the engine treats the origin as the center of mass
            polygon.MovePoints(-relativeCenterOfMass);

            // Create and add sub-polygons
            PhysicsObject obj = physicsObjectScene.Instantiate<PhysicsObject>();
            GetParent().AddChild(obj);

            // Set initial properties and match current physics state
            obj.Rotation = Rotation;
            obj.GlobalPosition = GlobalPosition + relativeCenterOfMass.Rotated(Rotation);
            obj.SetPolygon(polygon);
            obj.LinearVelocity = LinearVelocity;
            obj.AngularVelocity = AngularVelocity;
        }

        // Remove origin object
        QueueFree();
    }

    // Shorthand way to update all polygon-related data at once
    public void SetPolygon(Vector2[] polygon)
    {
        collider.AssignPolygon(polygon);
        Mass = polygon.CalculateArea();

        // If this polygon is too small too exist, destroy self
        if (Mass < MINIMUM_AREA)
        {
            QueueFree();
        }
    }

    // Shorthand for access the collider's polygon's bounding box
    public Vector2 GetBoundingBox()
    {
        return collider.Polygon.GetBoundingBox();
    }

    // If an object clips out of the world bounds and is far from the viewport then it gets destroyed
    void _onScreenExit()
    {
        QueueFree();
    }

}