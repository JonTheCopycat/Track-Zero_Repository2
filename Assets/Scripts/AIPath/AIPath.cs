using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

//[System.Serializable]
//public class AIPath
//{
//    [System.Serializable]
//    public class Point
//    {
//        private float[] pos;
//        private float[] dir;
//        private float[] normal;
//        private float width;


//        public Point()
//        {
//            pos = new float[3];
//            dir = new float[3];
//            normal = new float[3];
//            width = 10;
//        }

//        public Point(Vector3 position, Vector3 direction, Vector3 n) : this(position, direction, n, 10)
//        {
//        }

//        public Point(Vector3 position, Vector3 direction, Vector3 n, float width)
//        {
//            pos = new float[3];
//            pos[0] = position.x;
//            pos[1] = position.y;
//            pos[2] = position.z;

//            dir = new float[3];
//            dir[0] = direction.x;
//            dir[1] = direction.y;
//            dir[2] = direction.z;

//            normal = new float[3];
//            normal[0] = n[0];
//            normal[1] = n[1];
//            normal[2] = n[2];

//            this.width = width;
//        }

//        public Vector3 GetPos()
//        {
//            return new Vector3(pos[0], pos[1], pos[2]);
//        }

//        public Vector3 GetDir()
//        {
//            return new Vector3(dir[0], dir[1], dir[2]);
//        }

//        public Vector3 GetNormal()
//        {
//            return new Vector3(normal[0], normal[1], normal[2]);
//        }

//        public float GetWidth()
//        {
//            return width;
//        }

//        public void SetPos(Vector3 newPos)
//        {
//            pos[0] = newPos.x;
//            pos[1] = newPos.y;
//            pos[2] = newPos.z;
//        }

//        public void SetDir(Vector3 newDir)
//        {
//            dir[0] = newDir.x;
//            dir[1] = newDir.y;
//            dir[2] = newDir.z;
//        }

//        public void SetNormal(Vector3 newNormal)
//        {
//            normal[0] = newNormal.x;
//            normal[1] = newNormal.y;
//            normal[2] = newNormal.z;
//        }

//        public void SetWidth(float newWidth)
//        {
//            width = newWidth;
//        }

//        public void Print()
//        {
//            Debug.Log("pos: " + GetPos() + ",\t dir: " + GetDir() + "\n");
//        }
//    }

//    Point[][] paths;
//    (int, int)[] splits; //every id in order where the paths split
//    (int, int)[] merges; //every id in order where the paths merge
//    int[] segments;

//    public AIPath()
//    {
//        paths = new Point[0][];
//        segments = new int[0];
//    }

//    public AIPath(Point[][] path, (int, int)[] splits, (int, int)[] merges)
//    {
//        if (path.Length > 0)
//        {
//            this.paths = path;
//            this.splits = splits;
//            this.merges = merges;
//            segments = new int[1];
//            segments[0] = 0;
//        }
//        else
//        {
//            throw new System.Exception("Path must have at least one point");
//        }

//    }

//    public AIPath(Point[][] path, (int, int)[] splits, (int, int)[] merges, int[] segmentStarts) : this(path, splits, merges)
//    {
//        this.segments = segmentStarts;
//    }

//    public AIPath(List<List<Point>> path, List<(int, int)> splits, List<(int, int)> merges)
//    {
//        this.paths = new Point[path.Count][];
//        for (int i = 0; i < paths.Length; i++)
//        {
//            paths[i] = new Point[path[i].Count];
//            for (int e = 0; e < paths[i].Length; e++)
//            {
//                paths[i][e] = path[i][e];
//            }
//        }

//        this.splits = splits.ToArray();
//        this.merges = merges.ToArray();

//        segments = new int[1];
//        segments[0] = 0;
//    }

//    public AIPath(List<List<Point>> path, List<(int, int)> splits, List<(int, int)> merges, List<int> segmentStarts) : this(path, splits, merges)
//    {
//        this.segments = segmentStarts.ToArray();
//    }

//    //use not recommended unless you know what you're doing
//    public Point[][] GetRawPath()
//    {
//        return paths;
//    }

//    public int GetSize()
//    {
//        return paths.Length;
//    }

//    public int[] GetRawSegments()
//    {
//        return segments;
//    }

//    public (int, int) GetSplits(int i)
//    {
//        try
//        {
//            return splits[i - 1];
//        }
//        catch
//        {
//            throw new System.IndexOutOfRangeException();
//        }

//    }

//    public (int, int) GetMerges(int i)
//    {
//        try
//        {
//            return merges[i - 1];
//        }
//        catch
//        {
//            throw new System.IndexOutOfRangeException();
//        }

//    }

//    public void SetRawPath(Point[][] points)
//    {
//        paths = points;
//    }

//    public void SetRawPath(List<List<Point>> points)
//    {
//        paths = new Point[points.Count][];
//        for (int i = 0; i < paths.Length; i++)
//        {
//            paths[i] = new Point[points[i].Count];
//            for (int e = 0; e < paths[i].Length; e++)
//            {
//                paths[i][e] = points[i][e];
//            }
//        }
//    }

//    /**
//     * returns the closest point in the path to position
//     * position: Vector3 position in world space
//     */
//    public Point GetClosestPoint(Vector3 position, int segment)
//    {
//        //check every point for the closest point to position
//        (int, int) closestId = (0, segments[segment]);
//        //int otherId = segments[segment];

//        for (int i = segments[segment]; i < paths[0].Length; i++)
//        {
//            if (segment < segments.Length - 1 && i >= segments[segment + 1])
//                break;

//            if ((paths[0][i].GetPos() - position).magnitude < (paths[closestId.Item1][closestId.Item2].GetPos() - position).magnitude)
//            {
//                closestId = (0, i);
//            }
//        }

//        for (int e = 1; e < paths.Length; e++)
//        {
//            for (int i = 0; i < paths[e].Length; i++)
//            {
//                if ((paths[e][i].GetPos() - position).magnitude < (paths[closestId.Item1][closestId.Item2].GetPos() - position).magnitude)
//                {
//                    closestId = (e, i);
//                }
//            }
//        }

//        //check from the resulting id up to 20 points ahead
//        //for (int i = closestId; i < closestId + 20 && i < paths.Length; i++)
//        //{
//        //    if ((paths[i].GetPos() - position).magnitude < (paths[closestId].GetPos() - position).magnitude)
//        //    {
//        //        closestId = i;
//        //    }
//        //}

//        return paths[closestId.Item1][closestId.Item2];
//    }

//    public (Point, Point, int, int) GetClosestLine(Vector3 position, int segment)
//    {
//        //iterate through every element of the array (slow)
//        //if segment is -1, check every segment

