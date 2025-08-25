//이 스크립트는 Enemy(Capsule) 오브젝트를 FSM(Finite State Machine) 방식으로 제어하는 기능을 수행합니다.

using System.Linq.Expressions;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private enum EnemyState //적의 상태를 정의하는 열거형
    {
        Patrol,     //순찰 상태
        Chase,      //추적 상태
        Attack,     //공격 상태
    }

    [SerializeField] private Transform player;          //플레이어 오브젝트의 Transform(위치)을 참조
    [SerializeField] private Transform[] waypoints;     //순찰 경로로 사용할 웨이포인트 배열

    //이동 관련 변수
    float fMoveSpeed = 3.0f;    //적의 이동 속도

    //추적 관련 변수
    float fChaseRange = 5.0f;       //플레이어를 추적할 범위
    float fAttackRange = 1.5f;      //플레이어를 공격할 범위
    float fChaseTimeBuffer = 1.2f;  //플레이어를 놓친 후 추적을 중단하기까지의 시간

    //공격 관련 변수
    float fAttackCooldown = 0.8f; //공격 쿨다운 시간

    //상태 및 내부 값 변수
    EnemyState state = EnemyState.Patrol;   //기본 상태를 순찰로 설정 및 초기화
    float fNextAttackTime = 0.0f;           //다음 공격 가능 시간을 저장하는 변수
    float fDistToPlayer = 0.0f;             //플레이어와의 거리를 저장하는 변수
    int nWaypointIndex = 0;                 //현재 순찰 중인 웨이포인트의 인덱스
    

    //선택 사항 : 오브젝트가 유효하지 않은 경우를 처리하기 위한 변수
    bool isNotFoundObject = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        f_VaildateObjects(); //플레이어와 웨이포인트가 유효한지 확인(선태 사항)
    }

    // Update is called once per frame
    void Update()
    {
        if (isNotFoundObject) return; //플레이어 또는 웨이포인트가 유효하지 않은 경우 업데이트를 중단

        fDistToPlayer = f_UpdateDistanceToPlayer(); //플레이어와의 거리를 저장

        switch (state)
        {
            case EnemyState.Patrol: 
                f_UpdatePatrol(fDistToPlayer);
                
                Debug.Log("[Enemy] Patrol State");
                break;

            case EnemyState.Chase: 
                f_UpdateChase(fDistToPlayer);
                
                Debug.Log("[Enemy] Chase State");
                break;

            case EnemyState.Attack: 
                f_UpdateAttack(fDistToPlayer);
                
                Debug.Log("[Enemy] Attack State");
                break;

            default:
                Debug.LogError("[Enemy] Unknown State!"); //알 수 없는 상태에 대한 오류 메시지 출력
                break;
        }
    }

    //플레이어와의 거리를 계산하여 반환하는 메소드
    float f_UpdateDistanceToPlayer()
    {
        if(player == null) { return Mathf.Infinity; } //플레이어가 할당되지 않은 경우 무한대 거리 반환

        return Vector3.Distance(transform.position, player.position); //적과 플레이어 사이의 거리 계산
    }

    //목표 위치로 이동하는 메소드
    //예제에서는 높이는 고려하지 않고 수평 이동만을 가정한 간단한 이동 함수를 구현합니다.
    void f_MoveTarget(Vector3 targetPosition)
    {
        Vector3 vTargetPos = new Vector3(targetPosition.x, transform.position.y, targetPosition.z); //높이 축 y는 현재 위치와 동일하게 설정

        transform.position = Vector3.MoveTowards(transform.position, vTargetPos, fMoveSpeed * Time.deltaTime); //목표 위치로 이동
    }

    //적이 순찰하는 상태로 전환
    void f_UpdatePatrol(float fDistance)
    {
        if (fDistance <= fChaseRange)
        {
            state = EnemyState.Chase; //플레이어가 추적 범위에 들어오면 상태를 추적으로 변경
            return;
        }

        Transform target = waypoints[nWaypointIndex]; //목표 순찰 웨이포인트를 가져옴

        f_MoveTarget(target.position); //목표 웨이포인트로 이동

        if (Vector3.Distance(transform.position, target.position) < 1.0f)
        {
            nWaypointIndex = (nWaypointIndex + 1) % waypoints.Length; //현재 웨이포인트에 도달하면 다음 웨이포인트로 이동(0 → 1 → 2 → 0 ...)
        }
    }


    //적이 플레이어를 추적하는 상태로 전환
    void f_UpdateChase(float fDistance)
    {
        if (fDistance <= fAttackRange) //플레이어가 공격 범위에 들어오면 상태를 공격으로 변경
        {
            state = EnemyState.Attack;
            return;
        }

        if (fDistance > fChaseRange * fChaseTimeBuffer) //플레이어가 추적 범위를 벗어나면 상태를 순찰로 변경함
        {
            state = EnemyState.Patrol;
            return;
        }

        f_MoveTarget(player.position);
    }

    //적이 플레이어를 공격하는 상태로 전환
    void f_UpdateAttack(float fDistance)
    {
        if (fDistance > fAttackRange) //플레이어가 공격 범위를 벗어나면 상태를 추적으로 변경
        {
            state = EnemyState.Chase;
            return;
        }

        if (Time.time >= fNextAttackTime) //현재 시간이 다음 공격 가능 시간보다 크거나 같으면 공격
        {
            fNextAttackTime = Time.time + fAttackCooldown;
            Debug.Log("[Enemy] Attack!");
        }
    }

    /// <summary> 게임 오브젝트가 유효한지 검사하는 메소드(예제에서는 선택사항) </summary>
    /// <returns> 유효한 경우 true, 그렇지 않은 경우 false </returns>
    bool f_VaildateObjects()
    {
        bool isMissingPlayer = player == null; //플레이어가 할당되지 않은 경우
        bool isMissingWaypoints = waypoints == null || waypoints.Length == 0; //웨이포인트 배열이 null이거나 길이가 0인 경우

        if (!isMissingWaypoints) //배열내 null 값이 있는지 검사
        {
            for(int i = 0; i < waypoints.Length; i++) //배열길이 만큼 반복
            {
                if (waypoints[i] == null)
                {
                    isMissingWaypoints = true; //웨이포인트 배열 내에 null 값이 있는 경우 true 후 탈출
                    break;
                }
            }
        }

        isNotFoundObject = isMissingPlayer || isMissingWaypoints; //플레이어 또는 웨이포인트가 할당되지 않은 경우

        if(isNotFoundObject)
        {
            if(isMissingPlayer)
            {
                Debug.LogError("[Enemy] Player가 할당되지 않음");
            }

            if(isMissingWaypoints)
            {
                Debug.LogError("[Enemy] Waypoint가 할당되지 않았거나 null을 포함하고 있음");
            }
        }
        else
        {
            Debug.Log("[Enemy] 유효성 검사 완료.");
        }

        return !isNotFoundObject; //유효한 경우 false를 ! 연산하여 true로 반환함
    }
}
