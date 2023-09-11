using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private bool hasControl = false;
    [SerializeField] private Grid tiles; 
    [SerializeField] private Grid units;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private InputActionReference Keyboard, Click;
    [SerializeField] private float cameraMoveSpeed = 5f;
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private float edgeScrollThreshold = 20f;
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

    private bool moveAction = true;



    private void Awake()
    {
        GameManager.OnGameStateChanged += GameStateChanged;
        Click.action.performed += HandleMouseDown;
        Keyboard.action.started += HandleTabKeyDown;
        Keyboard.action.canceled += HandleTabKeyUp;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChanged;
        Click.action.performed -= HandleMouseDown;
        
        Keyboard.action.started -= HandleTabKeyDown;
        Keyboard.action.canceled -= HandleTabKeyUp;
    }

    private void HandleTabKeyUp(InputAction.CallbackContext context)
    {
        // Check if the Tab key was released
        if (context.control.name == "tab")
        {
            // Tab key was released
            Debug.Log("Tab key up.");
        }
    }

    private void HandleTabKeyDown(InputAction.CallbackContext context)
    {
        // Check if the Tab key was pressed down
        if (context.control.name == "tab")
        {
            // Tab key was pressed down
            Debug.Log("Tab key down.");
        }
    }

    private void Update(){
        if (Mouse.current.leftButton.ReadValue() == 0f){
            handleMouseRelease();
        }
        checkCameraMovement();


        if (currentState == GameManager.GameState.PlayerTurn){
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
                    dragUnit(unitManager.GetUnitOnTile(selectedTile));
                }


                checkMouseOverTile(selected, script);
            }
            else{
            // no selected tile 
                if (previousTile){
                // if the selected tile is occupied by a unit 
                    previousTile.GetComponent<TileBehaviour>().highlightTile(false);
                }
                previousTile = null;
            }    
        }
    }

    private void checkMouseOverTile(GameObject selected, TileBehaviour script){
        if (selected != previousTile && belongsTo(selected, tiles.transform) && selected.name != "Fire"){
            // new tile selected 
            if (Mouse.current.leftButton.ReadValue() == 0f)
            {
                // LMB up
                if (selected.GetComponent<TileBehaviour>().IsOccupied())
                {
                    // if the selected tile is occupied by a unit 
                    script.highlightTile(true);
                }
                else
                {
                    script.highlightTile(false);
                }
                if (previousTile)
                {
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
                                moveAction = false;
                            }
                        }  
                    }
                    else if (Mouse.current.rightButton.ReadValue() == 1f){
                        // RMB is down
                        
                        unitManager.requestCancel(downTile);

                        if (belongsTo(downTile, tiles.transform)){
                        // tile is clicked
                            if (downTile.GetComponent<TileBehaviour>().IsOccupied()){
                            // selecting a unit
                                Debug.Log("selectUnit"); 
                                selectUnit(downTile);
                                moveAction = true;
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
                unitManager.moveUnitToTile(clickedUnit, upTile);
                CenterUnitToTile(clickedUnit, upTile);
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
        CenterUnitToTile(unit, upTile);
    }

    private void CenterUnitToTile(GameObject unit, GameObject tile){

        Vector3 tilePosition = tile.transform.position;
        Vector3Int cellPos = units.WorldToCell(tilePosition); 

        Vector3 gridPosition = units.transform.position;
        Vector3 adjustedNewPosition = units.GetCellCenterWorld(cellPos) + new Vector3(gridPosition.x+0.1f, 0f, gridPosition.z-0.3f);

        adjustedNewPosition.y = unit.transform.position.y;

        unit.transform.position = adjustedNewPosition;
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
}
