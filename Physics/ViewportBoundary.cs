using Godot;

// Placed on the edges of the screen to imitate the objects colliding with the edge of it.
public partial class ViewportBoundary : AnimatableBody2D
{
    // Which side of the screen this boundary should be attached to 
    [Export(PropertyHint.Enum, "N,E,S,W")] int edge = 0;

    // Position to interpolate current position towards
    Vector2 targetPosition;

    RectangleShape2D colliderShape;

    // Margin of shape beyond screen
    const float DEPTH = 500;

    // The weight of linear interpolation that is done to keep at the boundary of the viewport.
    const float SMOOTHING_WEIGHT = 0.1f;

    public override void _Ready()
    {
        colliderShape = (RectangleShape2D)GetNode<CollisionShape2D>("Collider").Shape;

        // React to changes in window size
        GetViewport().Connect("size_changed", new Callable(this, MethodName._onViewportSizeChanged));
        _onViewportSizeChanged();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Linearly interpolate position towards the edge of the screen, based on axis.
        // Because this is an AnimatableBody, changes in its position is interpretated as velocity to other objects, causing objects to be pushed around when screen size changes.
        // Position along the edges of the screen is not interpolated since this should be immediately updated to prevent physics objects escaping.
        // Movement on Y axis
        if (edge % 2 == 0)
        {
            GlobalPosition = new Vector2(targetPosition.X, GlobalPosition.Y + (targetPosition.Y - GlobalPosition.Y) * SMOOTHING_WEIGHT);
            // Movement on X axis
        }
        else
        {
            GlobalPosition = new Vector2(GlobalPosition.X + (targetPosition.X - GlobalPosition.X) * SMOOTHING_WEIGHT, targetPosition.Y);
        }

    }

    // Update size and target position to match the viewport size, based on which edge this boundary is on 
    void _onViewportSizeChanged()
    {
        Rect2 viewportRect = GetViewportRect();
        Vector2 center = viewportRect.Position + viewportRect.Size / 2;
        Vector2 size = Vector2.One * DEPTH;
        switch (edge)
        {
            case 0: // N
                targetPosition.Y = viewportRect.Position.Y - DEPTH / 2;
                targetPosition.X = center.X;
                size.X = viewportRect.Size.X + DEPTH;

                break;
            case 1: // E
                targetPosition.X = viewportRect.End.X + DEPTH / 2;
                targetPosition.Y = center.Y;
                size.Y = viewportRect.Size.Y + DEPTH;
                break;
            case 2: // S
                targetPosition.Y = viewportRect.End.Y + DEPTH / 2;
                targetPosition.X = center.X;
                size.X = viewportRect.Size.X + DEPTH;
                break;
            case 3: // W
                targetPosition.X = viewportRect.Position.X - DEPTH / 2;
                targetPosition.Y = center.Y;
                size.Y = viewportRect.Size.Y + DEPTH;
                break;
        }

        colliderShape.Size = size;
    }
}
