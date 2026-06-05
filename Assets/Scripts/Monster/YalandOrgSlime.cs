using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class YalandOrgSlime : MonoBehaviour
{
    Rigidbody2D rigid;
    [SerializeField] public float speed;
    [SerializeField] private Animator SlimeAnimation;
    [SerializeField] private bool isActive = false;
    [SerializeField] private GameObject player;
    [SerializeField] private Vector2 playerPosition;
    [SerializeField] private float distance;
    [SerializeField] private float range = 5;
    [SerializeField] private int life = 3;
    SpriteRenderer sprite;
    [SerializeField] private AudioSource audioSource;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        SlimeAnimation = GetComponent<Animator>();
        sprite= GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
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
        if(range > distance)
            isActive= true;
        else
            isActive = false;

        if (isActive)
        {
            //������������ ����
            if (SlimeAnimation.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            {
                rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);
            }
            else//���������� ��� �������� ��
            {
                rigid.linearVelocity = new Vector2(speed * -1, rigid.linearVelocity.y - 1);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (playerPosition.y > 0.5 + transform.position.y)
            {
                StartCoroutine("Blink");
                life--;
                SlimeAnimation.SetTrigger("Hit");
            }

            if (life < 1)
            {
                audioSource.Play();
                isActive = false;
                transform.GetComponent<CircleCollider2D>().enabled = false;
                rigid.simulated = false;
                SlimeAnimation.SetBool("Dead", true);
                Destroy(this.gameObject, 3f);
            }
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
