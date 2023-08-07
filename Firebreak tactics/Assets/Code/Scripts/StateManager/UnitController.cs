using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public GameObject baseUnit;
    public GameObject striker;
    public GameObject foamUnit;
    public GameObject tanker;
    public GameObject scout;

    // Start is called before the first frame update
    void Start()
    {
        // instantiate the 4 unit prefabs 
        Instantiate(striker, transform.position, Quaternion.identity);
        Instantiate(foamUnit, transform.position, Quaternion.identity);
        Instantiate(tanker, transform.position, Quaternion.identity);
        Instantiate(scout, transform.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
