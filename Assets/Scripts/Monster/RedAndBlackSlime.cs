using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class RedAndBlackSlime : MonoBehaviour
{
    Rigidbody2D rigid;
    [SerializeField] private float speed;
    [SerializeField] private int moveDirection = -1;
    SpriteRenderer sprite;
    [SerializeField] private Animator SlimeAnimation;
    [SerializeField] private bool isActive = false;
    [SerializeField] private GameObject player;
    [SerializeField] private Vector2 playerPosition;
    [SerializeField] private float distance;
    [SerializeField] private float range = 5;
    [SerializeField] private int life = 3;
    [SerializeField] private AudioSource audioSource;


    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        SlimeAnimation = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //캐릭터와 몬스터 사이 거리계산, 활동할지 결정
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
            //데미지받으면 경직
            if (SlimeAnimation.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            {
                rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);
            }
            else
            {
                rigid.linearVelocity = new Vector2(speed * moveDirection, rigid.linearVelocity.y - 1);
            }




            //아래로 나가는 광선에 닿는게 없으면(=낭떠러지면) 반대방향으로 이동
            Vector2 downVector = new Vector2(rigid.position.x + moveDirection * 0.3f, rigid.position.y);
            Debug.DrawRay(downVector, Vector2.down * 2, new Color(0, 1, 0));
            RaycastHit2D rayHitdown = Physics2D.Raycast(downVector, Vector2.down, 2, LayerMask.GetMask("Ground"));
            if (rayHitdown.collider == null)
                moveDirection *= -1;

            if (moveDirection == 1)
                sprite.flipX = true;
            else
                sprite.flipX = false;

            //벽이 있으면 반대로 이동
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

    
    //캐릭터와 충돌했을때 데미지 여부 검사
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            if(playerPosition.y > 0.5 + transform.position.y)
            {
                life--;
                StartCoroutine("Blink");
                if (life < 1)
                {
                    audioSource.Play();
                    isActive = false;
                    transform.GetComponent<CircleCollider2D>().enabled = false;
                    rigid.simulated = false;
                    SlimeAnimation.SetBool("Dead", true);
                    Destroy(this.gameObject, 3f);
                }
                
                SlimeAnimation.SetTrigger("Hit");
            }

            
        }

        if (collision.gameObject.tag == "Ball")
        {
            life--;
            StartCoroutine("Blink");
            if (life < 1)
            {
                audioSource.Play();
                isActive = false;
                transform.GetComponent<CircleCollider2D>().enabled = false;
                rigid.simulated = false;
                SlimeAnimation.SetBool("Dead", true);
                Destroy(this.gameObject, 3f);
            }

            SlimeAnimation.SetTrigger("Hit");
        }
    }

    public void RayHit()
    {
        life = 0;
        audioSource.Play();
        isActive = false;
        SlimeAnimation.SetTrigger("Hit");
        transform.GetComponent<CircleCollider2D>().enabled = false;
        rigid.simulated = false;
        SlimeAnimation.SetBool("Dead", true);
        Destroy(this.gameObject, 3f);

    }

    IEnumerator Blink()
    {
        int countTime = 0;

        while (countTime < 10)
        {
            if (countTime % 2 == 0)
                sprite.color = new Color32(255, 255, 255, 90);
            else
                sprite.color = new Color32(255, 255, 255, 180);

            yield return new WaitForSeconds(0.1f);

            countTime++;
        }

        sprite.color = new Color32(255, 255, 255, 255);

        yield return null;
    }
}
