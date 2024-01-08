using Godot;
using System;

public partial class node_3d : Node3D
{
	[Export]
	public Color floorColor;
	[Export]
	public Color wallColor;

	[Export]
	public bool useDelay = true;

	[Export]
	public float waitTime = 0.1f;
	[Export]
	public int MAXTILES = 400;

	private const float BOXSIZE = 2.0f;
	

	private PackedScene boxScene = ResourceLoader.Load<PackedScene>("res://box.tscn");


	private RandomNumberGenerator rng = new RandomNumberGenerator();
	private Node3D levelData;
	private Timer timer;
	private Label placedTiles;
	private SpinBox numTiles;
	private SpinBox numThickness;
	private GridMap gridMap;
	private SpinBox wallHeight;
	private CheckBox visualizeGeneration;
	private LineEdit seed;
	private CheckBox randSeed;
	private CheckBox randThick;

	private bool visualize = false;
	private bool started = false;
	private bool running = false;
	private bool done = false;
	private int halfTiles ;

	

	private enum WALKER_DIRECTION : int
	{
		FORWARD,
		BACKWARD,
		LEFT,
		RIGHT
	}

	private bool[,] visited;

	private Vector3 walker = new Vector3(0,0,0);
	private int walker_x = 0;
	private int walker_y = 0;
	private int walker_z = 0;
	private int numPlacedTiles = 0;
	private double millis;
		

	public override void _Ready()
	{
		levelData = (Node3D)GetNode("Level");
		timer = (Timer)GetNode("Timer");
		placedTiles = (Label)GetNode("CanvasLayer/Panel/VBoxContainer/Placed");
		numTiles = (SpinBox)GetNode("CanvasLayer/Panel/VBoxContainer/HBoxContainer/SpinBox");
		numThickness = (SpinBox)GetNode("CanvasLayer/Panel/VBoxContainer/HBoxContainer2/thick");
		gridMap = (GridMap)GetNode("GridMap");
		wallHeight = (SpinBox)GetNode("CanvasLayer/Panel/VBoxContainer/HBoxContainer3/wallHeight");
		visualizeGeneration = (CheckBox)GetNode("CanvasLayer/Panel/VBoxContainer/visualize");
		seed = (LineEdit)GetNode("CanvasLayer/Panel/VBoxContainer/HBoxContainer4/Seed");
		randSeed = (CheckBox)GetNode("CanvasLayer/Panel/VBoxContainer/RandSeed");
		randThick = (CheckBox)GetNode("CanvasLayer/Panel/VBoxContainer/RandThick");

		seed.Text = rng.Seed.ToString();
		timer.OneShot = true;

		rng = new RandomNumberGenerator();

		

		
	}

    public override void _Process(double delta)
    {		
/*		if (Input.IsActionJustPressed("start"))
		{
			
			GD.Print(visualize);
			started = true;
		}*/

        if (started)
		{
			ClearLevelData();
			
			started = false;
			running = true;
		}
		if (running)
		{
			if (visualize)
				RandomWalk();
			else
				RunBackground();

			millis += delta;
		}

		/*if(Input.IsActionJustPressed("walls") && done)
		{
			MakeWalls();
		}*/
    }

	private void ClearLevelData()
	{
		gridMap.Clear();
		MAXTILES = (int)numTiles.Value;
		halfTiles = MAXTILES/2;
		visited = new bool[MAXTILES*2,MAXTILES*2];

		foreach (Node n in levelData.GetChildren())
			n.QueueFree();

		for (int i=0;i<MAXTILES;i++)
		 for (int j=0;j<MAXTILES;j++)
		  	visited[i,j] = false;

		walker = new Vector3(0,0,0);
		walker_x = 0;
		walker_y = 0;
		walker_z = 0;
		numPlacedTiles = 0;
		millis = 0;
		done = false;

	}
	
	private void RunBackground()
	{
		while (running)
			RandomWalk();
		PlotMap();
	}

    private void RandomWalk() 
	{
		if (useDelay && (timer.TimeLeft>0))
			return;

		if (numPlacedTiles<MAXTILES)
		{	
			
			// Place Tile
			Place();
			
			
			// Pick Random Direction			
			bool foundDirection = false;
			if (IsStuck(walker_x,walker_y,walker_z))
				FindFreeSpot();
			while (foundDirection==false)
			{
				int newDirection = rng.RandiRange(0,3);
				switch ((WALKER_DIRECTION)newDirection)
				{
					case WALKER_DIRECTION.FORWARD:
						if (!CanPlace(walker_x,walker_y,walker_z-1))
						{
							walker_z--;
							foundDirection=true;
						}
						break;
					case WALKER_DIRECTION.BACKWARD:
						if (!CanPlace(walker_x,walker_y,walker_z+1))
						{
							walker_z++;
							foundDirection= true;
						}
						break;
					case WALKER_DIRECTION.LEFT:
						if (!CanPlace(walker_x-1,walker_y,walker_z))
						{
							walker_x--;
							foundDirection = true;
						}
						break;
					case WALKER_DIRECTION.RIGHT:
						if (!CanPlace(walker_x+1,walker_y,walker_z))
						{
							walker_x++;
							foundDirection = true;
						}
						break;
				}
			}
			if (useDelay) timer.Start(waitTime);			
		} else
		{
			//ThickenFloors();
		//	MakeWalls();
			running = false;
			GD.Print("done " + millis.ToString());
			done = true;
			
		}
	}
	
