using Godot;


// Basic editor tool that updates the outline and shape visuals of a polygon's collider shape when it is assigned.
[Tool]
public partial class PolygonSpriteCollider : CollisionPolygon2D
{
    Polygon2D sprite;
    Line2D outline;

    public override void _Ready()
    {
        sprite = GetNode<Polygon2D>("Sprite");
        outline = GetNode<Line2D>("Outline");
    }

    public void AssignPolygon(Vector2[] polygon)
    {
        Polygon = polygon;
        sprite.Polygon = polygon;
        outline.Points = polygon;
    }
}