//        bool beyondLastIndex = false;
//        (int, int) closestId;
//        if (segment >= 0)
//        {
//           closestId = (0, segments[segment]);
//        }
//        else
//        {
//            closestId = (0, 0);
//        }


//        for (int i = segment > 0 ? segments[segment] : 0; i < paths[0].Length; i++)
//        {
//            if (segment >= 0 && segment < segments.Length - 1 && i >= segments[segment + 1])
//                break;

//            if ((paths[0][i].GetPos() - position).magnitude < (paths[closestId.Item1][closestId.Item2].GetPos() - position).magnitude)
//            {
//                closestId = (0, i);
//            }
//        }


//        for (int e = 1; e < paths.Length; e++)
//        {
//            for (int i = 0; i < paths[e].Length; i++)
//            {
//                if ((paths[e][i].GetPos() - position).magnitude < (paths[closestId.Item1][closestId.Item2].GetPos() - position).magnitude)
//                {
//                    closestId = (e, i);
//                }
//            }
//        }

//        (int, int) idBefore = (closestId.Item1, closestId.Item2 - 1);
//        if (idBefore.Item2 < 0)
//        {
//            idBefore.Item2 = paths[closestId.Item1].Length + idBefore.Item2;
//        }
//        //having found the closest point, check whether the one infront of behind is closer
//        try
//        {
//            int count = 0;
//            Plane plane1;
//            Plane plane2;
//            float distance1;
//            float distance2;

//            //Vector3.ProjectOnPlane(paths[idBefore.Item1][idBefore.Item2].GetPos() - position, paths[closestId.Item1][closestId.Item2].GetNormal()).magnitude < Vector3.ProjectOnPlane(paths[closestId.Item1][(closestId.Item2 + 1) % paths[closestId.Item1].Length].GetPos() - position, paths[closestId.Item1][closestId.Item2].GetNormal()).magnitude
//            while (count < 8)
//            {
//                plane1 = new Plane(paths[idBefore.Item1][idBefore.Item2].GetDir(), paths[idBefore.Item1][idBefore.Item2].GetPos());
//                plane2 = new Plane(paths[closestId.Item1][closestId.Item2].GetDir(), paths[closestId.Item1][closestId.Item2].GetPos());

//                distance1 = plane1.GetDistanceToPoint(position);
//                distance2 = plane2.GetDistanceToPoint(position);

//                if (distance1 > 0 && distance2 < 0)
//                {
//                    Debug.DrawLine(paths[idBefore.Item1][idBefore.Item2].GetPos(), paths[closestId.Item1][closestId.Item2].GetPos());
//                    return (paths[idBefore.Item1][idBefore.Item2], paths[closestId.Item1][closestId.Item2], idBefore.Item1, idBefore.Item2);
//                }
//                else
//                {
//                    closestId = (closestId.Item1, Wrap(closestId.Item2 + 1, 0, paths[closestId.Item1].Length - 1));
//                    idBefore = (closestId.Item1, closestId.Item2 - 1);
//                    if (idBefore.Item2 < 0)
//                    {
//                        idBefore.Item2 = paths[closestId.Item1].Length + idBefore.Item2;
//                    }
//                    count++;
//                }
//            }

//            //Debug.DrawLine(paths[idBefore.Item1][idBefore.Item2].GetPos(), paths[closestId.Item1][closestId.Item2].GetPos(), Color.red, Time.deltaTime);

//            Debug.DrawLine(paths[closestId.Item1][closestId.Item2].GetPos(), paths[closestId.Item1][(closestId.Item2 + 1) % paths[closestId.Item1].Length].GetPos());
//            return (paths[closestId.Item1][closestId.Item2], paths[closestId.Item1][(closestId.Item2 + 1) % paths[closestId.Item1].Length], closestId.Item1, closestId.Item2);
//        }
//        catch
//        {
//            Debug.LogError("Path Finding Failed\n" + paths.Length + " " + idBefore + " " + closestId + " " + (closestId.Item1, (closestId.Item2 + 1) % paths[closestId.Item1].Length));
//            return (paths[closestId.Item1][closestId.Item2], paths[closestId.Item1][closestId.Item2], closestId.Item1, closestId.Item2);
//        }

//    }

//    public (Point, Point, int, int) GetClosestLine(Vector3 position, int segment, Quaternion orientation, int pathId = -1)
//    {
//        if (orientation * Vector3.forward == Vector3.zero)
//        {
//            throw new System.Exception("forward Vector must be non-zero");
//        }

//        //iterate through every element of the array (slow)
//        //if segment is -1, check every segment
//        (int, int) closestId;
//        if (segment >= 0)
//        {
//            closestId = (0, segments[segment]);
//        }
//        else
//        {
//            closestId = (0, 0);
//        }

//        if (pathId < 1)
//        {
//            for (int i = segment > 0 ? segments[segment] : 0; i < paths[0].Length; i++)
//            {
//                if (segment >= 0 && segment < segments.Length - 1 && i >= segments[segment + 1])
//                    break;

//                if ((paths[0][i].GetPos() - position).magnitude < (paths[closestId.Item1][closestId.Item2].GetPos() - position).magnitude)
//                {
//                    closestId = (0, i);
//                }
//            }
//        }

//        if (pathId < 0)
//        {
//            for (int e = 1; e < paths.Length; e++)
//            {
//                for (int i = 0; i < paths[e].Length; i++)
//                {
//                    if ((paths[e][i].GetPos() - position).magnitude < (paths[closestId.Item1][closestId.Item2].GetPos() - position).magnitude)
//                    {
//                        closestId = (e, i);
//                    }
//                }
//            }
//        }

//        if (pathId > 0)
//        {
//            for (int i = 0; i < paths[pathId].Length; i++)
//            {
//                if ((paths[pathId][i].GetPos() - position).magnitude < (paths[closestId.Item1][closestId.Item2].GetPos() - position).magnitude)
//                {
//                    closestId = (pathId, i);
//                }
//            }
//        }


//        (int, int) idBefore = (closestId.Item1, closestId.Item2 - 1);
//        if (idBefore.Item2 < 0)
//        {
//            idBefore.Item2 = paths[closestId.Item1].Length + idBefore.Item2;
//        }
//        //having found the closest point, check whether the one infront of behind is closer
//        try
//        {
//            int count = 0;
//            Plane plane1;
//            Plane plane2;
//            float distance1;
//            float distance2;

//            //Vector3.ProjectOnPlane(paths[idBefore.Item1][idBefore.Item2].GetPos() - position, paths[closestId.Item1][closestId.Item2].GetNormal()).magnitude < Vector3.ProjectOnPlane(paths[closestId.Item1][(closestId.Item2 + 1) % paths[closestId.Item1].Length].GetPos() - position, paths[closestId.Item1][closestId.Item2].GetNormal()).magnitude

