using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxAi : MonoBehaviour
{
    [SerializeField] public float wakeRange = 10;
    [SerializeField] public float traceRange = 20;
    [SerializeField] public float range;
    [SerializeField] public float distance;
    [SerializeField] Rigidbody2D rigid;
    [SerializeField] private float speed = 3;
    private int moveDirection = -1;
    public bool isActive = false;
    [SerializeField] public int life = 2;
    [SerializeField] public int yarnLife = 1;
    [SerializeField] private Animator FoxAnimation;
    SpriteRenderer sprite;
    [SerializeField] private GameObject player;
    [SerializeField] private Vector2 playerPosition;
    [SerializeField] private FoxDetectPlayer foxDetect;
    [SerializeField] private AudioSource audioSource;
    // Start is called before the first frame update

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sprite= GetComponent<SpriteRenderer>();
        FoxAnimation = GetComponent<Animator>();
        foxDetect = transform.parent.GetComponent<FoxDetectPlayer>();
        range = wakeRange;
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
        if (range > distance && foxDetect.playerCollider != null)
            isActive = true;
        else
            isActive = false;

        //플레이어를 감지했을때 활성화
        if (isActive) 
        {
            //transform.GetComponent<CircleCollider2D>().enabled = true;
            //rigid.simulated = true;
            range = traceRange;
            FoxAnimation.SetBool("Sleep", false);
            FoxAnimation.SetBool("Wakeup", true);        

            //방향전환
            if (transform.position.x > playerPosition.x)
            { 
                moveDirection = -1;
                sprite.flipX = true;
            }
            else {
                moveDirection = 1;
                sprite.flipX = false;
            }

            if (FoxAnimation.GetCurrentAnimatorStateInfo(0).IsName("FoxIdle"))
            {
                rigid.linearVelocity = new Vector2(speed * moveDirection, rigid.linearVelocity.y - 1);
            }
            else
            {
                rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);
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
        }
        else
        {
            range = wakeRange;
            FoxAnimation.SetBool("Wakeup", false);
            FoxAnimation.SetBool("Sleep", true);
            //transform.GetComponent<CircleCollider2D>().enabled = false;
            //rigid.simulated = false;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //털실피격
        if (collision.gameObject.tag == "Wool")
        {
            StartCoroutine("Blink");
            yarnLife--;
            rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);
            FoxAnimation.SetTrigger("YarnHit");
        } 
        
        //플레이어가 위에서 떨어지면 피격
        if (collision.gameObject.tag == "Player")
        {

            if (collision.transform.position.y > transform.position.y + 0.5f)
            {
                StartCoroutine("Blink");
                life--;
                FoxAnimation.SetTrigger("Hit");
                rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);
            }
        }

        if (collision.gameObject.tag == "Ball")
        {
            life--;
            FoxAnimation.SetTrigger("Hit");
            StartCoroutine("Blink");
        }

        if (yarnLife <= 0 || life <= 0)
        {
            rigid.simulated = false;
            transform.GetComponent<CircleCollider2D>().enabled = false;
            audioSource.Play();
            FoxAnimation.SetBool("Dead", true);
            Destroy(transform.parent.gameObject, 1.5f);
        }
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
