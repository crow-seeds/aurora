using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Text;
using System.Globalization;
using System.Xml;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

// ***************************************************
// ** PUT YOUR SAVE PATH NAME IN THE VARIABLE BELOW **
// ***************************************************

// This class acts sort of as an emulator for
// PlayerPrefs, but saves to a specified
// place in IndexedDB so saved games don't
// get wiped when game updates are uploaded

// PlayerPrefs is similar (but not identical)
// to a hash of ints, floats, and strings, so
// this uses a Dictionary in C# (like a hash)

// Note that in this implementation all of
// the values are stored as strings even if
// they're really int or float, but they
// get parsed when values are returned

// Also note that I call Save() any time
// a value gets changed, because I think
// the risk of not saving stuff leading to
// bugginess outweighs the computational
// cost of saving more frequently.
// That could potentially be changed fairly
// easily as long as you know it's happening
// and that you just need to comment out the
// Save() statements at the end of the Set
// methods.

#if UNITY_EDITOR
#elif UNITY_WEBGL
public static class PlayerPrefs {
// **********************************
// ** PUT YOUR SAVE PATH NAME HERE **
// **********************************  
  static string savePathName = "1979370";
  static string fileName;
  static string[] fileContents;
  static Dictionary<string, string> saveData = new Dictionary<string, string>();
  
  // This is the static constructor for the class
  // When invoked, it looks for a savegame file
  // and reads the keys and values
  static PlayerPrefs() {
    fileName = "/idbfs/" + savePathName + "/NGsave.dat";
    
    // Open the savegame file and read all of the lines
    // into fileContents
    // First make sure the directory and save file exist,
    // and make them if they don't already
    // (If the file is created, the filestream needs to be
    // closed afterward so it can be saved to later)
    if (!Directory.Exists("/idbfs/" + savePathName)) {
      Directory.CreateDirectory("/idbfs/" + savePathName);
    }
    if (!File.Exists(fileName)) {
      FileStream fs = File.Create(fileName);
      fs.Close();
    } else {
      // Read the file if it already existed
      fileContents = File.ReadAllLines(fileName);
      
      // If you want to use encryption/decryption, add your
      // code for decrypting here
      //   ******* decryption algorithm ********
      
      // Put all of the values into saveData
      for (int i=0; i<fileContents.Length; i += 2) {
        saveData.Add(fileContents[i], fileContents[i+1]);
      }
    }
  }
  
  // This saves the saveData to the player's IndexedDB
  public static void Save() {
    // Put the saveData dictionary into the fileContents
    // array of strings
    Array.Resize(ref fileContents, 2 * saveData.Count);
    int i=0;
    foreach (string key in saveData.Keys) {
      fileContents[i++] = key;
      fileContents[i++] = saveData[key];
    }
    
    // If you want to use encryption/decryption, add your
    // code for encrypting here
    //   ******* encryption algorithm ********
    
    // Write fileContents to the save file
    File.WriteAllLines(fileName, fileContents);
  }
  
  // The following methods emulate what PlayerPrefs does
  public static void DeleteAll() {
    saveData.Clear();
    Save();
  }
  
  public static void DeleteKey(string key) {
    saveData.Remove(key);
    Save();
  }
  
  public static float GetFloat(string key) {
    if (saveData.ContainsKey(key)) {
      return float.Parse(saveData[key]);
    } else {
      return 0;
    }
  }
  public static float GetFloat(string key, float defaultValue) {
    if (saveData.ContainsKey(key)) {
      return float.Parse(saveData[key]);
    } else {
      return defaultValue;
    }
  }
  
  public static int GetInt(string key) {
    if (saveData.ContainsKey(key)) {
      return int.Parse(saveData[key]);
    } else {
      return 0;
    }
  }
  public static int GetInt(string key, int defaultValue) {
    if (saveData.ContainsKey(key)) {
      return int.Parse(saveData[key]);
    } else {
      return defaultValue;
    }
  }
  
  public static string GetString(string key) {
    if (saveData.ContainsKey(key)) {
      return saveData[key];
    } else {
      return "";
    }
  }
  public static string GetString(string key, string defaultValue) {
    if (saveData.ContainsKey(key)) {
      return saveData[key];
    } else {
      return defaultValue;
    }
  }
  
  public static bool HasKey(string key) {
    return saveData.ContainsKey(key);
  }
  
  public static void SetFloat(string key, float setValue) {
    if (saveData.ContainsKey(key)) {
      saveData[key] = setValue.ToString();
    } else {
      saveData.Add(key, setValue.ToString());
    }
    Save();
  }
  
