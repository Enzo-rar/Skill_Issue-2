using UnityEngine;

public class PlayerSoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource m_SoundSource;
    [SerializeField]
    private AudioClip onHitSound;
    [SerializeField]
    private AudioClip onDeathSound;
    [SerializeField]
    private AudioClip onSlideSound;

    [SerializeField]
    private AudioClip onFlashbangSound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playDeathSound()
    {
        if (onDeathSound != null)
        {
            m_SoundSource.PlayOneShot(onDeathSound);
        }else{
            Debug.LogWarning("Death sound clip is not assigned in PlayerSoundManager.");
        }
    }

    public void playFlashbangSound()
    {
        // Implement flashbang sound logic here if needed
        if(onFlashbangSound != null)
        {
            m_SoundSource.PlayOneShot(onFlashbangSound);
        }else{
            Debug.LogWarning("Flashbang sound clip is not assigned in PlayerSoundManager.");
        }
    }

    public void playSlideSound()
    {
        if (onSlideSound != null)
        {
            m_SoundSource.PlayOneShot(onSlideSound);
        }
    }

    public void playHitSound()
    {
        if (onHitSound != null)
        {
            m_SoundSource.PlayOneShot(onHitSound);
        }
    }

}
