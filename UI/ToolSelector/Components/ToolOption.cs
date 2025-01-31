using Godot;

[Tool]
// Basic class to simplify changes in the editor
public partial class ToolOption : Control
{
	[Export] public ToolSelector.Tool ToolType;

    public Button Button;

	public override void _Ready()
	{
        Button = GetNode<Button>("Button");
	}

}
