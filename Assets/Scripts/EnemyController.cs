//�� ��ũ��Ʈ�� Enemy(Capsule) ������Ʈ�� FSM(Finite State Machine) ������� �����ϴ� ����� �����մϴ�.

using System.Linq.Expressions;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private enum EnemyState //���� ���¸� �����ϴ� ������
    {
        Patrol,     //���� ����
        Chase,      //���� ����
        Attack,     //���� ����
    }

    [SerializeField] private Transform player;          //�÷��̾� ������Ʈ�� Transform(��ġ)�� ����
    [SerializeField] private Transform[] waypoints;     //���� ��η� ����� ��������Ʈ �迭

    //�̵� ���� ����
    float fMoveSpeed = 3.0f;    //���� �̵� �ӵ�

    //���� ���� ����
    float fChaseRange = 5.0f;       //�÷��̾ ������ ����
    float fAttackRange = 1.5f;      //�÷��̾ ������ ����
    float fChaseTimeBuffer = 1.2f;  //�÷��̾ ��ģ �� ������ �ߴ��ϱ������ �ð�

    //���� ���� ����
    float fAttackCooldown = 0.8f; //���� ��ٿ� �ð�

    //���� �� ���� �� ����
    EnemyState state = EnemyState.Patrol;   //�⺻ ���¸� ������ ���� �� �ʱ�ȭ
    float fNextAttackTime = 0.0f;           //���� ���� ���� �ð��� �����ϴ� ����
    float fDistToPlayer = 0.0f;             //�÷��̾���� �Ÿ��� �����ϴ� ����
    int nWaypointIndex = 0;                 //���� ���� ���� ��������Ʈ�� �ε���
    

    //���� ���� : ������Ʈ�� ��ȿ���� ���� ��츦 ó���ϱ� ���� ����
    bool isNotFoundObject = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        f_VaildateObjects(); //�÷��̾�� ��������Ʈ�� ��ȿ���� Ȯ��(���� ����)
    }

    // Update is called once per frame
    void Update()
    {
        if (isNotFoundObject) return; //�÷��̾� �Ǵ� ��������Ʈ�� ��ȿ���� ���� ��� ������Ʈ�� �ߴ�

        fDistToPlayer = f_UpdateDistanceToPlayer(); //�÷��̾���� �Ÿ��� ����

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
                Debug.LogError("[Enemy] Unknown State!"); //�� �� ���� ���¿� ���� ���� �޽��� ���
                break;
        }
    }

    //�÷��̾���� �Ÿ��� ����Ͽ� ��ȯ�ϴ� �޼ҵ�
    float f_UpdateDistanceToPlayer()
    {
        if(player == null) { return Mathf.Infinity; } //�÷��̾ �Ҵ���� ���� ��� ���Ѵ� �Ÿ� ��ȯ

        return Vector3.Distance(transform.position, player.position); //���� �÷��̾� ������ �Ÿ� ���
    }

    //��ǥ ��ġ�� �̵��ϴ� �޼ҵ�
    //���������� ���̴� ������� �ʰ� ���� �̵����� ������ ������ �̵� �Լ��� �����մϴ�.
    void f_MoveTarget(Vector3 targetPosition)
    {
        Vector3 vTargetPos = new Vector3(targetPosition.x, transform.position.y, targetPosition.z); //���� �� y�� ���� ��ġ�� �����ϰ� ����

        transform.position = Vector3.MoveTowards(transform.position, vTargetPos, fMoveSpeed * Time.deltaTime); //��ǥ ��ġ�� �̵�
    }

    //���� �����ϴ� ���·� ��ȯ
    void f_UpdatePatrol(float fDistance)
    {
        if (fDistance <= fChaseRange)
        {
            state = EnemyState.Chase; //�÷��̾ ���� ������ ������ ���¸� �������� ����
            return;
        }

        Transform target = waypoints[nWaypointIndex]; //��ǥ ���� ��������Ʈ�� ������

        f_MoveTarget(target.position); //��ǥ ��������Ʈ�� �̵�

        if (Vector3.Distance(transform.position, target.position) < 1.0f)
        {
            nWaypointIndex = (nWaypointIndex + 1) % waypoints.Length; //���� ��������Ʈ�� �����ϸ� ���� ��������Ʈ�� �̵�(0 �� 1 �� 2 �� 0 ...)
        }
    }


    //���� �÷��̾ �����ϴ� ���·� ��ȯ
    void f_UpdateChase(float fDistance)
    {
        if (fDistance <= fAttackRange) //�÷��̾ ���� ������ ������ ���¸� �������� ����
        {
            state = EnemyState.Attack;
            return;
        }

        if (fDistance > fChaseRange * fChaseTimeBuffer) //�÷��̾ ���� ������ ����� ���¸� ������ ������
        {
            state = EnemyState.Patrol;
            return;
        }

        f_MoveTarget(player.position);
    }

    //���� �÷��̾ �����ϴ� ���·� ��ȯ
    void f_UpdateAttack(float fDistance)
    {
        if (fDistance > fAttackRange) //�÷��̾ ���� ������ ����� ���¸� �������� ����
        {
            state = EnemyState.Chase;
            return;
        }

        if (Time.time >= fNextAttackTime) //���� �ð��� ���� ���� ���� �ð����� ũ�ų� ������ ����
        {
            fNextAttackTime = Time.time + fAttackCooldown;
            Debug.Log("[Enemy] Attack!");
        }
    }

    /// <summary> ���� ������Ʈ�� ��ȿ���� �˻��ϴ� �޼ҵ�(���������� ���û���) </summary>
    /// <returns> ��ȿ�� ��� true, �׷��� ���� ��� false </returns>
    bool f_VaildateObjects()
    {
        bool isMissingPlayer = player == null; //�÷��̾ �Ҵ���� ���� ���
        bool isMissingWaypoints = waypoints == null || waypoints.Length == 0; //��������Ʈ �迭�� null�̰ų� ���̰� 0�� ���

        if (!isMissingWaypoints) //�迭�� null ���� �ִ��� �˻�
        {
            for(int i = 0; i < waypoints.Length; i++) //�迭���� ��ŭ �ݺ�
            {
                if (waypoints[i] == null)
                {
                    isMissingWaypoints = true; //��������Ʈ �迭 ���� null ���� �ִ� ��� true �� Ż��
                    break;
                }
            }
        }

        isNotFoundObject = isMissingPlayer || isMissingWaypoints; //�÷��̾� �Ǵ� ��������Ʈ�� �Ҵ���� ���� ���

        if(isNotFoundObject)
        {
            if(isMissingPlayer)
            {
                Debug.LogError("[Enemy] Player�� �Ҵ���� ����");
            }

            if(isMissingWaypoints)
            {
                Debug.LogError("[Enemy] Waypoint�� �Ҵ���� �ʾҰų� null�� �����ϰ� ����");
            }
        }
        else
        {
            Debug.Log("[Enemy] ��ȿ�� �˻� �Ϸ�.");
        }

        return !isNotFoundObject; //��ȿ�� ��� false�� ! �����Ͽ� true�� ��ȯ��
    }
}
