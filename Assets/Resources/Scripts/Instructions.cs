using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Globalization;
using System.Xml;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class Instructions : MonoBehaviour
{
    int[] instructions = new int[64];
    [SerializeField] List<Hexagon> hexagons;
    List<int> breakpoints = new List<int>();
    float timeBetweenInstructions = .3f;
    int currentInstruction = 0;
    int previousInstruction = 0;
    int lastInstruction = 63;

    [SerializeField] ScrollRect scroll;
    EasingFunction.Function function;
    float timer = 0;
    bool isMoving = false;

    TextAsset levelDat;

    List<List<List<Color>>> testCases = new List<List<List<Color>>>();
    int[] barPosition = new int[5];

    [SerializeField] List<Bar> bars = new List<Bar>();
    [SerializeField] TextMeshProUGUI levelName;
    [SerializeField] TextMeshProUGUI description;
    int testCaseNum = 0;

    IFormatProvider format = new CultureInfo("en-US");
    int inBar1 = -1;
    int inBar2 = -1;
    int inBar3 = -1;
    int outBar1 = -1;
    int outBar2 = -1;
    int actOutBar1 = -1;
    int actOutBar2 = -1;

    int inHex1 = -1;
    int inHex2 = -1;
    int inHex3 = -1;
    int outHex1 = -1;
    int outHex2 = -1;

    public int levelID = 0;

    List<int[]> timeslices = new List<int[]>();
    public int timesliceIndex = 0;

    bool onCutscene;
    [SerializeField] GameObject cutsceneObject;
    [SerializeField] RawImage cutsceneDrawing;

    achivementHandler aHandler;


    private void Awake()
    {
        levelID = PlayerPrefs.GetInt("selectedLevel", 0);
        setButtons();
        loadData(levelID);
        renderTestCase();
        timeslices.Add((int[])instructions.Clone());

        if (PlayerPrefs.GetInt("seen_" + levelID.ToString(), 0) == 0)
        {
            PlayerPrefs.SetInt("seen_" + levelID.ToString(), 1);
            viewCutscene();
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        aHandler = FindObjectOfType<achivementHandler>();
        EasingFunction.Ease movement = EasingFunction.Ease.EaseOutBack;
        function = EasingFunction.GetEasingFunction(movement);
        scroll.verticalNormalizedPosition = 1;
        //scroll.verticalNormalizedPosition = .997f;
    }

    void loadData(int level)
    {
        levelDat = Resources.Load<TextAsset>("Data/level" + level.ToString());

        string data = levelDat.text;
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(new StringReader(data));

        XmlNode dataNode = xmlDoc.SelectSingleNode("//data");
        XmlNode layoutNode = dataNode.SelectSingleNode("layout");
        string onHex = layoutNode.Attributes["hex"].Value;
        List<int> onHexNums = (new List<string>(onHex.Split(','))).ConvertAll(int.Parse);
        levelName.text = layoutNode.Attributes["name"].Value;
        description.text = layoutNode.Attributes["description"].Value;


        foreach (int i in onHexNums)
        {
            hexagons[i - 1].gameObject.SetActive(true);
        }

        int inAmt = Convert.ToInt32(layoutNode.Attributes["in"].Value, format);
        int outAmt = Convert.ToInt32(layoutNode.Attributes["out"].Value, format);


        if(inAmt == 3)
        {
            inBar1 = 0;
            inBar2 = 1;
            inBar3 = 2;
        }
        else if(inAmt == 2)
        {
            inBar1 = 0;
            inBar2 = 1;
        }
        else
        {
            inBar1 = 0;
        }

        actOutBar1 = inAmt + outAmt;
        bars[actOutBar1].text.text = layoutNode.Attributes["exName1"].Value;
        bars[inAmt + outAmt].gameObject.SetActive(true);

        if (outAmt == 2)
        {
            outBar1 = inAmt;
            outBar2 = inAmt + 1;
            actOutBar2 = inAmt + outAmt + 1;
            bars[actOutBar2].text.text = layoutNode.Attributes["exName2"].Value;
            bars[inAmt + outAmt + 1].gameObject.SetActive(true);
        }
        else
        {
            outBar1 = inAmt;
        }

        for(int i = 0; i < inAmt + outAmt; i++)
        {
            testCases.Add(new List<List<Color>>());
            bars[i].gameObject.SetActive(true);
        }

        if(layoutNode.Attributes["inHex1"] != null) { inHex1 = Convert.ToInt32(layoutNode.Attributes["inHex1"].Value, format)-1; hexagons[inHex1].isInput = 1; }
        if(layoutNode.Attributes["inHex2"] != null) { inHex2 = Convert.ToInt32(layoutNode.Attributes["inHex2"].Value, format)-1; hexagons[inHex2].isInput = 2; }
        if(layoutNode.Attributes["inHex3"] != null) { inHex3 = Convert.ToInt32(layoutNode.Attributes["inHex3"].Value, format)-1; hexagons[inHex3].isInput = 3; }
        if(layoutNode.Attributes["outHex1"] != null) { outHex1 = Convert.ToInt32(layoutNode.Attributes["outHex1"].Value, format) - 1; hexagons[outHex1].isOutput = 1; }
        if(layoutNode.Attributes["outHex2"] != null) { outHex2 = Convert.ToInt32(layoutNode.Attributes["outHex2"].Value, format) - 1; hexagons[outHex2].isOutput = 2; }


        foreach (XmlNode objnode in dataNode.SelectNodes("row"))
        {
            string name = objnode.Attributes["name"].Value;
            List<string> colorStrings = (new List<string>(objnode.Attributes["data"].Value.Split(',')));

            switch (objnode.Attributes["id"].Value)
            {
                case "in0":
                    fillBar(bars[inBar1], inBar1, name, colorStrings);
                    break;
                case "in1":
                    fillBar(bars[inBar2], inBar2, name, colorStrings);
                    break;
                case "in2":
                    fillBar(bars[inBar3], inBar3, name, colorStrings);
                    break;
                case "out0":
                    fillBar(bars[outBar1], outBar1, name, colorStrings);
                    break;
                case "out1":
                    fillBar(bars[outBar2], outBar2, name, colorStrings);
                    break;
            }
        }
    }

    void fillBar(Bar b, int colorIndex, string name, List<string> colorStrings)
    {
        b.text.text = name;
        List<Color> colors = new List<Color>();
        foreach(string s in colorStrings)
        {
            switch (s)
            {
                case "r":
                    colors.Add(Color.red);
                    break;
                case "y":
                    colors.Add(Color.yellow);
                    break;
                case "g":
                    colors.Add(Color.green);
                    break;
                case "c":
                    colors.Add(Color.cyan);
                    break;
                case "b":
                    colors.Add(Color.blue);
                    break;
                case "m":
                    colors.Add(Color.magenta);
                    break;
                case "w":
                    colors.Add(Color.white);
                    break;
            }
        }

        testCases[colorIndex].Add(colors);
    }

    void renderTestCase()
    {
        for(int i = 0; i < barPosition.Length; i++)
        {
            barPosition[i] = 0;
        }
        clearBars();
        renderBar(inBar1, testCaseNum);
        renderBar(inBar2, testCaseNum);
        renderBar(inBar3, testCaseNum);
        renderBar(outBar1, testCaseNum);
        renderBar(outBar2, testCaseNum);

        clearHexagons();
        if(inHex1 != -1) {hexagons[inHex1].isInput = 0; hexagons[inHex1].giveAll(0f, testCases[inBar1][testCaseNum][0]); }
        if(inHex2 != -1) {hexagons[inHex2].isInput = 1; hexagons[inHex2].giveAll(0f, testCases[inBar2][testCaseNum][0]); }
        if(inHex3 != -1) {hexagons[inHex3].isInput = 2; hexagons[inHex3].giveAll(0f, testCases[inBar3][testCaseNum][0]); }
    }

    void clearBars()
    {
        foreach(Bar b in bars)
        {
            foreach(RawImage r in b.barCells)
            {
                r.color = Color.black;
            }
        }
    }

    void clearHexagons()
    {
        foreach(Hexagon h in hexagons)
        {
            if (h.isActiveAndEnabled)
            {
                h.takeAll(0f, Color.white);
            }
        }
    }

    public Color getColor(int i)
    {
        if(i == 0)
        {
            barPosition[inBar1]++;
            if(barPosition[inBar1] >= testCases[inBar1][testCaseNum].Count)
            {
                return Color.gray;
            }

            return testCases[inBar1][testCaseNum][barPosition[inBar1]];
        }else if(i == 1)
        {
            barPosition[inBar2]++;
            if (barPosition[inBar2] >= testCases[inBar2][testCaseNum].Count)
            {
                return Color.gray;
            }
            return testCases[inBar2][testCaseNum][barPosition[inBar2]];
        }else if(i == 2)
        {
            barPosition[inBar3]++;
            if (barPosition[inBar3] >= testCases[inBar3][testCaseNum].Count)
            {
                return Color.gray;
            }
            return testCases[inBar3][testCaseNum][barPosition[inBar3]];
        }

        return Color.white; 
    }

    float colorDifference(Color c1, Color c2)
    {
        return Mathf.Abs(Mathf.Clamp01(c1.r) - Mathf.Clamp01(c2.r)) + Mathf.Abs(Mathf.Clamp01(c1.g) - Mathf.Clamp01(c2.g)) + Mathf.Abs(Mathf.Clamp01(c1.b) - Mathf.Clamp01(c2.b));
    }

    [SerializeField] Transform canvas;
    [SerializeField] GameObject completeScreen;
    bool gotWrong = false;

    void addColorToBar(int i, Color c)
    {
        if(colorDifference(c, Color.gray) < .1f || !isRunning)
        {
            return;
        }
        int compare = outBar1;
        int num = actOutBar1;
        if(i == 2)
        {
            num = actOutBar2;
            compare = outBar2;
        }

        if(barPosition[actOutBar1] >= testCases[outBar1][testCaseNum].Count - 1 && (actOutBar2 == -1 || barPosition[actOutBar2] >= testCases[outBar2][testCaseNum].Count - 1))
        {
            if(barPosition[actOutBar1] == testCases[outBar1][testCaseNum].Count - 1)
            {
                bars[num].barCells[barPosition[num]].color = c;

                if (colorDifference(c, testCases[compare][testCaseNum][barPosition[num]]) > .1f)
                {
                    GameObject g = Instantiate(Resources.Load<GameObject>("Prefabs/Wrong"));
                    g.transform.SetParent(canvas);
                    g.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                    g.transform.position = bars[num].barCells[barPosition[num]].transform.position;
                    gotWrong = true;
                    GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/colorWrong"));
                }
                else
                {
                    GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/colorCorrect"), .5f);
                }

                barPosition[num]++;
            }

            if (!gotWrong)
            {
                if(testCaseNum == 2)
                {
                    if (cycleCount > worstCycleCount)
                    {
                        worstCycleCount = cycleCount;
                    }
                    cycleCount = 0;
                    step.SetActive(false);
                    pause.SetActive(true);
                    completeScreen.SetActive(true);
                    pauseButton();
                    GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/programComplete"), .6f);
                    renderComplete();
                }
                else
                {
                    if(cycleCount > worstCycleCount)
                    {
                        worstCycleCount = cycleCount;
                    }
                    cycleCount = 0;
                    testCaseNum++;
                    currentInstruction = 0;
                    renderTestCase();
                }
            }
            
            return;
        }


        bars[num].barCells[barPosition[num]].color = c;

        if(colorDifference(c, testCases[compare][testCaseNum][barPosition[num]]) > .1f)
        {
            GameObject g = Instantiate(Resources.Load<GameObject>("Prefabs/Wrong"));
            GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/colorWrong"));
            g.transform.SetParent(canvas);
            g.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            g.transform.position = bars[num].barCells[barPosition[num]].transform.position;
            gotWrong = true;
        }
        else
        {
            GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/colorCorrect"), .5f);
        }

        barPosition[num]++;
    }

    void renderBar(int barNum, int testCase)
    {
        if (barNum == -1)
        {
            return;
        }

        List<Color> colors = testCases[barNum][testCase];
        Bar b = bars[barNum];
        for(int i = 0; i < colors.Count; i++)
        {
            b.barCells[i].color = colors[i];
        }
    }


    // Update is called once per frame
    void Update()
    {
        

        if (isMoving)
        {
            if(timeBetweenInstructions > .1f)
            {
                if(timer >= 1)
                {
                    scroll.verticalNormalizedPosition = 1 - (currentInstruction / 63f);
                    currentInstructionBar.localPosition = instructionObjects[currentInstruction].localPosition;
                }
                else
                {
                    timer += Time.deltaTime / timeBetweenInstructions;
                    scroll.verticalNormalizedPosition = function(1 - (previousInstruction / 63f), 1 - (currentInstruction / 63f), timer);
                    currentInstructionBar.localPosition = new Vector2(instructionObjects[currentInstruction].localPosition.x, function(instructionObjects[previousInstruction].localPosition.y, instructionObjects[currentInstruction].localPosition.y, timer));
                }
            }
            else
            {
                scroll.verticalNormalizedPosition = 1 - (currentInstruction / 63f);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            optionButton();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                redo();
            }
            else
            {
                undo();
            }
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            redo();
        }
    }

    [SerializeField] Texture outlineTexture;
    [SerializeField] Texture circleTexture;

    public void pressButton(ButtonParameters b)
    {
        if (isRunning)
        {
            return;
        }

        int r = b.row;
        int c = b.col;
        b.active = !b.active;

        instructions[r] = (instructions[r] ^ (1 << c));

        PlayerPrefs.SetInt("instruction_" + levelID.ToString() + "_" + b.row.ToString(), instructions[r]);

        if (b.active)
        {
            b.GetComponent<RawImage>().texture = circleTexture;
            GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/buttonClick"));
        }
        else
        {
            b.GetComponent<RawImage>().texture = outlineTexture;
            GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/buttonClick2"));
        }

        saveSlice();
    }

    [SerializeField] List<Transform> instructionObjects;

    public void setButtons()
    {
        for(int i = 0; i < 64; i++)
        {
            int num = PlayerPrefs.GetInt("instruction_" + levelID.ToString() + "_" + i.ToString(), 0);
            instructions[i] = num;
            for (int j = 0; j < 10; j++)
            {
                if (((num >> j) & 1) == 1)
                {
                    instructionObjects[i].GetChild(10 - j).GetComponent<RawImage>().texture = circleTexture;
                    instructionObjects[i].GetChild(10 - j).GetComponent<ButtonParameters>().active = true;
                }
                else
                {
                    instructionObjects[i].GetChild(10 - j).GetComponent<RawImage>().texture = outlineTexture;
                    instructionObjects[i].GetChild(10 - j).GetComponent<ButtonParameters>().active = false;
                }
            }

            if (PlayerPrefs.GetInt("breakpoint_" + levelID.ToString() + "_" + i.ToString(), 0) == 1)
            {
                instructionObjects[i].GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.gray;
                instructionObjects[i].GetChild(0).GetComponent<ButtonParameters>().active = true;
                breakpoints.Add(i);
            }
            else
            {
                instructionObjects[i].GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
                instructionObjects[i].GetChild(0).GetComponent<ButtonParameters>().active = false;
                breakpoints.Remove(i);
            }
        }
    }

    bool isRunning = false;

    public void runProgram()
    {
        if (!isRunning)
        {
            stopOutline.color = Color.white;
            stopOutline2.color = Color.white;
            stopOutline3.color = Color.white;
            renderTestCase();
            isRunning = true;
            isMoving = true;
            lastInstruction = -1;
            currentInstruction = 0;
            for (int i = 63; i >= 0; i--)
            {
                if (instructions[i] != 0)
                {
                    lastInstruction = i;
                    break;
                }
            }
            StartCoroutine(runProgramHelper());
        }
        
    }

    IEnumerator runProgramHelper()
    {
        if(currentInstruction <= lastInstruction && currentInstruction <= 63 && isRunning)
        {
            int instruction = instructions[currentInstruction];
            int opcode = (instruction >> 7) & 0b111;
            int hex = ((instruction >> 3) & 0b1111) - 1;
            GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/move"));

            if (instruction != 0)
            {
                cycleCount++;
            }
            
            if(opcode != 4 && (hex >= hexagons.Count || (hex != -1 && !hexagons[hex].isActiveAndEnabled)))
            {
                opcode = 99;
            }

            int sign = (instruction >> 2) & 0b1;
            int amt = instruction & 0b11;
            int dir = instruction & 0b111;
            int jumpTo = 0;

            scroll.vertical = false;

            bool willJump = false;

            foreach(Hexagon h in hexagons)
            {
                if (h.isActiveAndEnabled)
                {
                    h.updateGrayness();
                }
            }

            switch (opcode)
            {
                case 0: //rotate
                    if(hex == -1)
                    {
                        for(int i = 0; i < hexagons.Count; i++)
                        {
                            Hexagon h = hexagons[i];
                            if (h.isActiveAndEnabled)
                            {
                                StartCoroutine(rotate(i, sign, amt));
                            }
                        }
                    }
                    else if(hex >= 0 && hex < hexagons.Count)
                    {
                        StartCoroutine(rotate(hex, sign, amt));
                    }
                    break;
                case 1: //move
                    if(hex == -1)
                    {
                        for (int i = 0; i < hexagons.Count; i++)
                        {
                            Hexagon h = hexagons[i];
                            if (h.isActiveAndEnabled)
                            {
                                if (hexagons[i].isInput >= 0 && (getDestHex(i, dir) == -1 || !hexagons[getDestHex(i, dir)].isActiveAndEnabled))
                                {
                                    StartCoroutine(move(i, dir, true));
                                }
                                else
                                {
                                    StartCoroutine(move(i, dir, false));
                                }
                            }
                        }
                    }
                    else if(hex >= 0 && hex < hexagons.Count)
                    {
                        if(hexagons[hex].isInput >= 0 && (getDestHex(hex, dir) == -1 || !hexagons[getDestHex(hex, dir)].isActiveAndEnabled))
                        {
                            StartCoroutine(move(hex, dir, true));
                        }
                        else
                        {
                            StartCoroutine(move(hex, dir, false));
                        }
                    }
                    break;
                case 2: //add-subtract
                    if(sign == 0)
                    {
                        if(hex == -1)
                        {
                            for (int i = 0; i < hexagons.Count; i++)
                            {
                                Hexagon h = hexagons[i];
                                if (h.isActiveAndEnabled)
                                {
                                    add(i, amt);
                                }
                            }
                        }
                        else
                        {
                            add(hex, amt);
                        }
                        
                    }
                    else
                    {
                        if (hex == -1)
                        {
                            for (int i = 0; i < hexagons.Count; i++)
                            {
                                Hexagon h = hexagons[i];
                                if (h.isActiveAndEnabled)
                                {
                                    StartCoroutine(sub(i, amt));
                                }
                            }
                        }
                        else
                        {
                            StartCoroutine(sub(hex, amt));
                        }
                    }
                    break;
                case 3: //copy
                    if (hex == -1)
                    {
                        for (int i = 0; i < hexagons.Count; i++)
                        {
                            Hexagon h = hexagons[i];
                            if (h.isActiveAndEnabled)
                            {
                                copy(i, amt, sign);
                            }
                        }
                    }
                    else if (hex >= 0 && hex < hexagons.Count)
                    {
                        copy(hex, amt, sign);
                    }
                    break;
                case 4: //jump
                    willJump = true;
                    Debug.Log("jumping!!!");
                    jumpTo = instruction & 0b1111111;
                    Debug.Log("jumping to: " + jumpTo.ToString());
                    break;
                case 5: //jump if red
                    if(hex == -1)
                    {
                        for (int i = 0; i < hexagons.Count; i++)
                        {
                            Hexagon h = hexagons[i];
                            if (h.isActiveAndEnabled && h.hasAny(Color.red))
                            {
                                willJump = true;
                            }
                        }
                    }
                    else
                    {
                        if(hexagons[hex].isActiveAndEnabled && hexagons[hex].hasAny(Color.red))
                        {
                            willJump = true;
                        }
                    }
                    jumpTo = (instruction & 0b111) + currentInstruction;
                    break;
                case 6: //jump if green
                    if (hex == -1)
                    {
                        for (int i = 0; i < hexagons.Count; i++)
                        {
                            Hexagon h = hexagons[i];
                            if (h.isActiveAndEnabled && h.hasAny(Color.green))
                            {
                                willJump = true;
                            }
                        }
                    }
                    else
                    {
                        if (hexagons[hex].isActiveAndEnabled && hexagons[hex].hasAny(Color.green))
                        {
                            willJump = true;
                        }
                    }
                    jumpTo = (instruction & 0b111) + currentInstruction;
                    break;
                case 7: //jump if blue
                    if (hex == -1)
                    {
                        for (int i = 0; i < hexagons.Count; i++)
                        {
                            Hexagon h = hexagons[i];
                            if (h.isActiveAndEnabled && h.hasAny(Color.blue))
                            {
                                willJump = true;
                            }
                        }
                    }
                    else
                    {
                        if (hexagons[hex].isActiveAndEnabled && hexagons[hex].hasAny(Color.blue))
                        {
                            willJump = true;
                        }
                    }
                    jumpTo = (instruction & 0b111) + currentInstruction;
                    break;

            }

            
            previousInstruction = currentInstruction;
            if (!willJump)
            {
                currentInstruction++;
            }
            else
            {
                //GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/slide" + UnityEngine.Random.Range(0, 2).ToString()));
                currentInstruction = jumpTo;
            }
            
            timer = 0;
            yield return new WaitForSeconds(timeBetweenInstructions);
            canStep = true;
            if (!isPaused && !breakpoints.Contains(currentInstruction))
            {
                StartCoroutine(runProgramHelper());
            }

            if(breakpoints.Contains(currentInstruction) && !isPaused)
            {
                pauseButton();
            }
        }
        else if(isRunning)
        {
            
            currentInstruction = 0;
            timer = 0;
            yield return new WaitForSeconds(timeBetweenInstructions);
            canStep = true;
            if (!isPaused && !breakpoints.Contains(currentInstruction))
            {
                //GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/slide" + UnityEngine.Random.Range(0, 2).ToString()));
                StartCoroutine(runProgramHelper());
            }
            //isRunning = false;
            //isMoving = false;
            //scroll.vertical = true;
        }
        
    }

    public IEnumerator rotate(int hex, int sign, int amount)
    {
        Hexagon h = hexagons[hex];
        float rotAmount = (sign == 0 ? 1 : -1) * -60 * amount;
        Instantiate(Resources.Load<Rotator>("Prefabs/Rotator")).set(h.GetComponent<RectTransform>(), rotAmount, timeBetweenInstructions);
        yield return new WaitForSeconds(timeBetweenInstructions);
        h.rotate(sign, amount);
    }

    int getDestHex(int hex, int dir)
    {

        switch (dir)
        {
            case 1:
                if (hex > 3)
                {
                    return hex - 4;
                }
                break;
            case 2:
                if (hex % 4 != 3 && hex != 0 && hex != 2)
                {
                    if (hex % 2 == 1)
                    {
                        return hex + 1;
                    }
                    else
                    {
                        return hex - 3;
                    }
                }
                break;
            case 3:
                if (hex % 4 != 3 && hex != 13 && hex != 14)
                {
                    if (hex % 2 == 1)
                    {
                        return hex + 5;
                    }
                    else
                    {
                        return hex + 1;
                    }
                }
                break;
            case 4:
                if (hex < 11)
                {
                    return hex + 4;
                }
                break;
            case 5:
                if (hex % 4 != 0 && hex != 13)
                {
                    if (hex % 2 == 1)
                    {
                        return hex + 3;
                    }
                    else
                    {
                        return hex - 1;
                    }
                }
                break;
            case 6:
                if (hex % 4 != 0 && hex != 2)
                {
                    if (hex % 2 == 1)
                    {
                        return hex - 1;
                    }
                    else
                    {
                        return hex - 5;
                    }
                }
                break;
        }

        return -1;
    }

    int getDestSide(int dir)
    {
        switch (dir)
        {
            case 1:
                return 3;
            case 2:
                return 4;
            case 3:
                return 5;
            case 4:
                return 0;
            case 5:
                return 1;
            case 6:
                return 2;
        }

        return -1;
    }


    public IEnumerator move(int hex, int dir, bool onInputTake)
    {
        if(dir > 6)
        {
            yield break;
        }

        if(dir == 0)
        {
            StartCoroutine(move(hex, 1, true));
            StartCoroutine(move(hex, 2, true));
            StartCoroutine(move(hex, 3, true));
            StartCoroutine(move(hex, 4, true));
            StartCoroutine(move(hex, 5, true));

            bool willTake = true;
            for(int i = 0; i < 6; i++)
            {
                if(getDestHex(hex, i) != -1 && hexagons[getDestHex(hex, i)].isActiveAndEnabled)
                {
                    willTake = false;
                }
            }
            StartCoroutine(move(hex, 6, willTake));
            yield break;
        }

        
        if (hexagons[hex].wasGray[dir - 1])
        {
            yield break;
        }

        if (hexagons[hex].isOutput >= 0)
        {
            yield break;
        }

        Hexagon h = hexagons[hex];
        Color c = h.getColor(dir);

        int outHexNum = getDestHex(hex, dir);
        if (outHexNum != -1 && hexagons[outHexNum].isActiveAndEnabled)
        {
            hexagons[outHexNum].give(getDestSide(dir), timeBetweenInstructions, c);
        }

        if(h.isInput >= 0)
        {
            if (!onInputTake)
            {
                if(timeBetweenInstructions > .1f)
                {
                    yield return new WaitForSeconds(0);
                    h.takeAll(timeBetweenInstructions / 2.5f, Color.white);
                    yield return new WaitForSeconds(timeBetweenInstructions / 2.5f);
                    h.giveAll(timeBetweenInstructions / 2.5f, getColor(h.isInput));
                    yield return new WaitForSeconds(timeBetweenInstructions / 2.5f);
                }
                else
                {
                    h.takeAll(0, Color.white);
                    h.giveAll(0, getColor(h.isInput));
                }

                
            }   
        }
        else
        {
            if (outHexNum != -1 && hexagons[outHexNum].isActiveAndEnabled)
            {
                h.take(dir - 1, timeBetweenInstructions, Color.white);
            }
            yield return new WaitForSeconds(timeBetweenInstructions);
        }

        if(outHexNum >= 0 && hexagons[outHexNum].isActiveAndEnabled && hexagons[outHexNum].isOutput >= 0)
        {
            //hexagons[outHexNum].giveAllForce(timeBetweenInstructions, c);
            addColorToBar(hexagons[outHexNum].isOutput, c);
        }
        
    }

    public void add(int hex, int color)
    {
        Color c = Color.white;
        switch (color)
        {
            case 1:
                c = Color.red;
                break;
            case 2:
                c = Color.green;
                break;
            case 3:
                c = Color.blue;
                break;
        }
        if(hexagons[hex].isOutput >= 0)
        {
            hexagons[hex].giveAllForce(timeBetweenInstructions, c);
            addColorToBar(hexagons[hex].isOutput, c);
        }
        else
        {
            hexagons[hex].giveAll(timeBetweenInstructions, c);
        }
    }


    public IEnumerator sub(int hex, int color)
    {
        Color c = Color.white;
        switch (color)
        {
            case 1:
                c = Color.red;
                break;
            case 2:
                c = Color.green;
                break;
            case 3:
                c = Color.blue;
                break;
        }

        if ((colorDifference(Color.white, c) < .1f || (hexagons[hex].allTheSameShape() && colorDifference(c, hexagons[hex].getColor(1)) < .2f)) && hexagons[hex].isInput >= 0)
        {
            if(timeBetweenInstructions > .1f)
            {
                yield return new WaitForSeconds(0);
                hexagons[hex].takeAll(timeBetweenInstructions / 2.5f, Color.white);
                yield return new WaitForSeconds(timeBetweenInstructions / 2.5f);
                hexagons[hex].giveAll(timeBetweenInstructions / 2.5f, getColor(hexagons[hex].isInput));
                yield return new WaitForSeconds(timeBetweenInstructions / 2.5f);
            }
            else
            {
                hexagons[hex].takeAll(0, Color.white);
                hexagons[hex].giveAll(0, getColor(hexagons[hex].isInput));
            }
        }
        else if (hexagons[hex].isOutput < 0)
        {
            hexagons[hex].takeAll(timeBetweenInstructions, c);
        }
    }

    public void copy(int hex, int dir, int sign)
    {
        Hexagon h = hexagons[hex];
        Color c = Color.gray;
        if (dir == 0)
        {
            for (int i = 1; i < 7; i++) { 
                if (!h.wasGray[i-1])
                {
                    c += h.getColor(i);
                }
            }
        }
        else
        {
            c = h.getColor(dir);
        }

        if(colorDifference(c, Color.gray) < .1f)
        {
            return;
        }
        

        if (hexagons[hex].isOutput >= 0)
        {
            if(sign == 0)
            {
                hexagons[hex].giveAllForce(timeBetweenInstructions, c);
                addColorToBar(hexagons[hex].isOutput, c);
            }
        }
        else
        {
            if(sign == 0)
            {
                hexagons[hex].giveAll(timeBetweenInstructions, c);
            }
            else
            {
                hexagons[hex].takeAll(timeBetweenInstructions, c);
            } 
        }
    }

    public void setBreakpoint(ButtonParameters p)
    {
        if(p.col == 0)
        {
            if (breakpoints.Contains(p.row))
            {
                breakpoints.Remove(p.row);
                p.active = false;
                PlayerPrefs.SetInt("breakpoint_" + levelID.ToString() + "_" + p.row.ToString(), 0);
                p.GetComponent<TextMeshProUGUI>().color = Color.white;
            }
            else
            {
                breakpoints.Add(p.row);
                p.active = true;
                PlayerPrefs.SetInt("breakpoint_" + levelID.ToString() + "_" + p.row.ToString(), 1);
                p.GetComponent<TextMeshProUGUI>().color = Color.gray;
            }
        }else if(p.col == 1)
        {
            if(instructions[p.row] == 0)
            {
                return;
            }

            int index = Mathf.Min(p.row + 1, 63);
            instructions[index] = instructions[p.row];
            instructions[p.row] = 0;

            int num = instructions[index];

            for (int j = 0; j < 10; j++)
            {
                if (((num >> j) & 1) == 1)
                {
                    instructionObjects[index].GetChild(10 - j).GetComponent<RawImage>().texture = circleTexture;
                    instructionObjects[index].GetChild(10 - j).GetComponent<ButtonParameters>().active = true;
                }
                else
                {
                    instructionObjects[index].GetChild(10 - j).GetComponent<RawImage>().texture = outlineTexture;
                    instructionObjects[index].GetChild(10 - j).GetComponent<ButtonParameters>().active = false;
                }

                instructionObjects[p.row].GetChild(10 - j).GetComponent<RawImage>().texture = outlineTexture;
                instructionObjects[p.row].GetChild(10 - j).GetComponent<ButtonParameters>().active = false;
            }

            PlayerPrefs.SetInt("instruction_" + levelID.ToString() + "_" + p.row.ToString(), instructions[p.row]);
            PlayerPrefs.SetInt("instruction_" + levelID.ToString() + "_" + index.ToString(), instructions[index]);
            saveSlice();
        }
        else if(p.col == 2)
        {
            if (instructions[p.row] == 0)
            {
                return;
            }

            int index = Mathf.Max(p.row - 1, 0);
            instructions[index] = instructions[p.row];
            instructions[p.row] = 0;

            int num = instructions[index];

            for (int j = 0; j < 10; j++)
            {
                if (((num >> j) & 1) == 1)
                {
                    instructionObjects[index].GetChild(10 - j).GetComponent<RawImage>().texture = circleTexture;
                    instructionObjects[index].GetChild(10 - j).GetComponent<ButtonParameters>().active = true;
                }
                else
                {
                    instructionObjects[index].GetChild(10 - j).GetComponent<RawImage>().texture = outlineTexture;
                    instructionObjects[index].GetChild(10 - j).GetComponent<ButtonParameters>().active = false;
                }

                instructionObjects[p.row].GetChild(10 - j).GetComponent<RawImage>().texture = outlineTexture;
                instructionObjects[p.row].GetChild(10 - j).GetComponent<ButtonParameters>().active = false;
            }
            PlayerPrefs.SetInt("instruction_" + levelID.ToString() + "_" + p.row.ToString(), instructions[p.row]);
            PlayerPrefs.SetInt("instruction_" + levelID.ToString() + "_" + index.ToString(), instructions[index]);
            saveSlice();
        }
        
    }

    [SerializeField] GameObject playReg;
    [SerializeField] GameObject playFast;
    [SerializeField] GameObject pause;
    [SerializeField] GameObject step;

    bool isPaused = false;
    [SerializeField] RawImage stopOutline;
    [SerializeField] RawImage stopOutline2;
    [SerializeField] TextMeshProUGUI stopOutline3;
    [SerializeField] Transform currentInstructionBar;


    public void playButton()
    {
        if (!isRunning)
        {
            runProgram();
        }

        if (isPaused)
        {
            StartCoroutine(runProgramHelper());
        }

        isMoving = true;
        isPaused = false;
        step.SetActive(false);
        pause.SetActive(true);


        if (playReg.activeSelf)
        {
            playReg.SetActive(false);
            playFast.SetActive(true);
            timeBetweenInstructions = .3f;
        }
        else
        {
            playReg.SetActive(true);
            playFast.SetActive(false);
            timeBetweenInstructions = .005f;
        }
    }

    public void stopButton()
    {
        stopOutline.color = Color.gray;
        stopOutline2.color = Color.gray;
        stopOutline3.color = Color.gray;
        currentInstructionBar.localPosition = instructionObjects[0].localPosition;
        scroll.verticalNormalizedPosition = 1;
        gotWrong = false;
        GameObject[] procs = GameObject.FindGameObjectsWithTag("process");
        foreach (GameObject g in procs)
        {
            if(g.GetComponent<Rotator>() != null)
            {
                g.GetComponent<Rotator>().restart();
            }else if(g.GetComponent<ColorFader>() != null)
            {
                g.GetComponent<ColorFader>().restart();
            }
            Destroy(g);
        }

        GameObject[] wrongs = GameObject.FindGameObjectsWithTag("wrong");
        foreach (GameObject g in wrongs)
        {
            Destroy(g);
        }
        testCaseNum = 0;
        playReg.SetActive(true);
        playFast.SetActive(false);
        step.SetActive(true);
        pause.SetActive(false);
        //StopAllCoroutines();
        renderTestCase();
        currentInstruction = 0;
        timer = 0;
        cycleCount = 0;
        worstCycleCount = 0;
        isRunning = false;
        isMoving = false;
        scroll.vertical = true;
        isPaused = false;
    }

    bool canStep = true;
    public void pauseButton()
    {
        isPaused = true;
        timeBetweenInstructions = .3f;
        if (pause.activeSelf)
        {
            //StopAllCoroutines();
            playReg.SetActive(true);
            playFast.SetActive(false);
            step.SetActive(true);
            pause.SetActive(false);
            scroll.verticalNormalizedPosition = 1 - (currentInstruction / 63f);
            //isMoving = false;
        }
        else
        {
            if (!isRunning)
            {
                runProgram();
            }
            else if(canStep)
            {
                canStep = false;
                StartCoroutine(runProgramHelper());
            }
        }
    }
    [SerializeField] GameObject overlay;
    public void optionButton()
    {
        if (onComplete || onCutscene)
        {
            return;
        }

        if (!overlay.activeSelf)
        {
            //music.mute = true;
            step.SetActive(false);
            pause.SetActive(true);
            overlay.SetActive(true);
            pauseButton();
        }
        else
        {
            //music.mute = false;
            overlay.SetActive(false);
        }
        GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/beep" + UnityEngine.Random.Range(0, 4).ToString()), .5f);
    }

    public void backToLevelSelect()
    {
        GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/beep" + UnityEngine.Random.Range(0, 4).ToString()), .5f);
        canvas.gameObject.SetActive(false);
        music.gameObject.SetActive(false);
        StartCoroutine(backToLevelSelectCo());
    }

    public void backToLevelSelectComplete()
    {
        GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/beep" + UnityEngine.Random.Range(0, 4).ToString()), .5f);
        canvas.gameObject.SetActive(false);
        music.gameObject.SetActive(false);
        if(levelID == 0)
        {
            if (aHandler != null) { aHandler.unlockAchievement(1); }
        }

        if(levelID == 10 || levelID == 4 || levelID == 6 || levelID == 3)
        {
            if (aHandler != null) { aHandler.unlockAchievement(2); }
        }

        bool completeAll = true;

        for(int i = 0; i < 11; i++)
        {
            if(PlayerPrefs.GetInt("completed_" + i.ToString(), 0) == 0)
            {
                completeAll = false;
                break;
            }
        }

        if (completeAll)
        {
            if (aHandler != null) { aHandler.unlockAchievement(4); }
        }

        if (levelID == 9)
        {
            if (aHandler != null) { aHandler.unlockAchievement(3); }
            StartCoroutine(goToEnding());
        }
        else
        {
            StartCoroutine(backToLevelSelectCo());
        }
        
    }

    IEnumerator backToLevelSelectCo()
    {
        
        yield return new WaitForSeconds(.5f);
        SceneManager.LoadScene("LevelSelect");
    }

    IEnumerator goToEnding()
    {

        yield return new WaitForSeconds(.5f);
        SceneManager.LoadScene("Ending");
    }

    [SerializeField] GameObject manual;
    public void viewManual()
    {
        if (aHandler != null) { aHandler.unlockAchievement(0); }
        manual.SetActive(true);
        overlay.SetActive(false);
        GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/beep" + UnityEngine.Random.Range(0, 4).ToString()), .5f);
    }

    public void leaveManual()
    {
        manual.SetActive(false);
        overlay.SetActive(false);
        GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/beep" + UnityEngine.Random.Range(0, 4).ToString()), .5f);
    }

    [SerializeField] AudioSource dialogue;
    public void viewCutscene()
    {
        stopButton();
        music.mute = true;
        gameObject.GetComponent<AudioSource>().mute = true;
        cutsceneObject.SetActive(true);
        cutsceneDrawing.texture = Resources.Load<Texture>("Sprites/Cutscenes/" + levelID.ToString());
        onCutscene = true;
        cutsceneObject.transform.localPosition = new Vector2(0, 0);
        

        StartCoroutine(viewCutsceneCo());
    }

    IEnumerator viewCutsceneCo()
    {
        dialogue.clip = Resources.Load<AudioClip>("SoundEffects/CutsceneVoices/" + levelID.ToString());
        dialogue.time = 0;
        yield return new WaitWhile(() => !(dialogue.clip.loadState.Equals(AudioDataLoadState.Loaded)));
        dialogue.Play();
        yield return new WaitWhile(() => dialogue.isPlaying);
        Instantiate(Resources.Load<Mover>("Prefabs/Mover")).set(cutsceneObject.GetComponent<RectTransform>(), new Vector2(0,-900), 1f);
        music.mute = false;
        gameObject.GetComponent<AudioSource>().mute = false;
        onCutscene = false;

    }

    public void clearProgram()
    {
        stopButton();
        GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SoundEffects/beep" + UnityEngine.Random.Range(0, 4).ToString()), .5f);
        int lNum = PlayerPrefs.GetInt("selectedLevel", 0); ;
        for (int i = 0; i < 64; i++)
        {
            PlayerPrefs.SetInt("instruction_" + lNum.ToString() + "_" + i.ToString(), 0);
            PlayerPrefs.SetInt("breakpoint_" + lNum.ToString() + "_" + i.ToString(), 0);
        }
        setButtons();
        saveSlice();
    }

    bool onComplete = false;
    int cycleCount = 0;
    int worstCycleCount = 0;

    [SerializeField] Histogram instructionHistogram;
    [SerializeField] Histogram cycleHistogram;
    public void renderComplete()
    {
        int oldInst = PlayerPrefs.GetInt("bestInstructions_" + levelID.ToString(), -1);
        int oldCyc = PlayerPrefs.GetInt("bestCycles_" + levelID.ToString(), -1);

        bool updateValuesCyc = false;
        bool updateValuesInst = false;

        PlayerPrefs.SetInt("completed_" + levelID.ToString(), 1);
        if(PlayerPrefs.GetInt("bestCycles_" + levelID.ToString(), -1) == -1 || (worstCycleCount < PlayerPrefs.GetInt("bestCycles_" + levelID.ToString(), -1)))
        {
            PlayerPrefs.SetInt("bestCycles_" + levelID.ToString(), worstCycleCount);
            updateValuesCyc = true;
        }

        int instCount = 0;
        foreach(int i in instructions)
        {
            if(i != 0)
            {
                instCount++;
            }
        }

        

        if (PlayerPrefs.GetInt("bestInstructions_" + PlayerPrefs.GetInt("selectedLevel", 0).ToString(), -1) == -1 || (instCount < PlayerPrefs.GetInt("bestInstructions_" + PlayerPrefs.GetInt("selectedLevel", 0).ToString(), -1)))
        {
            PlayerPrefs.SetInt("bestInstructions_" + levelID.ToString(), instCount);
            updateValuesInst = true;
        }

        instructionHistogram.SetIndicators();
        cycleHistogram.SetIndicators();
        onComplete = true;
        int oldBarNumCycles = oldCyc;
        int oldBarNumInst = oldInst;

        if(oldBarNumCycles > 0)
        {
            oldBarNumCycles = Mathf.Min((int)(oldCyc / 62.5f), 15);
        }

        if(oldBarNumInst > 0)
        {
            oldBarNumInst = Mathf.Min((int)(oldInst / 4), 15);
        }


        int barNumCycles = Mathf.Min((int)(worstCycleCount / 62.5f), 15);
        int barNumInstructions = Mathf.Min((int)(instCount / 4), 15);

        if (updateValuesCyc)
        {
            StartCoroutine(sendData("cycles", barNumCycles, oldBarNumCycles));
        }

        if (updateValuesInst)
        {
            StartCoroutine(sendData("instructions", barNumInstructions, oldBarNumInst));
        }
    }

    IEnumerator sendData(string mode, int barNum, int oldDat)
    {
        Debug.Log("sending!!!");
        WWWForm form = new WWWForm();
        form.AddField("old", oldDat);
        form.AddField("mode", mode);
        form.AddField("level", levelID);
        form.AddField("barNum", barNum);

        using (UnityWebRequest www = UnityWebRequest.Post("https://crowseeds.com/AURORA/scoreReceive.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                instructionHistogram.SetBarsStarter();
                cycleHistogram.SetBarsStarter();
            }
        }
    }
    [SerializeField] AudioSource music;

    public void commitInstructions()
    {
        for(int i = 0; i < 63; i++)
        {
            PlayerPrefs.SetInt("instruction_" + levelID.ToString() + "_" + i.ToString(), instructions[i]);
        }
        setButtons();
    }


    public void undo()
    {
        if (!isRunning && timesliceIndex > 0)
        {
            Debug.Log("mioafmeo");
            instructions = (int[])timeslices[timesliceIndex - 1].Clone();
            commitInstructions();
            timesliceIndex = timesliceIndex - 1;
        }
    }

    public void redo()
    {
        if (!isRunning &&  timesliceIndex < timeslices.Count - 1)
        {
            Debug.Log("mioafmeo");
            instructions = (int[])timeslices[timesliceIndex + 1].Clone();
            commitInstructions();
            timesliceIndex = timesliceIndex + 1;
        }
    }

    void saveSlice()
    {
        if (timeslices.Count < 30 && timesliceIndex == timeslices.Count - 1)
        {
            timeslices.Add((int[])instructions.Clone());
            timesliceIndex++;
        }
        else if (timesliceIndex == 29)
        {
            for (int i = 0; i < 29; i++)
            {
                timeslices[i] = timeslices[i + 1];
            }
            timeslices[29] = (int[])instructions.Clone();
        }
        else
        {
            timeslices[timesliceIndex + 1] = (int[])instructions.Clone();
            for (int i = timesliceIndex + 2; i < timeslices.Count; i++)
            {
                timeslices.RemoveAt(timesliceIndex + 2);
            }
            timesliceIndex++;
        }
    }
}
