//이 스크립트는 Player(Capsule) 오브젝트를 제어하는 기능을 수행합니다.

using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    float fMoveSpeed = 5.0f;    //플레이어의 이동 속도
    float fAxisX = 0.0f;        //수평축(X) 입력 값
    float fAxisZ = 0.0f;        //수직축(Y) 입력 값

    Vector3 vMoveDirection = Vector3.zero; //플레이어의 이동 방향을 저장하는 벡터

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Keyboard InputKey = Keyboard.current;   //현재 키보드 입력을 가져옵니다.
        if (InputKey == null) return;           //키보드 입력이 없으면 아래 코드가 실행되지 않도록 합니다.

        //키보드의 WASD 또는 화살표 키 입력을 통해 플레이어의 이동 방향을 결정합니다.
        fAxisX = (InputKey.aKey.isPressed || InputKey.leftArrowKey.isPressed ? -1.0f : 0.0f) +
                 (InputKey.dKey.isPressed || InputKey.rightArrowKey.isPressed ? 1.0f : 0.0f);

        fAxisZ = (InputKey.wKey.isPressed || InputKey.upArrowKey.isPressed ? 1.0f : 0.0f) +
                 (InputKey.sKey.isPressed || InputKey.downArrowKey.isPressed ? -1.0f : 0.0f);
        
        //입력된 방향을 정규화하여 단위 벡터를 만듭니다.(대각선 이동 시 속도가 빨라지는 것을 방지합니다.)
        vMoveDirection = new Vector3(fAxisX, 0.0f, fAxisZ).normalized;

        transform.Translate(vMoveDirection * fMoveSpeed * Time.deltaTime); //플레이어 오브젝트를 이동시킵니다.
    }
}
