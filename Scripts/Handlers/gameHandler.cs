using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//island types in prefab names
public enum IslandTypes
{
    
    forestLand,
    hillLand
    //swamp
}

//spawn grid
public enum GridLines
{
    line1 = 4,
    line2 = 5,
    line3 = 6,
    line4 = 5,
    line5 = 4
}

public class gameHandler : MonoBehaviour
{



    //Player struct
    struct PlayerInfo
    {
        int playerNumber;
        int amountGrain;
        int amountWood;
        int amountDiamond;
        int maxStorage;

    }

    Dictionary<string, PlayerInfo> Players = new Dictionary<string, PlayerInfo>();


    //distance between island parts
    GameObject startIsland = null;
    float horizontalDistance = 40;
    float verticleDistance = 35;

    List<GameObject> AllIslandParts;

    //settings
    float maxViewSize = 50;
    float minViewSize = 10;
    float zoomSpeed = 15;
    float yStartPos = 0;
    float cameraHeight = 5;

    int[] possibleRot = new int[6] { 0, 60, 120, 180, 240, 300 };

    int amountOfPlayers = 4; //amount of player playing
    int currentPlayer = 1; //player number of player that needs to play his turn.


    GameObject testIslandToWalkTo = null;
    GameObject testBuilder = null;
    GameObject testBuilderIcon = null;

    List<Point> islandCoords = new List<Point>();

    // Use this for initialization
    void Start()
    {
        AllIslandParts = new List<GameObject>();

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0);
        plane.transform.localScale *= 4;
        plane.transform.position = new Vector3(0, 0.1f, 0);
        plane.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");

        if (Application.platform == RuntimePlatform.Android)
        {
            mouseSensitivity = 0.3f;
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            mouseSensitivity = 0.3f;
        }

        System.Array prefabnames = System.Enum.GetValues(typeof(IslandTypes));

        List<GameObject> islandParts = new List<GameObject>();

        //load all known island parts
        foreach (var name in prefabnames)
        {
            IslandTypes Name = (IslandTypes)name;
            string touse = Name.ToString();
            islandParts.Add((GameObject)Resources.Load("Prefabs/IslandParts/" + touse) as GameObject);
        }

        //island prefabs
        GameObject beachLand = (GameObject)Resources.Load("Prefabs/IslandParts/beachLand") as GameObject;


        System.Array landgrid = System.Enum.GetValues(typeof(GridLines));

        //collection of island parts ingame
        GameObject IslandParts = GameObject.Find("IslandParts");

        int linenumber = 0;
        //load all known island parts
        foreach (var line in landgrid)
        {
            GridLines gridLine = (GridLines)line;
            for (int i = 0; i < (int)gridLine; i++)
            {
                if (linenumber == 0 && i == 0)
                {
                    GameObject islandPart = (GameObject)Instantiate(beachLand, new Vector3(0, 0, 0), Quaternion.Euler(-90, 0, 0));
                    MeshCollider startRenderer = islandPart.transform.FindChild("hexacollider").GetComponent<MeshCollider>();
                    horizontalDistance = startRenderer.bounds.size.x;
                    verticleDistance = startRenderer.bounds.size.z / 1.35f;
                    islandPart.transform.position += new Vector3(horizontalDistance * -0.5f, 0, 0);
                    islandPart.transform.SetParent(IslandParts.transform);
                    testBuilder = (GameObject)Instantiate((GameObject)Resources.Load("Prefabs/testPrefabs/Civilian") as GameObject, new Vector3(islandPart.transform.position.x, islandPart.transform.position.y + 2f, islandPart.transform.position.z), Quaternion.identity);
                    GameObject canvas = GameObject.Find("Canvas");
                    testBuilderIcon = (GameObject)Instantiate((GameObject)Resources.Load("Prefabs/UI/BuilderLocator"));
                    testBuilderIcon.transform.SetParent(canvas.transform);
                    testBuilderIcon.transform.position = Camera.main.WorldToScreenPoint(testBuilder.transform.position);
                    AllIslandParts.Add(islandPart);
                }
                else
                {
                    GameObject rdnIsland = islandParts[Random.Range(0, islandParts.Count)]; //choose random island
                    int rdnRotation = possibleRot[Random.Range(0, possibleRot.Length)]; //random rotation
                    Quaternion rdnRotationQ = Quaternion.Euler(new Vector3(270, rdnRotation, 0));

                    if (linenumber % 2 != 0) //is the number odd
                    {
                        Vector3 spawnPosition = new Vector3(-(horizontalDistance * i), 0, linenumber * verticleDistance);
                        GameObject islandPart = (GameObject)Instantiate(rdnIsland, spawnPosition, rdnRotationQ);
                        islandPart.transform.SetParent(IslandParts.transform);
                        AllIslandParts.Add(islandPart);
                        //TODO REMOVE
                        if (i == 2)
                            testIslandToWalkTo = islandPart;
                    }
                    else //even
                    {
                        Vector3 spawnPosition = new Vector3(-(horizontalDistance * 0.5f) - (horizontalDistance * i), 0, linenumber * verticleDistance);
                        GameObject islandPart = (GameObject)Instantiate(rdnIsland, spawnPosition, rdnRotationQ);
                        islandPart.transform.SetParent(IslandParts.transform);
                        AllIslandParts.Add(islandPart);
                    }
                }

            }
            linenumber++;
        }

