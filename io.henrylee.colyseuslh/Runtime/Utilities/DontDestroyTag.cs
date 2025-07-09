using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LhUtils
{
    public class DontDestroyTag : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }


}


