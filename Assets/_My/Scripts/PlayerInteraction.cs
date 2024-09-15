using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private SculptureManager currentSculpture;

    // Ʈ���� ������ ������ ��
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Ʈ���ŵ� ������Ʈ: " + other.gameObject.name);  // ������ ������Ʈ�� �̸��� ���

        SculptureManager sculptureManager = other.GetComponentInParent<SculptureManager>();

        if (sculptureManager != null)
        {
            currentSculpture = sculptureManager;
            Debug.Log("Exit Sculpture ��ó�� ����");
        }
    }


    // Ʈ���� ������ ������ ��
    private void OnTriggerExit(Collider other)
    {
        // Exit Sculpture�� ����� ��
        if (other.CompareTag("ExitSculpture"))
        {
            currentSculpture = null;
            Debug.Log("Exit Sculpture���� �־���");
        }
    }

    // Update���� ��ȣ�ۿ� Ű �Է� ó��
    void Update()
    {
        // VŰ�� ������ �� ��ȣ�ۿ�
        if (currentSculpture != null && Input.GetKeyDown(KeyCode.V))
        {
            currentSculpture.CollectSculpture();  // SculptureManager ��ũ��Ʈ���� CollectSculpture ȣ��
            Debug.Log("������ ������");
        }
    }
}
