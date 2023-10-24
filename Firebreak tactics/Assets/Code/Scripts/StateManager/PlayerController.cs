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
    [SerializeField] private GameObject tutorial;
    [SerializeField] private InputActionReference Keyboard, Click;
    [SerializeField] private float cameraMoveSpeed = 5f;
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private float edgeScrollThreshold = 20f;
    [SerializeField] private GameObject settings;
    [SerializeField] private Canvas UnitUI;
    [SerializeField] private Canvas WaterUI;
    [SerializeField] private float maxZoom = 60.0f;
    [SerializeField] private float minZoom = 20.0f;

    private GameManager.GameState currentState;
    private Ray ray; 
    private GameObject previousTile = null;
    private GameObject selectedTile = null;
    private GameObject upTile = null;
    private GameObject downTile = null;
    private GameObject clickedUnit = null;
    private string actionRejected = "";
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
        if (Input.GetKeyDown(KeyCode.Escape)){
            Debug.Log("escape");
            if (settings.activeSelf){
                settings.SetActive(false);
            }
            else{
                settings.SetActive(true);
            }
        }

        if (Mouse.current.leftButton.wasPressedThisFrame){
            handleLeftClick();
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            handleRightClick();
        }

        if (movingUnit)
        {
            MoveUnit();
        }
        else if (currentState == GameManager.GameState.PlayerTurn && !tutorial.activeSelf){
        // if it's the player's turn

        checkCameraMovement();
        
            ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)){
            // if player has their mouse over a tile
                GameObject selected = hit.collider.gameObject;
                TileBehaviour script = selected.GetComponent<TileBehaviour>();

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

    private void handleLeftClick(){
        // checks a left click user input to see if the player is attempting an interaction or making
        // a random input 

        ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)){
            GameObject input = hit.collider.gameObject; // tile clicked

            if (input.GetComponent<TileBehaviour>().IsOccupied()){
                // if the left click input is made to a unit 
                // will determine if it's a tanker action or a select action 

                clickedUnit = unitManager.GetUnitOnTile(input); // unit of the tile  
                displayUnitInterface(input);
            }
        }        
    }

    private void displayUnitInterface(GameObject tile){
        // open the Unit Selection interface and let the user make a command 
        selectUnit(tile.GetComponent<TileBehaviour>(), true);
        displayUnitUI(true, tile.GetComponent<TileBehaviour>().GetOccupyingUnit());
        displayWaterUI(false, null);
    }

    private void LeftClick(){
        // determines what interaction is being made to the unit using left click
        // called by handleLeftClick

        if (upTile.GetComponent<TileBehaviour>().IsOccupied()){
        // if the click is on a unit 
            GameObject upUnit = unitManager.GetUnitOnTile(upTile);      
            if (clickedUnit && clickedUnit != upUnit){
                // if a unit was clicked and then a new unit is clicked
                // specifically for tanker unit refilling other units

                //if next to other vehicle
                Vector3Int depPos = clickedUnit.GetComponent<UnitBehaviour>().getCellPos();
                Vector3Int targetPos = upUnit.GetComponent<UnitBehaviour>().getCellPos();

                if (GetDistance(depPos, targetPos) == 1)
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

        closeInteraction();
    }

    private void handleRightClick(){

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

    private void selectUnit(TileBehaviour script, bool select){
        script.selectTile(select);
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

    private void displayWaterUI(bool active, GameObject tile){
        RectTransform unitCanvas = WaterUI.GetComponent<RectTransform>();
        WaterUI.enabled = active;
        if (tile){
            TileBehaviour tileScript = tile.GetComponent<TileBehaviour>();
            Transform tilename = WaterUI.transform.Find("tile");
            Transform actions = WaterUI.transform.Find("actions");
            TMP_Text tileText = tilename.GetComponent<TMP_Text>();
            TMP_Text actionsText = actions.GetComponent<TMP_Text>();

            if (tileText != null)
            {
                tileText.text = "water tile";
            }

            if (actionsText != null)
            {
                if (tileScript.getCapacity() > 0){
                    actionsText.text = "capacity: " +tileScript.getCapacity();
                }
                else{
                    actionsText.text = "empty";
                }
            }
        }
        unitCanvas.gameObject.SetActive(active);
        unitCanvas.position = Input.mousePosition;
    }

    public void interactButton(string button){
        // reads inputs from the user interaction interface
        Debug.Log(button);
        interfaceHandler(button);
    }

    private void interfaceHandler(string button){
        // processes inputs from the user interaction interface

        UnitBehaviour unitScript = clickedUnit.GetComponent<UnitBehaviour>();
        switch(button){
            case "A":
                Debug.Log("A");
                actionHandler(checkInteraction());
                break;
            case "B":
                Debug.Log("B");
                moveCommand();
                break;
            case "C":
                Debug.Log("C");
                refillCommand();
                break;
            default: 
                break;
        }
    }

    private string checkInteraction(){
        // determines what interaction the user commanded
        
        UnitBehaviour unitScript = clickedUnit.GetComponent<UnitBehaviour>();    
        if (unitScript.getSupport()){
            return "support";
        }
        else if (unitScript.getExtinguish()){
            return "extinguish";
        }
        else if (unitScript.getPreventative()){
            return "foam";
        }
        else{
            return "";
        }
    }

    private void actionHandler(string action){
    // specifically handles support, extinguish, or foam action
    
        switch(action){
            case "support":
                // tanker function
                break;
            case "extinguish":
                // extinguish function
                break;
            case "foam":
                // foam function
                break;
            default:
                break;
        }

    }

    private void moveCommand(){
        // issues the move command 
        if (!moveAction && !movingUnit){
        // selecting a unit
            selectUnit(clickedUnit);
            moveAction = true;
            UnitBehaviour unitScript = clickedUnit.GetComponent<UnitBehaviour>();
            highlightMovementTiles(unitScript.GetOccupyingTile().GetComponent<TileBehaviour>(),
                unitScript.GetMovements(), 
                true);
        }
    }

    private void refillCommand(){

    }

    private void displayUnitUI(bool active, GameObject unit){
        RectTransform unitCanvas = UnitUI.GetComponent<RectTransform>();
        UnitUI.enabled = active;
        if (unit){

            UnitBehaviour unitScript = unit.GetComponent<UnitBehaviour>();

            Transform displayA = UnitUI.transform.Find("displayA");
            TMP_Text displayAText = displayA.GetComponent<TMP_Text>();

            Transform displayB = UnitUI.transform.Find("displayB");
            TMP_Text displayBText = displayB.GetComponent<TMP_Text>();

            Transform displayC = UnitUI.transform.Find("displayC");
            TMP_Text displayCText = displayC.GetComponent<TMP_Text>();

            Transform displayD = UnitUI.transform.Find("displayD");
            TMP_Text displayDText = displayD.GetComponent<TMP_Text>();

            displayAText.text = "movements: " + unitScript.getMovements();

            if (unitScript.getSupport() || unitScript.getPreventative() || unitScript.getExtinguish()){
                // if the unit is anything but a scout
                unitCanvas.sizeDelta = new Vector2(unitCanvas.sizeDelta.x, 175f);

                displayBText.text = "range: " + unitScript.getRange();
                displayCText.text = "actions: " + unitScript.getActions();

                if (unitScript.getWater() > 0){
                    displayDText.text = "water: " + unitScript.getWater() + "/" + unitScript.getCapacity();
                }
                else{
                    displayDText.text = "water: empty";
                }
                
            }
            else{
                // scouts don't need to display range, actions, or water

                unitCanvas.sizeDelta = new Vector2(unitCanvas.sizeDelta.x, 130f);

                displayBText.text = "";
                displayCText.text = "";
                displayDText.text = "";
            }
        }
        unitCanvas.gameObject.SetActive(active);

        // mouse offset with Unit Button interface below unit
        Vector3 desiredPosition = Input.mousePosition + new Vector3(0f, -unitCanvas.rect.height+50f, 0f); 

        // clamp to the bounds of the screen to not overlap existing UI
        Vector3 clampedPosition = new Vector3(
            Mathf.Clamp(desiredPosition.x, 150f, Screen.width - unitCanvas.rect.width - 100f),
            Mathf.Clamp(desiredPosition.y, unitCanvas.rect.width - 90f, Screen.height - unitCanvas.rect.height),
            desiredPosition.z
        );

        unitCanvas.position = clampedPosition;
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
                    highlightUnit(selected.GetComponent<TileBehaviour>(), true);
                }
                else if (selected.name == "Water"){
                    displayWaterUI(true, selected);
                }
                else{
                    //highlightUnit(script, false);
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

    private void HandleMouseDown(InputAction.CallbackContext context){
        // fired when left or right mouse button down
        
        if (currentState == GameManager.GameState.PlayerTurn){
            if (context.action == Click.action){
                ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit)){
                
                    downTile = hit.collider.gameObject;

                
                    if (Mouse.current.rightButton.ReadValue() == 1f){
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

    private void closeInteraction(){
        actionRejected = "";
        clickedUnit = null;
        upTile = null;
        downTile = null;
        moveAction = false;
    }

    private void handleTileInteraction(){
    // determines action type
        TileBehaviour tileScript = upTile.GetComponent<TileBehaviour>();
        UnitBehaviour unitScript = clickedUnit.GetComponent<UnitBehaviour>();

        //print(GetDistance(unitScript.getCellPos(), tiles.WorldToCell(upTile.transform.position)));

        if (moveAction) 
        {
            
            if (upTile.name != "Fire" && upTile.name != "Water" && !tileScript.IsOccupied() && !tileScript.isBaseTile())
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

                //if in range
                Vector3Int depPos = unitScript.getCellPos();
                Vector3Int targetPos = tiles.WorldToCell(upTile.transform.position);

                if (GetDistance(depPos, targetPos) <= unitScript.getRange())
                    unitManager.interactFire(clickedUnit, upTile);
            }
            else{
                actionRejected = "unit has no water";
            }  
        }
        else if (upTile.name == "Water"){
        // water tile (refill)
            if (tileScript.getCapacity() > 0 && (unitScript.getWater() < unitScript.getCapacity())){
                // if the tile has water left 

                //if next to water
                Vector3Int depPos = unitScript.getCellPos();
                Vector3Int targetPos = tiles.WorldToCell(upTile.transform.position);

                if (GetDistance(depPos, targetPos) == 1)
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
        //convert coords to axial
        Vector2Int depAxial = OffsetToAxial(dep);
        Vector2Int targAxial = OffsetToAxial(target);

        //get axial distance
        return (Mathf.Abs(depAxial.x - targAxial.x)
            + Mathf.Abs(depAxial.x + depAxial.y - targAxial.x - targAxial.y)
            + Mathf.Abs(depAxial.y - targAxial.y)) / 2;
    }

    public Vector2Int OffsetToAxial(Vector3Int hex)
    {
        int q = hex.x - (hex.y - (hex.y % 2)) / 2;
        int r = hex.y;

        return new Vector2Int(q, r);
    }
}
