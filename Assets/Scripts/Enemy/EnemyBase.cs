using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] protected Transform _target;

    [SerializeField] protected float _speed = 0.01f;
    [SerializeField] protected float _maxDistance = 2;
    [SerializeField] protected int _maxHP = 3;
    [SerializeField] protected int _currentHP = 0;


    private void OnEnable()
    {
        Init();
    }

    private void Update()
    {
        Movement();
    }

    public void SetData(Transform target)
    {
        this._target = target;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnHit(collision);
        }
    }


    protected abstract void Init();
    protected abstract void OnHit(Collision collision);
    protected abstract void Movement();
    protected abstract void OnDamage();

    public virtual void OnAttackPlayer()
    {
        this.gameObject.SetActive(false);
    }
}
