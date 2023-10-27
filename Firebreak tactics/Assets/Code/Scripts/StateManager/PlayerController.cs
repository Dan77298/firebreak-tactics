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
    [SerializeField] private GameObject prompt;
    [SerializeField] private Canvas UnitUI;
    [SerializeField] private Canvas WaterUI;
    [SerializeField] private float maxZoom = 60.0f;
    [SerializeField] private float minZoom = 20.0f;

    private GameManager.GameState currentState;
    private Ray ray; 
    private GameObject previousTile = null;
    private GameObject selectedTile = null;
    private GameObject clickedUnit = null;
    private GameObject upTile = null;
    private string actionType = "";
    private string actionRejected = "";
    private ControllerInput controllerInput;

    private bool moveAction = false;
    private bool selecting = false; // tracks whether the player is performing an action
    private List<Vector2Int> movePath = null;
    private bool movingUnit = false;
    private GameObject unitToMove = null; //reference to clickedUnit when move made as
                                          //clickedUnit sets to null as a bug outside of move update
                                          //for unknown reasons
    private float moveTime = 0.5f;
    private float moveTimeElapsed = 0f;

    private Vector2 camMoveVector;
    private bool moveCam = false;



    private void Awake()
    {
        controllerInput = new ControllerInput();
        controllerInput.Enable();

        GameManager.OnGameStateChanged += GameStateChanged;

        controllerInput.Controls.Move.performed += OnMovementPerformed;
        controllerInput.Controls.Move.canceled += OnMovementCanceled;


    }

    private void OnDestroy()
    {
        controllerInput.Disable();

        GameManager.OnGameStateChanged -= GameStateChanged;

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

    private void handleLeftClick()
    {
        // checks a left click user input to see if the player is attempting an interaction or making
        // a random input 

        ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GameObject newTile = hit.collider.gameObject; // tile clicked

            if (!movingUnit){
                if (newTile.GetComponent<TileBehaviour>().IsOccupied()){
                    // if the click is on a unit 
                    if (clickedUnit != null && selecting){
                        // if an interaction is being made 
                        if (clickedUnit != unitManager.GetUnitOnTile(newTile)){
                            // for any other unit than the first unit
                            Vector3Int depPos = clickedUnit.GetComponent<UnitBehaviour>().getCellPos();
                            Vector3Int targetPos = unitManager.GetUnitOnTile(newTile).GetComponent<UnitBehaviour>().getCellPos();

                            if (GetDistance(depPos, targetPos) == 1)
                            {
                                handleUnitInteraction(newTile);
                                closeAction();
                            }
                        }
                        else
                        {
                            clickedUnit = unitManager.GetUnitOnTile(newTile); // unit of the tile  
                            displayUnitInterface(newTile);
                        }
                    }
                    else if (clickedUnit == null){
                        // no unit has been selected yet 
                        clickedUnit = unitManager.GetUnitOnTile(newTile); // unit of the tile  
                        displayUnitInterface(newTile);
                    }
                }
                else
                {
                    if (clickedUnit && selecting)
                    {
                        // if the click is on a tile 
                        highlightMovementTiles(clickedUnit.GetComponent<UnitBehaviour>().GetOccupyingTile().GetComponent<TileBehaviour>(), 
                        50, false);
                        handleTileInteraction(newTile);
                        if (!movingUnit){
                            // end all actions other than move actions, they end elsewhere
                            selecting = false;
                            clickedUnit = null;
                            prompt.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    private void displayUnitInterface(GameObject tile){
        // open the Unit Selection interface and let the user make a command 
        newAction();
        selectUnit(tile.GetComponent<TileBehaviour>(), true);
        positionUnitUI();
        displayUnitUI(true, tile.GetComponent<TileBehaviour>().GetOccupyingUnit());
        displayWaterUI(false, null);
    }

    private void LeftClick(){
        // determines what interaction is being made to the unit using left click
        // called by handleLeftClick

        if (upTile.GetComponent<TileBehaviour>().IsOccupied()){
        // if the click is on a unit 
            GameObject upUnit = unitManager.GetUnitOnTile(upTile);      
            if (clickedUnit != null && clickedUnit != upUnit){
                // if a unit was clicked and then a new unit is clicked
                // specifically for tanker unit refilling other units

                //if next to other vehicle
                Vector3Int depPos = clickedUnit.GetComponent<UnitBehaviour>().getCellPos();
                Vector3Int targetPos = upUnit.GetComponent<UnitBehaviour>().getCellPos();

                if (GetDistance(depPos, targetPos) == 1)
                    handleUnitInteraction(upUnit);
            }
            else if (clickedUnit == null && upUnit){
            // if there's no unit selected 
                clickedUnit = upUnit;
            }        
        }
        else if (clickedUnit){
        // if a unit was previously clicked and a tile is clicked 
            //handleTileInteraction(null);
        }  
    }

    private void handleRightClick(){
        // handles right click inputs 
        if (selecting && !movingUnit){
            // if the user is performing an action (issues an action cancel)
            newAction();
            displayUnitUI(true, clickedUnit);
        }
        else if (clickedUnit && !movingUnit){
            // if the interface is open 
            displayUnitUI(false, null);
            clickedUnit = null;
        }
    }

    private void MoveUnit()
    {
        moveTimeElapsed -= Time.deltaTime;

        if (moveTimeElapsed < 0f)
        {
            prompt.SetActive(false);
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
                closeAction();
                clickedUnit = null;
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

    private void highlightMovementTiles(TileBehaviour origin, int range, bool show){
        Debug.Log("highlight : " + show);
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
                    if (actionType == "extinguish"){
                        Debug.Log(actionType);
                        tileBehaviour.highlightFireTile(show);
                    }
                    else{
                        tileBehaviour.highlightTile(show);
                    }
                    
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
                            if (actionType == "move"){
                               if ((candidate.GetTraversalRule() == 1 || candidate.GetTraversalRule() == 2) && !candidate.GetOnFire())
                                toAdd.Add(candidate); 
                            }
                            else if (actionType == "extinguish"){
                                if (candidateGO.name == "Fire" && candidate.GetOnFire()){
                                    toAdd.Add(candidate);
                                }
                            }
                            else if (actionType == "foam"){
                                if (candidateGO.name != "Fire" && candidateGO.name != "Water"){
                                    toAdd.Add(candidate);
                                }
                            }
                            else if (actionType == "support"){
                                if (candidate.IsOccupied() && origin != candidateGO){
                                    toAdd.Add(candidate);
                                }
                            }
                            else if (actionType == "refill"){
                                if (candidateGO.name == "Water"){
                                    toAdd.Add(candidate);
                                }
                            }
                            
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
            Transform actions = WaterUI.transform.Find("actions");
            TMP_Text actionsText = actions.GetComponent<TMP_Text>();

            if (tileScript.getCapacity() > 0){
                actionsText.text = "" + tileScript.getCapacity();
            }
            else{
                actionsText.text = "";
            }
        }
        unitCanvas.gameObject.SetActive(active);
        unitCanvas.position = Input.mousePosition;
    }

    public void interactButton(string button){
        // reads inputs from the user interaction interface
        interfaceHandler(button);
    }

    private void interfaceHandler(string button){
    // processes inputs from the user interaction interface

    selectingAction(); // sets prompt, removes interface, shows grid 
    UnitBehaviour unitScript = clickedUnit.GetComponent<UnitBehaviour>();
    switch(button){
        case "A":
            moveCommand();
            break;
        case "B":
            actionHandler(checkInteraction(clickedUnit));               
            break;
        case "C":
            adjacentCommand();
            break;
        default: 
            break;
    }
}

    private void selectingAction(){
        // handles action selection via unit interface 
        selecting = true;
        displayUnitUI(false, null);

        // Assuming that prompt is the GameObject containing the RectTransform.
        prompt.SetActive(true);
    }

    private void newAction(){
        if (clickedUnit){
            // erase any pre-existing radius tiles if a unit is still pre-selected
          highlightMovementTiles(clickedUnit.GetComponent<UnitBehaviour>().GetOccupyingTile().GetComponent<TileBehaviour>(), 
            50, false);  
        }
        
        movingUnit = false;
        moveAction = false;
        selecting = false;
        displayUnitUI(false, null);
        prompt.SetActive(false);
    }

    private void closeAction(){
        // handles action closure via unit interface
        Debug.Log("closeAction");
        newAction();
        actionType = "";
        actionRejected = "";
        clickedUnit = null;        
    }

    private string checkInteraction(GameObject unit){
        // determines what interaction the user commanded
        
        UnitBehaviour unitScript = unit.GetComponent<UnitBehaviour>();    
        if (unitScript.getSupport()){
            actionType = "support";
            return "support";
        }
        else if (unitScript.getExtinguish()){
            actionType = "extinguish";
            return "extinguish";
        }
        else if (unitScript.getPreventative()){
            actionType = "foam";
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
                rangeCommand();
                break;
            case "extinguish":
                rangeCommand();
                break;
            case "foam":
                rangeCommand();
                break;
            default:
                break;
        }

    }

    private void moveCommand(){
        // display the movement grid 
        selectUnit(clickedUnit);
        actionType = "move";
        moveAction = true;
        UnitBehaviour unitScript = clickedUnit.GetComponent<UnitBehaviour>();
        highlightMovementTiles(unitScript.GetOccupyingTile().GetComponent<TileBehaviour>(),
            unitScript.GetMovements(), 
            true);
    }

    private void rangeCommand(){
        // display the range grid
        UnitBehaviour unitScript = clickedUnit.GetComponent<UnitBehaviour>();
        highlightMovementTiles(unitScript.GetOccupyingTile().GetComponent<TileBehaviour>(),
                unitScript.getRange(), 
                true);
    }

    private void adjacentCommand(){
        // display the adjacent grid 
        actionType = "refill";
        UnitBehaviour unitScript = clickedUnit.GetComponent<UnitBehaviour>();
        highlightMovementTiles(unitScript.GetOccupyingTile().GetComponent<TileBehaviour>(),
                1, 
                true);
    }

    private void positionUnitUI(){
        // mouse offset with Unit Button interface below unit
        RectTransform unitCanvas = UnitUI.GetComponent<RectTransform>();

        Vector3 desiredPosition = Input.mousePosition + new Vector3(0f, -unitCanvas.rect.height+50f, 0f); 

        // clamp to the bounds of the screen to not overlap existing UI
        Vector3 clampedPosition = new Vector3(
            Mathf.Clamp(desiredPosition.x, 150f, Screen.width - unitCanvas.rect.width - 100f),
            Mathf.Clamp(desiredPosition.y, unitCanvas.rect.width - 90f, Screen.height - unitCanvas.rect.height),
            desiredPosition.z
        );

        unitCanvas.position = clampedPosition;
    }

    private void displayUnitUI(bool active, GameObject unit){
        RectTransform unitCanvas = UnitUI.GetComponent<RectTransform>();
        UnitUI.enabled = active;
        if (unit){
            
            UnitBehaviour unitScript = unit.GetComponent<UnitBehaviour>();  

            TMP_Text unitName = UnitUI.transform.Find("Title").GetComponent<TMP_Text>();
            TMP_Text displayAText = UnitUI.transform.Find("displayA").GetComponent<TMP_Text>();
            TMP_Text displayBText = UnitUI.transform.Find("displayB").GetComponent<TMP_Text>();
            TMP_Text displayCText = UnitUI.transform.Find("displayC").GetComponent<TMP_Text>();
            TMP_Text displayDText = UnitUI.transform.Find("displayD").GetComponent<TMP_Text>();
            Transform interactA = UnitUI.transform.Find("ActionA");
            TMP_Text ActionAText = interactA.Find("description").GetComponent<TMP_Text>();
            Transform interactB = UnitUI.transform.Find("ActionB");
            TMP_Text ActionBText = interactB.Find("description").GetComponent<TMP_Text>();
            Transform interactC = UnitUI.transform.Find("ActionC");
            TMP_Text ActionCText = interactC.GetComponent<TMP_Text>();

            unitName.text = unit.name + " unit"; 

            // display texts
            displayAText.text = "movements: " + unitScript.getMovements();
            if (unitScript.getMovements() > 0){
                displayAText.color = Color.white;
                ActionAText.color = Color.white;
            }
            else{
                displayAText.color = Color.red;
                 ActionAText.color = Color.red;
            }

            if (unitScript.getSupport() || unitScript.getPreventative() || unitScript.getExtinguish()){
                // if the unit is anything but a scout
                unitCanvas.sizeDelta = new Vector2(unitCanvas.sizeDelta.x, 175f);

                displayBText.text = "range: " + unitScript.getRange();
                displayCText.text = "actions: " + unitScript.getActions();
                if (unitScript.getActions() > 0 || (unitScript.getWater() <= 0 && unitScript.getSupport())){
                    displayCText.color = Color.white;
                     ActionBText.color = Color.white;
                }
                else{
                    displayCText.color = Color.red;
                    ActionBText.color = Color.red;
                }

                if (unitScript.getWater() > 0){
                    displayDText.text = "water: " + unitScript.getWater() + "/" + unitScript.getCapacity();
                    displayDText.color = Color.white;
                }
                else{
                    displayDText.text = "water: empty";
                    displayDText.color = Color.red;
                }
                
            }
            else{
                // scouts don't need to display range, actions, or water

                unitCanvas.sizeDelta = new Vector2(unitCanvas.sizeDelta.x, 130f);

                displayBText.text = "";
                displayCText.text = "";
                displayDText.text = "";
            }

            //action texts
            ActionBText.text = checkInteraction(unit);

            if (unitScript.getMovements() <= 0){
                // if unit is out of moves
                interactA.gameObject.SetActive(false);
            }
            else{
                interactA.gameObject.SetActive(true);
            }

            if ((checkInteraction(unit) == "" || unitScript.getActions() <= 0) || (unitScript.getSupport() && unitScript.getWater() <= 0)){
                // if the unit can't perform an action, or is out of actions
                // if the tanker unit is out of water
                interactB.gameObject.SetActive(false);
            }
            else{
                interactB.gameObject.SetActive(true);
            }

            if (unitManager.canRefill(unit)){
                // if the unit can refill from water
                interactC.gameObject.SetActive(true);
            }
            else{
                interactC.gameObject.SetActive(false);
            }
        }
        unitCanvas.gameObject.SetActive(active);
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
                    displayWaterUI(false, null);
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

    private void handleUnitInteraction(GameObject upUnit){
        // unit to unit interaction
        GameObject unit = unitManager.GetUnitOnTile(upUnit);

        if (clickedUnit.GetComponent<UnitBehaviour>().getSupport()){
        // if the interaction is to refill another unit 
            unitManager.interactUnit(clickedUnit, upUnit);
        }   
    }

    private void handleTileInteraction(GameObject tile){
    // determines action type

        if (clickedUnit != null){
            UnitBehaviour unitScript = clickedUnit.GetComponent<UnitBehaviour>();
            TileBehaviour tileScript = tile.GetComponent<TileBehaviour>();

            if (moveAction) 
            {        
                if (tile.name != "Fire" && tile.name != "Water" && !tileScript.IsOccupied() && !tileScript.isBaseTile())
                {
                    Debug.Log("move tile");
                    //get distance
                    Vector3Int depPos = unitScript.getCellPos();
                    Vector3Int targetPos = tiles.WorldToCell(tile.transform.position);
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
            else if (tile.name == "Fire"){
            // fire tile
                if (unitScript.getWater() > 0){
                    // if the unit has water 

                    //if in range
                    Vector3Int depPos = unitScript.getCellPos();
                    Vector3Int targetPos = tiles.WorldToCell(tile.transform.position);

                    if (GetDistance(depPos, targetPos) <= unitScript.getRange())
                        unitManager.interactFire(clickedUnit, tile);
                }
                else{
                    actionRejected = "unit has no water";
                }  
            }
            else if (tile.name == "Water"){
            // water tile (refill)
                if (tileScript.getCapacity() > 0 && (unitScript.getWater() < unitScript.getCapacity())){
                    // if the tile has water left 

                    //if next to water
                    Vector3Int depPos = unitScript.getCellPos();
                    Vector3Int targetPos = tiles.WorldToCell(tile.transform.position);

                    if (GetDistance(depPos, targetPos) == 1)
                        unitManager.interactTile(clickedUnit, tile);  
                }
            }
            else if (tile.name != "Road" && tile.name != "Burned"){
            // regular tile 
                unitManager.interactTile(clickedUnit, tile);
            }
            else{
                actionRejected = "tile cannot be interacted with"; // check where this occurs, make edge cases for it 
            }
        }
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
            closeAction();
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
