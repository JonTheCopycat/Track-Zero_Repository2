using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AI
{
    public class CarAI : MonoBehaviour
    {
        Rigidbody rb;
        PlayerInfo playerInfo;

        Vector3[] correctDirection = new Vector3[4];
        Vector3[] correctNormal = new Vector3[4];
        RaycastHit currentHit; //legacy
        RaycastHit noseHit;
        RaycastHit leftHit;
        RaycastHit leftHit2;
        RaycastHit rightHit;
        RaycastHit rightHit2;
        bool leftWallFound;
        bool rightWallFound;

        Vector3 leftWallLine; //this line will be assumed to start at leftHit.point
        Vector3 rightWallLine; //this line will be assumed to start at rightHit.point

        public bool scanning;
        List<AIPath.Point> pathPoints;
        int currentSegment;
        bool allowRespawn;
        public static AIPath aiPath;
        int lastPoint;
        int lastRespawnPoint;
        (int, int) pointsToJoinLater;
        bool dontConnect;
        float timestamp;
        float positionWithinId;
        bool outOfBounds = false;
        float OOBtimestamp = 0;

        public bool editingPath;
        public Vector3 scaleOrigin;
        public float scale;
        public Vector3 positionTranslate;

        byte scanMode = 0x0;

        byte S_FOLLOWME = 0x1;
        byte S_ASKING = 0x2;
        byte S_AIRSCAN = 0x4;


        bool started = false;

        public Vector3[] GetCorrectDirections()
        {
            return correctDirection;
        }

        public Vector3[] GetCorrectNormals()
        {
            return correctNormal;
        }

        public AIPath.Point GetLastRespawnPoint()
        {
            return aiPath.GetPointById(lastRespawnPoint);
        }

        public AIPath.Point GetLastPoint()
        {
            return aiPath.GetPointById(lastPoint);
        }

        public float GetPositionWithin()
        {
            return positionWithinId;
        }

        public bool isScanning()
        {
            return ((scanMode & S_ASKING) == 0 && scanning);
        }

        // Start is called before the first frame update
        void OnEnable()
        {
            rb = GetComponent<Rigidbody>();
            playerInfo = GetComponent<PlayerInfo>();
        }

        public void CustomStart()
        {
            correctDirection[0] = rb.rotation * Vector3.forward;
            correctDirection[1] = rb.rotation * Vector3.forward;
            correctDirection[2] = rb.rotation * Vector3.forward;
            correctDirection[3] = rb.rotation * Vector3.forward;




            if (scanning)
            {
                pathPoints = new List<AIPath.Point>();

                dontConnect = true;
            }
            else
            {
                //aiPath.PrintPath();
            }

            if (editingPath)
            {
                StartCoroutine(PathEditing());
            }

            timestamp = 0;
            lastPoint = 0;
            lastRespawnPoint = 0;
            currentSegment = 0;

            if (aiPath.GetSize() <= 0)
            {
                Debug.LogWarning("AiPath Failed To Load");
            }

            Debug.Log("CarAI");
            started = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (started && Time.timeScale > 0.0001f)
            {
                //handle track segments
                currentSegment = playerInfo.GetCheckpointsPassed();
                //keep track of last point to detect large leaps
                RaycastHit tempHit = new RaycastHit();

                //everything else
                RaycastHit result;
                bool isGrounded;
                if (Physics.Raycast(new Ray(rb.position, rb.rotation * -Vector3.up), out result, 1f * transform.localScale.y))
                {
                    if (result.transform.tag.Equals("KillPlane"))
                        isGrounded = false;
                    else
                        isGrounded = true;
                }
                else
                    isGrounded = false;


                //calculate the correct direction
                if (scanning || aiPath.GetSize() <= 0)
                {
                    //always scan for the walls

                    Physics.Raycast(new Ray(rb.position + rb.rotation * Vector3.up * 0.25f, rb.rotation * Vector3.forward), out noseHit, rb.velocity.magnitude);
                    Physics.Raycast(new Ray(rb.position + rb.rotation * Vector3.up * 0.25f, rb.velocity), out currentHit, rb.velocity.magnitude);

                    float offset;
                    bool goneDown = false;
                    offset = 0;
                    while (Mathf.Abs(transform.InverseTransformVector(currentHit.normal).y) > 0.2f)
                    {
                        Physics.Raycast(new Ray(rb.position + rb.rotation * Vector3.up * 0.25f, Vector3.RotateTowards(
                            Quaternion.AngleAxis(0, rb.rotation * Vector3.up) * rb.rotation * Vector3.forward,
                            rb.rotation * Vector3.up,
                            offset * Mathf.Deg2Rad, 1)), out currentHit, rb.velocity.magnitude);

                        if (Mathf.Abs(offset) > 80)
                            break;

                        if (currentHit.collider == null)
                        {
                            goneDown = true;
                            offset -= 5f;
                        }
                        else if (Mathf.Abs(transform.InverseTransformVector(currentHit.normal).y) > 0.2f)
                        {
                            if (goneDown)
                            {
                                break;
                            }
                            offset += 5f;
                        }
                    }


                    float nextShift;
                    bool surrounded = false;
                    offset = 0;
                    nextShift = 90;
                    do
                    {
                        Debug.DrawRay(rb.position + transform.up, Vector3.RotateTowards(
                            Quaternion.AngleAxis(-90, rb.rotation * Vector3.up) * rb.rotation * Vector3.forward,
                            rb.rotation * Vector3.up,
                            offset * Mathf.Deg2Rad, 1), Color.red, Time.deltaTime * 1.1f);


                        Physics.Raycast(new Ray(rb.position + transform.up * 0.75f, Vector3.RotateTowards(
                            Quaternion.AngleAxis(-90, rb.rotation * Vector3.up) * rb.rotation * Vector3.forward,
                            rb.rotation * Vector3.up,
                            offset * Mathf.Deg2Rad, 1)), out leftHit, 80);

                        Physics.Raycast(new Ray(rb.position + transform.up * 0.75f, Vector3.RotateTowards(
                            Quaternion.AngleAxis(-88, rb.rotation * Vector3.up) * rb.rotation * Vector3.forward,
                            rb.rotation * Vector3.up,
                            offset * Mathf.Deg2Rad, 1)), out leftHit2, 80);

                        if (surrounded)
                        {
                            if (transform.InverseTransformVector(leftHit.normal).y < 0.2f || transform.InverseTransformVector(leftHit2.normal).y < 0.2f)
                            {
                                offset -= nextShift;
                            }
                            else if (transform.InverseTransformVector(leftHit.normal).y > 0.2f || transform.InverseTransformVector(leftHit2.normal).y > 0.2f)
                            {

                                if (nextShift < 1)
                                {
                                    break;
                                }
                                offset += nextShift;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (leftHit.collider == null || leftHit2.collider == null)
                            {
                                offset -= nextShift;
                            }
                            else if (leftHit.collider != null && leftHit2.collider != null)
                            {
                                if (offset > 80)
                                {
                                    offset = 0;
                                    nextShift = 90;
                                    surrounded = true;
                                    continue;
                                }
                                if (nextShift < 1)
                                {
                                    break;
                                }
                                offset += nextShift;
                            }
                        }

                        nextShift = nextShift / 2;

                    } while (nextShift > 0.01f && (Mathf.Abs(transform.InverseTransformVector(leftHit.normal).y) > 0.1f || Mathf.Abs(transform.InverseTransformVector(leftHit2.normal).y) > 0.1f || leftHit.collider == null || leftHit2.collider == null));


                    offset = 0;
                    nextShift = 90;
                    do
                    {

                        Physics.Raycast(new Ray(rb.position + transform.up * 0.75f, Vector3.RotateTowards(
                            Quaternion.AngleAxis(90, rb.rotation * Vector3.up) * (rb.rotation * Vector3.forward),
                            rb.rotation * Vector3.up,
                            offset * Mathf.Deg2Rad, 1)), out rightHit, 80);

                        Physics.Raycast(new Ray(rb.position + transform.up * 0.75f, Vector3.RotateTowards(
                            Quaternion.AngleAxis(88, rb.rotation * Vector3.up) * (rb.rotation * Vector3.forward),
                            rb.rotation * Vector3.up,
                            offset * Mathf.Deg2Rad, 1)), out rightHit2, 80);

                        if (surrounded)
                        {
                            if (transform.InverseTransformVector(rightHit.normal).y < 0.2f || transform.InverseTransformVector(rightHit2.normal).y < 0.2f)
                            {
                                offset -= nextShift;
                            }
                            else if (transform.InverseTransformVector(rightHit.normal).y > 0.2f || transform.InverseTransformVector(rightHit2.normal).y > 0.2f)
                            {

                                if (nextShift < 1)
                                {
                                    break;
                                }
                                offset += nextShift;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (rightHit.collider == null || rightHit2.collider == null)
                            {
                                offset -= nextShift;
                            }
                            else if (rightHit.collider != null && rightHit2.collider != null)
                            {
                                if (offset > 80)
                                {
                                    offset = 0;
                                    nextShift = 90;
                                    surrounded = true;
                                    continue;
                                }
                                if (nextShift < 1)
                                {
                                    break;
                                }
                                offset += nextShift;
                            }
                        }
                        nextShift = nextShift / 2;
                    } while (nextShift > 0.01f && (Mathf.Abs(transform.InverseTransformVector(rightHit.normal).y) > 0.1f || Mathf.Abs(transform.InverseTransformVector(rightHit2.normal).y) > 0.1f || rightHit.collider == null || rightHit2.collider == null));


                    //find the leftWall
                    if ((leftHit.collider != null) && (leftHit2.collider != null))
                    {
                        leftWallFound = true;
                        leftWallLine = leftHit2.point - leftHit.point;
                    }
                    else
                    {
                        leftWallFound = false;
                    }

                    //find the rightWall
                    if ((rightHit.collider != null) && (rightHit2.collider != null))
                    {
                        rightWallFound = true;
                        rightWallLine = rightHit2.point - rightHit.point;
                    }
                    else
                    {
                        rightWallFound = false;
                    }

                    //calculate the correct direction
                    if (!leftWallFound && !rightWallFound)
                    {
                        correctDirection[0] = rb.velocity.normalized;

                    }
                    else if (!leftWallFound)
                    {

                        correctDirection[0] = Vector3.Project(transform.forward, rightWallLine).normalized;
                    }
                    else if (!rightWallFound)
                    {
                        correctDirection[0] = Vector3.Project(transform.forward, leftWallLine).normalized;
                    }
                    else
                    {
                        correctDirection[0] = Vector3.Project(transform.forward, leftWallLine + rightWallLine).normalized;
                    }

                    Debug.DrawRay(leftHit.point, leftWallLine, Color.red, Time.deltaTime * 2f);
                    Debug.DrawRay(rightHit.point, rightWallLine, Color.red, Time.deltaTime * 2f);
                    Debug.DrawRay(noseHit.point, noseHit.normal, Color.red, Time.deltaTime * 2f);
                    Debug.DrawRay(currentHit.point, currentHit.normal, new Color(1, 0.5f, 0.5f), Time.deltaTime * 2f);
                    Debug.DrawRay(rb.position + transform.up * 0.75f, correctDirection[0], Color.magenta, Time.deltaTime);
                }
                else
                {
                    //follow the path
                    currentSegment = playerInfo.GetCheckpointsPassed();

                    //correctDirection[0]
                    (int, int) line = aiPath.GetClosestLine(rb.position, aiPath.GetPointById(lastPoint), currentSegment, 50, true);
                    (AIPath.Point, AIPath.Point) linePoints = (aiPath.GetPointById(line.Item1), aiPath.GetPointById(line.Item2));

                    Debug.DrawLine(linePoints.Item1.GetPos(), linePoints.Item2.GetPos(), Color.white, Time.deltaTime * 2);

                    lastPoint = line.Item1;
                    if (isGrounded && aiPath.GetPointById(lastPoint).IsRespawnAllowed())
                    {
                        lastRespawnPoint = lastPoint;
                    }
                    aiPath.GetPointById(lastPoint).GetDir();
                    Debug.DrawRay(rb.position + transform.up, aiPath.GetPointById(lastPoint).GetDir(), Color.red, Time.deltaTime);

                    //Debug.Log("widths: " + line.Item1.GetWidth());
                    Vector3 testPosition = rb.position + rb.velocity.normalized * -4;
                    Plane plane1 = new Plane(linePoints.Item1.GetDir(), linePoints.Item1.GetPos());
                    Plane plane2 = new Plane(linePoints.Item2.GetDir(), linePoints.Item2.GetPos());

                    float distance1 = Mathf.Abs(plane1.GetDistanceToPoint(rb.position));
                    float distance2 = Mathf.Abs(plane2.GetDistanceToPoint(rb.position));
                    float total = distance1 + distance2;
                    positionWithinId = Mathf.Lerp(0, 1, distance1 / total);
                    AIPath.Point interpolated = new AIPath.Point(-1,
                        Vector3.Lerp(linePoints.Item1.GetPos(), linePoints.Item2.GetPos(), distance1 / total),
                        Vector3.Lerp(linePoints.Item1.GetDir(), linePoints.Item2.GetDir(), distance1 / total),
                        Vector3.Lerp(linePoints.Item1.GetNormal(), linePoints.Item2.GetNormal(), distance1 / total),
                        Mathf.Lerp(linePoints.Item1.GetWidth(), linePoints.Item2.GetWidth(), distance1 / total));

                    correctDirection[0] = interpolated.GetDir();
                    correctNormal[0] = interpolated.GetNormal();
                    Vector3 roadPos = Quaternion.Inverse(Quaternion.LookRotation(interpolated.GetDir(), interpolated.GetNormal())) * (rb.position - interpolated.GetPos());
                    //Debug.Log("RoadPos: " + roadPos);


                    //if within the bounds, proceed normally. otherwise, point to the nearest point

                    /*
                     * (line.Item1.GetPos() - rb.position).magnitude <= 50
                        || (line.Item2.GetPos() - rb.position).magnitude <= 50
                     */
                    if (Vector3.Project(roadPos, Vector3.right).magnitude < interpolated.GetWidth() / 2)
                    {
                        if (Mathf.Abs(roadPos.x) > interpolated.GetWidth() * 0.25f)
                        {
                            correctDirection[0] = Vector3.RotateTowards(correctDirection[0],
                                interpolated.GetPos() - rb.position,
                                Mathf.Deg2Rad * 2.5f, 0);
                        }

                        testPosition = MoveDistanceAlongPath(testPosition, aiPath.GetPointById(line.Item1), 8f, out line);
                        linePoints = (aiPath.GetPointById(line.Item1), aiPath.GetPointById(line.Item2));


                        plane1 = new Plane(linePoints.Item1.GetDir(), linePoints.Item1.GetPos());
                        plane2 = new Plane(linePoints.Item2.GetDir(), linePoints.Item2.GetPos());
                        distance1 = Mathf.Abs(plane1.GetDistanceToPoint(testPosition));
                        distance2 = Mathf.Abs(plane2.GetDistanceToPoint(testPosition));
                        total = distance1 + distance2;
                        interpolated = new AIPath.Point(-1,
                            Vector3.Lerp(linePoints.Item1.GetPos(), linePoints.Item2.GetPos(), distance1 / total),
                            Vector3.Lerp(linePoints.Item1.GetDir(), linePoints.Item2.GetDir(), distance1 / total),
                            Vector3.Lerp(linePoints.Item1.GetNormal(), linePoints.Item2.GetNormal(), distance1 / total),
                            Mathf.Lerp(linePoints.Item1.GetWidth(), linePoints.Item2.GetWidth(), distance1 / total));

                        correctDirection[1] = interpolated.GetDir();
                        correctNormal[1] = interpolated.GetNormal();
                        if (Mathf.Abs(roadPos.x) > interpolated.GetWidth() * 0.25f)
                        {
                            correctDirection[1] = Vector3.RotateTowards(correctDirection[1],
                                interpolated.GetPos() - rb.position,
                                Mathf.Deg2Rad * 2.5f, 0);
                        }

                        testPosition = MoveDistanceAlongPath(testPosition, aiPath.GetPointById(line.Item1), 2 + 26 * rb.velocity.magnitude / 300, out line);
                        linePoints = (aiPath.GetPointById(line.Item1), aiPath.GetPointById(line.Item2));

                        plane1 = new Plane(linePoints.Item1.GetDir(), linePoints.Item1.GetPos());
                        plane2 = new Plane(linePoints.Item2.GetDir(), linePoints.Item2.GetPos());
                        distance1 = Mathf.Abs(plane1.GetDistanceToPoint(testPosition));
                        distance2 = Mathf.Abs(plane2.GetDistanceToPoint(testPosition));
                        total = distance1 + distance2;
                        interpolated = new AIPath.Point(-1,
                            Vector3.Lerp(linePoints.Item1.GetPos(), linePoints.Item2.GetPos(), distance1 / total),
                            Vector3.Lerp(linePoints.Item1.GetDir(), linePoints.Item2.GetDir(), distance1 / total),
                            Vector3.Lerp(linePoints.Item1.GetNormal(), linePoints.Item2.GetNormal(), distance1 / total),
                            Mathf.Lerp(linePoints.Item1.GetWidth(), linePoints.Item2.GetWidth(), distance1 / total));

                        correctDirection[2] = interpolated.GetDir();
                        correctNormal[2] = interpolated.GetNormal();

                        testPosition = MoveDistanceAlongPath(testPosition, aiPath.GetPointById(line.Item1), 18f, out line);
                        linePoints = (aiPath.GetPointById(line.Item1), aiPath.GetPointById(line.Item2));

                        plane1 = new Plane(linePoints.Item1.GetDir(), linePoints.Item1.GetPos());
                        plane2 = new Plane(linePoints.Item2.GetDir(), linePoints.Item2.GetPos());
                        distance1 = Mathf.Abs(plane1.GetDistanceToPoint(testPosition));
                        distance2 = Mathf.Abs(plane2.GetDistanceToPoint(testPosition));
                        total = distance1 + distance2;
                        interpolated = new AIPath.Point(-1,
                            Vector3.Lerp(linePoints.Item1.GetPos(), linePoints.Item2.GetPos(), distance1 / total),
                            Vector3.Lerp(linePoints.Item1.GetDir(), linePoints.Item2.GetDir(), distance1 / total),
                            Vector3.Lerp(linePoints.Item1.GetNormal(), linePoints.Item2.GetNormal(), distance1 / total),
                            Mathf.Lerp(linePoints.Item1.GetWidth(), linePoints.Item2.GetWidth(), distance1 / total));

                        correctDirection[3] = interpolated.GetDir();
                        correctNormal[3] = interpolated.GetNormal();
                    }
                    else
                    {
                        //aiPath.GetRawPath()[line.Item3][Mathf.Clamp(line.Item4 + 3, 0, aiPath.GetRawPath()[line.Item3].Length - 1)].GetPos() - rb.position
                        correctDirection[0] = (linePoints.Item2.GetPos() - rb.position).normalized;

                        AIPath.Point nextPoint = aiPath.GetPointById(linePoints.Item2.GetNextPoints()[0]);
                        correctDirection[1] = (nextPoint.GetPos() - rb.position).normalized;

                        nextPoint = aiPath.GetPointById(nextPoint.GetNextPoints()[0]);
                        correctDirection[2] = (nextPoint.GetPos() - rb.position).normalized;

                        nextPoint = aiPath.GetPointById(nextPoint.GetNextPoints()[0]);
                        correctDirection[3] = (nextPoint.GetPos() - rb.position).normalized;


                    }

                    //out of bounds check
                    if (roadPos.magnitude < interpolated.GetWidth() * 4f)
                    {
                        if (outOfBounds == false)
                        {
                            outOfBounds = true;
                        }
                        OOBtimestamp += 60 * Time.timeScale;
                    }
                    else
                    {
                        outOfBounds = false;
                        OOBtimestamp = 0;
                    }

                }


                if (correctDirection[0] == Vector3.zero)
                {
                    correctDirection[0] = rb.rotation * Vector3.forward;
                }



                if (correctDirection[1] == Vector3.zero)
                {
                    correctDirection[1] = rb.rotation * Vector3.forward;
                }

                if (correctDirection[2] == Vector3.zero)
                {
                    correctDirection[2] = rb.rotation * Vector3.forward;
                }

                if (correctDirection[3] == Vector3.zero)
                {
                    correctDirection[3] = rb.rotation * Vector3.forward;
                }

                //if(!scanning)
                //{
                //    correctDirection[0] = Vector3.ProjectOnPlane(correctDirection[0], rb.rotation * Vector3.up).normalized;
                //    correctDirection[1] = Vector3.ProjectOnPlane(correctDirection[1], rb.rotation * Vector3.up).normalized;
                //    correctDirection[2] = Vector3.ProjectOnPlane(correctDirection[2], rb.rotation * Vector3.up).normalized;
                //    correctDirection[3] = Vector3.ProjectOnPlane(correctDirection[3], rb.rotation * Vector3.up).normalized;
                //}


                if (scanning)
                {

                    if (/*Input.GetKeyDown(KeyCode.Alpha3) */Keyboard.current[Key.Digit3].wasPressedThisFrame)
                    {
                        scanMode ^= S_FOLLOWME;

                        Debug.Log("Following: " + ((scanMode & S_FOLLOWME) != 0));
                    }

                    if (/*Input.GetKeyDown(KeyCode.Alpha4)*/ Keyboard.current[Key.Digit4].wasPressedThisFrame)
                    {
                        scanMode ^= S_AIRSCAN;

                        Debug.Log("Air Scan: " + ((scanMode & S_AIRSCAN) != 0));
                    }

                    if ((scanMode & S_ASKING) == 0)
                    {
                        if (/*Input.GetKeyDown(KeyCode.Alpha2)*/ Keyboard.current[Key.Digit2].wasPressedThisFrame)
                        {
                            //stop adding points
                            //find the most appropriate point to connect to
                            if (aiPath.GetSize() > 0)
                            {
                                (int, int) line = aiPath.GetClosestLine(rb.position);

                                //join the last point added to this point and vise versa
                                pathPoints[pathPoints.Count - 1].AddNextEdge(line.Item2);
                                pathPoints[line.Item2].AddPrevEdge(pathPoints.Count - 1);

                                //join the previously saved points representing the start of this path the point of the split
                                pathPoints[pointsToJoinLater.Item1].AddNextEdge(pointsToJoinLater.Item2);
                                pathPoints[pointsToJoinLater.Item2].AddPrevEdge(pointsToJoinLater.Item1);
                            }
                            else
                            {
                                pathPoints[pathPoints.Count - 1].AddNextEdge(0);
                                pathPoints[0].AddPrevEdge(pathPoints.Count - 1);
                            }


                            //store the current path
                            aiPath = new AIPath(pathPoints);

                            //go into asking state.
                            scanMode |= S_ASKING;
                            Debug.Log("Path finished.\n    S to Scan another path\tF to Finish and Save the scan");
                        }
                        //(pathPoints[pathId].Count < 1 || (pathPoints[pathId][pathPoints[pathId].Count - 1].GetPos() - rb.position).magnitude > 300)
                        if (timestamp < Time.time && /*!Input.GetKey(KeyCode.Space)*/ !Keyboard.current[Key.Space].wasPressedThisFrame)
                        {


                            if ((isGrounded || (scanMode & S_AIRSCAN) != 0)
                                && (pathPoints.Count < 1
                                || (rb.position - pathPoints[0].GetPos()).magnitude > pathPoints[0].GetWidth()))
                            {
                                timestamp = Time.time + 0.33f;

                                float width;
                                Vector3 trackCenter;
                                if ((scanMode & S_FOLLOWME) != 0)
                                {
                                    trackCenter = rb.position;
                                    width = 50;
                                }
                                else if (leftHit.collider != null && rightHit.collider != null)
                                {
                                    trackCenter = (leftHit.point + rightHit.point) / 2;
                                    width = (leftHit.point - rightHit.point).magnitude;
                                }
                                else if (leftHit.collider != null || rightHit.collider != null)
                                {
                                    trackCenter = rb.position;
                                    width = leftHit.collider != null ?
                                        (leftHit.point - rb.position).magnitude * 2f :
                                        (rightHit.point - rb.position).magnitude * 2f;
                                }
                                else
                                {
                                    trackCenter = rb.position;
                                    width = 50;
                                }

                                Vector3 savedDirection = (scanMode & S_FOLLOWME) != 0 ? rb.velocity : correctDirection[0];
                                savedDirection.Normalize();




                                pathPoints.Add(new AIPath.Point(pathPoints.Count, trackCenter, savedDirection, rb.rotation * Vector3.up, currentSegment, width));

                                if (dontConnect)
                                    dontConnect = false;
                                else
                                {
                                    pathPoints[lastPoint].AddNextEdge(pathPoints.Count - 1);
                                    pathPoints[pathPoints.Count - 1].AddPrevEdge(lastPoint);
                                }
                                lastPoint = pathPoints.Count - 1;

                                Debug.Log("Point " + (pathPoints.Count - 1) + " Added");

                            }

                        }
                        else if (pathPoints.Count > 1 && (rb.position - pathPoints[0].GetPos()).magnitude < pathPoints[0].GetWidth() && /*!Input.GetKey(KeyCode.Space)*/ !Keyboard.current[Key.Space].wasPressedThisFrame)
                        {
                            //go into asking state.
                            scanMode |= S_ASKING;
                            Debug.Log("Path finished.\n    S to Scan another path\tF to Finish and Save the scan");

                            //loop back
                            pathPoints[lastPoint].AddNextEdge(0);
                            pathPoints[0].AddPrevEdge(lastPoint);

                            //construct the current path
                            aiPath = new AIPath(pathPoints);
                        }
                    }
                    else
                    {
                        if (/*Input.GetKeyDown(KeyCode.S)*/ Keyboard.current[Key.S].wasPressedThisFrame)
                        {
                            scanMode &= (byte)~S_ASKING;

                            //find the most appropriate point to connect to
                            (int, int) line = aiPath.GetClosestLine(rb.position);

                            //indicate the appropriate point to continue from when next point is scanned
                            lastPoint = line.Item1;
                            pointsToJoinLater = (lastPoint, pathPoints.Count);
                            dontConnect = true;
                        }

                        if (/*Input.GetKeyDown(KeyCode.F)*/ Keyboard.current[Key.F].wasPressedThisFrame)
                        {
                            scanMode = 0x0;

                            scanning = false;
                            aiPath = new AIPath(pathPoints);
                            aiPath.PrintPath();
                            Debug.Log("Finished Scanning");

                            PathDataManager.SaveAIPath(ScreenManager.current.GetSceneName(), aiPath);
                        }
                    }
                }
            }
        }

        //other functions

        public Vector3 MoveDistanceAlongPath(Vector3 initialPos, AIPath.Point initialPoint, float distance, out (int, int) outputLine)
        {
            Vector3 testPosition = initialPos;
            (int, int) line = aiPath.GetClosestLine(initialPos, initialPoint, 5);
            (AIPath.Point, AIPath.Point) linePoints = (aiPath.GetPointById(line.Item1), aiPath.GetPointById(line.Item2));

            Plane plane = new Plane(linePoints.Item2.GetDir(), linePoints.Item2.GetPos());
            float d = Mathf.Abs(plane.GetDistanceToPoint(initialPos));

            int count = 0;
            while (distance > 0)
            {
                if (count >= 100)
                {
                    Debug.LogError("Infinite Loop Detected");
                    break;
                }

                if (d <= distance && count < 100)
                {
                    testPosition = plane.ClosestPointOnPlane(testPosition + linePoints.Item1.GetDir() * d * 0.5f + linePoints.Item2.GetDir() * d * 0.5f);
                    line = aiPath.GetClosestLine(testPosition, linePoints.Item2, 2);
                    linePoints = (aiPath.GetPointById(line.Item1), aiPath.GetPointById(line.Item2));
                    distance -= d;

                    plane = new Plane(linePoints.Item2.GetDir(), linePoints.Item2.GetPos());
                    d = Mathf.Abs(plane.GetDistanceToPoint(testPosition));

                }
                else
                {
                    testPosition = plane.ClosestPointOnPlane(testPosition);
                    testPosition = testPosition + linePoints.Item2.GetDir().normalized * distance;
                    line = aiPath.GetClosestLine(testPosition, linePoints.Item2, 2);
                    break;
                }
                count++;
            }

            outputLine = line;
            return testPosition;
        }

        //everything track progression
        float trackProgression = 0;
        int currentLap = 0;
        public float GetTrackCompletion()
        {
            //for the main path
            (int, int) line = aiPath.GetClosestLine(rb.position);
            (AIPath.Point, AIPath.Point) linePoints = (aiPath.GetPointById(line.Item1), aiPath.GetPointById(line.Item2));

            Plane plane1 = new Plane(linePoints.Item1.GetDir(), linePoints.Item1.GetPos());
            Plane plane2 = new Plane(linePoints.Item2.GetDir(), linePoints.Item2.GetPos());

            float distance1 = Mathf.Abs(plane1.GetDistanceToPoint(rb.position));
            float distance2 = Mathf.Abs(plane2.GetDistanceToPoint(rb.position));
            float total = distance1 + distance2;
            float positionWithinId = Mathf.Lerp(0, 1, distance1 / total);

            if (playerInfo.GetCurrentLap() > currentLap)
            {
                currentLap = playerInfo.GetCurrentLap();
                trackProgression = 0;
            }
            else
            {
                float item1progression = (float)aiPath.CalculateDepth(line.Item1) / aiPath.GetMaxDepth();
                float deltaProgPerId = 1.0f / aiPath.GetMaxDepth();
                trackProgression = item1progression + deltaProgPerId * positionWithinId;
            }

            return trackProgression;
        }

        public bool isOutOfBounds()
        {
            return (outOfBounds && OOBtimestamp > 300);
        }

        private IEnumerator PathEditing()
        {
            AIPath newPath = aiPath;

            newPath.ScalePath(scale, scaleOrigin);
            newPath.TransformPath(positionTranslate);

            newPath.PrintPath();
            Debug.Log("Showing what the path will be. Is this ok? Press the 1 key twice to confirm or the 2 key to cancel...");
            int times1Pressed = 0;
            int optionPicked = 0; // 1 = conform; 2 = cancel;
            while (optionPicked == 0)
            {
                if (Keyboard.current[Key.Digit1].wasPressedThisFrame)
                {
                    times1Pressed++;
                }
                if (times1Pressed > 1)
                {
                    optionPicked = 1;
                }
                if (Keyboard.current[Key.Digit2].wasPressedThisFrame)
                {
                    optionPicked = 2;
                }
                yield return null;
            }

            if (optionPicked == 1)
            {
                PathDataManager.SaveAIPath(ScreenManager.current.GetSceneName(), newPath);
                Debug.Log("The modified has been saved as the new path");
            }
            else
            {
                Debug.Log("Nothing was saved");
            }
        }
    }
}
