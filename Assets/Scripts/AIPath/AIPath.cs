using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

namespace AI
{
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
                if ((closestPoints[i].GetPos() - position).magnitude < (result.GetPos() - position).magnitude)
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

        public (int, int) GetClosestLine(Vector3 position)
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
}