//            while (count < 8)
//            {
//                plane1 = new Plane(paths[idBefore.Item1][idBefore.Item2].GetDir(), paths[idBefore.Item1][idBefore.Item2].GetPos());
//                plane2 = new Plane(paths[closestId.Item1][closestId.Item2].GetDir(), paths[closestId.Item1][closestId.Item2].GetPos());

//                distance1 = plane1.GetDistanceToPoint(position);
//                distance2 = plane2.GetDistanceToPoint(position);

//                if (distance1 > 0 && distance2 < 0)
//                {
//                    Debug.DrawLine(paths[idBefore.Item1][idBefore.Item2].GetPos(), paths[closestId.Item1][closestId.Item2].GetPos());
//                    return (paths[idBefore.Item1][idBefore.Item2], paths[closestId.Item1][closestId.Item2], idBefore.Item1, idBefore.Item2);
//                }
//                else
//                {
//                    closestId = (closestId.Item1, Wrap(closestId.Item2 + 1, 0, paths[closestId.Item1].Length - 1));
//                    idBefore = (closestId.Item1, closestId.Item2 - 1);
//                    if (idBefore.Item2 < 0)
//                    {
//                        idBefore.Item2 = paths[closestId.Item1].Length + idBefore.Item2;
//                    }
//                    count++;
//                }
//            }

//            //Debug.DrawLine(paths[idBefore.Item1][idBefore.Item2].GetPos(), paths[closestId.Item1][closestId.Item2].GetPos(), Color.red, Time.deltaTime);

//            Debug.DrawLine(paths[closestId.Item1][closestId.Item2].GetPos(), paths[closestId.Item1][(closestId.Item2 + 1) % paths[closestId.Item1].Length].GetPos());
//            return (paths[closestId.Item1][closestId.Item2], paths[closestId.Item1][(closestId.Item2 + 1) % paths[closestId.Item1].Length], closestId.Item1, closestId.Item2);
//        }
//        catch
//        {
//            Debug.Log(paths.Length + " " + idBefore + " " + closestId + " " + (closestId.Item1, (closestId.Item2 + 1) % paths[closestId.Item1].Length));
//            return (paths[closestId.Item1][closestId.Item2], paths[closestId.Item1][closestId.Item2], closestId.Item1, closestId.Item2);
//        }

//    }

//    //public (Point, Point, int, int) GetClosestLine(Vector3 position, int start, int priorityWindow = 10)
//    //{
//    //    //iterate through every element of the array (slow)
//    //    int result1 = start;
//    //    int result2 = start;
//    //    int closestId = start;

//    //    bool looped = false;
//    //    int i = start;
//    //    while (i < paths.Length && (!looped && i < start + priorityWindow / 2) || (looped && i + paths.Length - 1 < start + priorityWindow / 2))
//    //    {
//    //        if ((paths[i].GetPos() - position).magnitude < (paths[result1].GetPos() - position).magnitude)
//    //        {
//    //            result1 = i;
//    //        }
//    //        if (++i >= paths.Length)
//    //        {
//    //            i = i % paths.Length;
//    //            looped = true;
//    //        }
//    //    }

//    //    looped = false;
//    //    i = start;
//    //    while (i >= 0 && (!looped && i > start - priorityWindow / 2) || (looped && i - paths.Length > start - priorityWindow / 2))
//    //    {
//    //        if ((paths[i].GetPos() - position).magnitude < (paths[result2].GetPos() - position).magnitude)
//    //        {
//    //            result2 = i;
//    //        }

//    //        if (--i < 0)
//    //        {
//    //            i = i + paths.Length;
//    //            looped = true;
//    //        }
//    //    }

//    //    if ((paths[result1].GetPos() - position).magnitude < paths[result1].GetWidth()
//    //        && (paths[result2].GetPos() - position).magnitude < paths[result2].GetWidth())
//    //    {
//    //        if ((paths[result1].GetPos() - position).magnitude < (paths[result2].GetPos() - position).magnitude)
//    //        {
//    //            closestId = result1;
//    //        }
//    //        else if ((paths[result2].GetPos() - position).magnitude < (paths[result1].GetPos() - position).magnitude)
//    //        {
//    //            closestId = result2;
//    //        }
//    //    }
//    //    else if ((paths[result1].GetPos() - position).magnitude < paths[result1].GetWidth())
//    //    {
//    //        closestId = result1;
//    //    }
//    //    else if ((paths[result2].GetPos() - position).magnitude < paths[result2].GetWidth())
//    //    {
//    //        closestId = result2;
//    //    }
//    //    else
//    //    {
//    //        Debug.Log("Checking rest of the list");

//    //        //find the correct segment
//    //        for (int s = segments.Length - 1; i >= 0; i--)
//    //        {
//    //            if (start >= segments[s])
//    //            {
//    //                return GetClosestLine(position, s);
//    //            }
//    //        }
//    //        return GetClosestLine(position, 0);
//    //    }

//    //    int idBefore = closestId - 1;
//    //    if (idBefore < 0)
//    //    {
//    //        idBefore = paths.Length + idBefore;
//    //    }
//    //    //having found the closest point, check whether the one infront of behind is closer
//    //    try
//    //    {
//    //        if ((paths[idBefore].GetPos() - position).magnitude < (paths[(closestId + 1) % paths.Length].GetPos() - position).magnitude)
//    //        {
//    //            Debug.DrawLine(paths[idBefore].GetPos(), paths[closestId].GetPos());
//    //            return (paths[idBefore], paths[closestId], idBefore);
//    //        }
//    //        else
//    //        {
//    //            Debug.DrawLine(paths[closestId].GetPos(), paths[(closestId + 1) % paths.Length].GetPos());
//    //            return (paths[closestId], paths[(closestId + 1) % paths.Length], closestId);
//    //        }
//    //    }
//    //    catch
//    //    {
//    //        Debug.Log(paths.Length + " " + idBefore + " " + closestId + " " + ((closestId + 1) % paths.Length));
//    //        return (paths[closestId], paths[closestId], closestId);
//    //    }

//    //}

//    public void ScalePath(float scale, Vector3 origin)
//    {
//        for (int i = 0; i < paths.Length; i++)
//        {
//            for (int e = 0; e < paths[i].Length; e++)
//            {
//                paths[i][e].SetPos((paths[i][e].GetPos() - origin) * scale + origin);
//                paths[i][e].SetWidth(paths[i][e].GetWidth() * scale);
//            }
//        }
//    }

