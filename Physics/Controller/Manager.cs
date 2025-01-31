using Godot;
using System;
using System.Collections.Generic;

namespace Controller
{
    // Singleton found at the root of the world scene that controls the player's ability to interact with the physics and provides visual feedback
    public partial class Manager : Node2D
    {
        // Current action being completed by the player
        State state = State.None;

        // The starting position of the mouse during any action
        Vector2 actionStartPosition;

        Slicer slicer;
        ToolSelector toolSelector;
        (Area2D Area, CollisionShape2D Collider) validator;
        readonly Dictionary<String, PackedScene> objectScenes = new Dictionary<string, PackedScene>();

        enum State
        {
            None,
            UseTool,
            Delete
        }

        public override void _Ready()
        {
            // Preloaded so that the sources do not have to be loaded whenever a new object is draw
            objectScenes.Add("Rectangle", GD.Load<PackedScene>("Physics/PhysicsObject/PhysicsRectangle/PhysicsRectangle.tscn"));
            objectScenes.Add("Circle", GD.Load<PackedScene>("Physics/PhysicsObject/PhysicsCircle/PhysicsCircle.tscn"));

            toolSelector = GetNode<ToolSelector>("%ToolSelector");
            slicer = GetNode<Slicer>("%Slicer");
            validator = (GetNode<Area2D>("%ValidationArea"), GetNode<CollisionShape2D>("%ValidationArea/Collider"));

            // Set the minimum size of the window so that the player cannot push the Engine physics to its limit as easily
            if (GetViewport() is Window)
            {
                Window window = (Window)GetViewport();
                window.MinSize = Vector2I.One * 300;
            }
        }

        public override void _Process(double delta)
        {
            // Redraw items every frame if an action is being done
            if (state != State.None) QueueRedraw();
        }


        public override void _Input(InputEvent @event)
        {
            // Mouse inputs
            if (@event is InputEventMouse)
            {
                // Regardless of action, mouse position will be needed
                Vector2 mousePosition = ((InputEventMouse)@event).Position;

                // If any mouse button is pressed or released (including mouse wheel and extras)
                if (@event is InputEventMouseButton)
                {
                    // Cast to MouseButton event
                    InputEventMouseButton buttonEvent = (InputEventMouseButton)@event;

                    // Tool functionality
                    if (buttonEvent.ButtonIndex == MouseButton.Left)
                    {
                        // Start of action
                        if (buttonEvent.Pressed && state == State.None)
                        {
                            state = State.UseTool;
                            actionStartPosition = buttonEvent.Position;

                            // Area should notify if overlapping with world bounds since an object cannot be placed inside it
                            validator.Area.SetCollisionMaskValue(1, true);

                            // Set shape of validation box to match shape if there is one
                            switch (toolSelector.SelectedTool)
                            {
                                case ToolSelector.Tool.Slice:
                                    break;
                                case ToolSelector.Tool.DrawRectangle:
                                    validator.Collider.Shape = new RectangleShape2D();
                                    break;
                                case ToolSelector.Tool.DrawCircle:
                                    validator.Collider.Shape = new CircleShape2D();
                                    break;
                            }
                        }
                        // End of action
                        else if (state == State.UseTool)
                        {
                            // Depending on selected tool, do action if valid
                            state = State.None;
                            switch (toolSelector.SelectedTool)
                            {
                                case ToolSelector.Tool.Slice:
                                    if (slicer.IsColliding()) slicer.SliceAll(actionStartPosition, buttonEvent.Position);
                                    break;
                                case ToolSelector.Tool.DrawRectangle:
                                    if (!validator.Area.HasOverlappingBodies()) spawnRectangle(actionStartPosition, buttonEvent.Position);
                                    break;
                                case ToolSelector.Tool.DrawCircle:
                                    if (!validator.Area.HasOverlappingBodies()) spawnCircle(actionStartPosition, buttonEvent.Position);
                                    break;
                            }

                            QueueRedraw(); // Update canvas once before it stops being updated (since the state is now none)
                        }
                    }

                    // Delete functionality
                    else if (buttonEvent.ButtonIndex == MouseButton.Right)
                    {
                        // Start of action
                        if (buttonEvent.Pressed && state == State.None)
                        {
                            // Area should not notify if overlapping with world bounds since it does not obstruct deletion
                            validator.Area.SetCollisionMaskValue(1, false);
                            state = State.Delete;
                            actionStartPosition = buttonEvent.Position;

                            // Deletion will always be in a rectangle selection area
                            validator.Collider.Shape = new RectangleShape2D();
                        }

                        // End of action
                        else if (state == State.Delete)
                        {
                            state = State.None;
                            deleteObjectsInValidationArea();
                            QueueRedraw();  // Update canvas once before it stops being updated (since the state is now none)
                        }
                    }
                }

                // Executed every time anytime a mouse button is pressed or it is moved

                if (state == State.None)
                {
                    // Reset shape of validation area to null, essentially disabling it
                    validator.Collider.Shape = null;

                    // Allow selected tool to change again
                    toolSelector.Locked = false;
                }
                else
                {
                    // Prevent selected tool from changing mid-action
                    toolSelector.Locked = true;

                    // Update validation area shape and raycast based on new mouse position / new action
                    Vector2 difference = mousePosition - actionStartPosition;
                    if (validator.Collider.Shape is RectangleShape2D)
                    {
                        RectangleShape2D shape = (RectangleShape2D)validator.Collider.Shape;
                        shape.Size = difference.Abs();
                        validator.Area.GlobalPosition = actionStartPosition + difference / 2;
                    }
                    else if (validator.Collider.Shape is CircleShape2D)
                    {
                        CircleShape2D shape = (CircleShape2D)validator.Collider.Shape;
                        shape.Radius = Mathf.Max(difference.Abs().X, difference.Abs().Y) / 2;
                        validator.Area.GlobalPosition = actionStartPosition + difference / 2;
                    }
                    else
                    {
                        slicer.GlobalPosition = actionStartPosition;
                        slicer.TargetPosition = mousePosition - actionStartPosition;
                    }
                }
            }
        }