	// returns false if tile is not visited
	private bool CanPlace(int x,int y,int z)
	{
	/*	if (((x+halfTiles) < 0) || ((y+halfTiles) < 0) || ((z+halfTiles) < 0) || 
			((x+halfTiles) > MAXTILES-1) || ((y+halfTiles) > MAXTILES-1) || ((z+halfTiles) > MAXTILES-1))
			return true;*/
		return visited[(int)(x+halfTiles),(int)(z+halfTiles)];
	}

	private bool IsStuck(int x,int y,int z)
	{
		/*if (((x+halfTiles) < 0) || ((y+halfTiles) < 0) || ((z+halfTiles) < 0) || 
			((x+halfTiles) > MAXTILES-1) || ((y+halfTiles) > MAXTILES-1) || ((z+halfTiles) > MAXTILES-1))
			return true;*/

		return (
				visited[(int)(x+1+halfTiles),(int)(z+halfTiles)] && 
				visited[(int)(x-1+halfTiles),(int)(z+halfTiles)] && 
				visited[(int)(x+halfTiles),(int)(z+1+halfTiles)] && 
				visited[(int)(x+halfTiles),(int)(z-1+halfTiles)]);		
	}

	private bool hasNeighbour(Vector3 pos)
	{
		return (visited[(int)(pos.X+1+halfTiles),(int)(pos.Z+halfTiles)] ||
				visited[(int)(pos.X-1+halfTiles),(int)(pos.Z+halfTiles)] || 
				visited[(int)(pos.X+halfTiles),(int)(pos.Z+1+halfTiles)] || 
				visited[(int)(pos.X+halfTiles),(int)(pos.Z-1+halfTiles)]);		
	}

	private bool hasNeighbourOffset(int x,int y,int z,int ox,int oy,int oz)
	{
		return (visited[x+ox,z+oz]);
	}

	private void FindFreeSpot()
	{
		bool found = false;
		int check_x = walker_x;
		int check_y = walker_y;
		int check_z = walker_z;
		while (found==false)
		{

			if (!IsStuck(check_x,check_y,check_z))
			{
				walker_x = check_x;
				walker_y = check_y;
				walker_z = check_z;
				return;
			} else
			{
				int newDirection = rng.RandiRange(0,3);
				switch ((WALKER_DIRECTION)newDirection)
				{
					case WALKER_DIRECTION.FORWARD:
						check_z -= 1;
						break;
					case WALKER_DIRECTION.BACKWARD:
						check_z += 1;
						break;
					case WALKER_DIRECTION.LEFT:
						check_x -= 1;
						break;
					case WALKER_DIRECTION.RIGHT:
						check_x += 1;
						break; 
				}
			}

		}
		
	}

	private void PlaceBlock(int x,int y,int z,Color color)
	{
		if (color == floorColor)
			gridMap.SetCellItem(new Vector3I(x,y,z),0,0);
		else
			gridMap.SetCellItem(new Vector3I(x,y,z),1,0);
		
	}