//    public void TransformPath(Vector3 transform)
//    {
//        for (int i = 0; i < paths.Length; i++)
//        {
//            for (int e = 0; e < paths[i].Length; e++)
//            {
//                paths[i][e].SetPos(paths[i][e].GetPos() + transform);
//            }
//        }
//    }

//    public void PrintPath()
//    {
//        string output = "AIPath: \n";
//        for(int e = 0; e < paths.Length; e++)
//        {
//            for (int i = 0; i < paths[e].Length; i++)
//            {
//                output += "pos: " + paths[e][i].GetPos() + ",\t dir: " + paths[e][i].GetDir() + "\n";

//                //visualize the path
//                Debug.DrawRay(paths[e][i].GetPos(), paths[e][i].GetDir(), Color.red, 60f);
//                Debug.DrawRay(paths[e][i].GetPos(), paths[e][i].GetNormal(), Color.blue, 60f);
//                Debug.DrawLine(paths[e][i].GetPos() + Vector3.Cross(paths[e][i].GetDir(), paths[e][i].GetNormal()).normalized * (paths[e][i].GetWidth() / 2),
//                    paths[e][i].GetPos() - Vector3.Cross(paths[e][i].GetDir(), paths[e][i].GetNormal()).normalized * (paths[e][i].GetWidth() / 2),
//                    Color.green, 60f);
//                if (i > 0)
//                {
//                    Debug.DrawLine(paths[e][i - 1].GetPos(), paths[e][i].GetPos(), Color.white, 60f);

//                }
//                if (i == paths[e].Length - 1 && e == 0)
//                {
//                    Debug.DrawLine(paths[e][0].GetPos(), paths[e][paths.Length - 1].GetPos(), Color.white, 60f);
//                }
//            }
//            output += "\n\n";
//        }


//            output += "\n\n";
//        Debug.Log(output);
//    }

//    private int Wrap(int num, int min, int max)
//    {
//        int result = num;
//        if (result > max)
//        {
//            result -= max;
//        }
//        else if (result < min)
//        {
//            result += min;
//        }
//        return result;
//    }
//}

//a path as a graph
[System.Serializable]
public class AIPath
{
    [System.Serializable]
    public class Point
    {
        private int id;
        private float[] pos;
        private float[] dir;
        private float[] normal;
        private float width;
        private int segment;
        private bool allowRespawn;
        private int nextEdgeCount;
        private int prevEdgeCount;
        private int[] nextPointIds;
        private int[] previousPointIds;

        public Point()
        {
            id = -1;
            pos = new float[3];
            dir = new float[3];
            normal = new float[3];
            width = 10;
            nextPointIds = new int[3];
        }

        public Point(int id, Vector3 position, Vector3 direction, Vector3 nor) : this(id, position, direction, nor, 0, true, 10)
        {
        }

        public Point(int id, Vector3 position, Vector3 direction, Vector3 nor, float width = 10) : this(id, position, direction, nor, 0, true, width)
        {
        }

        public Point(int id, Vector3 position, Vector3 direction, Vector3 nor, int segment, float width = 10) : this(id, position, direction, nor, segment, true, width)
        {
        }

        public Point(int id, Vector3 position, Vector3 direction, Vector3 nor, bool allowRespawn, float width = 10) : this(id, position, direction, nor, 0, allowRespawn, width)
        {
        }

        public Point(int id, Vector3 position, Vector3 direction, Vector3 nor, int segment, bool allowRespawn, float width = 10)
        {
            this.id = id;
            
            pos = new float[3];
            pos[0] = position.x;
            pos[1] = position.y;
            pos[2] = position.z;

            dir = new float[3];
            dir[0] = direction.x;
            dir[1] = direction.y;
            dir[2] = direction.z;

            normal = new float[3];
            normal[0] = nor[0];
            normal[1] = nor[1];
            normal[2] = nor[2];

            this.width = width;
            this.segment = segment;
            this.allowRespawn = allowRespawn;
            nextEdgeCount = 0;
            prevEdgeCount = 0;
            nextPointIds = new int[3];
            previousPointIds = new int[3];
        }
        public int GetId()
        {
            return id;
        }

        public Vector3 GetPos()
        {
            return new Vector3(pos[0], pos[1], pos[2]);
        }

        public Vector3 GetDir()
        {
            return new Vector3(dir[0], dir[1], dir[2]);
        }

        public Vector3 GetNormal()
        {
            return new Vector3(normal[0], normal[1], normal[2]);
        }

        public float GetWidth()
        {
            return width;
        }

        public int GetSegment()
        {
            return segment;
        }

        public bool IsRespawnAllowed()
        {
            return allowRespawn;
        }

        public int[] GetNextPoints()
        {
            return nextPointIds;
        }

        public int[] GetPrevPoints()
        {
            return previousPointIds;
        }

        public int GetNextEdgeCount()
        {
            return nextEdgeCount;
        }
        public int GetPrevEdgeCount()
        {
            return prevEdgeCount;
        }

        public void SetPos(Vector3 newPos)
        {
            pos[0] = newPos.x;
            pos[1] = newPos.y;
            pos[2] = newPos.z;
        }

        public void SetDir(Vector3 newDir)
        {
            dir[0] = newDir.x;
            dir[1] = newDir.y;
            dir[2] = newDir.z;
        }

        public void SetNormal(Vector3 newNormal)
        {
            normal[0] = newNormal.x;
            normal[1] = newNormal.y;
            normal[2] = newNormal.z;
        }

        public void SetWidth(float newWidth)
        {
            width = newWidth;
        }

        public void AddNextEdge(Point point)
        {
            if (nextEdgeCount >= nextPointIds.Length)
            {
                Debug.LogWarning("Unable to add point: Reached Maximum amount of connected points");
                return;
            }

            nextPointIds[nextEdgeCount] = point.GetId();
            nextEdgeCount++;
        }

        public void AddNextEdge(int id)
        {
            if (nextEdgeCount >= nextPointIds.Length)
            {
                Debug.LogWarning("Unable to add point: Reached Maximum amount of connected points");
                return;
            }

            nextPointIds[nextEdgeCount] = id;
            nextEdgeCount++;
        }

        public void AddPrevEdge(Point point)
        {
            if (prevEdgeCount >= previousPointIds.Length)
            {
                Debug.LogWarning("Unable to add point: Reached Maximum amount of connected points");
                return;
            }

            previousPointIds[nextEdgeCount] = point.GetId();
            prevEdgeCount++;
        }

        public void AddPrevEdge(int id)
        {
            if (prevEdgeCount >= previousPointIds.Length)
            {
                Debug.LogWarning("Unable to add point: Reached Maximum amount of connected points");
                return;
            }

            previousPointIds[nextEdgeCount] = id;
            prevEdgeCount++;
        }

