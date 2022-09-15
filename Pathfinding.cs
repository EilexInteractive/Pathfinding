using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Pathfinding : TileMap
{
    Vector2 StartPoint;
    Vector2 EndPoint;
    Line2D line;

    public int WalkableTile = 14;

    Godot.Collections.Array _AllCells;              // Reference to all the cells in the map
    private List<Tile> _AllTiles = new List<Tile>();

    private List<Tile> Path = new List<Tile>();

    private float timerCount = 0;
    [Export] private float timerMax = 0.3f;
    bool runTimer = true;

    public override void _Ready()
    {
        base._Ready();

        _AllCells = GetUsedCells();
    

        foreach(var tile in _AllCells)
        {
            // Get the values as a string break them apart
            var value = _AllCells[0].ToString();
            value = value.Replace("(", string.Empty);
            value = value.Replace(")", string.Empty);
            var VectorString = value.Split(',');

            // Get the values as an int and create a new vector 2
            int xValue = int.Parse(VectorString[0]);
            int yValue = int.Parse(VectorString[1]);

            bool walkable = GetCell(xValue, yValue) != 14;                      // Cell that can be walked on
            _AllTiles.Add(new Tile(new Vector2(xValue, yValue), walkable));               // Create the tile for each of the cells

        }

        Tile current = null;

        // Get the start and end positions
        StartPoint = GetNode<KinematicBody2D>("/root/Main/NavAgent").Position;
        EndPoint = GetNode<Sprite>("/root/Main/EndPoint").Position;


        var startTile = WorldToMap(StartPoint);
        var endTile = WorldToMap(EndPoint);
        
        var openList = new List<Tile>();
        var closedList = new List<Tile>();
        int g = 0;

        openList.Add(new Tile(startTile));              // Add the start tile to the list as the first item

        while(openList.Count > 0)
        {
            // Get the sqaure with the lowest F Score
            var lowest = openList.Min(t => t.F);
            current = openList.First(t => t.F == lowest);
            closedList.Add(current);
            openList.Remove(current);

            // Add tile to the path
            Path.Add(current);


            // We have reached the end tile
            if(closedList.FirstOrDefault(t => t.TilePosition.x == endTile.x && t.TilePosition.y == endTile.y) != null)
            {
                break;
            }

            var walkableSquares = GetWalkableSquares(current);
            g++;

            foreach(var square in walkableSquares)
            {
                if(closedList.FirstOrDefault(t => t.TilePosition == square.TilePosition) != null)
                    continue;
                                         // Set the parent

                if(openList.FirstOrDefault(t => t.TilePosition == square.TilePosition) == null)
                {
                    var tilePos = MapToWorld(square.TilePosition);                    
                    square.G = g;
                    square.H = ComputeHScore(tilePos, EndPoint);
                    square.F = square.G + square.H;
                    square.Parent = current;                                            // Set the parent
                    openList.Insert(0, square);                                     // Add it to first in the list
                } else 
                {
                    if(g + square.H < square.F)
                    {
                        square.G = g;
                        square.F = square.G + square.H;
                        square.Parent = current;
                    }
                }
            }
        }

        

        
    }

    public int ComputeHScore(Vector2 tilePos, Vector2 targetPos)
    {
        return (int)Math.Abs(targetPos.x - tilePos.x) + (int)Math.Abs(targetPos.y - tilePos.y);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if(runTimer)
        {
            timerCount += 1 * delta;
            if(timerCount > timerMax)
            {
                timerCount = 0;
                if(Path.Count > 0)
                {
                    var pos = MapToWorld(Path.First().TilePosition);

                    //GetNode<KinematicBody2D>("/root/Main/NavAgent").Position = pos;
                    GetNode<Line2D>("/root/Main/Line2D").AddPoint(pos);
                    Update();
                    Path.RemoveAt(0);
                } else 
                {
                    GD.Print("You have arrived");
                }
            }
        }

        if(Input.IsActionJustPressed("ui_accept"))
            runTimer = !runTimer;

    }


    private List<Tile> GetWalkableSquares(Tile tile)
    {
        var ProposedTiles = new List<Tile>()
        {
            new Tile(new Vector2(tile.TilePosition.x, tile.TilePosition.y + 1)),
            new Tile(new Vector2(tile.TilePosition.x, tile.TilePosition.y - 1)),
            new Tile(new Vector2(tile.TilePosition.x + 1, tile.TilePosition.y)),
            new Tile(new Vector2(tile.TilePosition.x - 1, tile.TilePosition.y))
        };


        // Loop through each tile and ensure they are walkable
        foreach(var t in ProposedTiles)
        {
            int cellID = GetCell((int)t.TilePosition.x, (int)t.TilePosition.y);
            if(cellID == 14)
                t._Walkable = true;
            else
                t._Walkable = false;
        }
        return ProposedTiles.Where(t => t._Walkable == true).ToList();
    }
}

public class Tile
{
    public Vector2 TilePosition;
    public bool _Walkable;
    public float F;
    public float G;
    public float H;
    public Tile Parent;

    public Tile(Vector2 pos, bool _Walkable = false)
    {
        TilePosition = pos;
        this._Walkable = _Walkable;
    }

    public Tile(int x, int y)
    {
        TilePosition = new Vector2(x, y);
    }

    
    
}
