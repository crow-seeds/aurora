using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using System.Xml;
using System.IO;

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
public class NGHelper : MonoBehaviour
{
    public io.newgrounds.core ngio_core;
    public bool hasNewgrounds;
    float timer = 0;
    bool loggedIn = false;

    achivementHandler aHandler;

    // Start is called before the first frame update
    void Start()
    {
        aHandler = gameObject.GetComponent<achivementHandler>();

        if(/*Application.platform == RuntimePlatform.WebGLPlayer &&*/ hasNewgrounds)
        {
            ngio_core.onReady(() =>
            {
                ngio_core.checkLogin((bool logged_in) => {
                    if (logged_in)
                    {
                        onLoggedIn();
                    }
                    else
                    {
                        requestLogIn();
                    }

                });
            });
        }
        else
        {
            ngio_core.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (hasNewgrounds)
        {
            timer += Time.deltaTime;
            if(timer > 10)
            {
                timer = 0;
                io.newgrounds.components.Gateway.ping p = new io.newgrounds.components.Gateway.ping();
                p.callWith(ngio_core, ping);
            }
        }
    }

    void ping(io.newgrounds.results.Gateway.ping result)
    {
        Debug.Log(result.pong);
    }

    void onLoggedIn()
    {
        loggedIn = true;
        aHandler.runThroughAchievements();
    }

    void requestLogIn()
    {
        ngio_core.requestLogin(onLoggedIn);
    }

    public void unlockMedal(int medal_id)
    {
        if (hasNewgrounds && loggedIn)
        {
            io.newgrounds.components.Medal.unlock medal_unlock = new io.newgrounds.components.Medal.unlock();

            medal_unlock.id = medal_id;

            medal_unlock.callWith(ngio_core, onMedalUnlocked);
        }
    }

    void onMedalUnlocked(io.newgrounds.results.Medal.unlock result)
    {
        io.newgrounds.objects.medal medal = result.medal;
        Debug.Log("Medal Unlocked: " + medal.name + " (" + medal.value + " points)");
    }

    public void submitScore(int score_id, int score)
    {
        io.newgrounds.components.ScoreBoard.postScore submit_score = new io.newgrounds.components.ScoreBoard.postScore();
        submit_score.id = score_id;
        submit_score.value = score;
        submit_score.callWith(ngio_core);
    }
}