        public void Print()
        {
            Debug.Log("id: " + GetId() + "pos: " + GetPos() + ",\t dir: " + GetDir() + "\n");
        }
    }

    Point[] allPoints;
    int maxDepth;

    public AIPath()
    {
        allPoints = new Point[0];
        maxDepth = 0;
    }

    public AIPath(Point[] points)
    {
        if (points.Length > 0)
        {
            allPoints = points;
            CalculateMaxDepth();
        }
        else
        {
            throw new System.Exception("Path must have at least one point");
        }

    }

    public AIPath(List<Point> path)
    {
        this.allPoints = new Point[path.Count];
        for (int i = 0; i < allPoints.Length; i++)
        {
            allPoints[i] = path[i];
        }
    }

    public Point GetPointById(int id)
    {
        try
        {
            return allPoints[id];
        }
        catch
        {
            Debug.LogError($"Id {id} is out of bounds");
            return allPoints[0];
        }
        
    }

    public int GetMaxDepth()
    {
        return maxDepth;
    }

    public int GetSize()
    {
        return allPoints.Length;
    }

    //use not recommended unless you know what you're doing
    public Point[] GetRawPath()
    {
        return allPoints;
    }

    public void SetRawPath(Point[] points)
    {
        allPoints = points;
    }

    public void SetRawPath(List<List<Point>> points)
    {
        
    }

    /**
     * returns the closest point in the path to position
     * position: Vector3 position in world space
     */
    public int GetClosestPoint(Vector3 position, int pointId, int distance)
    {
        return GetClosestPoint(position, allPoints[pointId], distance);
    }

    public int GetClosestPoint(Vector3 position, Point point, int distance)
    {
        if (distance <= 0)
            return point.GetId();

        Point currentPoint = point;
        int[] nextIds = currentPoint.GetNextPoints();

        Point[] closestPoints = new Point[nextIds.Length];
        for (int i = 0; i < nextIds.Length; i++)
        {
            if ((allPoints[nextIds[i]].GetPos() - position).magnitude < (currentPoint.GetPos() - position).magnitude)
            {
                closestPoints[i] = allPoints[GetClosestPoint(position, nextIds[i], distance - 1)];
            }
            else
            {
                closestPoints[i] = currentPoint;
            }
        }

        Point result = currentPoint;
        for (int i = 0; i < closestPoints.Length; i++)
        {
            if ((closestPoints[i].GetPos() - position).magnitude< (result.GetPos() - position).magnitude)
            {
                result = closestPoints[i];
            }
        }

        return result.GetId();
    }

    public int GetClosestPoint(Vector3 position, int pointId, int segment, int distance)
    {
        return GetClosestPoint(position, allPoints[pointId], segment, distance);
    }

    public int GetClosestPoint(Vector3 position, Point point, int segment, int distance)
    {
        if (distance <= 0)
            return point.GetId();

        Point currentPoint = point;
        int[] nextIds = currentPoint.GetNextPoints();

        Point[] closestPoints = new Point[point.GetNextEdgeCount()];
        for (int i = 0; i < closestPoints.Length; i++)
        {
            if ((allPoints[nextIds[i]].GetPos() - position).magnitude < (currentPoint.GetPos() - position).magnitude && allPoints[nextIds[i]].GetSegment() == segment)
            {
                closestPoints[i] = allPoints[GetClosestPoint(position, nextIds[i], distance - 1)];
            }
            else
            {
                closestPoints[i] = currentPoint;
            }
        }

        Point result = currentPoint;
        for (int i = 0; i < closestPoints.Length; i++)
        {
            if ((closestPoints[i].GetPos() - position).magnitude < (result.GetPos() - position).magnitude)
            {
                result = closestPoints[i];
            }
        }

        return result.GetId();
    }

    public (int , int) GetClosestLine(Vector3 position)
    {
        var visited = new HashSet<(int, int)>();
        var queue = new Queue<int>();
        (int, int) closestEdge = (-1, -1);

        queue.Enqueue(0);

        int[] nextPoints;
        int[] prevPoints;

        while (queue.Count > 0)
        {
            var vertex = queue.Dequeue();

            nextPoints = allPoints[vertex].GetNextPoints();
            prevPoints = allPoints[vertex].GetPrevPoints();
            for (int i = 0; i < allPoints[vertex].GetNextEdgeCount(); i++)
            {
                if (!visited.Contains((vertex, nextPoints[i])))
                {
                    queue.Enqueue(nextPoints[i]);

                    if (closestEdge.Item1 == -1)
                    {
                        closestEdge = (vertex, nextPoints[i]);
                    }
                    else
                    {
                        float distance1 = (((allPoints[vertex].GetPos() + allPoints[nextPoints[i]].GetPos()) * 0.5f) - position).magnitude;
                        float distance2 = (((allPoints[closestEdge.Item1].GetPos() + allPoints[closestEdge.Item2].GetPos()) * 0.5f) - position).magnitude;

                        if (distance1 < distance2)
                            closestEdge = (vertex, nextPoints[i]);
                    }

                    visited.Add((vertex, nextPoints[i]));
                }
            }

            for (int i = 0; i < allPoints[vertex].GetPrevEdgeCount(); i++)
            {
                if (!visited.Contains((prevPoints[i], vertex)))
                {
                    queue.Enqueue(prevPoints[i]);

                    if (closestEdge.Item1 == -1)
                    {
                        closestEdge = (prevPoints[i], vertex);
                    }
                    else
                    {
                        float distance1 = (((allPoints[vertex].GetPos() + allPoints[prevPoints[i]].GetPos()) * 0.5f) - position).magnitude;
                        float distance2 = (((allPoints[closestEdge.Item1].GetPos() + allPoints[closestEdge.Item2].GetPos()) * 0.5f) - position).magnitude;

                        if (distance1 < distance2)
                            closestEdge = (prevPoints[i], vertex);
                    }

                    visited.Add((prevPoints[i], vertex));
                }
            }
        }

        return closestEdge;
    }

