using Godot;

// Generic UI element that can shrink its root container and collapse its siblings when toggled
public partial class ExpandArrow : Button
{
    [Export] NodePath RootContainerPath = "..";
    Container rootContainer;

    TextureRect icon;
    public override void _Ready()
    {
        icon = GetNode<TextureRect>("Icon");
        rootContainer = GetNode<Container>(RootContainerPath);
    }

    // Toggle collapsing container
    public void _OnToggled(bool pressed)
    {
        icon.FlipH = pressed;

        // Collapses all sibling nodes
        foreach (Control uiElement in GetParent().GetChildren()) if (uiElement != this) uiElement.Visible = pressed;

        // Manually updates the container at the highest level to shrink to minimum size.
        rootContainer.Size = Vector2.Zero;
    }
}
