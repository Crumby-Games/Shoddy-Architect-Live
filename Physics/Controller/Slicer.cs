using Godot;
using System.Collections.Generic;

namespace Controller
{
    // An overflow of Control behaviour into a raycast for slicing objects
    public partial class Slicer : RayCast2D
    {
        // A raycast that is configured to hit from inside bodies but has a length of 0. In practice this means it only triggers when it starts from within a body, which allows us to determine whether the end point of a slice is in a body.
        RayCast2D endPositionChecker;

        public override void _Ready()
        {
            endPositionChecker = GetNode<RayCast2D>("Checker");
        }

        // Instead of stopping at the first body like a regular raycast, force update and continue after registering the presence of each object and the raycast's collision points
        public Dictionary<PhysicsObject, Vector2> GetAllCollidingObjects()
        {
            var collidingObjects = new Dictionary<PhysicsObject, Vector2>();
            PhysicsObject endObject = (PhysicsObject)endPositionChecker.GetCollider();
            while (IsColliding())
            {
                PhysicsObject collidingObject = (PhysicsObject)GetCollider();

                // If the endPositionChecker intersects with an object and it's this one, exit the loop without resolving intersections. (When the endPositionChecker is not intersecting a body (i.e., the end of the slice is not inside an object), lastObject will be null, which collidingObject will not be.)
                if (collidingObject == endObject) break;

                // Take note of object's collision
                collidingObjects[collidingObject] = GetCollisionPoint();
                
                // Ignore object temporarily
                AddException(collidingObject);
                ForceRaycastUpdate();
            }

            // Reset collision exceptions
            foreach (PhysicsObject excludedObject in collidingObjects.Keys) RemoveException(excludedObject);

            return collidingObjects;
        }

        // Split all PhysicsObjects in path iteratively
        public void SliceAll(Vector2 startPos, Vector2 endPos)
        {

            Position = startPos;
            TargetPosition = endPos - startPos;
            endPositionChecker.Position = TargetPosition;

            // Force update casts this frame
            ForceRaycastUpdate();
            endPositionChecker.ForceRaycastUpdate();

            // Split each polygon in path in two, creating new polygons
            foreach ((PhysicsObject collidingObject, Vector2 position) in GetAllCollidingObjects())
            {
                Vector2 start = (position - collidingObject.GlobalPosition).Rotated(-collidingObject.Rotation);
                Vector2 direction = TargetPosition.Normalized().Rotated(-collidingObject.Rotation);
                collidingObject.Split(start, direction);
            }
        }
    }
}