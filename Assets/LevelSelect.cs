using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Globalization;
using System.Xml;
using System.IO;


public class LevelSelect : MonoBehaviour
{
    [SerializeField] List<LevelButton> buttons;
    [SerializeField] bool debugMode = false;

    [SerializeField] List<string> levelNames = new List<string>{"0. TRANSPORTATION PIPELINE", "1. DYE MIXER", "2. STAIN REMOVER", "3. SIMPLE BLUE GREEN SEPARATOR", "4. COLOR ERROR DETECTION", "5. BLEACH FILTER", "6. COLOR DECOMPOSER", "7. COLOR TRANSISTOR", "8. HUE INVERTER", "9. COMMAND EXECUTOR", "10. HUE SHIFTER" };
    [SerializeField] TextMeshProUGUI levelName;

    // Start is called before the first frame update
    void Start()
    {
        loadLevelUnlockData();
    }

    void loadLevelUnlockData()
    {
        if (!debugMode)
        {
            foreach (LevelButton b in buttons)
            {
                if(b.levelPredessors == "")
                {
                    continue;
                }
                List<int> onHexNums = (new List<string>(b.levelPredessors.Split(','))).ConvertAll(int.Parse);

                if (onHexNums.Count > 0)
                {
                    b.outerHex.color = new Color(.2f, .2f, .2f);
                    b.GetComponent<Button>().enabled = false;
                    b.levelNum.color = Color.black;
                    b.transform.SetAsFirstSibling();
                }

                foreach (int i in onHexNums)
                {
                    if (PlayerPrefs.GetInt("completed_" + i.ToString(), 0) == 1)
                    {
                        b.outerHex.color = Color.gray;
                        b.GetComponent<Button>().enabled = true;
                        b.levelNum.color = Color.gray;
                        b.transform.SetAsLastSibling();
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < buttons.Count; i++)
        {
            if (PlayerPrefs.GetInt("completed_" + i.ToString(), 0) == 1)
            {
                buttons[i].outerHex.color = new Color(.67f, 1f, .67f);
                buttons[i].levelNum.color = new Color(.67f, 1f, .67f);
            }
        }

        int num = PlayerPrefs.GetInt("selectedLevel", 0);
        buttons[num].transform.SetAsLastSibling();
        buttons[num].gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1);
        buttons[num].GetComponent<LevelButton>().outerHex.color = Color.white;
        buttons[num].GetComponent<LevelButton>().levelNum.color = Color.white;
        levelName.text = levelNames[num];
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }

    public void selectLevel(int i)
    {
        buttons[i].transform.SetAsLastSibling();
        int oldSelect = PlayerPrefs.GetInt("selectedLevel", 0);
        Instantiate(Resources.Load<Rotator>("Prefabs/Rotator")).set(buttons[oldSelect].GetComponent<RectTransform>(), -360, .25f);
        Instantiate(Resources.Load<Scaler>("Prefabs/Scaler")).set(buttons[oldSelect].GetComponent<RectTransform>(), new Vector2(1f, 1f), .25f);

        if (PlayerPrefs.GetInt("completed_" + oldSelect.ToString(), 0) == 1)
        {
            Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(buttons[oldSelect].outerHex, new Color(.67f, 1f, .67f), .25f);
            Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(buttons[oldSelect].levelNum, new Color(.67f, 1f, .67f), .25f);
        }
        else
        {
            Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(buttons[oldSelect].outerHex, Color.gray, .25f);
            Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(buttons[oldSelect].levelNum, Color.gray, .25f);
        }
        

        PlayerPrefs.SetInt("selectedLevel", i);
        Instantiate(Resources.Load<Rotator>("Prefabs/Rotator")).set(buttons[i].GetComponent<RectTransform>(), 360, .25f);
        Instantiate(Resources.Load<Scaler>("Prefabs/Scaler")).set(buttons[i].GetComponent<RectTransform>(), new Vector2(1.1f,1.1f), .25f);
        Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(buttons[i].outerHex, Color.white, .25f);
        Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(buttons[i].levelNum, Color.white, .25f);
        levelName.text = levelNames[i];

        instructionHistogram.SetBarsStarter();
        cycleHistogram.SetBarsStarter();
        instructionHistogram.SetIndicators();
        cycleHistogram.SetIndicators();

        GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/beep" + UnityEngine.Random.Range(0, 4).ToString()), .5f);
    }

    public void editProgram()
    {
        GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/beep" + UnityEngine.Random.Range(0, 4).ToString()), .5f);
        music.gameObject.SetActive(false);
        StartCoroutine(editProgramCo());
    }
    [SerializeField] GameObject canvas;
    [SerializeField] AudioSource music;

    IEnumerator editProgramCo()
    {
        yield return new WaitForSeconds(0);
        canvas.SetActive(false);
        
        yield return new WaitForSeconds(.5f);
        SceneManager.LoadScene("program");
    }

    [SerializeField] Histogram instructionHistogram;
    [SerializeField] Histogram cycleHistogram;


    public void clearProgram()
    {
        int lNum = PlayerPrefs.GetInt("selectedLevel", 0); ;
        for(int i = 0; i < 64; i++)
        {
            PlayerPrefs.SetInt("instruction_" + lNum.ToString() + "_" + i.ToString(), 0);
            PlayerPrefs.SetInt("breakpoint_" + lNum.ToString() + "_" + i.ToString(), 0);
        }
    }

    
}