    public (int, int) GetClosestLine(Vector3 position, Point point, int distance, bool countPrevious = false)
    {

        //BFS Style
        //edge case: no edges
        if (point.GetNextEdgeCount() <= 0 && point.GetPrevEdgeCount() <= 0)
        {
            Debug.LogError("Point " + point.GetId() + " has no edges");
            return (point.GetId(), point.GetId());
        }
        
        var visited = new HashSet<(int, int)>();
        var queue = new Queue<int>();

        (int, int) closestEdge = (-1, -1);

        queue.Enqueue(point.GetId());

        int[] nextPoints;
        int[] prevPoints;
        while (queue.Count > 0 && distance > 0)
        {
            var vertex = queue.Dequeue();

            nextPoints = allPoints[vertex].GetNextPoints();
            prevPoints = allPoints[vertex].GetPrevPoints();
            for (int i = 0; i < allPoints[vertex].GetNextEdgeCount(); i++)
            {
                if (!visited.Contains((vertex, nextPoints[i])))
                {
                    queue.Enqueue(nextPoints[i]);

                    if (closestEdge.Item1 == -1)
                    {
                        closestEdge = (vertex, nextPoints[i]);
                    }
                    else
                    {
                        float distance1 = (((allPoints[vertex].GetPos() + allPoints[nextPoints[i]].GetPos()) * 0.5f) - position).magnitude;
                        float distance2 = (((allPoints[closestEdge.Item1].GetPos() + allPoints[closestEdge.Item2].GetPos()) * 0.5f) - position).magnitude;

                        if (distance1 < distance2)
                            closestEdge = (vertex, nextPoints[i]);
                    }

                    ////check to see if the point is within bounds. if so, instantly return this point
                    //Plane plane1;
                    //Plane plane2;
                    //float planeDist1;
                    //float planeDist2;

                    //plane1 = new Plane(GetPointById(vertex).GetDir(), GetPointById(vertex).GetPos());
                    //plane2 = new Plane(GetPointById(nextPoints[i]).GetDir(), GetPointById(nextPoints[i]).GetPos());

                    //planeDist1 = plane1.GetDistanceToPoint(position);
                    //planeDist2 = plane2.GetDistanceToPoint(position);

                    //if (planeDist1 > 0 && planeDist2 < 0)
                    //{
                    //    return (vertex, nextPoints[i]);
                    //}

                    visited.Add((vertex, nextPoints[i]));
                }
            }

            if (countPrevious)
            {
                for (int i = 0; i < allPoints[vertex].GetPrevEdgeCount(); i++)
                {
                    if (!visited.Contains((prevPoints[i], vertex)))
                    {
                        queue.Enqueue(prevPoints[i]);

                        if (closestEdge.Item1 == -1)
                        {
                            closestEdge = (prevPoints[i], vertex);
                        }
                        else
                        {
                            float distance1 = (((allPoints[vertex].GetPos() + allPoints[prevPoints[i]].GetPos()) * 0.5f) - position).magnitude;
                            float distance2 = (((allPoints[closestEdge.Item1].GetPos() + allPoints[closestEdge.Item2].GetPos()) * 0.5f) - position).magnitude;

                            if (distance1 < distance2)
                                closestEdge = (prevPoints[i], vertex);
                        }

                        ////check to see if the point is within bounds. if so, instantly return this point
                        //Plane plane1;
                        //Plane plane2;
                        //float planeDist1;
                        //float planeDist2;

                        //plane1 = new Plane(GetPointById(prevPoints[i]).GetDir(), GetPointById(prevPoints[i]).GetPos());
                        //plane2 = new Plane(GetPointById(vertex).GetDir(), GetPointById(vertex).GetPos());

                        //planeDist1 = plane1.GetDistanceToPoint(position);
                        //planeDist2 = plane2.GetDistanceToPoint(position);

                        //if (planeDist1 > 0 && planeDist2 < 0)
                        //{
                        //    return (prevPoints[i], vertex);
                        //}

                        visited.Add((prevPoints[i], vertex));
                    }
                }
            }

            distance--;
        }

        return closestEdge;
    }

    public (int, int) GetClosestLine(Vector3 position, Point point, Quaternion orientation, int distance, bool countPrevious = false)
    {
        //BFS Style
        //edge case: no edges
        if (point.GetNextEdgeCount() <= 0)
        {
            Debug.LogError("Point " + point.GetId() + "has no edges");
            return (point.GetId(), point.GetId());
        }

        var visited = new HashSet<(int, int)>();
        var queue = new Queue<int>();

        (int, int) closestEdge = (-1, -1);

        queue.Enqueue(point.GetId());

        int[] nextPoints;
        int[] prevPoints;

        while (queue.Count > 0 && distance > 0)
        {
            var vertex = queue.Dequeue();

            nextPoints = allPoints[vertex].GetNextPoints();
            prevPoints = allPoints[vertex].GetPrevPoints();
            for (int i = 0; i < allPoints[vertex].GetNextEdgeCount(); i++)
            {
                if (!visited.Contains((vertex, nextPoints[i])))
                {
                    queue.Enqueue(nextPoints[i]);

                    if (closestEdge.Item1 == -1)
                    {
                        closestEdge = (vertex, nextPoints[i]);
                    }
                    else
                    {
                        float distance1 = Vector3.ProjectOnPlane(((allPoints[vertex].GetPos() + allPoints[nextPoints[i]].GetPos()) * 0.5f) - position, orientation * Vector3.up).magnitude;
                        float distance2 = Vector3.ProjectOnPlane(((allPoints[closestEdge.Item1].GetPos() + allPoints[closestEdge.Item2].GetPos()) * 0.5f) - position, orientation * Vector3.up).magnitude;

                        if (distance1 < distance2)
                            closestEdge = (vertex, nextPoints[i]);
                    }

                    ////check to see if the point is within bounds. if so, instantly return this point
                    //Plane plane1;
                    //Plane plane2;
                    //float planeDist1;
                    //float planeDist2;

                    //plane1 = new Plane(GetPointById(vertex).GetDir(), GetPointById(vertex).GetPos());
                    //plane2 = new Plane(GetPointById(nextPoints[i]).GetDir(), GetPointById(nextPoints[i]).GetPos());

                    //planeDist1 = plane1.GetDistanceToPoint(position);
                    //planeDist2 = plane2.GetDistanceToPoint(position);

                    //if (planeDist1 > 0 && planeDist2 < 0)
                    //{
                    //    return (vertex, nextPoints[i]);
                    //}

                    visited.Add((vertex, nextPoints[i]));
                }
            }

            if (countPrevious)
            {
                for (int i = 0; i < allPoints[vertex].GetPrevEdgeCount(); i++)
                {
                    if (!visited.Contains((prevPoints[i], vertex)))
                    {
                        queue.Enqueue(prevPoints[i]);

                        if (closestEdge.Item1 == -1)
                        {
                            closestEdge = (prevPoints[i], vertex);
                        }
                        else
                        {
                            float distance1 = Vector3.ProjectOnPlane(((allPoints[vertex].GetPos() + allPoints[prevPoints[i]].GetPos()) * 0.5f) - position, orientation * Vector3.up).magnitude;
                            float distance2 = Vector3.ProjectOnPlane(((allPoints[closestEdge.Item1].GetPos() + allPoints[closestEdge.Item2].GetPos()) * 0.5f) - position, orientation * Vector3.up).magnitude;

                            if (distance1 < distance2)
                                closestEdge = (prevPoints[i], vertex);
                        }

                        ////check to see if the point is within bounds. if so, instantly return this point
                        //Plane plane1;
                        //Plane plane2;
                        //float planeDist1;
                        //float planeDist2;

                        //plane1 = new Plane(GetPointById(prevPoints[i]).GetDir(), GetPointById(prevPoints[i]).GetPos());
                        //plane2 = new Plane(GetPointById(vertex).GetDir(), GetPointById(vertex).GetPos());

                        //planeDist1 = plane1.GetDistanceToPoint(position);
                        //planeDist2 = plane2.GetDistanceToPoint(position);

                        //if (planeDist1 > 0 && planeDist2 < 0)
                        //{
                        //    return (prevPoints[i], vertex);
                        //}

                        visited.Add((prevPoints[i], vertex));
                    }
                }
            }

            distance--;
        }

        return closestEdge;
    }

