using UnityEngine;
using UnityEngine.AI;

public class Enemy_Basic : EnemyBase
{

    protected override void Movement()
    {
        if (Vector3.Distance(this.transform.position, _target.position) >= _maxDistance)
        {
            this.transform.Translate((_target.position - this.transform.position) * Time.deltaTime * _speed);
        }
    }

    protected override void OnDamage()
    {
        _currentHP--;
        if(_currentHP <= 0)
        {
            this.gameObject.SetActive(false);
        }
    }

    protected override void Init()
    {
        _currentHP = _maxHP;
    }

    public void SetData(Transform target, float speed)
    {
        SetData(target);
        this._speed = speed;
    }


    protected override void OnHit(Collision collision)
    {
        OnDamage();
    }
}

