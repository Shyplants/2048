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

    private IEnumerator MoveTo(Vector3 end)
    {
        float current = 0;
        float percent = 0;
        float moveTime = 0.5f;
        Vector3 start = GetComponent<RectTransform>().localPosition;

        while(percent < 1.0f)
        {
            current += Time.deltaTime;
            percent = current / moveTime;

            GetComponent<RectTransform>().localPosition = Vector3.Lerp(start, end, percent);

            yield return null;
        }
    }
}
