using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class PathFinding {

    public static Vector2 startPoint;
    public static Vector2 endPoint;

    public static Path findPath(List<Point> islandsGrid)
    {
        

        List<Point> openPoints = new List<Point>();
        List<Point> closedPoints = new List<Point>();

        foreach (Point isle in islandsGrid)
        {
            if (isle.x == startPoint.x && isle.y == startPoint.y)
            {
                openPoints.Add(isle);
                break;
            }
        }

        Point current = new Point();
        bool foundPath = false;
        while (true)
        {
            current = new Point();
            foreach (Point p in openPoints)
            {
                if (current.Fcost <= 0) {
                    current = p;             
                }
                else
                {
                    if (p.Fcost < current.Fcost)
                    {
                        current = p;
                    }
                    if (p.Fcost == current.Fcost)
                    {
                        if (p.Hcost < current.Hcost)
                        {
                            current = p;
                        }
                    }
                }
            }

            openPoints.Remove(current);
            closedPoints.Add(current);

            if (current.x == endPoint.x && current.y == endPoint.y)
            {
                foundPath = true;               

                return MakeListMap(current);
            }
            else
            {
                foreach (Point neighbour in current.neighbours)
                {
                    if (!closedPoints.Contains(neighbour))
                    {
                        if (!openPoints.Contains(neighbour))
                        {
                            neighbour.Gcost = PathFinding.Gcost(neighbour);
                            neighbour.Hcost = PathFinding.Hcost(neighbour);
                            neighbour.Fcost = PathFinding.Fcost(neighbour);
                            neighbour.parent = current;
                            openPoints.Add(neighbour);
                        } 
                    }
                }
            }

            if (foundPath)
            {
                break;
            }
        }

        return MakeListMap(current);
    }

    public static Path listmap = new Path();


    //recursive path generating from parent line.
    public static Path MakeListMap(Point endpoint)
    {
        
        if (endpoint.parent != null)
        {
            listmap.addPoint(endpoint.parent);
            return MakeListMap(endpoint.parent);
        }

        return listmap;

    }

    //distance from starting point
    public static int Gcost(Point point)
    {
        return (int)Vector3.Distance(new Vector3(startPoint.x, startPoint.y), new Vector3(point.x, point.y));
    }

    //distance from end point
    public static int Hcost(Point point)
    {
        return (int)Vector3.Distance(new Vector3(endPoint.x, endPoint.y), new Vector3(point.x, point.y));
    }

    public static int Fcost(Point point)
    {
        return (int)point.Fcost + point.Gcost;
    }

    public static GameObject GetStartIsland(List<GameObject> islandparts)
    {
            foreach (GameObject g in islandparts)
            {
                if (startPoint.x == g.transform.position.x && startPoint.y == g.transform.position.z)
                {
                return g;
                }
            }
        return null;
    }

    public static GameObject GetEndIsland(List<GameObject> islandparts)
    {
        foreach (GameObject g in islandparts)
        {
            if (endPoint.x == g.transform.position.x && endPoint.y == g.transform.position.z)
            {
                return g;
            }
        }
        return null;
    }

}