        public override void _Draw()
        {
            if (state != State.None)
            {
                Vector2 mousePosition = GetLocalMousePosition();
                Vector2 dimensions = mousePosition - actionStartPosition;

                // Based on current action, give different visual feedback.
                switch (state)
                {
                    // For tools, blue if possible. Else, red.
                    case State.UseTool:
                        Color color = Colors.LightBlue;
                        switch (toolSelector.SelectedTool)
                        {
                            case ToolSelector.Tool.Slice:
                                if (!slicer.IsColliding()) color = Colors.PaleVioletRed;
                                DrawDashedLine(actionStartPosition, mousePosition, color);
                                break;

                            case ToolSelector.Tool.DrawRectangle:
                                if (validator.Area.HasOverlappingBodies()) color = Colors.PaleVioletRed;
                                DrawRect(new Rect2(actionStartPosition, dimensions), color);
                                break;
                            case ToolSelector.Tool.DrawCircle:
                                if (validator.Area.HasOverlappingBodies()) color = Colors.PaleVioletRed;
                                Vector2 difference = mousePosition - actionStartPosition;
                                float radius = Mathf.Max(difference.Abs().X, difference.Abs().Y) / 2; ;
                                DrawCircle(actionStartPosition + difference / 2, radius, color);
                                break;
                        }
                        break;
                    // For deletion, always red.
                    case State.Delete:
                        dimensions = mousePosition - actionStartPosition;
                        DrawRect(new Rect2(actionStartPosition, dimensions), Colors.PaleVioletRed);
                        break;
                }
            }
        }

        // Create a new PhysicsRectangle between two points and add it to tree
        void spawnRectangle(Vector2 c1, Vector2 c2)
        {
            Vector2 difference = c2 - c1;

            // Save unnecessary node creation if the object is too small to be created in the first place
            if (Mathf.Abs(difference.X * difference.Y) > PhysicsObject.MINIMUM_AREA)
            {
                PhysicsRectangle rect = objectScenes["Rectangle"].Instantiate<PhysicsRectangle>();
                AddChild(rect);

                rect.Dimensions = difference / 2;
                rect.GlobalPosition = c1 + difference / 2;

            }
        }

        // Create a new PhysicsCircle between two points and add it to tree
        void spawnCircle(Vector2 c1, Vector2 c2)
        {
            Vector2 difference = c2 - c1;
            float radius = Mathf.Max(difference.Abs().X, difference.Abs().Y) / 2;

            // Save unnecessary node creation if the object is too small to be created in the first place
            if (Mathf.Pi * radius * radius > PhysicsObject.MINIMUM_AREA)
            {
                PhysicsCircle circle = objectScenes["Circle"].Instantiate<PhysicsCircle>();
                AddChild(circle);

                circle.Radius = radius;
                circle.GlobalPosition = c1 + difference / 2;
            }
        }


        // Destroy all PhysicsObject overlapping with the validationArea 
        // It is assumed that the validationArea has been placed in the correct position in advance, since otherwise it would require about 80ms to update.
        void deleteObjectsInValidationArea()
        {
            foreach (Node2D node in validator.Area.GetOverlappingBodies()) if (node is PhysicsObject) node.QueueFree();
        }


    }
}