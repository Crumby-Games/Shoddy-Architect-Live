using Godot;
using System;
using System.Collections.Generic;

// Gives the user the ability to select different tools with on-screen buttons or keyboard keys
public partial class ToolSelector : HBoxContainer
{
    public Tool SelectedTool
    {
        get { return selectedTool; }
        set { selectTool(value); }
    }

    // Whether the selectedTool can currently be changed
    public bool Locked;

    Tool selectedTool = Tool.DrawRectangle;
    Dictionary<Tool, ToolOption> toolOptions = new Dictionary<Tool, ToolOption>();

    // Number of implemented tools, derived from enum.
    readonly int NUM_TOOLS = Enum.GetNames(typeof(Tool)).Length;

    public enum Tool
    {
        Slice,
        DrawRectangle,
        DrawCircle
    }

    public override void _Ready()
    {
        // Iterate through each button child, bind to it, and assign which tool corresponds to which
        foreach (Node child in GetChildren()) if (child is ToolOption)
            {
                ToolOption toolOption = (ToolOption)child;
                toolOptions.Add(toolOption.ToolType, toolOption);
                toolOption.Button.Pressed += () => SelectedTool = toolOption.ToolType;
            }

        // Update buttons to show starting tool
        selectTool(selectedTool);
    }

    public override void _Input(InputEvent @event)
    {
        // Change selected tool if buttons are pressed
        if (@event is InputEventKey && !Locked)
        {
            int index = (int)SelectedTool;
            // Select next
            if (@event.IsActionPressed("ui_right"))
            {
                index = (index + 1) % NUM_TOOLS;
                SelectedTool = (Tool)index;
            }
            // Select previous
            else if (@event.IsActionPressed("ui_left"))
            {
                index = Mathf.PosMod(index - 1, NUM_TOOLS);
                SelectedTool = (Tool)index;
            }
        }
    }

    // Setter for selectedTool which updates buttons to show when a new tool has been selected
    void selectTool(Tool value)
    {
        if (value == selectedTool || !Locked)
        {
            toolOptions[selectedTool].Button.ButtonPressed = false;
            selectedTool = value;
            toolOptions[selectedTool].Button.ButtonPressed = true;
        }
    }
}
