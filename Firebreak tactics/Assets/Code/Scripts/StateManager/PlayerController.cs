using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerController : MonoBehaviour
{
    private bool hasControl = false;
    [SerializeField] private Grid tiles; 
    [SerializeField] private Grid units;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private InputActionReference Keyboard, Click;
    [SerializeField] private float cameraMoveSpeed = 5f;
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private float edgeScrollThreshold = 20f;
    [SerializeField] private Canvas UnitUI;
    [SerializeField] private float maxZoom = 60.0f;
    [SerializeField] private float minZoom = 20.0f;

    private GameManager.GameState currentState;
    private Ray ray; 
    private GameObject previousTile = null;
    private GameObject selectedTile = null;
    private GameObject upTile = null;
    private GameObject downTile = null;
    private GameObject clickedUnit = null;
    private bool dragging = false;
    private string actionRejected = "";
    private Vector3 mouseStart;
    private ControllerInput controllerInput;

    private bool moveAction = false;
    private List<Vector2Int> movePath = null;
    private bool movingUnit = false;
    private GameObject unitToMove = null; //reference to clickedUnit when move made as
                                          //clickedUnit sets to null as a bug outside of move update
                                          //for unknown reasons
    private float moveTime = 1f;
    private float moveTimeElapsed = 0f;

    private Vector2 camMoveVector;
    private bool moveCam = false;



    private void Awake()
    {
        controllerInput = new ControllerInput();
        controllerInput.Enable();

        GameManager.OnGameStateChanged += GameStateChanged;
        Click.action.performed += HandleMouseDown;

        controllerInput.Controls.Move.performed += OnMovementPerformed;
        controllerInput.Controls.Move.canceled += OnMovementCanceled;


    }

    private void OnDestroy()
    {
        controllerInput.Disable();

        GameManager.OnGameStateChanged -= GameStateChanged;
        Click.action.performed -= HandleMouseDown;

        controllerInput.Controls.Move.performed -= OnMovementPerformed;
        controllerInput.Controls.Move.canceled -= OnMovementCanceled;
    }

    public void OnMovementPerformed(InputAction.CallbackContext value)
    {
        //directional pressed
        camMoveVector = value.ReadValue<Vector2>();
        moveCam = true;
    }

    public void OnMovementCanceled(InputAction.CallbackContext value)
    {
        //directional released
        camMoveVector = Vector2.zero;
        moveCam = false;
    }



    private void Update(){
        if (Mouse.current.leftButton.ReadValue() == 0f){
            handleMouseRelease();
        }
        
        checkCameraMovement();


        if (movingUnit)
        {
            MoveUnit();
        }
        else if (currentState == GameManager.GameState.PlayerTurn){
        // if it's the player's turn
            ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)){
            // if player has their mouse over a tile
                GameObject selected = hit.collider.gameObject;
                TileBehaviour script = selected.GetComponent<TileBehaviour>();


                if (Mouse.current.leftButton.ReadValue() == 1f && selectedTile)
                {
                    // unit tracks mouse movement when mouse is down
                    if (script.IsOccupied())
                        dragUnit(unitManager.GetUnitOnTile(selectedTile));
                }


                checkMouseOverTile(selected, script);
            }
            else{
            // no selected tile 
                if (previousTile){
                    // if the selected tile is occupied by a unit 
                    if (previousTile.GetComponent<TileBehaviour>().IsOccupied())
                        previousTile.GetComponent<TileBehaviour>().highlightTile(false);
                }
                previousTile = null;
            }    
        }
    }

    private void MoveUnit()
    {
        moveTimeElapsed -= Time.deltaTime;

        if (moveTimeElapsed < 0f)
        {
            //move to next tile
            moveTimeElapsed = moveTime;

            //travel path
            GameObject nextTile = gridManager.getTile(new Vector3Int(
                    movePath[0].x, movePath[0].y, 0));

            //print(movePath[0]);
            //print(nextTile);
            //print(unitToMove);

            unitManager.moveUnitToTile(unitToMove, nextTile);
            unitManager.CenterUnitToTile(unitToMove, nextTile);

            movePath.RemoveAt(0);

            if (movePath.Count == 0)
            {
                movingUnit = false;
            }
        }
    }

    private void highlightUnit(TileBehaviour script, bool select){
        script.highlightTile(select);
        // change logic to fit the new display/highlight system 
    }

    private void highlightMovementTiles(TileBehaviour origin, int range, bool show)
    {

        List<TileBehaviour> processed = new List<TileBehaviour>();
        List<TileBehaviour> toSearch = new List<TileBehaviour>();
        List<TileBehaviour> toAdd = new List<TileBehaviour>();
        int distance = -1;

        //y odd
        (int, int)[] oddneighbourRelativeCoords = new (int, int)[] { (-1, 0), (0, 1), (0, -1), (1, 1), (1, 0), (1, -1) };

        //y even
        (int, int)[] evenNeighbourRelativeCoords = new (int, int)[] { (-1, -1), (-1, 0), (-1, 1), (0, 1), (1, 0), (0, -1) };

        (int, int)[] relCoords;

        toSearch.Add(origin);

        while (toSearch.Count > 0)
        {
            distance++;

            if (distance > range)
                break;

            foreach (TileBehaviour tileBehaviour in toSearch)
            {
                Vector3Int currentPos = tileBehaviour.getCellPos();

                if (tileBehaviour != origin)
                    tileBehaviour.highlightTile(show);
                

                processed.Add(tileBehaviour);

                //add neighbours to tosearch
                if (currentPos.y % 2 == 0)
                    relCoords = evenNeighbourRelativeCoords;
                else relCoords = oddneighbourRelativeCoords;

                foreach ((int, int) coord in relCoords)
                {

                    GameObject candidateGO = gridManager.getTile(
                            new Vector3Int(currentPos.x + coord.Item1, currentPos.y + coord.Item2, 0)
                        );


                    if (candidateGO != null)
                    {
                        TileBehaviour candidate = candidateGO.GetComponent<TileBehaviour>();

                        if (!processed.Contains(candidate) &&
                            !toSearch.Contains(candidate) && !toAdd.Contains(candidate))
                        {
                            //rule check
                            if (candidate.GetTraversalRule() == 1 || candidate.GetTraversalRule() == 2)
                                toAdd.Add(candidate);
                        }
                    }
                }
            }

            toSearch.Clear();

            foreach (TileBehaviour tileBehaviour in toAdd)
                toSearch.Add(tileBehaviour);

            toAdd.Clear();

        }
    }

    private void displayUnitUI(bool active, GameObject unit){
        RectTransform unitCanvas = UnitUI.GetComponent<RectTransform>();

        if (unit){
            UnitBehaviour unitScript = unit.GetComponent<UnitBehaviour>();
            Transform water = UnitUI.transform.Find("water");
            Transform movement  = UnitUI.transform.Find("movement");
            Transform actions  = UnitUI.transform.Find("actions");
            Transform unitName  = UnitUI.transform.Find("unit");
            TMP_Text waterText = water.GetComponent<TMP_Text>();
            TMP_Text movementText = movement.GetComponent<TMP_Text>(); 
            TMP_Text actionsText = actions.GetComponent<TMP_Text>(); 
            TMP_Text unitText = unitName.GetComponent<TMP_Text>(); 

            if (waterText != null)
            {
                waterText.text = "water: [" +unitScript.getWater() + "/" + unitScript.getCapacity()+"]";
            }

            if (movementText != null)
            {
                movementText.text = "movements: " +unitScript.getMovements();
            }

            if (actionsText != null)
            {
                actionsText.text = "actions: " +unitScript.getActions();
            }

            if (unitText != null)
            {
                unitText.text = unit.name + " unit";
            }
        }
        unitCanvas.gameObject.SetActive(active);
        unitCanvas.position = Input.mousePosition;
    }

    private void checkMouseOverTile(GameObject selected, TileBehaviour script){

        if (movingUnit) return;

        if (selected != previousTile && belongsTo(selected, tiles.transform) && selected.name != "Fire"){
            // new tile selected 
            if (Mouse.current.leftButton.ReadValue() == 0f)
            {
                // LMB up
                if (selected.GetComponent<TileBehaviour>().IsOccupied()){
                // if the selected tile is occupied by a unit 
                    highlightUnit(script, true);
                    displayUnitUI(true, selected.GetComponent<TileBehaviour>().GetOccupyingUnit());
                }
                else{
                    //highlightUnit(script, false);
                    displayUnitUI(false, null);
                }
                if (previousTile){
                    if (previousTile.GetComponent<TileBehaviour>().IsOccupied())
                        previousTile.GetComponent<TileBehaviour>().highlightTile(false);
                        
                }   
                previousTile = selected;  
            }
            else if (Mouse.current.leftButton.ReadValue() == 1f && selectedTile)
            {
                // LMB down
                clickedUnit = null;
                if (!selected.GetComponent<TileBehaviour>().IsOccupied())
                {
                    script.selectTile(true);
                }
                else
                {
                    script.selectTile(false);
                }
                if (previousTile)
                {
                    previousTile.GetComponent<TileBehaviour>().selectTile(false);
                }
                previousTile = selected;
            }
        }
    }

    private void checkCameraMovement(){
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        if (moveCam)
        {
            cameraHolder.transform.Translate(new Vector3(camMoveVector.x, 0, camMoveVector.y) * cameraMoveSpeed * Time.deltaTime);
        }
        else
        {
            if (mousePosition.x <= edgeScrollThreshold)
            {
                cameraHolder.transform.Translate(Vector3.left * cameraMoveSpeed * Time.deltaTime);
            }
            else if (mousePosition.x >= screenSize.x - edgeScrollThreshold)
            {
                cameraHolder.transform.Translate(Vector3.right * cameraMoveSpeed * Time.deltaTime);
            }

            if (mousePosition.y <= edgeScrollThreshold)
            {
                cameraHolder.transform.Translate(Vector3.back * cameraMoveSpeed * Time.deltaTime);
            }
            else if (mousePosition.y >= screenSize.y - edgeScrollThreshold)
            {
                cameraHolder.transform.Translate(Vector3.forward * cameraMoveSpeed * Time.deltaTime);
            }
        }

        // Zooming with the scroll wheel
        float scrollWheel = Mouse.current.scroll.ReadValue().y * 0.1f;
        float zoomSpeed = 0.1f;

        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView - scrollWheel * zoomSpeed, minZoom, maxZoom);
    }


    private void dragUnit(GameObject unit){
    // make unit follow cursor 
        dragging = true;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)){
            Vector3 mousePosition3D = hit.point;
            
            if (mouseStart == Vector3.zero){
                mouseStart = mousePosition3D;
            }

            Vector3 offset = mousePosition3D - mouseStart;
            Vector3 newPosition = unit.transform.position + offset;
            newPosition.y = unit.transform.position.y;

            unit.transform.position = newPosition;

            mouseStart = mousePosition3D;
        }


        displayUnitUI(true, unit);
    }

    private void HandleMouseDown(InputAction.CallbackContext context){
        // fired when left or right mouse button down
        
        if (currentState == GameManager.GameState.PlayerTurn){
            if (context.action == Click.action){
                ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit)){
                
                    downTile = hit.collider.gameObject;

                    if (Mouse.current.leftButton.ReadValue() == 1f){
                        // LMB is down
                        if (belongsTo(downTile, tiles.transform)){
                        // tile is clicked
                            if (downTile.GetComponent<TileBehaviour>().IsOccupied()){
                            // selecting a unit
                                Debug.Log("selectUnit"); 
                                selectUnit(downTile);
                            }
                        }  
                    }
                    else if (Mouse.current.rightButton.ReadValue() == 1f){
                        // RMB is down
                        
                        unitManager.requestCancel(downTile);

                        if (belongsTo(downTile, tiles.transform)){
                        // tile is clicked
                            if (!moveAction && !movingUnit && downTile.GetComponent<TileBehaviour>().IsOccupied()){
                            // selecting a unit
                                Debug.Log("selectUnit"); 
                                selectUnit(downTile);
                                moveAction = true;


                                highlightMovementTiles(downTile.GetComponent<TileBehaviour>(),
                                    unitManager.GetUnitOnTile(downTile).GetComponent<UnitBehaviour>().GetMovements(), 
                                    true);

                            }
                        } 
                    }  
                }
            }
        }
    }

    private void handleMouseRelease(){
        ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)){
            upTile = hit.collider.gameObject;

            if (downTile == upTile){
            // mouse is released on same tile
                handleMouseClick();
            }
        }
        if (dragging){
            dropUnit();
        }
        mouseUp();
    }
        
    private void handleUnitInteraction(){

        // unit to unit interaction
        GameObject unit = unitManager.GetUnitOnTile(upTile);

        if (clickedUnit.GetComponent<UnitBehaviour>().getSupport()){
        // if the interaction is to refill another unit 
            unitManager.interactUnit(clickedUnit, upTile);
        }
        else{
        // if it's not to refill another unit, swap clickedUnits
            clickedUnit = unit; 
        }
        closeInteraction();   
    }

    private void mouseUp(){
        //selectedTile = null;
        //previousTile = null;
        mouseStart = Vector3.zero; 
        dragging = false;
    }

    private void closeInteraction(){
        actionRejected = "";
        dragging = false;  
        clickedUnit = null;
        upTile = null;
        downTile = null;
        moveAction = false;
    }

    private void handleTileInteraction(){
    // determines action type
        TileBehaviour tileScript = upTile.GetComponent<TileBehaviour>();
        UnitBehaviour unitScript = clickedUnit.GetComponent<UnitBehaviour>();

        if (moveAction) 
        {
            
            if (upTile.name != "Fire" && upTile.name != "Water")
            {
                //get distance
                Vector3Int depPos = unitScript.getCellPos();
                Vector3Int targetPos = tiles.WorldToCell(upTile.transform.position);

                if (GetDistance(depPos, targetPos) <= unitScript.GetMovements())
                {
                    //find path
                    GameObject depTile = gridManager.getTile(depPos);

                    movePath = gridManager.FindPath(depTile.GetComponent<TileBehaviour>(),
                        tileScript, unitScript.GetTraversalType());

                    moveAction = false;
                    movingUnit = true;
                    unitToMove = clickedUnit;

                    highlightMovementTiles(depTile.GetComponent<TileBehaviour>(), unitScript.GetMovements(), false);

                    unitScript.SetMovements(unitScript.GetMovements() -
                        depTile.GetComponent<TileBehaviour>().GetDistance(tileScript));

                    return;
                }
                
            }
        }
        else if (upTile.name == "Fire"){
        // fire tile
            if (unitScript.getWater() > 0){
            // if the unit has water 
                unitManager.interactFire(clickedUnit, upTile);
            }
            else{
                actionRejected = "unit has no water";
            }  
        }
        else if (upTile.name == "Water"){
        // water tile (refill)
            if (tileScript.getCapacity() >0 && (unitScript.getWater() < unitScript.getCapacity())){
            // if the tile has water left 
              unitManager.interactTile(clickedUnit, upTile);  
            }
        }
        else if (upTile.name != "Road" && upTile.name != "Burned"){
        // regular tile 
            unitManager.interactTile(clickedUnit, upTile);
        }
        else{
            actionRejected = "tile cannot be interacted with"; // check where this occurs, make edge cases for it 
        }
        closeInteraction();
    }


    private void handleMouseClick(){
    // mouse click
        if (upTile.GetComponent<TileBehaviour>().IsOccupied()){
        // if the click is on a unit 
            GameObject upUnit = unitManager.GetUnitOnTile(upTile);      
            if (clickedUnit && clickedUnit != upUnit){
            // if a unit was clicked and then a new unit is clicked
                handleUnitInteraction();
            }
            else if (clickedUnit == null && upUnit){
            // if there's no unit selected 
                clickedUnit = upUnit;
            }        
        }
        else if (clickedUnit){
        // if a unit was previously clicked and a tile is clicked 
            handleTileInteraction();
        }    
    }

    private void dropUnit(){
        // drop the unit on the tile

        GameObject unit = unitManager.GetUnitOnTile(downTile);
        unitManager.moveUnitToTile(unit, upTile);
        downTile.GetComponent<TileBehaviour>().highlightTile(false);
        unitManager.CenterUnitToTile(unit, upTile);
    }



    private void selectUnit(GameObject tile){
        selectedTile = tile;
    }

    private bool belongsTo(GameObject obj, Transform gridTransform){
        return obj.transform.IsChildOf(gridTransform);
    }

    private void GameStateChanged(GameManager.GameState newState){
    // set unit positions at start of turn 
        currentState = newState;
        if (currentState == GameManager.GameState.PlayerTurn){
            closeInteraction();
            foreach (Transform unitTransform in units.transform){
            // set all unit originPos
                if (unitTransform.tag == "Unit"){
                    UnitBehaviour unitBehaviour = unitTransform.GetComponent<UnitBehaviour>();
                    unitBehaviour.setOriginPos();
                }
    }
        }
    }

    public int GetDistance(Vector3Int dep, Vector3Int target)
    {
        return Mathf.Max(
                Mathf.Abs(target.y - dep.y),
                Mathf.Abs(target.y - dep.y),
                Mathf.Abs(target.y - dep.y)
            );
    }
}
