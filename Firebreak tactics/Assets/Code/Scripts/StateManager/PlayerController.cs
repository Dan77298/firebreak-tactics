using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private bool hasControl = false;
    [SerializeField] private Grid tiles; 
    [SerializeField] private TileManager tileManager;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private InputActionReference Keyboard, Click;

    private GameManager.GameState currentState;
    private float mouseButtonValue;
    private Ray ray; 
    private GameObject selectedTile;

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
            Debug.Log("up");
        }
        if (currentState == GameManager.GameState.PlayerTurn){
        // if it's the player's turn


            if (!Input.GetMouseButton(0)){
            // if mouse button isn't down
                ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit)){
                // if player has their mouse over a tile
                    GameObject selected = hit.collider.gameObject;

                    if (selected != selectedTile && belongsTo(selected, tiles.transform)){
                    // new tile selected 
                        tileManager.highlightTile(selectedTile);
                        tileManager.highlightTile(selected);
                        selectedTile = selected;
                    }
                }
                else{
                // no selected tile 
                    if (selectedTile){
                        tileManager.highlightTile(selectedTile);
                    }
                    selectedTile = null;
                }  
            }   
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

                        }
                    }
                }
            }
        }
    }

    private bool belongsTo(GameObject obj, Transform gridTransform)
    {
        return obj.transform.IsChildOf(gridTransform);
    }

    private void GameStateChanged(GameManager.GameState newState)
    {
        currentState = newState;
    }
}
