using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AHUIController : MonoBehaviour
{
    public GameObject ahUI;
    public GameObject originUI;
    public void SetIsAH(bool val)
    {
        ahUI.SetActive(val);
        originUI.SetActive(!val);
    }
}
