using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{

    int burgersMade = 0;
    public TextMeshProUGUI burgerScore;



    public void MadeBurger()
    {
        burgersMade++;
        Debug.Log("Burgers Made:" + burgersMade);
        burgerScore.text = burgersMade.ToString();
    }

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