        List<float> distances = new List<float>();

        foreach (GameObject island in AllIslandParts)
        {
            foreach (GameObject island2 in AllIslandParts)
            {
                float distance = Vector3.Distance(island.transform.position, island2.transform.position);
                if (island != island2)
                {
                    distances.Add(distance);
                }
            }
        }

        float[] arrdistances = new float[distances.Count];

        for (int i = 0; i < arrdistances.Length; i++)
        {
            arrdistances[i] = distances[i];
        }

        float adjacentDistance = Mathf.Min(arrdistances) + 0.1f;

        foreach (GameObject island in AllIslandParts)
        {
            foreach (GameObject island2 in AllIslandParts)
            {
                float distance = Vector3.Distance(island.transform.position, island2.transform.position);
                if (distance <= adjacentDistance && island != island2)
                {
                    island.GetComponent<IslandPart>().AddAdjacentPart(island2);
                }
            }
        }

        StartCoroutine(TryoutBuilderWalk());

        //pathfinding


        islandCoords = new List<Point>();

        foreach (GameObject island in AllIslandParts)
        {
            Point islandToPoint = new Point();

            islandToPoint.x = island.transform.position.x;
            islandToPoint.y = island.transform.position.z;

            List<Vector2> neighbourPos = new List<Vector2>();

            foreach (GameObject neighisland in island.GetComponent<IslandPart>().GetAdjacentParts())
            {
                neighbourPos.Add(new Vector2(neighisland.transform.position.x, neighisland.transform.position.z));
            }

            islandToPoint.positions = neighbourPos;

            islandCoords.Add(islandToPoint);
        }

        int startind = Random.Range(0, islandCoords.Count - 1); Debug.Log("start: " + startind);
        int endind = Random.Range(0, islandCoords.Count - 1); Debug.Log("end: " + endind);
        PathFinding.startPoint = new Vector2(islandCoords[startind].x, islandCoords[startind].y);
        PathFinding.endPoint = new Vector2(islandCoords[endind].x, islandCoords[endind].y);

        foreach (Point islandPoint in islandCoords)
        {
            islandPoint.Gcost = PathFinding.Gcost(islandPoint);
            islandPoint.Hcost = PathFinding.Hcost(islandPoint);
            islandPoint.Fcost = PathFinding.Fcost(islandPoint);
            foreach (Point compareisland in islandCoords)
            {
                if (islandPoint != compareisland)
                {
                    foreach (Vector2 neighPos in islandPoint.positions)
                    {
                        if (compareisland.x == neighPos.x && compareisland.y == neighPos.y)
                        {
                            islandPoint.neighbours.Add(compareisland);
                        }
                    }
                }
            }
        }

        Path patje = PathFinding.findPath(islandCoords);

        PathFinding.GetStartIsland(AllIslandParts).GetComponent<Renderer>().material.color = new Color(0.3f, 0.9f, 0.5f);
        PathFinding.GetEndIsland(AllIslandParts).GetComponent<Renderer>().material.color = new Color(0.6f, 0.1f, 0.3f);

