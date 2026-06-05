using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class HegdhogAi : MonoBehaviour
{
    Rigidbody2D rigid;
    [SerializeField] private float speed;
    [SerializeField] private int moveDirection = -1;
    SpriteRenderer sprite;
    [SerializeField] private Animator HegdhogAnimation;
    [SerializeField] private bool isActive = false;
    [SerializeField] private GameObject player;
    [SerializeField] private Vector2 playerPosition;
    [SerializeField] private float distance;
    [SerializeField] private float range = 5;


    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        HegdhogAnimation = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //ĳ���Ϳ� ���� ���� �Ÿ����, Ȱ������ ����
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
        playerPosition = player.transform.position;
        distance = Mathf.Abs(Vector2.Distance(playerPosition, transform.position));
        if (range > distance)
            isActive = true;
        else
            isActive = false;


        if (isActive)
        {
            //������������ ����
            if (HegdhogAnimation.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            {
                rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);
            }
            else
            {
                rigid.linearVelocity = new Vector2(speed * moveDirection, rigid.linearVelocity.y - 1);
            }

            //�Ʒ��� ������ ������ ��°� ������(=����������) �ݴ�������� �̵�
            Vector2 downVector = new Vector2(rigid.position.x + moveDirection * 0.3f, rigid.position.y);
            Debug.DrawRay(downVector, Vector2.down * 2, new Color(0, 1, 0));
            RaycastHit2D rayHitdown = Physics2D.Raycast(downVector, Vector2.down, 2, LayerMask.GetMask("Ground"));
            if (rayHitdown.collider == null)
                moveDirection *= -1;

            if (moveDirection == 1)
                sprite.flipX = true;
            else
                sprite.flipX = false;

            //���� ������ �ݴ�� �̵�
            Vector2 frontVector = new Vector2(rigid.position.x, rigid.position.y - 0.2f);
            Debug.DrawRay(frontVector, Vector2.right * moveDirection * 0.5f, new Color(1, 0, 0));
            RaycastHit2D rayHitfront = Physics2D.Raycast(frontVector, Vector2.right * moveDirection, 0.5f, LayerMask.GetMask("Ground"));
            if (rayHitfront.collider != null)
                moveDirection *= -1;

            if (moveDirection == 1)
                sprite.flipX = true;
            else
                sprite.flipX = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ball")
        {
            transform.GetComponent<Animator>().SetTrigger("Hit");
            StartCoroutine("Blink");
        }
    }

    IEnumerator Blink()
    {
        int countTime = 0;

        while(countTime < 10)
        {
            if (countTime % 2 == 0)
                sprite.color = new Color32(255, 255, 255, 90);
            else
                sprite.color = new Color32(255, 255, 255, 180);

            yield return new WaitForSeconds(0.2f);

            countTime++;
        }

        sprite.color = new Color32(255, 255, 255, 255);

        yield return null;
    }
}
