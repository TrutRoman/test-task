using UnityEngine;

// One per scene: UI and feedback sounds go through one AudioSource, the looping music through another.
public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource uiSource;
    [SerializeField] AudioSource musicSource;

    [Header("Clips")]
    [SerializeField] AudioClip hover;
    [SerializeField] AudioClip click;
    [SerializeField] AudioClip save;
    [SerializeField] AudioClip load;
    [SerializeField] AudioClip pause;
    [SerializeField] AudioClip resume;
    [SerializeField] AudioClip music;

    void Awake()
    {
        if (uiSource == null || musicSource == null) throw new MissingReferenceException("SoundManager: audio sources are not assigned.");
        if (hover == null || click == null || save == null || load == null) throw new MissingReferenceException("SoundManager: a UI clip is not assigned.");
        if (pause == null || resume == null || music == null) throw new MissingReferenceException("SoundManager: a pause or music clip is not assigned.");

        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.volume = SoundLevels.Music;
        musicSource.Play();
    }

    public void PlayHover() => uiSource.PlayOneShot(hover, SoundLevels.Ui);
    public void PlayClick() => uiSource.PlayOneShot(click, SoundLevels.Ui);
    public void PlaySave() => uiSource.PlayOneShot(save, SoundLevels.Feedback);
    public void PlayLoad() => uiSource.PlayOneShot(load, SoundLevels.Feedback);
    public void PlayPause() => uiSource.PlayOneShot(pause, SoundLevels.Feedback);
    public void PlayResume() => uiSource.PlayOneShot(resume, SoundLevels.Feedback);
}