	private void Place()
	{
		visited[(int)(walker_x+halfTiles),(int)(walker_z+halfTiles)] = true;
		numPlacedTiles++;

		if ((int)numThickness.Value>1)
		{
			int th = (int)numThickness.Value;
			if (randThick.ButtonPressed)
			{
				th = rng.RandiRange(1,(int)numThickness.Value);
				GD.Print(th);
			}
			
			for (int k=0;k<th;k++)
			{
				if ((walker_x-k > 0) && (walker_x+k < MAXTILES*2-1) && (walker_z-k > 0) && (walker_z+k < MAXTILES*2-1))
				{
					visited[walker_x-k+halfTiles,  walker_z  +halfTiles] = true;
					visited[walker_x+k+halfTiles,  walker_z  +halfTiles] = true;
					visited[walker_x  +halfTiles,  walker_z-k+halfTiles] = true;
					visited[walker_x  +halfTiles,  walker_z+k+halfTiles] = true;
					visited[walker_x-k+halfTiles,  walker_z-k+halfTiles] = true;
					visited[walker_x+k+halfTiles,  walker_z+k+halfTiles] = true;
					visited[walker_x+k+halfTiles,  walker_z-k+halfTiles] = true;
					visited[walker_x-k+halfTiles,  walker_z+k+halfTiles] = true;
					if (visualize)
					{
						PlaceBlock(walker_x-k,walker_y,walker_z,floorColor);
						PlaceBlock(walker_x+k,walker_y,walker_z,floorColor);
						PlaceBlock(walker_x  ,walker_y,walker_z-k,floorColor);
						PlaceBlock(walker_x  ,walker_y,walker_z+k,floorColor);
						PlaceBlock(walker_x-k,walker_y,walker_z-k,floorColor);
						PlaceBlock(walker_x+k,walker_y,walker_z+k,floorColor);
						PlaceBlock(walker_x+k,walker_y,walker_z-k,floorColor);
						PlaceBlock(walker_x-k,walker_y,walker_z+k,floorColor);
					}
				}
			}
		}




		placedTiles.Text = "Placed Tiles : " + numPlacedTiles.ToString() + " / " + MAXTILES.ToString() + "   TIME : " + millis.ToString();
		if (visualize)
			PlaceBlock(walker_x,walker_y,walker_z,floorColor);
	}


	private void PlotMap()
	{
		//ThickenFloors();
		for (int i=0;i<MAXTILES*2-1;i++)
		 for (int j=0;j<MAXTILES*2-1;j++)
			if (visited[i,j]==true) 
				PlaceBlock((i-halfTiles),0,(j-halfTiles),floorColor);
		
		done = true;
	}

	private void ThickenFloors()
	{
		bool[,] newVisited = (bool[,])visited.Clone();
		for (int i=0;i<MAXTILES*2-1;i++)
		 for (int j=0;j<MAXTILES*2-1;j++)
			if (visited[i,j])
			{
				int th = (int)numThickness.Value;
				if (randThick.ButtonPressed)
					th = rng.RandiRange(0,(int)numThickness.Value);
				
				for (int k=0;k<th;k++)
				{
					if ((i-k > 0) && (i+k < MAXTILES*2-1) && (j-k > 0) && (j+k < MAXTILES*2-1))
					{
						newVisited[i-k,  j] = true;
						newVisited[i+k,  j] = true;
						newVisited[i  ,j-k] = true;
						newVisited[i  ,j+k] = true;

						newVisited[i-k,j-k] = true;
						newVisited[i+k,j+k] = true;
						newVisited[i+k,j-k] = true;
						newVisited[i-k,j+k] = true;
						if (visualize)
						{
							PlaceBlock((i-k-halfTiles),0,(j-halfTiles),floorColor);
							PlaceBlock((i+k-halfTiles),0,(j-halfTiles),floorColor);
							PlaceBlock((i-halfTiles),0,(j-k-halfTiles),floorColor);
							PlaceBlock((i-halfTiles),0,(j+k-halfTiles),floorColor);

							PlaceBlock((i-k-halfTiles),0,(j-k-halfTiles),floorColor);
							PlaceBlock((i+k-halfTiles),0,(j+k-halfTiles),floorColor);
							PlaceBlock((i+k-halfTiles),0,(j-k-halfTiles),floorColor);
							PlaceBlock((i-k-halfTiles),0,(j+k-halfTiles),floorColor);
						}
					}
				}
			}
		visited = (bool[,])newVisited.Clone();;
	}

	private void MakeWalls()
	{
		for (int i=0;i<MAXTILES*2-1;i++)
		 for (int j=0;j<MAXTILES*2-1;j++)
			if (visited[i,j])
			{
				for (int k=0;k<(int)wallHeight.Value;k++)
				{
					if (!hasNeighbourOffset(i,0,j,-1,0,0))
						PlaceBlock((i-1-halfTiles),k,(j-halfTiles),wallColor);
					if (!hasNeighbourOffset(i,0,j,1,0,0))
						PlaceBlock((i+1-halfTiles),k,(j-halfTiles),wallColor);

					if (!hasNeighbourOffset(i,0,j,0,0,-1))
						PlaceBlock((i-halfTiles),k,(j-1-halfTiles),wallColor);
					if (!hasNeighbourOffset(i,0,j,0,0,1))
						PlaceBlock((i-halfTiles),k,(j+1-halfTiles),wallColor);
				}
			}
	}	

	private void _on_generate_pressed()
	{
		GD.Print("start");
		visualize = visualizeGeneration.ButtonPressed;
		if (randSeed.ButtonPressed)
		{
			rng.Randomize();
			seed.Text = rng.Seed.ToString();
		} else
			rng.Seed = (ulong)Convert.ToInt64(seed.Text);

		ClearLevelData();
		running = true;
	}

	private void _on_make_walls_pressed()
	{
		if (done)
			MakeWalls();
	}
}