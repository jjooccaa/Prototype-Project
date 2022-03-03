using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardManager : MonoBehaviour
{
    [Header("Art stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.5f;
    [SerializeField] private float deathSpacing = 0.3f;
   

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabsCops;
    [SerializeField] private GameObject[] prefabsRebels;
    [SerializeField] private GameObject[] prefabsSpecialOps;
    [SerializeField] private GameObject[] prefabsOldOnes;

    [SerializeField] private TextMeshProUGUI textWarnings;

    // LOGIC
    private Unit[,] unitsOnBoard;
    private Unit currentlySelected;
    public List<Unit> whiteUnits = new List<Unit>();
    public List<Unit> blackUnits = new List<Unit>();

    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<Vector2Int> availableRange = new List<Vector2Int>();

    private List<Unit> deadWhites = new List<Unit>();
    private List<Unit> deadBlacks = new List<Unit>();

    private const int TILE_COUNT_X = 10;
    private const int TILE_COUNT_Y = 10;

    public GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;

    private bool isWhiteTurn;
    public bool IsWhiteTurn { get { return isWhiteTurn; } set { isWhiteTurn = value; } }

    private int countingMoves;
    public int CountingMoves { get { return countingMoves; } set { countingMoves = value; } }

    private int countingAttacks;
    public int CountingAttacks { get {return countingAttacks; } set { countingAttacks = value; } }

    private bool isMoving;
    private bool isShooting;



    private void Awake()
    {
        // white team has first turn
        isWhiteTurn = true;

        isMoving = false;
        isShooting = false;
       
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        SpawnAllUnits();
        PositionAllUnits();


    }
    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "HighlightMove", "HighlightRange")))
        {
            // Get the indexes of the tile we've hit with raycast
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            //If we are hovering a tile for first time
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If we are hovering a tile after first time, and are not in moving or shooting phase
            if ((currentHover != hitPosition) && !isMoving && !isShooting)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile"); 

                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            if ((currentHover != hitPosition) && isMoving && !isShooting) // -||- and we are in moving phase
            {
                // if there are available moves, change layer to move tiles, if there are not, cange it back to regular tile
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover) ? LayerMask.NameToLayer("HighlightMove") : LayerMask.NameToLayer("Tile"));

                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            if ((currentHover != hitPosition) && !isMoving && isShooting) // -||- and we are in shooting phase
            {
                // if there are available shooting range, change layer to range tiles, if there are not, change it back to regular tile
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidRange(ref availableRange, currentHover) ? LayerMask.NameToLayer("HighlightRange") : LayerMask.NameToLayer("Tile"));

                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If we press down left click on the mouse
            if (Input.GetMouseButtonDown(0))
            {
                if (unitsOnBoard[hitPosition.x, hitPosition.y] !=null) // checking if there is any unit on selected tile
                {
                    // Is it white team or black team turn
                    if ((unitsOnBoard[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn) || (unitsOnBoard[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn))
                    {
                        isMoving = true;
                        currentlySelected = unitsOnBoard[hitPosition.x, hitPosition.y];
                        // Get a list of where unit can go, highlight tiles as well
                        availableMoves = currentlySelected.GetAvailableMoves(ref unitsOnBoard, TILE_COUNT_X, TILE_COUNT_Y);
                        HighlightMoveTile();
                    }
                }
            }
            // If we are realising the mouse left button
            if (currentlySelected != null && Input.GetMouseButtonUp(0))
            {
                MoveTo(currentlySelected, hitPosition.x, hitPosition.y);// check if unit can move there, if it can, move it

                isMoving = false;
                currentlySelected = null;
                RemoveHighlighTile();
            }
            // If We press down right click on the mouse
            if (Input.GetMouseButtonDown(1))
            {
                if(unitsOnBoard[hitPosition.x, hitPosition.y] != null) // checking if there is any unit on selected tile
                {
                    // Is it white tema or black team turn
                    if((unitsOnBoard[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn) || (unitsOnBoard[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn))
                    {
                        isShooting = true;
                        currentlySelected = unitsOnBoard[hitPosition.x, hitPosition.y];
                        //Get a list of where unit can shoot, highlight tiles as well
                        availableRange = currentlySelected.GetAvailableRange(ref unitsOnBoard, TILE_COUNT_X, TILE_COUNT_Y);
                        HighlightRangeTile();

                    }
                }
            }
            // If we are realising the mouse right button
            if (currentlySelected != null && Input.GetMouseButtonUp(1))
            {
                Attack(currentlySelected, hitPosition.x, hitPosition.y); // check if unit can shoot there, if  can, do it

                isShooting = false;
                currentlySelected = null;
                RemoveHighlighRangeTile();
            
            }
        }
        else // if we move mouse from the board, reset hover
        {
            if (currentHover != -Vector2Int.one && !isMoving && !isShooting)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
            if (currentHover != -Vector2Int.one && isMoving && !isShooting)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover) ? LayerMask.NameToLayer("HighlightMove") : LayerMask.NameToLayer("Tile"));
                currentHover = -Vector2Int.one;
            }
            if (currentHover != -Vector2Int.one && !isMoving && isShooting)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover) ? LayerMask.NameToLayer("HighlightRange") : LayerMask.NameToLayer("Tile"));
                currentHover = -Vector2Int.one;
            }

        }

    }

    // Generate the board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {

        yOffset += transform.position.y; //move pieces up when board moves too
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;


        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        // create empty mesh for tile 
        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        //make triangles for mesh tile

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    // Spawning of the pieces
    private void SpawnAllUnits()
    {
        unitsOnBoard = new Unit[TILE_COUNT_X, TILE_COUNT_Y];

        int whiteTeam = 0;
        int blackTeam = 1;

        // White team
        unitsOnBoard[1, 0] = SpawnSingleUnit(CopsUnitType.Aurelia, whiteTeam);
        unitsOnBoard[5, 0] = SpawnSingleUnit(CopsUnitType.Bishop, whiteTeam);
        unitsOnBoard[7, 0] = SpawnSingleUnit(CopsUnitType.Bradley, whiteTeam);
        unitsOnBoard[8, 0] = SpawnSingleUnit(CopsUnitType.Phoenix, whiteTeam);


        // Black team
        unitsOnBoard[1, 9] = SpawnSingleUnit(SpecialOpsUnitType.Blaze, blackTeam);
        unitsOnBoard[4, 9] = SpawnSingleUnit(SpecialOpsUnitType.Ember, blackTeam);
        unitsOnBoard[7, 9] = SpawnSingleUnit(SpecialOpsUnitType.Karmak, blackTeam);
        unitsOnBoard[8, 9] = SpawnSingleUnit(SpecialOpsUnitType.Nova, blackTeam);


    }

    private Unit SpawnSingleUnit(CopsUnitType type, int team)
    {
        Unit u = Instantiate(prefabsCops[(int)type - 1], transform).GetComponent<Unit>();

        u.copsType = type;
        u.team = team;
        if(team == 0) // add unit to list if white team
        {
            whiteUnits.Add(u);
        }
        if(team == 1)// for black team
        {
            blackUnits.Add(u);
        }
        
        
        return u;
    }
    // overlaod for rebel
    private Unit SpawnSingleUnit(RebelsUnitType type, int team)
    {
        Unit u = Instantiate(prefabsRebels[(int)type - 1], transform).GetComponent<Unit>();

        u.rebelType = type;
        u.team = team;

        return u;
    }
    // overload for special ops
    private Unit SpawnSingleUnit(SpecialOpsUnitType type, int team)
    {
        Unit u = Instantiate(prefabsSpecialOps[(int)type - 1], transform).GetComponent<Unit>();

        u.sOType = type;
        u.team = team;

        return u;
    }
    // overload for Old ones
    private Unit SpawnSingleUnit(OldOnesUnitType type, int team)
    {
        Unit u = Instantiate(prefabsOldOnes[(int)type - 1], transform).GetComponent<Unit>();

        u.oldOnesType = type;
        u.team = team;

        return u;
    }

    // Highlight Tiles
    private void HighlightMoveTile()
    {
        // highlight all tiles where unit can move
        for(int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("HighlightMove");
        }

    }
    private void HighlightRangeTile()
    {
        // highlight all tiles where unit can shoot
        for (int i = 0; i < availableRange.Count; i++)
        {
            tiles[availableRange[i].x, availableRange[i].y].layer = LayerMask.NameToLayer("HighlightRange");
        }
    }


    private void RemoveHighlighTile()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
     
        availableMoves.Clear();

    }
    private void RemoveHighlighRangeTile()
    {
        for (int i = 0; i < availableRange.Count; i++)
        {
            tiles[availableRange[i].x, availableRange[i].y].layer = LayerMask.NameToLayer("Tile");
        }

        availableRange.Clear();

    }

    // Positioning
    private void PositionAllUnits()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (unitsOnBoard[x, y] != null)
                {
                    PositionSingleUnit(x, y, true);
                }
           }
        }
    }

    private void PositionSingleUnit(int x, int y, bool force = false)
    {
        unitsOnBoard[x, y].currentX = x;
        unitsOnBoard[x, y].currentY = y;
        unitsOnBoard[x, y].SetPosition(GetTileCenter(x,y), force);
   }

   private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    // Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        // go through all available moves and if it's matching one, return true
        for (int i = 0; i < moves.Count; i++)
        {
            if(moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }

        return false;
    }
    private bool ContainsValidRange(ref List<Vector2Int> range, Vector2 pos)
    {
        // go through all available range and if it's matching one, return true
        for (int i = 0; i < range.Count; i++)
        {
            if (range[i].x == pos.x && range[i].y == pos.y)
            {
                return true;
            }
        }

        return false;
    }
    private void MoveTo(Unit u, int x, int y)
    {
        //if player has already used his one move
        if(countingMoves >= 1)
        {
            StartCoroutine(ShowTextWarningOnScreen("You've already moved", 2.0f)); // show warning text on screen for duration
            return;
        }

        //if unit's chose his own spot 
        if(u.currentX == x && u.currentY == y)
        {
            return;
        }

        //if unit's move isn't avaialble, return
        if(!ContainsValidMove(ref availableMoves, new Vector2Int(x,y)))
        {
            StartCoroutine(ShowTextWarningOnScreen("You can't move there!", 2.0f));
            return;
        }

        Vector2Int previousPosition = new Vector2Int(u.currentX, u.currentY);

        // Is there another unit on the target position?
        if (unitsOnBoard[x, y] != null)
        {
            return; 
        }

        unitsOnBoard[x, y] = u; // update new position in units array
        unitsOnBoard[previousPosition.x, previousPosition.y] = null; // reset prevous position in units array

        PositionSingleUnit(x, y);

        countingMoves++;
    }
    private void Attack(Unit u, int x, int y)
    {
        //if player has already used his one attack
        if (countingAttacks >= 1)
        {
            StartCoroutine(ShowTextWarningOnScreen("You've already attacked", 2.0f)); // show warning text on screen for duration
            return;
        }
        //if player chose own spot for attack
        if (u.currentX == x && u.currentY == y)
        {
            return;
        }
        //if unit's can't attack there return
        if (!ContainsValidRange(ref availableRange, new Vector2Int(x, y)))
        {
            StartCoroutine(ShowTextWarningOnScreen("You can't attack there", 2.0f));
            return;
        }

        // if there is another unit at targeted tile
        if (unitsOnBoard[x, y] != null)
        {
            Unit otherU = unitsOnBoard[x, y];

            if (otherU.team == u.team) // if it's the same team, don't attack
            {
                StartCoroutine(ShowTextWarningOnScreen("You can't attack your teammates", 2.0f));
                return;
            }
            if (otherU.team != u.team) // if it's enemy team, attack
            {
                if (otherU.team == 0) // if white team is attacking
                {
                    Debug.Log("Attack!!!");
                    otherU.health -= u.damage;
                    if (otherU.health <= 0) // unit dies
                    {
                        unitsOnBoard[otherU.currentX, otherU.currentY] = null; // remove unit from list
                        //otherU.fireDeathParticle.Play(); // play fire animation


                        otherU.fireDeathParticle.Stop();

                        //send unit to right part of the board
                        deadWhites.Add(otherU);
                        otherU.SetScale(Vector3.one * deathSize); // set scale of unit when it dies
                        otherU.SetPosition(new Vector3(10 * tileSize, yOffset, -1 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2) // move to the center of the tile
                            + (Vector3.forward * deathSpacing) * deadWhites.Count); // direction where the units go
                    }
                }
                else // if black team is attacking
                {
                    StartCoroutine(ShowTextWarningOnScreen("Attack", 2.0f));
                    otherU.health -= u.damage;
                    if (otherU.health <= 0) // unit dies
                    {
                        unitsOnBoard[otherU.currentX, otherU.currentY] = null; // remove unit from list

                        // send unit to right part of the board
                        deadBlacks.Add(otherU);
                        otherU.SetScale(Vector3.one * deathSize); // set scale of unit when it dies
                        otherU.SetPosition(new Vector3(-1 * tileSize, yOffset, 10 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2) // move to the center of the tile
                            + (Vector3.back * deathSpacing) * deadWhites.Count); // direction where the units go
                    }
                }
            }
        }


        countingAttacks++;

    }
    private Vector2Int LookupTileIndex(GameObject hitinfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] == hitinfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return -Vector2Int.one; // If loop doesn't find tile - break game, should never happens
    }

    IEnumerator ShowTextWarningOnScreen(string text, float duration)
    {
        textWarnings.gameObject.SetActive(true);
        textWarnings.text = text;

        yield return new WaitForSeconds(duration);

        textWarnings.gameObject.SetActive(false);
        textWarnings.text = "";

    }

}
