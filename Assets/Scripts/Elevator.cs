using UnityEngine;

public class Elevator : MonoBehaviour
{
    [SerializeField] private GameObject elevator;
    [SerializeField] private float timeBeforeLeave = 3f;
    [SerializeField] private float levelHeight = 20f; 
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private AudioSource elevatorMusic;

    private float _timeOnThePlatform = 0f;
    private bool _isMoving = false;
    private int _level = 1;

    private void OnTriggerStay(Collider other)
    {
        if (!_isMoving && other.CompareTag("Player"))
        {
            _timeOnThePlatform += Time.deltaTime;
        }

        if (!_isMoving && _timeOnThePlatform >= timeBeforeLeave)
        {
            StartCoroutine(GoTo(_level == 2 ? 1 : 2));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _timeOnThePlatform = 0f;
            
            MusicManager.instance.audioSource.UnPause();
        }
    }

    public async Awaitable GoTo(int level)
    {
        if (_isMoving || _level == level) return;

        MusicManager.instance.audioSource.Pause();
        
        elevatorMusic.Play();
        
        _timeOnThePlatform = 0f;
        
        _isMoving = true;
        
        var elapsedTime = 0f;

        var currentPosition = elevator.transform.position;
        var targetPosition = currentPosition + Vector3.up * ((level - _level) * levelHeight);

        while (elapsedTime < duration)
        {
            var t = elapsedTime / duration;
            elevator.transform.position = Vector3.Lerp(currentPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }
        
        elevator.transform.position = targetPosition;
        
        _level = level;
        _isMoving = false;
    }
}