        foreach (GameObject isleC in AllIslandParts)
        {
            foreach (Point p in patje.getPath())
            {
                if (p.x == isleC.transform.position.x && p.y == isleC.transform.position.z)
                {
                    isleC.GetComponent<Renderer>().material.color = new Color(0.5f, 0.1f, 0.5f);
                    Debug.Log(p.x + " , " + p.y);
                }
            }
        }

       //StartCoroutine(randompaths());
    }

    IEnumerator randompaths()
    {
        yield return new WaitForSeconds(1);
        PathFinding.listmap = new Path();
        int startind = Random.Range(0, islandCoords.Count - 1); Debug.Log("start: " + startind);
        int endind = Random.Range(0, islandCoords.Count - 1); Debug.Log("end: " + endind);
        PathFinding.startPoint = new Vector2(islandCoords[startind].x, islandCoords[startind].y);
        PathFinding.endPoint = new Vector2(islandCoords[endind].x, islandCoords[endind].y);

        foreach (Point p in islandCoords)
        {
            p.Gcost = PathFinding.Gcost(p);
            p.Hcost = PathFinding.Hcost(p);
            p.Fcost = PathFinding.Fcost(p);
            p.positions = null;
            p.parent = null;
        }

        foreach (GameObject g in AllIslandParts)
        {
            g.GetComponent<Renderer>().material.color = Color.gray;

        }

        yield return new WaitForSeconds(1);
        Path patje = PathFinding.findPath(islandCoords);

        PathFinding.GetStartIsland(AllIslandParts).GetComponent<Renderer>().material.color = new Color(0.3f, 0.9f, 0.5f);
        PathFinding.GetEndIsland(AllIslandParts).GetComponent<Renderer>().material.color = new Color(0.6f, 0.1f, 0.3f);

        foreach (GameObject isleC in AllIslandParts)
        {
            foreach (Point p in patje.getPath())
            {
                if (p.x == isleC.transform.position.x && p.y == isleC.transform.position.z)
                {
                    isleC.GetComponent<Renderer>().material.color = new Color(0.5f, 0.1f, 0.5f);
                    Debug.Log(p.x + " , " + p.y);
                }
            }
        }

        StartCoroutine(randompaths());
    }

    float startTime = 0;
    float speed = 0.2f;
    float journeyLength = 0;
    bool walking = false;
    Vector3 builderStartPos = new Vector3();

    public Transform Target;
    public float RotationSpeed = 1;

    private Quaternion _lookRotation;
    private Vector3 _direction;

    IEnumerator TryoutBuilderWalk()
    {
        if (!walking) {
            startTime = Time.time;
            journeyLength = Vector3.Distance(testBuilder.transform.position, testIslandToWalkTo.transform.position);
            builderStartPos = testBuilder.transform.position;

            //find the vector pointing from our position to the target
            _direction = (testIslandToWalkTo.transform.position - builderStartPos).normalized;

            //create the rotation we need to be in to look at the target
            _lookRotation = Quaternion.LookRotation(_direction);

            //rotate us over time according to speed until we are in the required rotation
            testBuilder.transform.rotation = _lookRotation;
            //testBuilder.GetComponent<Animator>().SetBool("isWalking", true);

            walking = true;

        }
        yield return new WaitForSeconds(0.01f);

        float distCovered = (Time.time - startTime) * speed;
        float fracJourney = distCovered / journeyLength;
        Vector3 newBpos = Vector3.Lerp(builderStartPos, testIslandToWalkTo.transform.position, fracJourney);


        Vector3 down = testBuilder.transform.TransformDirection(Vector3.down);
        Vector3 up = testBuilder.transform.TransformDirection(Vector3.up);
        Debug.DrawRay(testBuilder.transform.position, up * 50, Color.green);
        RaycastHit objectHitD = new RaycastHit(); //down
        RaycastHit objectHitU = new RaycastHit(); //up


        if (Physics.Raycast(testBuilder.transform.position, up, out objectHitU))
        {
            testBuilder.transform.position = new Vector3(newBpos.x, objectHitU.point.y + 0.05f, newBpos.z);
        }
        else if (Physics.Raycast(testBuilder.transform.position, down, out objectHitD))
        {
            testBuilder.transform.position = new Vector3(newBpos.x, objectHitD.point.y + 0.05f, newBpos.z);
        }

        Vector3 iconPos = Camera.main.WorldToScreenPoint(testBuilder.transform.position);
        iconPos += new Vector3(0, 50, 0);
        testBuilderIcon.transform.position = iconPos;

        if (fracJourney < 1)
        {
            StartCoroutine(TryoutBuilderWalk());
        }
        else
        {
            //testBuilder.GetComponent<Animator>().SetBool("isWalking", false);
            builderStartPos = testBuilder.transform.position;
            testIslandToWalkTo = AllIslandParts[Random.Range(0, AllIslandParts.Count)];
            startTime = Time.time;
            StartCoroutine(TryoutBuilderWalk());
        }
    }

    bool expanded = false;

    public void ExpandBuildPanel()
    {
        GameObject bPanel = GameObject.Find("BuildingPanel");
        Animation anim = bPanel.GetComponent<Animation>();

        if (!expanded)
        {
            anim["stretchBuildingPanel"].speed = 1;
            anim.Play();
            expanded = true;
        }
        else
        { 
            anim["stretchBuildingPanel"].time = anim["stretchBuildingPanel"].length;
            anim["stretchBuildingPanel"].speed = -1;
            anim.Play();
            expanded = false;
        }

    }

    Vector3 lastPos = new Vector3();
    float mouseSensitivity = 0.5f;
    GameObject selectedIsle = null;
    Color originColor = new Color();

    // Update is called once per frame
    void Update()
    {

        //handle zoom by scroll
        float d = Input.GetAxis("Mouse ScrollWheel");
        if (d != 0)
        {
            if (d > 0)
            {
                if (Camera.main.fieldOfView < maxViewSize)
                {
                    Camera.main.fieldOfView += d * zoomSpeed;
                }
            }
            else
            {
                if (Camera.main.fieldOfView > minViewSize)
                {
                    Camera.main.fieldOfView += d * zoomSpeed;
                }
            }
        }

        //handler right mouse button dragging
        if (Input.GetMouseButtonDown(1))
        {
            lastPos = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 deltam = Input.mousePosition - lastPos;
            deltam *= -Mathf.Lerp(0.004f, 0.03f, Mathf.InverseLerp(minViewSize, maxViewSize, Camera.main.fieldOfView));
            Camera.main.transform.localPosition = new Vector3(Camera.main.transform.localPosition.x + (deltam.x * mouseSensitivity), Camera.main.transform.localPosition.y, Camera.main.transform.localPosition.z + (deltam.y * mouseSensitivity));
            lastPos = Input.mousePosition;
        }


        //touch input handling
        switch (Input.touchCount)
        {
            case 1:
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    lastPos = Input.GetTouch(0).position;
                }


                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    Vector3 deltam = (Vector3)Input.GetTouch(0).position - lastPos;
                    deltam *= -Mathf.Lerp(0.004f, 0.03f, Mathf.InverseLerp(minViewSize, maxViewSize, Camera.main.fieldOfView));
                    Camera.main.transform.localPosition = new Vector3(Camera.main.transform.localPosition.x + (deltam.x * mouseSensitivity), Camera.main.transform.localPosition.y, Camera.main.transform.localPosition.z + (deltam.y * mouseSensitivity));
                    lastPos = Input.GetTouch(0).position;
                }
                break;
            case 2:

                break;
        }

        //mouse / touch raycast
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //draw invisible ray cast/vector
            //Debug.DrawLine(ray.origin, hit.point);

                if (hit.collider.transform.tag == "island")
                {
                    GameObject parent = hit.collider.transform.parent.gameObject;
                    if (selectedIsle != parent)
                    {
                        if (parent != null)
                        {
                            //Debug.DrawLine(ray.origin, hit.point, Color.red);
                            if (selectedIsle != null)
                            {
                                selectedIsle.GetComponent<Renderer>().material.color = originColor;
                            }
                            selectedIsle = parent;
                            originColor = parent.GetComponent<Renderer>().material.color;
                            parent.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
                            //lastHover = parent;
                            //Color othercol = new Color(0.5f, 1, 0.5f, 1);
                            //foreach (GameObject item in parent.GetComponent<IslandPart>().GetAdjacentParts())
                            //{
                            //    item.GetComponent<Renderer>().material.color = othercol;
                            //}

                        }
                    }
                }
            }

        }

    }
}
