using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System;

public class AudioManager : MonoBehaviour
{

    //[SerializeField]
    //private OnOffImageToggle displayMuted;

    Dictionary<string, AudioSource> audios = new Dictionary<string, AudioSource>();

    [SerializeField]
    Transform audioSourceFolder;

    [SerializeField] AudioSource fireAudio;
    [SerializeField] AudioSource reloadAudio;

    private const string PPrefKeyMuted = "PPrefsMuted";
    private bool getPlayerPrefsMuted() {
        if (PlayerPrefs.HasKey(PPrefKeyMuted)) { return PlayerPrefs.GetInt(PPrefKeyMuted) > 0; }
        return false;
    }

    private Watchable<bool> _muted;
    private Watchable<bool> muted {
        get {
            if (_muted == null) {
                _muted = new Watchable<bool>(getPlayerPrefsMuted());
            }
            return _muted;
        }
    }

    private void setMuted(bool shouldMute) {
        muted._value = shouldMute;
    }


    public void toggleMute() {
        setMuted(!muted._value);
    }

    public static string Dink = "Dink";
    public static string Zap = "Zap";
    public void Awake() {
        foreach (AudioSource au in audioSourceFolder.GetComponentsInChildren<AudioSource>()) {
            audios.Add(au.gameObject.name, au);
        }
    }

    private void Start() {
        //muted.subscribe((bool b) => { displayMuted.toggle(b); });
        //setMuted(getPlayerPrefsMuted());
    }

    public void playDink() { play(Dink); }

    internal void playZap() { play(Zap); }

    public void play(string name) {
        if (muted._value) {
            return;
        }
        AudioSource aud = getSource(name);
        if (aud) { aud.Play(); }
    }

    //lazy loading
    private AudioSource getSource(string resourcesAudioRelativePath) {

        if (audios.ContainsKey(resourcesAudioRelativePath)) { return audios[resourcesAudioRelativePath]; }

        AudioClip clip = FindClip(resourcesAudioRelativePath);
        Assert.IsTrue(clip, "null audio clip? " + resourcesAudioRelativePath);
        GameObject go = new GameObject(resourcesAudioRelativePath);
        AudioSource aud = go.AddComponent<AudioSource>();
        aud.clip = clip;
        go.transform.SetParent(audioSourceFolder);
        audios.Add(go.name, aud);
        return aud;
    }

    public static AudioClip FindClip(string resourcesAudioRelativePath) {
        return Resources.Load<AudioClip>(string.Format("Audio/{0}", resourcesAudioRelativePath));
    }

}
