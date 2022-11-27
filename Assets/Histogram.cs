using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

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
        SetBarsStarter();
        SetIndicators();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetIndicators()
    {
        indicator.text = "";

        if (mode == "instructions" && PlayerPrefs.GetInt("bestInstructions_" + PlayerPrefs.GetInt("selectedLevel",0).ToString(), -1) != -1)
        {
            int num = PlayerPrefs.GetInt("bestInstructions_" + PlayerPrefs.GetInt("selectedLevel", 0).ToString(), -1);
            indicator.text = num.ToString() + " v";
            int barNum = Mathf.FloorToInt(num / 4);
            barNum = Mathf.Min(15, barNum);

            indicator.transform.localPosition = new Vector2(-320 + barNum*37.5f, indicator.transform.localPosition.y);
        }

        if(mode == "cycles" && PlayerPrefs.GetInt("bestCycles_" + PlayerPrefs.GetInt("selectedLevel", 0).ToString(), -1) != -1)
        {
            int num = PlayerPrefs.GetInt("bestCycles_" + PlayerPrefs.GetInt("selectedLevel", 0).ToString(), -1);
            indicator.text = num.ToString() + " v";
            int barNum = Mathf.FloorToInt(num / 62.5f);
            barNum = Mathf.Min(15, barNum);

            indicator.transform.localPosition = new Vector2(-320 + barNum * 37.5f, indicator.transform.localPosition.y);
        }
    }

    public void SetBarsStarter()
    {
        foreach(Image b in bars)
        {
            b.fillAmount = 0;
        }

        StartCoroutine(SetBars());
    }

    IEnumerator SetBars()
    {
        WWWForm form = new WWWForm();
        int num = PlayerPrefs.GetInt("selectedLevel", 0);


        form.AddField("mode", mode);
        form.AddField("level", num);

        using (UnityWebRequest www = UnityWebRequest.Post("https://crowseeds.com/AURORA/getHistogram.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                List<string> stuff = (new List<string>(www.downloadHandler.text.Split('\t')));
                stuff.RemoveAt(16);
                List<int> results = stuff.ConvertAll(int.Parse);
                int total = 0;
                foreach(int i in results)
                {
                    total += i;
                }

                if(total != 0)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        bars[i].fillAmount = ((float)results[i]) / total;
                    }
                }
            }
        }
    }

}
