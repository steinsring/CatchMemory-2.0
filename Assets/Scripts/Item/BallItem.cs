using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BallItem : MonoBehaviour
{

    [SerializeField] protected Rigidbody2D rigid;
    [SerializeField] private int speed;
    [SerializeField] private int MoveDirection = -1;
    [SerializeField] public bool isUse;
    [SerializeField] public float LimitTime = 3f;
    [SerializeField] protected GameObject charactorObj;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        isUse = true;
        charactorObj = GameObject.FindGameObjectWithTag("Player");
        if (charactorObj.GetComponent<SpriteRenderer>().flipX)
        {
            MoveDirection = -1;
        }
        else
        {
            MoveDirection = 1;
        }
        transform.position = new Vector2(charactorObj.transform.position.x + MoveDirection, charactorObj.transform.position.y);
    }


    void FixedUpdate()
    {
        playerUse();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Monster" || other.gameObject.tag == "Hedgehog")
        {
            Destroy(this.gameObject);           //������ġ�� �浹�ϸ� �ı�
        }
        else
        {
            Physics2D.IgnoreCollision(other.collider, transform.gameObject.GetComponent<Collider2D>());
        }
    }

    protected void TimeLimit()
    {
        Destroy(this.gameObject, LimitTime);    //LimitTime�Ŀ� �ı�
    }

    protected void playerUse()
    {
        if (isUse)
        {
            rigid.linearVelocity = new Vector2(speed * MoveDirection, 0);
        }
        TimeLimit();
    }
}