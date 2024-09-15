using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SculptureManager : MonoBehaviour
{
    [SerializeField] private GameObject exitSign;  // �ش� Exit Sculpture�� ������ Exit Sign
    private bool isCollected = false;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip collectionSound;  // ���� ���� �� ����� ����� Ŭ��
    private AudioSource audioSource;  // ������� ����� AudioSource

    private void Start()
    {
        // AudioSource�� �������ų� �߰�
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // ���� ���� �� ȣ��
    public void CollectSculpture()
    {
        if (!isCollected)
        {
            isCollected = true;
            exitSign.SetActive(false);  // Exit Sign ��Ȱ��ȭ
            GameManager.instance.SculptureCollected();  // GameManager�� ���� �˸�

            // ���� ���� ���
            if (collectionSound != null)
            {
                audioSource.PlayOneShot(collectionSound);
                StartCoroutine(StopSoundAfterDelay(2f));  // 2�� �� ���带 ����
            }
        }
    }

    // 2�� �� ���带 ���ߴ� �ڷ�ƾ
    private IEnumerator StopSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.Stop();  // ������� ����
    }
}
