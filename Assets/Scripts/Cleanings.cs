using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cleanings : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Cleaning")
        {
            Destroy(gameObject);
            Debug.Log("cleaned");
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
