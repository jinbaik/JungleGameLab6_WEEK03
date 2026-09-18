using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField]
    protected Transform target;

    [SerializeField]
    // 접근 속도
    protected float speed = 0.01f;
    [SerializeField]
    // 최대 접근 거리
    protected float maxDistance = 2;


    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        Movement();
    }

    public void SetData(Transform target)
    {
        this.target = target;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnHit(other);
        }
    }


    protected abstract void Init();
    protected abstract void OnHit(Collider other);
    protected abstract void Movement();
    protected abstract void OnDamage();

    public virtual void OnAttackPlayer()
    {
        this.gameObject.SetActive(false);
    }
}
