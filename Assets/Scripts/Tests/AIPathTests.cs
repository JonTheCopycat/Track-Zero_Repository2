using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


public class AIPathTests
{
    AIPath testPath;
    [SetUp]
    public void InitializeTestPath()
    {
        List<AIPath.Point> points = new List<AIPath.Point>();
        points.Add(new AIPath.Point(0, new Vector3(0, 0, 0), Vector3.up, Vector3.forward, 1f));

        points.Add(new AIPath.Point(1, new Vector3(0, 0, 2), Vector3.up, Vector3.forward, 1f));
        points[0].AddNextEdge(points[1]);

        points.Add(new AIPath.Point(2, new Vector3(2, 0, 4), Vector3.up, new Vector3(1, 0, 0), 1f));
        points[1].AddNextEdge(points[2]);

        points.Add(new AIPath.Point(3, new Vector3(4, 0, 2), Vector3.up, new Vector3(0, 0, -1), 1f));
        points[2].AddNextEdge(points[3]);

        points.Add(new AIPath.Point(4, new Vector3(4, 0, 0), Vector3.up, new Vector3(0, 0, -1), 1, 1f));
        points[3].AddNextEdge(points[4]);

        points.Add(new AIPath.Point(5, new Vector3(4, 0, -2), Vector3.up, new Vector3(0, 0, -1), 1, 1f));
        points[4].AddNextEdge(points[5]);

        points.Add(new AIPath.Point(6, new Vector3(2, 0, -4), Vector3.up, new Vector3(-1, 0, 0), 1, 1f));
        points[5].AddNextEdge(points[6]);

        points.Add(new AIPath.Point(7, new Vector3(0, 0, -2), Vector3.up, new Vector3(0, 0, 1), 1f));
        points[6].AddNextEdge(points[7]);

        points[7].AddNextEdge(points[0]);

        testPath = new AIPath(points);
    }
    
    
    // A Test behaves as an ordinary method
    [Test]
    public void AIPathTestsSimplePasses()
    {
        Assert.That(testPath.GetPointById(1).GetPos() == new Vector3(0, 0, 2), "GetPointById + GetPos is not returning the correct value");

        (int, int) resultingLine = testPath.GetClosestLine(new Vector3(3, 0, 3));
        Assert.That(resultingLine == (2, 3), "BFS version of GetClosestLine did not return the correct value");

        resultingLine = testPath.GetClosestLine(new Vector3(3, 0, 3), testPath.GetPointById(1), 4);
        Assert.That(resultingLine == (2, 3), "DFS version of GetClosestLine did not return the correct value\nReturned " + resultingLine.ToString());

        resultingLine = testPath.GetClosestLine(new Vector3(3, 0, 3), testPath.GetPointById(6), 8);
        Assert.That(resultingLine == (2, 3), "DFS GetClosestLine Edge Case Failed: Crossing Between Zero\nReturned " + resultingLine.ToString());

        //testing segments
        resultingLine = testPath.GetClosestLine(new Vector3(3, 0, -3), testPath.GetPointById(2), 0, 5);
        Assert.That(resultingLine == (3, 4), "DFS GetClosestLine Denying Transition To Segment Failed\nReturned " + resultingLine.ToString());

        resultingLine = testPath.GetClosestLine(new Vector3(3, 0, -3), testPath.GetPointById(2), 1, 5);
        Assert.That(resultingLine == (5, 6), "DFS GetClosestLine Accepting Transition To Segment Failed\nReturned " + resultingLine.ToString());

        Debug.Log("All Tests Done");
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator AIPathTestsWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
