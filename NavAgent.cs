using Godot;
using System;
using System.Collections.Generic;

public class NavAgent : KinematicBody2D
{
    Pathfinding map;

    List<Vector2> Path;

    float time = 0;
    float timeLen = 0.3f;

    Vector2 CurrentTilePos;
    Vector2 NextPosition;


    public override void _Ready()
    {
        base._Ready();

        map = GetNode<Pathfinding>("/root/Main/TileMap");
        Path = map.GetPath();
        CurrentTilePos =  Path[0];
        NextPosition = Path[1];

        foreach(var pos in Path)
        {
            GetNode<Line2D>("/root/Main/Line2D").AddPoint(pos);
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if(Path.Count > 0)
        {
            MoveAndCollide(GetMovementDirection());
            if(this.Position > Path[1])
                ReachedPath();

            
        }

        Update();

        
        
    }

    public Vector2 GetMovementDirection()
    {
        var Velocity = new Vector2();

        var currentTile = map.WorldToMap(CurrentTilePos);
        var nextTile = map.WorldToMap(Path[1]);

        if(nextTile.x > currentTile.x)
            Velocity.x += 1;

        if(nextTile.y > currentTile.y)
            Velocity.y += 1;

        if(nextTile.y < currentTile.y)
            Velocity.y -= 1;

        if(nextTile.x < currentTile.x)
            Velocity.x -= 1;


        return Velocity.Normalized();
    }

    public void ReachedPath()
    {
        GD.Print("Hit point");
        CurrentTilePos = NextPosition;
        if(Path.Count > 1)
        {
            Path.RemoveAt(0);
            NextPosition = Path[1];
        } else 
        {
            GD.Print("Have reached location");
        }
    }
}