  public static void SetInt(string key, int setValue) {
    if (saveData.ContainsKey(key)) {
      saveData[key] = setValue.ToString();
    } else {
      saveData.Add(key, setValue.ToString());
    }
    Save();
  }
  
  public static void SetString(string key, string setValue) {
    if (saveData.ContainsKey(key)) {
      saveData[key] = setValue;
    } else {
      saveData.Add(key, setValue);
    }
    Save();
  }
}
#endif

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
    [SerializeField] TextMeshProUGUI name;
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


    private void Awake()
    {
        setButtons();
        loadData(levelID);
        renderTestCase();
    }

    // Start is called before the first frame update
    void Start()
    {
        EasingFunction.Ease movement = EasingFunction.Ease.EaseOutBack;
        function = EasingFunction.GetEasingFunction(movement);
        scroll.verticalNormalizedPosition = .997f;
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
        name.text = layoutNode.Attributes["name"].Value;
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
            Debug.Log("2: ");
            Debug.Log(testCases[inBar2][testCaseNum][barPosition[inBar2]]);
            return testCases[inBar2][testCaseNum][barPosition[inBar2]];
        }else if(i == 3)
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
            if (!gotWrong)
            {
                if(testCaseNum == 2)
                {
                    //win
                }
                else
                {
                    Debug.Log("pee");
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
            g.transform.SetParent(canvas);
            g.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            g.transform.position = bars[num].barCells[barPosition[num]].transform.position;
            gotWrong = true;
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
            if(timeBetweenInstructions > 0)
            {
                timer += Time.deltaTime / timeBetweenInstructions;
                scroll.verticalNormalizedPosition = function(1 - (previousInstruction / 63f), 1 - (currentInstruction / 63f), timer);
                if(timer >= 1)
                {
                    scroll.verticalNormalizedPosition = 1 - (currentInstruction / 63f);
                }
            }
            else
            {
                scroll.verticalNormalizedPosition = 1 - (currentInstruction / 63f);
            }
        }
    }

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
            b.GetComponent<RawImage>().color = Color.gray;
        }
        else
        {
            b.GetComponent<RawImage>().color = Color.white;
        }
    }

    [SerializeField] List<Transform> instructionObjects;

    public void setButtons()
    {
        for(int i = 0; i < 64; i++)
        {
            int num = PlayerPrefs.GetInt("instruction_" + levelID.ToString() + "_" + i.ToString(), 0);
            if(num != 0)
            {
                instructions[i] = num;
                for(int j = 0; j < 10; j++)
                {
                    if(((num >> j) & 1) == 1)
                    {
                        instructionObjects[i].GetChild(10 - j).GetComponent<RawImage>().color = Color.gray;
                        instructionObjects[i].GetChild(10 - j).GetComponent<ButtonParameters>().active = true;
                    }
                }
            }
        }
    }

    bool isRunning = false;

    public void runProgram()
    {
        if (!isRunning)
        {
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
                                StartCoroutine(move(i, dir));
                            }
                        }
                    }
                    else if(hex >= 0 && hex < hexagons.Count)
                    {
                        StartCoroutine(move(hex, dir));
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
                                    sub(i, amt);
                                }
                            }
                        }
                        else
                        {
                            sub(hex, amt);
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
                                copy(i, dir);
                            }
                        }
                    }
                    else if (hex >= 0 && hex < hexagons.Count)
                    {
                        copy(hex, dir);
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
                    jumpTo = (sign == 1 ? -1 : 1 * (instruction & 0b11)) + currentInstruction;
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
                    jumpTo = (sign == 1 ? -1 : 1 * (instruction & 0b11)) + currentInstruction;
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
                    jumpTo = (sign == 1 ? -1 : 1 * (instruction & 0b11)) + currentInstruction;
                    break;

            }

            
            previousInstruction = currentInstruction;
            if (!willJump)
            {
                currentInstruction++;
            }
            else
            {
                currentInstruction = jumpTo;
            }
            
            timer = 0;
            yield return new WaitForSeconds(timeBetweenInstructions);
            canStep = true;
            if (!isPaused)
            {
                StartCoroutine(runProgramHelper());
            }
        }
        else if(isRunning)
        {
            currentInstruction = 0;
            timer = 0;
            yield return new WaitForSeconds(timeBetweenInstructions);
            canStep = true;
            if (!isPaused)
            {
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

    public IEnumerator move(int hex, int dir)
    {
        if(dir == 0)
        {
            StartCoroutine(move(hex, 1));
            StartCoroutine(move(hex, 2));
            StartCoroutine(move(hex, 3));
            StartCoroutine(move(hex, 4));
            StartCoroutine(move(hex, 5));
            StartCoroutine(move(hex, 6));
            yield break;
        }

        Hexagon h = hexagons[hex];
        Color c = h.getColor(dir);
        int outHexNum = -1;
        
        switch (dir)
        {
            case 1:
                if(hex > 3)
                {
                    hexagons[hex - 4].give(3, timeBetweenInstructions, c);
                    outHexNum = hex - 4;
                }
                break;
            case 2:
                if(hex % 4 != 3 && hex != 0 && hex != 2)
                {
                    if(hex % 2 == 1)
                    {
                        hexagons[hex + 1].give(4, timeBetweenInstructions, c);
                        outHexNum = hex + 1;
                    }
                    else
                    {
                        hexagons[hex - 3].give(4, timeBetweenInstructions, c);
                        outHexNum = hex - 3;
                    }
                }
                break;
            case 3:
                if (hex % 4 != 3 && hex != 13 && hex != 14)
                {
                    if (hex % 2 == 1)
                    {
                        Debug.Log("test!!!");
                        hexagons[hex + 5].give(5, timeBetweenInstructions, c);
                        outHexNum = hex + 5;
                    }
                    else
                    {
                        hexagons[hex + 1].give(5, timeBetweenInstructions, c);
                        outHexNum = hex + 1;
                    }
                }
                break;
            case 4:
                if (hex < 11)
                {
                    hexagons[hex + 4].give(0, timeBetweenInstructions, c);
                    outHexNum = hex + 4;
                }
                break;
            case 5:
                if (hex % 4 != 0 && hex != 13)
                {
                    if (hex % 2 == 1)
                    {
                        hexagons[hex + 3].give(1, timeBetweenInstructions, c);
                        outHexNum = hex + 3;
                    }
                    else
                    {
                        hexagons[hex - 1].give(1, timeBetweenInstructions, c);
                        outHexNum = hex - 1;
                    }
                }
                break;
            case 6:
                if (hex % 4 != 0 && hex != 2)
                {
                    if (hex % 2 == 1)
                    {
                        hexagons[hex - 1].give(2, timeBetweenInstructions, c);
                        outHexNum = hex - 1;
                    }
                    else
                    {
                        hexagons[hex - 5].give(2, timeBetweenInstructions, c);
                        outHexNum = hex - 5;
                    }
                }
                break;
        }

        if(h.isInput >= 0)
        {
            h.takeAll(timeBetweenInstructions / 2f, Color.white);
            yield return new WaitForSeconds(timeBetweenInstructions / 2f);
            h.giveAll(timeBetweenInstructions / 2f, getColor(h.isInput));
            yield return new WaitForSeconds(timeBetweenInstructions / 2f);
        }
        else
        {
            Debug.Log("ok?");
            h.take(dir-1, timeBetweenInstructions, Color.white);
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

    public void sub(int hex, int color)
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

        if(hexagons[hex].isOutput < 0)
        {
            hexagons[hex].takeAll(timeBetweenInstructions, c);
        }
    }

    public void copy(int hex, int dir)
    {
        Hexagon h = hexagons[hex];
        Color c = Color.black;
        if (dir == 0)
        {
            for (int i = 0; i < 6; i++)
            {
                c += h.getColor(i);
            }
        }
        else
        {
            c = h.getColor(dir);
            Debug.Log(c);
            Debug.Log(dir);
            Debug.Log(hex);
        }


        

        if (hexagons[hex].isOutput >= 0)
        {
            hexagons[hex].giveAllForce(timeBetweenInstructions, c);
            addColorToBar(hexagons[hex].isOutput, c);
        }
        else
        {
            hexagons[hex].giveAll(timeBetweenInstructions, c);
        }
    }

    public void setBreakpoint(int i)
    {
        if (breakpoints.Contains(i))
        {
            breakpoints.Remove(i);
        }
        else
        {
            breakpoints.Add(i);
        }
    }

    [SerializeField] GameObject playReg;
    [SerializeField] GameObject playFast;
    [SerializeField] GameObject pause;
    [SerializeField] GameObject step;

    bool isPaused = false;

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
            timeBetweenInstructions = .01f;
        }
    }

    public void stopButton()
    {
        GameObject[] procs = GameObject.FindGameObjectsWithTag("process");
        foreach (GameObject g in procs)
        {
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
}
