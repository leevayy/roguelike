using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public static MusicManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            
            return;
        }
        
        instance = this;
    }
}