    public (int, int) GetClosestLine(Vector3 position, Point point, int segment, int distance, bool countPrevious = false)
    {

        //BFS Style
        //edge case: no edges
        if (point.GetNextEdgeCount() <= 0)
        {
            Debug.LogError("Point " + point.GetId() + "has no edges");
            return (point.GetId(), point.GetId());
        }

        var visited = new HashSet<(int, int)>();
        var queue = new Queue<int>();

        (int, int) closestEdge = (-1, -1);

        queue.Enqueue(point.GetId());

        while (queue.Count > 0 && distance > 0)
        {
            var vertex = queue.Dequeue();

            int[] nextPoints = allPoints[vertex].GetNextPoints();
            int[] prevPoints = allPoints[vertex].GetPrevPoints();
            for (int i = 0; i < allPoints[vertex].GetNextEdgeCount(); i++)
            {
                if (!visited.Contains((vertex, nextPoints[i])))
                {
                    queue.Enqueue(nextPoints[i]);

                    if (allPoints[vertex].GetSegment() == segment)
                    {
                        if (closestEdge.Item1 == -1)
                        {
                            closestEdge = (vertex, nextPoints[i]);
                        }
                        else
                        {
                            float distance1 = (((allPoints[vertex].GetPos() + allPoints[nextPoints[i]].GetPos()) * 0.5f) - position).magnitude;
                            float distance2 = (((allPoints[closestEdge.Item1].GetPos() + allPoints[closestEdge.Item2].GetPos()) * 0.5f) - position).magnitude;

                            if (distance1 < distance2)
                                closestEdge = (vertex, nextPoints[i]);
                        }
                    }

                    ////check to see if the point is within bounds. if so, instantly return this point
                    //Plane plane1;
                    //Plane plane2;
                    //float planeDist1;
                    //float planeDist2;

                    //plane1 = new Plane(GetPointById(vertex).GetDir(), GetPointById(vertex).GetPos());
                    //plane2 = new Plane(GetPointById(nextPoints[i]).GetDir(), GetPointById(nextPoints[i]).GetPos());

                    //planeDist1 = plane1.GetDistanceToPoint(position);
                    //planeDist2 = plane2.GetDistanceToPoint(position);

                    //if (planeDist1 > 0 && planeDist2 < 0)
                    //{
                    //    return (vertex, nextPoints[i]);
                    //}

                    visited.Add((vertex, nextPoints[i]));
                }
            }

            if (countPrevious)
            {
                for (int i = 0; i < allPoints[vertex].GetPrevEdgeCount(); i++)
                {
                    if (!visited.Contains((prevPoints[i], vertex)))
                    {
                        queue.Enqueue(prevPoints[i]);

                        if (allPoints[vertex].GetSegment() == segment)
                        {
                            if (closestEdge.Item1 == -1)
                            {
                                closestEdge = (prevPoints[i], vertex);
                            }
                            else
                            {
                                float distance1 = (((allPoints[vertex].GetPos() + allPoints[prevPoints[i]].GetPos()) * 0.5f) - position).magnitude;
                                float distance2 = (((allPoints[closestEdge.Item1].GetPos() + allPoints[closestEdge.Item2].GetPos()) * 0.5f) - position).magnitude;

                                if (distance1 < distance2)
                                    closestEdge = (prevPoints[i], vertex);
                            }
                        }

                        ////check to see if the point is within bounds. if so, instantly return this point
                        //Plane plane1;
                        //Plane plane2;
                        //float planeDist1;
                        //float planeDist2;

                        //plane1 = new Plane(GetPointById(prevPoints[i]).GetDir(), GetPointById(prevPoints[i]).GetPos());
                        //plane2 = new Plane(GetPointById(vertex).GetDir(), GetPointById(vertex).GetPos());

                        //planeDist1 = plane1.GetDistanceToPoint(position);
                        //planeDist2 = plane2.GetDistanceToPoint(position);

                        //if (planeDist1 > 0 && planeDist2 < 0)
                        //{
                        //    return (prevPoints[i], vertex);
                        //}

                        visited.Add((prevPoints[i], vertex));
                    }
                }
            }

            distance--;
        }

        return closestEdge;
    }

