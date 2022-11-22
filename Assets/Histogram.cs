using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Histogram : MonoBehaviour
{
    [SerializeField] List<Image> bars;
    [SerializeField] TextMeshProUGUI leftEnd;
    [SerializeField] TextMeshProUGUI rightEnd;
    [SerializeField] string mode;
    [SerializeField] TextMeshProUGUI indicator;

    // Start is called before the first frame update
    void Start()
    {
        SetIndicators();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetIndicators()
    {
        if(mode == "instructions" && PlayerPrefs.GetInt("bestInstructions_" + PlayerPrefs.GetInt("selectedLevel",0).ToString(), -1) != -1)
        {
            int num = PlayerPrefs.GetInt("bestInstructions_" + PlayerPrefs.GetInt("selectedLevel", 0).ToString(), -1);
            indicator.text = num.ToString() + " v";
            int barNum = Mathf.FloorToInt(num / 4);
            barNum = Mathf.Min(15, barNum);

            indicator.transform.localPosition = new Vector2(-320 + barNum*37.5f, 24.8f);
        }

        if(mode == "cycles" && PlayerPrefs.GetInt("bestCycles_" + PlayerPrefs.GetInt("selectedLevel", 0).ToString(), -1) != -1)
        {
            int num = PlayerPrefs.GetInt("bestCycles_" + PlayerPrefs.GetInt("selectedLevel", 0).ToString(), -1);
            indicator.text = num.ToString() + " v";
            int barNum = Mathf.FloorToInt(num / 62.5f);
            barNum = Mathf.Min(15, barNum);

            indicator.transform.localPosition = new Vector2(-320 + barNum * 37.5f, 24.8f);
        }
    }
}
