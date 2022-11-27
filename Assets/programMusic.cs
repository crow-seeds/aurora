using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class programMusic : MonoBehaviour
{
    int musicTrack = 1;
    AudioSource source;
    AudioClip introSong;
    AudioClip loop;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        if(PlayerPrefs.GetInt("selectedLevel", 0) % 2 == 1)
        {
            musicTrack = 2;
        }
        introSong = Resources.Load<AudioClip>("Music/program" + musicTrack.ToString() + "_intro");
        loop = Resources.Load<AudioClip>("Music/program" + musicTrack.ToString() + "_loop"); 
        StartCoroutine(intro());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator intro()
    {
        source.clip = introSong;
        source.Play();
        yield return new WaitWhile(() => source.isPlaying);
        source.loop = true;
        source.clip = loop;
        source.time = 0;
        source.Play();
    }
}