    public (int, int) GetClosestLine(Vector3 position, Point point, int segment, Quaternion orientation, int distance, bool countPrevious = false)
    {

        //BFS Style
        //edge case: no edges
        if (point.GetNextEdgeCount() <= 0)
        {
            Debug.LogError("Point " + point.GetId() + "has no edges");
            return (point.GetId(), point.GetId());
        }

        var visited = new HashSet<(int, int)>();
        var queue = new Queue<int>();

        (int, int) closestEdge = (-1, -1);

        queue.Enqueue(point.GetId());

        while (queue.Count > 0 && distance > 0)
        {
            var vertex = queue.Dequeue();

            int[] nextPoints = allPoints[vertex].GetNextPoints();
            int[] prevPoints = allPoints[vertex].GetPrevPoints();
            for (int i = 0; i < allPoints[vertex].GetNextEdgeCount(); i++)
            {
                if (!visited.Contains((vertex, nextPoints[i])))
                {
                    queue.Enqueue(nextPoints[i]);

                    if (allPoints[vertex].GetSegment() == segment)
                    {
                        if (closestEdge.Item1 == -1)
                        {
                            closestEdge = (vertex, nextPoints[i]);
                        }
                        else
                        {
                            float distance1 = Vector3.ProjectOnPlane(((allPoints[vertex].GetPos() + allPoints[nextPoints[i]].GetPos()) * 0.5f) - position, orientation * Vector3.up).magnitude;
                            float distance2 = Vector3.ProjectOnPlane(((allPoints[closestEdge.Item1].GetPos() + allPoints[closestEdge.Item2].GetPos()) * 0.5f) - position, orientation * Vector3.up).magnitude;

                            if (distance1 < distance2)
                                closestEdge = (vertex, nextPoints[i]);
                        }
                    }

                    ////check to see if the point is within bounds. if so, instantly return this point
                    //Plane plane1;
                    //Plane plane2;
                    //float planeDist1;
                    //float planeDist2;

                    //plane1 = new Plane(GetPointById(vertex).GetDir(), GetPointById(vertex).GetPos());
                    //plane2 = new Plane(GetPointById(nextPoints[i]).GetDir(), GetPointById(nextPoints[i]).GetPos());

                    //planeDist1 = plane1.GetDistanceToPoint(position);
                    //planeDist2 = plane2.GetDistanceToPoint(position);

                    //if (planeDist1 > 0 && planeDist2 < 0)
                    //{
                    //    return (vertex, nextPoints[i]);
                    //}

                    visited.Add((vertex, nextPoints[i]));
                }
            }

            if (countPrevious)
            {
                for (int i = 0; i < allPoints[vertex].GetNextEdgeCount(); i++)
                {
                    if (!visited.Contains((prevPoints[i], vertex)))
                    {
                        queue.Enqueue(prevPoints[i]);

                        if (allPoints[vertex].GetSegment() == segment)
                        {
                            if (closestEdge.Item1 == -1)
                            {
                                closestEdge = (prevPoints[i], vertex);
                            }
                            else
                            {
                                float distance1 = Vector3.ProjectOnPlane(((allPoints[vertex].GetPos() + allPoints[prevPoints[i]].GetPos()) * 0.5f) - position, orientation * Vector3.up).magnitude;
                                float distance2 = Vector3.ProjectOnPlane(((allPoints[closestEdge.Item1].GetPos() + allPoints[closestEdge.Item2].GetPos()) * 0.5f) - position, orientation * Vector3.up).magnitude;

                                if (distance1 < distance2)
                                    closestEdge = (prevPoints[i], vertex);
                            }
                        }

                        ////check to see if the point is within bounds. if so, instantly return this point
                        //Plane plane1;
                        //Plane plane2;
                        //float planeDist1;
                        //float planeDist2;

                        //plane1 = new Plane(GetPointById(prevPoints[i]).GetDir(), GetPointById(prevPoints[i]).GetPos());
                        //plane2 = new Plane(GetPointById(vertex).GetDir(), GetPointById(vertex).GetPos());

                        //planeDist1 = plane1.GetDistanceToPoint(position);
                        //planeDist2 = plane2.GetDistanceToPoint(position);

                        //if (planeDist1 > 0 && planeDist2 < 0)
                        //{
                        //    return (prevPoints[i], vertex);
                        //}

                        //visited.Add((vertex, nextPoints[i]));
                    }
                }
            }

            distance--;
        }

        return closestEdge;
    }

    public int CalculateMaxDepth()
    {
        var visited = new HashSet<(int, int)>();
        var queue = new Queue<int>();

        queue.Enqueue(0);

        int count = 0;
        while (queue.Count > 0)
        {
            count++;
            var vertex = queue.Dequeue();

            int[] nextPoints = allPoints[vertex].GetNextPoints();
            for (int i = 0; i < allPoints[vertex].GetNextEdgeCount(); i++)
            {
                if (!visited.Contains((vertex, nextPoints[i])))
                {
                    queue.Enqueue(nextPoints[i]);
                    visited.Add((vertex, nextPoints[i]));
                }
            }
        }

        maxDepth = count;
        return count;
    }

    public int CalculateDepth(int pointId)
    {
        var visited = new HashSet<int>();
        var queue = new Queue<int>();

        queue.Enqueue(0);

        int count = 0;
        while (queue.Count > 0)
        {
            count++;
            var vertex = queue.Dequeue();

            if (vertex == pointId)
            {
                return count;
            }

            int[] nextPoints = allPoints[vertex].GetNextPoints();
            for (int i = 0; i < allPoints[vertex].GetNextEdgeCount(); i++)
            {
                if (!visited.Contains(nextPoints[i]))
                {
                    queue.Enqueue(nextPoints[i]);
                    visited.Add(nextPoints[i]);
                }
            }
            
        }

        return count;
    }

    public void ScalePath(float scale, Vector3 origin)
    {
        for (int i = 0; i < allPoints.Length; i++)
        {
            allPoints[i].SetPos((allPoints[i].GetPos() - origin) * scale + origin);
            allPoints[i].SetWidth(allPoints[i].GetWidth() * scale);
        }
    }

    public void TransformPath(Vector3 transform)
    {
        for (int i = 0; i < allPoints.Length; i++)
        {
            allPoints[i].SetPos(allPoints[i].GetPos() + transform);
        }
    }

    public void PrintPath()
    {
        var visited = new HashSet<(int, int)>();
        var queue = new Queue<int>();

        queue.Enqueue(0);
        string output = "AIPath: \n";

        while (queue.Count > 0)
        {
            var vertex = queue.Dequeue();

            int[] nextPoints = allPoints[vertex].GetNextPoints();
            for (int i = 0; i < allPoints[vertex].GetNextEdgeCount(); i++)
            {
                if (!visited.Contains((vertex, nextPoints[i])))
                {
                    queue.Enqueue(nextPoints[i]);

                    
                    output += allPoints[vertex].GetId() + "\t|| pos: " + allPoints[vertex].GetPos() + "\t,dir: " + allPoints[vertex].GetPos() + "\t,segment: " + allPoints[vertex].GetSegment() + "\n";

                    //visualize the path
                    Debug.DrawRay(allPoints[vertex].GetPos(), allPoints[vertex].GetDir(), Color.red, 120);
                    Debug.DrawRay(allPoints[vertex].GetPos(), allPoints[vertex].GetNormal(), Color.blue, 120);
                    Debug.DrawLine(allPoints[vertex].GetPos(), allPoints[nextPoints[i]].GetPos(), Color.green, 120);

                    visited.Add((vertex, nextPoints[i]));
                }
            }
        }

        output += "\n\n";

        try
        {
            // Check if file already exists. If yes, delete it.     
            if (File.Exists(Application.dataPath + "/logs.txt"))
            {
                File.Delete(Application.dataPath + "/logs.txt");
            }

            StreamWriter stream = File.CreateText(Application.dataPath + "/logs.txt");

            stream.Write(output);
            stream.Close();
        }
        catch (System.Exception Ex)
        {
            Debug.LogError(Ex.ToString());
        }
    }
}
