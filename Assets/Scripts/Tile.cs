using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tile : MonoBehaviour
{
    private TextMeshProUGUI textNumeric;

    private int numeric;
    public int Numeric
    {
        set
        {
            numeric = value;
            textNumeric.text = numeric.ToString();
        }
        get => numeric;
    }

    public void Setup(int level)
    {
        textNumeric = GetComponentInChildren<TextMeshProUGUI>();
        Numeric = (1 << level);
    }
}
