using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private SculptureManager currentSculpture;

    // 트리거 안으로 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("트리거된 오브젝트: " + other.gameObject.name);  // 감지된 오브젝트의 이름을 출력

        SculptureManager sculptureManager = other.GetComponentInParent<SculptureManager>();

        if (sculptureManager != null)
        {
            currentSculpture = sculptureManager;
            Debug.Log("Exit Sculpture 근처에 있음");
        }
    }


    // 트리거 밖으로 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        // Exit Sculpture를 벗어났을 때
        if (other.CompareTag("ExitSculpture"))
        {
            currentSculpture = null;
            Debug.Log("Exit Sculpture에서 멀어짐");
        }
    }

    // Update에서 상호작용 키 입력 처리
    void Update()
    {
        // V키를 눌렀을 때 상호작용
        if (currentSculpture != null && Input.GetKeyDown(KeyCode.V))
        {
            currentSculpture.CollectSculpture();  // SculptureManager 스크립트에서 CollectSculpture 호출
            Debug.Log("사인을 수집함");
        }
    }
}
