using UnityEngine;
using UnityEngine.AI;

public class Enemy_Basic : EnemyBase
{

    protected override void Movement()
    {
        if (Vector3.Distance(this.transform.position, target.position) >= maxDistance)
        {
            this.transform.Translate((target.position - this.transform.position) * Time.deltaTime * speed);
        }
    }

    protected override void OnDamage()
    {
        this.gameObject.SetActive(false);
    }

    protected override void Init()
    {

    }

    public void SetData(Transform target, float speed)
    {
        SetData(target);
        this.speed = speed;
    }


    protected override void OnHit(Collider other)
    {
        OnDamage();
    }
}

