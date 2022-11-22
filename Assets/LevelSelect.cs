using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
                }

                foreach (int i in onHexNums)
                {
                    if (PlayerPrefs.GetInt("completed_" + i.ToString(), 0) == 1)
                    {
                        b.outerHex.color = Color.gray;
                        b.GetComponent<Button>().enabled = true;
                        b.levelNum.color = Color.gray;
                        break;
                    }
                }
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
        
    }

    public void selectLevel(int i)
    {
        buttons[i].transform.SetAsLastSibling();
        int oldSelect = PlayerPrefs.GetInt("selectedLevel", 0);
        Instantiate(Resources.Load<Rotator>("Prefabs/Rotator")).set(buttons[oldSelect].GetComponent<RectTransform>(), -360, .25f);
        Instantiate(Resources.Load<Scaler>("Prefabs/Scaler")).set(buttons[i].GetComponent<RectTransform>(), new Vector2(1f, 1f), .25f);
        Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(buttons[oldSelect].outerHex, Color.gray, .25f);
        Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(buttons[oldSelect].levelNum, Color.gray, .25f);

        PlayerPrefs.SetInt("selectedLevel", i);
        Instantiate(Resources.Load<Rotator>("Prefabs/Rotator")).set(buttons[i].GetComponent<RectTransform>(), 360, .25f);
        Instantiate(Resources.Load<Scaler>("Prefabs/Scaler")).set(buttons[i].GetComponent<RectTransform>(), new Vector2(1.1f,1.1f), .25f);
        Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(buttons[i].outerHex, Color.white, .25f);
        Instantiate(Resources.Load<ColorFader>("Prefabs/ColorFader")).set(buttons[i].levelNum, Color.white, .25f);
        levelName.text = levelNames[i];
    }

    public void editProgram()
    {
        SceneManager.LoadScene("program");
    }

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
