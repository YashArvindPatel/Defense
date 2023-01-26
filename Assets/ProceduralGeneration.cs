using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ProceduralGeneration : MonoBehaviour
{
    public static ProceduralGeneration instance;
    public float progress;
    public bool isDone;
    int total;
    public Transform fog;
    public Color[] fogColors;
    public Color[] colors;
    public GameObject skipButton;

    private void Awake()
    {
        instance = this;
        //Input.backButtonLeavesApp = true;    

        int currentCardOpen = ModeDetails.currentCardOpen;

        Camera.main.backgroundColor = colors[currentCardOpen];

        for (int i = 0; i < fog.childCount; i++)
        {
            fog.GetChild(i).GetComponent<SpriteRenderer>().color = fogColors[currentCardOpen];
        }       

        int difficultySelected = ModeDetails.difficultySelected;
        int mainLevel = ModeDetails.mainLevel;
        tiles = new List<GameObject>();

        if (currentCardOpen == 0)
            tiles = tiles1;       
        else if (ModeDetails.currentCardOpen == 1)
            tiles = tiles2;  
        else if (ModeDetails.currentCardOpen == 2)
            tiles = tiles3;

        crossTile = crossTiles[currentCardOpen];
        straightTile = straightTiles[currentCardOpen];
        cornerTile = cornerTiles[currentCardOpen];

        ////Temporary
        //if (difficultySelected == 0)
        //    difficulty = Random.Range(0, 4);
        //else if (difficultySelected == 1)
        //    difficulty = Random.Range(4, 8);
        //else if (difficultySelected == 1)
        //    difficulty = Random.Range(8, 11);

        upperLimit = 4;

        if (difficultySelected >= 1)
            difficulty = difficultySelected * 2;
        else 
            difficulty = mainLevel - 1;

        width = height = mainLevel + 2;

        secondLane = (difficultySelected >= 1 || mainLevel >= 2) ? true : false;
        //
    }

    //Game Objects
    public GameObject[] crossTiles, straightTiles, cornerTiles;
    private GameObject crossTile, straightTile, cornerTile;
    public List<GameObject> tiles1, tiles2, tiles3;
    private List<GameObject> tiles;

    //Inputs
    public int width, height;
    [Range(0, 10)]
    public int difficulty;
    [Range(0, 10)]
    public int upperLimit = 6;

    //Private Variables
    private List<int> list;
    private bool stop = false;
    private int minX, maxX, minZ, maxZ;
    private Vector3 left, up, right, down;
    private int xScale, yScale;

    private List<Unit> units;
    private Unit lastUnit;

    private Vector3 initialPos;

    //For Second Lane
    public bool secondLane;        // <------ With Difficulty Setting
    private bool dontGen;
    private List<int> secondLaneList;

    //Fill Up
    private List<GameObject> fillUpTiles = new List<GameObject>();
    public GameObject tower;
    Transform towerTransform;
        
    //Points
    public Transform spawnPoint1, spawnPoint2, capturePoint;
    private List<Vector3> endPoints;

    //References
    public GameObject player, manager;
    public NavMeshSurface nav1,nav2;
    public Transform level, plane;

    //FillUp Parameters
    [Range(0f, 1f)]
    public float randomization;
    [Range(0f, 1f)]
    public float populated;
    private Vector3 playerPosFill;

    //Procedural Over
    bool over = false;

    //Skip
    private bool skipAnimation = false;
    
    public struct Unit
    {
        public int tileType;
        public int tileAngle;
        public Vector3 pos;
        public int banStraight, banLeft, banRight;
        public int dir;
        public string name;
        public GameObject tile;

        public Unit(int tileType, int tileAngle, Vector3 pos, int banLeft, int banRight, int banStraight, int dir, string name, GameObject tile)
        { 
            this.tileType = tileType;
            this.tileAngle = tileAngle;
            this.pos = pos;
            this.banLeft = banLeft;
            this.banRight = banRight;
            this.banStraight = banStraight;
            this.dir = dir;
            this.name = name;
            this.tile = tile;
        }
    }

    void Start()
    {
        xScale = width * 2 + 1;
        yScale = height * 2 + 1;
        total = xScale * yScale;
        plane.localScale = new Vector3(xScale, 1, yScale);

        SetList();

        stop = false;

        if (secondLane)
            dontGen = secondLane;

        initialPos = transform.position;

        minX = -width*10;
        maxX = width*10;
        minZ = -height*10;
        maxZ = height*10;

        left = new Vector3(-10, 0, 0);
        up = new Vector3(0, 0, 10);
        right = new Vector3(10, 0, 0);
        down = new Vector3(0, 0, -10);

        units = new List<Unit>();
        endPoints = new List<Vector3>();

        secondLaneList = new List<int> { 1, 2, 3, 4 };
        int random = Random.Range(1, 5);
        secondLaneList.Remove(random);

        if (random == 1)
            secondLaneList.Add(3);
        else if (random == 2)
            secondLaneList.Add(4);
        else if (random == 3)
            secondLaneList.Add(1);
        else if (random == 4)
            secondLaneList.Add(2);

        units.Add(new Unit(1, 0, transform.position, 0, 0, 0, random, "cross", Instantiate(crossTile, transform.position, Quaternion.identity)));
        lastUnit = units[units.Count - 1];
    }

    private void Update()
    {
        if (over)
            return;

        if (playerPosFill == Vector3.zero)
            playerPosFill = lastUnit.pos;

        if (stop)
        {
            if (dontGen)
            {
                List<int> tempList = new List<int>();

                foreach (var item in secondLaneList)
                {
                    tempList.Add(item);
                }

                foreach (var item in secondLaneList)
                {
                    transform.position = initialPos;

                    if (item == 1)
                    {
                        transform.position += left;
                        if (PositionOccupied())
                            tempList.Remove(item);
                    }
                    else if (item == 2)
                    {
                        transform.position += up;
                        if (PositionOccupied())
                            tempList.Remove(item);
                    }
                    else if (item == 3)
                    {
                        transform.position += right;
                        if (PositionOccupied())
                            tempList.Remove(item);
                    }
                    else if (item == 4)
                    {
                        transform.position += down;
                        if (PositionOccupied())
                            tempList.Remove(item);
                    }
                }

                if (tempList.Count == 0)
                {
                    FillUpRest();
                }
                else
                {
                    units.Reverse();
                    Unit unit = units[units.Count - 1];
                    unit.dir = tempList[Random.Range(0, tempList.Count)];
                    units[units.Count - 1] = unit;
                    lastUnit = units[units.Count - 1];
                    transform.position = initialPos;
                    dontGen = false;
                }

                stop = false;
            }
            else
            {
                stop = false;
                FillUpRest();
            }
        }
        else
        {
            GenerateBase();
        }       

        progress = (float)(units.Count + fillUpTiles.Count) / total;
    }

    void GenerateBase()
    {
        if (lastUnit.tileType == 1)
        {
            if (lastUnit.dir == 1)
                SpawnTypeLeft();
            else if (lastUnit.dir == 2)
                SpawnTypeUp();
            else if (lastUnit.dir == 3)
                SpawnTypeRight();
            else if (lastUnit.dir == 4)
                SpawnTypeDown();
        }
        else if (lastUnit.tileType == 2)
        {
            if (lastUnit.dir == 1)
                SpawnTypeLeft();
            else if (lastUnit.dir == 2)
                SpawnTypeUp();
            else if (lastUnit.dir == 3)
                SpawnTypeRight();
            else if (lastUnit.dir == 4)
                SpawnTypeDown();
        }
        else if (lastUnit.tileType == 3)
        {
            if (lastUnit.tileAngle == 0)
            {
                if (lastUnit.dir == 1)
                    SpawnTypeLeft();
                else if (lastUnit.dir == 2)
                    SpawnTypeUp();
            }
            else if (lastUnit.tileAngle == 90)
            {
                if (lastUnit.dir == 2)
                    SpawnTypeUp();
                else if (lastUnit.dir == 3)
                    SpawnTypeRight();
            }
            else if (lastUnit.tileAngle == 180)
            {
                if (lastUnit.dir == 3)
                    SpawnTypeRight();
                else if (lastUnit.dir == 4)
                    SpawnTypeDown();
            }
            else if (lastUnit.tileAngle == 270)
            {
                if (lastUnit.dir == 1)
                    SpawnTypeLeft();
                else if (lastUnit.dir == 4)
                    SpawnTypeDown();
            }
        }
    } 

    void SpawnTypeLeft()
    {
        transform.position += left;

        SetList();

        if (lastUnit.banStraight == 1)
            list.RemoveRange(0, upperLimit - difficulty);
        if (lastUnit.banRight == 1)
            list.Remove(2);
        if (lastUnit.banLeft == 1)
            list.Remove(3);

        if (list.Count == 0)
        {
            ResetGeneration();
            return;
        }

        int type = list[Random.Range(0, list.Count)];

        if (transform.position.x <= minX || transform.position.x >= maxX || transform.position.z <= minZ || transform.position.z >= maxZ)
        {
            type = 1;
            stop = true;
            endPoints.Add(transform.position);
        }
        else
        {
            if (PositionOccupied())
            {
                if (lastUnit.name == "straight")
                {
                    Unit unit = units[units.Count - 2];
                    unit.banStraight = 1;
                    units[units.Count - 2] = unit;
                }
                else if (lastUnit.name == "left")
                {
                    Unit unit = units[units.Count - 2];
                    unit.banLeft = 1;
                    units[units.Count - 2] = unit;
                }
                else if (lastUnit.name == "right")
                {
                    Unit unit = units[units.Count - 2];
                    unit.banRight = 1;
                    units[units.Count - 2] = unit;
                }

                Destroy(lastUnit.tile);
                units.Remove(lastUnit);
                lastUnit = units[units.Count - 1];
                transform.position = lastUnit.pos;

                return;
            }
        }      

        if (type == 1)
        {
            units.Add(new Unit(2, 90, transform.position, 0, 0, 0, lastUnit.dir, "straight", Instantiate(straightTile, transform.position, Quaternion.Euler(0, 90, 0))));
            lastUnit = units[units.Count - 1];        
        }
        else if (type == 2)
        {
            units.Add(new Unit(3, 90, transform.position, 0, 0, 0, 2, "right", Instantiate(cornerTile, transform.position, Quaternion.Euler(0, 90, 0))));
            lastUnit = units[units.Count - 1];           
        }
        else if (type == 3)
        {
            units.Add(new Unit(3, 180, transform.position, 0, 0, 0, 4, "left", Instantiate(cornerTile, transform.position, Quaternion.Euler(0, 180, 0))));
            lastUnit = units[units.Count - 1];          
        }
    }

    void SpawnTypeUp()
    {
        transform.position += up;

        SetList();

        if (lastUnit.banStraight == 1)
            list.RemoveRange(0, upperLimit - difficulty);
        if (lastUnit.banRight == 1)
            list.Remove(2);
        if (lastUnit.banLeft == 1)
            list.Remove(3);

        if (list.Count == 0)
        {
            ResetGeneration();
            return;
        }

        int type = list[Random.Range(0, list.Count)];

        if (transform.position.x <= minX || transform.position.x >= maxX || transform.position.z <= minZ || transform.position.z >= maxZ)
        {
            type = 1;
            stop = true;
            endPoints.Add(transform.position);
        }
        else
        {
            if (PositionOccupied())
            {
                if (lastUnit.name == "straight")
                {
                    Unit unit = units[units.Count - 2];
                    unit.banStraight = 1;
                    units[units.Count - 2] = unit;
                }
                else if (lastUnit.name == "left")
                {
                    Unit unit = units[units.Count - 2];
                    unit.banLeft = 1;
                    units[units.Count - 2] = unit;
                }
                else if (lastUnit.name == "right")
                {
                    Unit unit = units[units.Count - 2];
                    unit.banRight = 1;
                    units[units.Count - 2] = unit;
                }

                Destroy(lastUnit.tile);
                units.Remove(lastUnit);
                lastUnit = units[units.Count - 1];
                transform.position = lastUnit.pos;

                return;
            }
        }

        if (type == 1)
        {
            units.Add(new Unit(2, 0, transform.position, 0, 0, 0, lastUnit.dir, "straight", Instantiate(straightTile, transform.position, Quaternion.Euler(0, 0, 0))));
            lastUnit = units[units.Count - 1];          
        }
        else if (type == 2)
        {
            units.Add(new Unit(3, 180, transform.position, 0, 0, 0, 3, "right", Instantiate(cornerTile, transform.position, Quaternion.Euler(0, 180, 0))));
            lastUnit = units[units.Count - 1];           
        }
        else if (type == 3)
        {
            units.Add(new Unit(3, 270, transform.position, 0, 0, 0, 1, "left", Instantiate(cornerTile, transform.position, Quaternion.Euler(0, 270, 0))));
            lastUnit = units[units.Count - 1];           
        }
    }

    void SpawnTypeRight()
    {
        transform.position += right;

        SetList();

        if (lastUnit.banStraight == 1)
            list.RemoveRange(0, upperLimit - difficulty);
        if (lastUnit.banRight == 1)
            list.Remove(2);
        if (lastUnit.banLeft == 1)
            list.Remove(3);

        if (list.Count == 0)
        {
            ResetGeneration();
            return;
        }

        int type = list[Random.Range(0, list.Count)];

        if (transform.position.x <= minX || transform.position.x >= maxX || transform.position.z <= minZ || transform.position.z >= maxZ)
        {
            type = 1;
            stop = true;
            endPoints.Add(transform.position);
        }
        else
        {
            if (PositionOccupied())
            {
                if (lastUnit.name == "straight")
                {
                    Unit unit = units[units.Count - 2];
                    unit.banStraight = 1;
                    units[units.Count - 2] = unit;
                }
                else if (lastUnit.name == "left")
                {
                    Unit unit = units[units.Count - 2];
                    unit.banLeft = 1;
                    units[units.Count - 2] = unit;
                }
                else if (lastUnit.name == "right")
                {
                    Unit unit = units[units.Count - 2];
                    unit.banRight = 1;
                    units[units.Count - 2] = unit;
                }

                Destroy(lastUnit.tile);
                units.Remove(lastUnit);
                lastUnit = units[units.Count - 1];
                transform.position = lastUnit.pos;

                return;
            }
        }

        if (type == 1)
        {
            units.Add(new Unit(2, 90, transform.position, 0, 0, 0, lastUnit.dir, "straight", Instantiate(straightTile, transform.position, Quaternion.Euler(0, 90, 0))));
            lastUnit = units[units.Count - 1];           
        }
        else if (type == 2)
        {
            units.Add(new Unit(3, 270, transform.position, 0, 0, 0, 4, "right", Instantiate(cornerTile, transform.position, Quaternion.Euler(0, 270, 0))));
            lastUnit = units[units.Count - 1];           
        }
        else if (type == 3)
        {
            units.Add(new Unit(3, 0, transform.position, 0, 0, 0, 2, "left", Instantiate(cornerTile, transform.position, Quaternion.Euler(0, 0, 0))));
            lastUnit = units[units.Count - 1];          
        }
    }

    void SpawnTypeDown()
    {
        transform.position += down;

        SetList();

        if (lastUnit.banStraight == 1)
            list.RemoveRange(0, upperLimit - difficulty);
        if (lastUnit.banRight == 1)
            list.Remove(2);
        if (lastUnit.banLeft == 1)
            list.Remove(3);

        if (list.Count == 0)
        {
            ResetGeneration();
            return;
        }

        int type = list[Random.Range(0, list.Count)];

        if (transform.position.x <= minX || transform.position.x >= maxX || transform.position.z <= minZ || transform.position.z >= maxZ)
        {
            type = 1;
            stop = true;
            endPoints.Add(transform.position);
        }
        else
        {
            if (PositionOccupied())
            {
                if (lastUnit.name == "straight")
                {
                    Unit unit = units[units.Count - 2];
                    unit.banStraight = 1;
                    units[units.Count - 2] = unit;
                }
                else if (lastUnit.name == "left")
                {
                    Unit unit = units[units.Count - 2];
                    unit.banLeft = 1;
                    units[units.Count - 2] = unit;
                }
                else if (lastUnit.name == "right")
                {
                    Unit unit = units[units.Count - 2];
                    unit.banRight = 1;
                    units[units.Count - 2] = unit;
                }

                Destroy(lastUnit.tile);
                units.Remove(lastUnit);
                lastUnit = units[units.Count - 1];
                transform.position = lastUnit.pos;

                return;
            }
        }

        if (type == 1)
        {
            units.Add(new Unit(2, 0, transform.position, 0, 0, 0, lastUnit.dir, "straight", Instantiate(straightTile, transform.position, Quaternion.Euler(0, 0, 0))));
            lastUnit = units[units.Count - 1];           
        }
        else if (type == 2)
        {
            units.Add(new Unit(3, 0, transform.position, 0, 0, 0, 1, "right", Instantiate(cornerTile, transform.position, Quaternion.Euler(0, 0, 0))));
            lastUnit = units[units.Count - 1];     
        }
        else if (type == 3)
        {
            units.Add(new Unit(3, 90, transform.position, 0, 0, 0, 3, "left", Instantiate(cornerTile, transform.position, Quaternion.Euler(0, 90, 0))));
            lastUnit = units[units.Count - 1];
        }
    }

    void SetList()
    {
        list = new List<int>();

        for (int i = 0; i < upperLimit - difficulty; i++)
        {
            list.Add(1);
        }

        list.Add(2);
        list.Add(3);
    }

    public bool PositionOccupied()
    {
        foreach (var unit in units)
        {
            if (transform.position == unit.pos)
            {                  
                return true;
            }
        }

        return false;
    }

    public void ResetGeneration()
    {
        transform.position = initialPos;

        foreach (var unit in units)
        {
            Destroy(unit.tile);
        }

        if (fillUpTiles.Count > 0)
        {
            foreach (var tile in fillUpTiles)
            {
                Destroy(tile);
            }
        }

        Start();
    }   

    public void FillUpRest()
    {
        over = true;

        ExtraProceduralStuff();

        capturePoint.position = initialPos;

        if (endPoints.Count == 1)
        {
            spawnPoint1.position = endPoints[0];
            spawnPoint2.gameObject.SetActive(false);
        }
        else if (endPoints.Count == 2)
        {
            spawnPoint1.position = endPoints[0];
            spawnPoint2.position = endPoints[1];
        }

        towerTransform = Instantiate(tower, initialPos, Quaternion.identity).GetComponent<Transform>();

        GenerateNavMesh();

        //Hide Generation for Animation Intro

        towerTransform.localPosition += new Vector3(0, 100f);

        //level.localPosition -= new Vector3(0, 100f);                // Up
        //level.localPosition += new Vector3(0, 100f);                // Down

        for (int i = 0; i < level.childCount; i++)                    //Scale
        {
            level.GetChild(i).localScale = Vector3.zero;
        }

        //Re-arrange child position to Spiral Pattern

        int k = 1;
        po = initialPos;
        CheckIfTileAtPos();
        int maxC = width > height ? width : height;
        int maxCount = maxC * 2 + 1 + 1;
        for (int i = 1; i < maxCount; i++)
        {    
            for (int j = 0; j < i; j++)
            {
                po += down * k;
                CheckIfTileAtPos();
            }
            for (int j = 0; j < i && i != maxCount - 1; j++)
            {
                po += right * k;
                CheckIfTileAtPos();
            }

            k = -k;
        }

        //Indication for Procedural Generation Complete
        LeanTween.value(0, 1, 1f).setOnComplete(() => isDone = true);

        //Animation Intro for Player
        StartCoroutine(AnimationIntro());   
    }

    Vector3 po;
    void CheckIfTileAtPos()
    {
        for (int i = 0; i < level.childCount; i++)
        {
            Transform child = level.GetChild(i);
            if (child.localPosition == po)
                child.SetAsLastSibling();             
        }
    }

    public void ExtraProceduralStuff()
    {
        float maxDistance = Vector3.Distance(new Vector3(maxX, 0, maxZ), capturePoint.position);

        for (int i = minZ; i < maxZ + 10; i += 10)
        {
            for (int j = minX; j < maxX + 10; j += 10)
            {
                transform.position = new Vector3(j, 0, i);
                if (!PositionOccupied())
                {
                    int distance = Mathf.RoundToInt(Mathf.Clamp01(Vector3.Distance(transform.position, capturePoint.position) / maxDistance) * 10);

                    if (Random.Range(0f, 1f) < randomization)
                    {
                        fillUpTiles.Add(Instantiate(tiles[Random.Range(0, tiles.Count)], transform.position, Quaternion.identity));
                    }
                    else
                    {
                        if (Random.Range(0f, 1f) < populated)
                        {
                            fillUpTiles.Add(Instantiate(tiles[distance], transform.position, Quaternion.identity));
                        }
                        else
                        {
                            fillUpTiles.Add(Instantiate(tiles[0], transform.position, Quaternion.identity));
                        }
                    }
                }
            }
        }
    }

    public void GenerateNavMesh()
    {
        // USE ONLY IN CASE OTHER NAVMESH LOGIC DOESN'T WORK

        //foreach (var item in units)
        //{            
        //    NavMeshSurface surface = item.tile.AddComponent<NavMeshSurface>();
        //    surface.defaultArea = 3;
        //    surface.BuildNavMesh();
        //}

        //foreach (var item in fillUpTiles)
        //{
        //    NavMeshSurface surface = item.AddComponent<NavMeshSurface>();
        //    surface.BuildNavMesh();
        //}

        foreach (var item in units)
        {
            item.tile.transform.parent = level;
        }

        foreach (var item in fillUpTiles)
        {
            item.transform.parent = level;
        }

        nav1.BuildNavMesh();
        nav2.BuildNavMesh();
    }

    IEnumerator AnimationIntro()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < level.childCount; i++)
        {
            Transform child = level.GetChild(i);
            //child.LeanMoveLocalY(child.localPosition.y + 100f, .2f);          //Up
            //child.LeanMoveLocalY(child.localPosition.y - 100f, .1f);          //Down
            if (!skipAnimation)
            {
                child.LeanScale(Vector3.one, .2f);                                //Scale
                yield return new WaitForSeconds(.05f);
            }
            else
                child.localScale = Vector3.one;
        }

        if (skipButton.activeSelf)
            skipButton.SetActive(false);

        fog.LeanScale(new Vector3(xScale, 1, yScale), 1f);

        yield return new WaitForSeconds(1f);

        fog.GetComponent<FogHandler>().enabled = true;
        towerTransform.LeanMoveLocalY(towerTransform.localPosition.y - 100f, .5f).setOnComplete(() => { SFX.instance.PlaySingleSoundClip((int)SoundIndexes.PLACE_TURRET); ParticleSystem ps = manager.GetComponent<TurretSpawn>().turretSetupDust; ps.gameObject.SetActive(true); ps.Play(); });
        
        //Start Activating objects
        player.transform.position = playerPosFill;
        player.SetActive(true);
        manager.SetActive(true);

        enabled = false;
    }

    public void SkipAnimationCompletely()
    {
        skipAnimation = true;
        skipButton.SetActive(false);
    }
}
