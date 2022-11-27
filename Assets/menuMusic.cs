using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class menuMusic : MonoBehaviour
{
    [SerializeField] AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(changeTrack());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator changeTrack()
    {
        int num = Random.Range(0, 6);
        source.Stop();
        source.clip = Resources.Load<AudioClip>("Music/menu" + num.ToString());
        source.time = 0;
        source.Play();
        yield return new WaitWhile(() => source.isPlaying);
        StartCoroutine(changeTrack());
    }
}
