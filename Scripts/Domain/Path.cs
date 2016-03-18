using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Point {
    public float x;
    public float y;
    public int Gcost;
    public int Hcost;
    public int Fcost;
    public Point parent;
    public List<Point> neighbours;
    public List<Vector2> positions;

    public Point(float x, float y, int gcost, int hcost, int fcost, List<Point> neighbours)
    {
        this.x = x;
        this.y = y;
        Gcost = gcost;
        Hcost = hcost;
        Fcost = fcost;
        parent = null;
        this.neighbours = neighbours;
        positions = new List<Vector2>();
    }

    public Point()
    {
        x = 0;
        y = 0;
        Gcost = 0;
        Hcost = 0;
        Fcost = 0;
        parent = null;
        neighbours = new List<Point>();
        positions = new List<Vector2>();
    }

    public void setParent(Point point)
    {
        parent = point;
    }

}


public class Path {

    List<Point> path;

    public Path()
    {
        path = new List<Point>();
    }

    public bool addPoint(Point point)
    {
        path.Add(point);
        return true;
    }

    public List<Point> getPath()
    {
        return path;
    }
	
}
