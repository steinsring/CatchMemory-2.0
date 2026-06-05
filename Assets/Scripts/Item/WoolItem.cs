using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WoolItem : MonoBehaviour
{
    /*

    protected bool rightMove = false;
    protected bool leftMove = false;

    public int xForce;
    public int yForce;
    protected Vector2 woolVector;
    public float woolTorque;

    
    protected bool sameSpeedMove = false;
    public int force;
    */

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
        if(other.gameObject.name == "FoxCollider") {
            Destroy(this.gameObject);           //여우와 충돌하면 파괴
        }
        else
        {
            Physics2D.IgnoreCollision(other.collider, this.transform.gameObject.GetComponent<Collider2D>());
        }
        
        /*
        //땅에 닿으면 등속운동
        if(other.gameObject.tag == "Ground" && sameSpeedMove && charactorObj.GetComponent<SpriteRenderer>().flipX) {
            leftMove = true;
        }
        else if (other.gameObject.tag == "Ground" && sameSpeedMove && (charactorObj.GetComponent<SpriteRenderer>().flipX == false)) {
            rightMove = true;
        }
        */
            
    }

    protected void TimeLimit() {
        Destroy(this.gameObject, LimitTime);    //LimitTime후에 파괴
    }

    protected void playerUse() {
        if (isUse)
        {
            rigid.linearVelocity = new Vector2(speed * MoveDirection, 0);
        }
        TimeLimit();

        /*
        if (isUse)
        {
            //플레이어 보는 방향으로 발사
            charactorObj = GameObject.FindGameObjectWithTag("Player");
            if (charactorObj.GetComponent<SpriteRenderer>().flipX)
            {
                woolVector = new Vector2(-xForce, yForce);
            }
            else
            {
                woolVector = new Vector2(xForce, yForce);
                woolTorque *= -1;
            }

            rigid.AddForce(woolVector);          // 45도방향으로 힘 작용 - 포물선운동
            rigid.AddTorque(woolTorque);
            isUse = false;                       //한번만 작동
            sameSpeedMove = true;
            TimeLimit();
        }

        if (sameSpeedMove && rightMove)
        {
            rigid.AddForce(Vector2.right * force);
        }
        else if (sameSpeedMove && leftMove)
        {
            rigid.AddForce(Vector2.left * force);
        }
        */
    }
}
