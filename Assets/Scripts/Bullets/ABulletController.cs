using System;
using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;

public abstract class ABulletController : MonoBehaviour
{
    public int m_bouncesLeft = 0;
    public float LifeTime { get => m_lifetime; }
    public float m_lifetime = 0; // Life Time counter
    protected Vector2 lastCollisionNormal = Vector2.zero;
    protected Rigidbody2D m_rb;
    public float selfCollisionTreshold = 0.1f;
    public Action<ABulletController> OnSpawn = (ABulletController) => { }
                                    , OnFly = (ABulletController) => { }
                                    , OnHit = (ABulletController) => { }
                                    , OnBounce = (ABulletController) => { }
                                    , OnDetonate = (ABulletController) => { };
    [SerializeField] protected bool PREDICT_DESTRUCTION = true;

    protected virtual void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void OnDisable()
    {
        ResetBulletData();
    }

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), m_rb.velocity.normalized);
    }

    protected abstract void MoveBullet(float deltaTime);

    public abstract void Detonate(bool lifeTimeOver = false);

    protected abstract void OnCollisionEnter2D(Collision2D collision);

    protected void ResetBulletData()
    {
        gameObject.layer = 8;
        m_bouncesLeft = int.MaxValue; // int.MaxValue PARA EVITAR Detonate prematuro en colisiones previas a la inicializacion y simulacion de la bala.
        m_lifetime = 0;
        OnSpawn = (ABulletController) => { };
        OnFly = (ABulletController) => { };
        OnHit = (ABulletController) => { };
        OnBounce = (ABulletController) => { };
        OnDetonate = (ABulletController) => { };
    }
}
