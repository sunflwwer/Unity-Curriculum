using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SculptureManager : MonoBehaviour
{
    [SerializeField] private GameObject exitSign;  // 해당 Exit Sculpture와 연동된 Exit Sign
    private bool isCollected = false;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip collectionSound;  // 사인 수집 시 재생할 오디오 클립
    private AudioSource audioSource;  // 오디오를 재생할 AudioSource

    private void Start()
    {
        // AudioSource를 가져오거나 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // 조각 수집 시 호출
    public void CollectSculpture()
    {
        if (!isCollected)
        {
            isCollected = true;
            exitSign.SetActive(false);  // Exit Sign 비활성화
            GameManager.instance.SculptureCollected();  // GameManager에 수집 알림

            // 수집 사운드 재생
            if (collectionSound != null)
            {
                audioSource.PlayOneShot(collectionSound);
                StartCoroutine(StopSoundAfterDelay(2f));  // 2초 후 사운드를 멈춤
            }
        }
    }

    // 2초 후 사운드를 멈추는 코루틴
    private IEnumerator StopSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.Stop();  // 오디오를 멈춤
    }
}
