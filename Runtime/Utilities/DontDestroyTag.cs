using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace utilities
{
    public class DontDestroyTag : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }


}


