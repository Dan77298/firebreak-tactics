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

    private GameManager.GameState currentState;
    private float mouseButtonValue;
    private Ray ray; 
    private GameObject previousTile = null;
    private GameObject selectedTile = null;
    private Vector3 mouseStart;

    private void Awake()
    {
        GameManager.OnGameStateChanged += GameStateChanged;
        Click.action.performed += HandleMouseClick;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChanged;
        Click.action.performed -= HandleMouseClick;
    }

    private void Update(){
        if (Input.GetMouseButtonUp(0)){
        // LMB up
            releaseMouse();
        }

        if (currentState == GameManager.GameState.PlayerTurn){
        // if it's the player's turn
            ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)){
            // if player has their mouse over a tile
                GameObject selected = hit.collider.gameObject;
                TileBehaviour script = selected.GetComponent<TileBehaviour>();

                if (mouseButtonValue == 1f && selectedTile){
                // unit tracks mouse movement when mouse is down
                    moveWithMouse(unitManager.GetUnitOnTile(selectedTile));
                }
                if (selected != previousTile && belongsTo(selected, tiles.transform)){
                // new tile selected 
                    if (mouseButtonValue == 0f){
                    // LMB up
                        if (selected.GetComponent<TileBehaviour>().IsOccupied()){
                        // if the selected tile is occupied by a unit 
                            script.highlightTile(true);                          
                        }
                        else{
                            script.highlightTile(false); 
                        }
                        if (previousTile){
                            previousTile.GetComponent<TileBehaviour>().highlightTile(false);
                        }
                        previousTile = selected;  
                    }
                    else if (mouseButtonValue == 1f && selectedTile){
                    // LMB down
                        
                        if (!selected.GetComponent<TileBehaviour>().IsOccupied()){
                            script.selectTile(true);
                        }
                        else{
                            script.selectTile(false);
                        }
                        if (previousTile){
                            previousTile.GetComponent<TileBehaviour>().selectTile(false);
                        }
                        previousTile = selected;  
                    }
                }
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

    private void moveWithMouse(GameObject unit){
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

    private void HandleMouseClick(InputAction.CallbackContext context){

        if (currentState == GameManager.GameState.PlayerTurn){
            mouseButtonValue = Mouse.current.leftButton.ReadValue();
            Debug.Log("handlemouseClick");
            if (context.action == Click.action){
                
                if (mouseButtonValue == 1f){
                // LMB is down
                    ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit)){
                        GameObject selected = hit.collider.gameObject;

                        if (belongsTo(selected, tiles.transform)){
                        // tile is clicked
                            if (selected.GetComponent<TileBehaviour>().IsOccupied()){
                            // selecting a unit 
                                selectUnit(selected);
                            }
                        }
                    }
                }
            }
        }
    }

    private void releaseMouse(){
        ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)){
            GameObject selected = hit.collider.gameObject;

            if (belongsTo(selected, tiles.transform) && selectedTile){
            // mouse is released on a tile 
                GameObject unit = unitManager.GetUnitOnTile(selectedTile);
                unitManager.moveUnitToTile(unit, selected);
                selected.GetComponent<TileBehaviour>().highlightTile(false);
                CenterUnitToTile(unit, selected);
            }
        }
        selectedTile = null;
        previousTile = null;
        mouseButtonValue = 0f;
    }

    private void CenterUnitToTile(GameObject unit, GameObject tile)
    {
        Vector3 tilePosition = tile.transform.position;
        Vector3Int cellPos = units.WorldToCell(tilePosition); // Convert to cell position

        Vector3 gridPosition = units.transform.position; // Get Grid's position in world space
        Vector3 adjustedNewPosition = units.GetCellCenterWorld(cellPos) + new Vector3(gridPosition.x+0.1f, 0f, gridPosition.z-0.3f);

        adjustedNewPosition.y = unit.transform.position.y;

        unit.transform.position = adjustedNewPosition;
    }

    private void selectUnit(GameObject tile){
        Debug.Log("grabbed unit");
        selectedTile = tile;
    }

    private bool belongsTo(GameObject obj, Transform gridTransform)
    {
        return obj.transform.IsChildOf(gridTransform);
    }

    private void GameStateChanged(GameManager.GameState newState)
    {
        currentState = newState;
        if (currentState == GameManager.GameState.PlayerTurn){
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
