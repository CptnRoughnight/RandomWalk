using Godot;
using System;

[Tool]
public partial class box : MeshInstance3D
{
	[Export]
	public Color meshColor;

	public override void _Ready()
	{
		StandardMaterial3D mat = (StandardMaterial3D)MaterialOverride.Duplicate();		
		mat.AlbedoColor = meshColor;
		MaterialOverride = mat;

	}

	
	public override void _Process(double delta)
	{
	}
}
