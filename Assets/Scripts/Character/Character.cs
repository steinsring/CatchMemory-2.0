using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] float maxMoveSpeed = 5; //�����̴� �ӵ�
    [SerializeField] float acceleration = 8;//�������� ���ӵ�
    [SerializeField] float jumpPower = 5; //���� �Ŀ�
    [SerializeField] int jumpCount = 2; //���� ���� Ƚ��
    [SerializeField] float Gravity = 10; //�������� �ӵ�
    bool OnGround = true;

    float currentSpeed; //현재 속도 저장
    public static Vector2 contactPoint;

    [SerializeField] float pushForce = 7; //���Ϳ� �浹 �� �з����� ����
    [SerializeField] float pushJump = 4; //���Ϳ� �浹 �� Ƣ������� ����
    [SerializeField] float hitDelay = 1.5f; //�ǰݽ� �Է� �Ұ����� �ð�
    [SerializeField] float stunTime = 1.0f; //���� ���� �ð�
    [SerializeField] float blinkTime = 0.1f; //������ ���� �ð�
    [SerializeField] float itemUseDelay = 1.0f; //������ ���� �ð�


    private State currentState; //���� state����
    private State priviosState;

    private Character player;
    private PlayerControl playerUI;
    private RaycastHit2D hit;
    private float slopeAngle = 30f; // 경사각 설정


    private Inventory theInventory;
    private ItemList theitemlist;
    public Item nowitem; // 부딪힌 아이템

    public int currentStage; //현재 스테이지
    public Transform newSpawnPoint;

    void Start()
    {
        if (Camera.main.gameObject.GetComponent<CharacterCamera>() == null)
        {
            Camera.main.gameObject.AddComponent<CharacterCamera>();
        }
        playerUI = GameMNG.UI.ShowSceneUI<PlayerControl>("PlayerControl"); //UI���� ĳ���� �����ϴ°� ���Ƽ� ������ �ʿ��� �κ��� ���⼭ �������ָ� �ɵ�
        theInventory = playerUI.GetComponentInChildren<Inventory>();
        theitemlist = playerUI.GetComponentInChildren<ItemList>();
        newSpawnPoint = GameObject.FindWithTag("Respawn").transform;
        player = this;

        currentState = new Idle(player, playerUI);
        currentState.Enter();
        //GameObject.FindWithTag("BackGround").GetComponentInChildren<ParallaxBackground>().Init();
        //GameObject.FindWithTag("BackGround").GetComponentInChildren<ParallaxBackground_Cloud>().Init();

    }

    void Update()
    {
        hit = Physics2D.Raycast(player.transform.position, Vector2.down, 2.0f, ~LayerMask.GetMask("Character", "Bound", "UI"));

        if (hit.collider != null && hit.distance <= 1.8f && player.GetComponent<Rigidbody2D>().linearVelocity.y < 0) //가운데 ray가 땅에 닿기 전
        {
            player.GetComponent<Animator>().SetBool("OnGround", true);
        }
        if (hit.collider != null && hit.distance <= 0.9f && player.GetComponent<Rigidbody2D>().linearVelocity.y <= 0)
        {
            player.OnGround = true;
            newSpawnPoint.position = player.transform.position;
        }
        currentState.Update();
    }
    private void ChangeState(State newstate) //���º���
    {
        if (currentState != null)
        {
            currentState.Exit();
        }
        priviosState = currentState;
        currentState = newstate;
        currentState.Enter();
    }



    #region Class들 //묶어~

    public class Idle : State //idle ����
    {
        private Character player; //character ���� player -> character ������Ʈ�� gameobject�� ���õ� ��� �Ϳ� ���ٰ���
        private PlayerControl playerUI;
        public Idle(Character character, PlayerControl characterUI) //���� ȣ�� -> ���¸� ������ player����
        {
            player = character; //���������� player�����ϰ� �� player�� �ڵ� �������
            playerUI = characterUI;
        }
        public void Enter() //���°� �ٲ�� �ٷ� �ҷ����� 1�� ����Ǵ� �Լ�
        {
            player.GetComponent<Animator>().SetBool("IsMoving", false);
            //player.GetComponent<Animator>().Play("Idle");
        }
        public void Update() //���°� �ٲ�� main Update������ ����Ǵ� �Լ�
        {
            if (playerUI.isMovingLeft || playerUI.isMovingRight || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                player.ChangeState(new Move(player, playerUI));
            }
            if (playerUI.isJumpTriggerd || Input.GetKeyDown(KeyCode.Space))
            {
                player.ChangeState(new Jump(player, playerUI));
            }
            if (playerUI.isItemUseTriggerd)
            {
                player.ChangeState(new UseItem(player, playerUI));
            }
        }
        public void Exit() //���°� �ٲ� ���������� 1�� ����Ǵ� �Լ� //�� ������ ��� ������ ���¸� �ϰ� ���� ->State���� �������̽�ȭ �ص�
        {

        }
    }
    public class Move : State
    {
        private Character player;
        private PlayerControl playerUI;
        private float _moveSpeed;
        private RaycastHit2D hithead;
        private RaycastHit2D hittail;
        private Quaternion slopeRotation;
        private float maxDistanceDifference = 0.2f;
        private bool isRotated = false;

        public Move(Character character, PlayerControl characterUI)
        {
            player = character;
            playerUI = characterUI;
        }
        public void Enter()
        {
            _moveSpeed = player.currentSpeed;
            player.GetComponent<Animator>().SetBool("IsMoving", true);
            Quaternion slopeRotation = Quaternion.identity;
        }
        public void Update()
        {
            _moveSpeed = Mathf.Clamp(_moveSpeed, -player.maxMoveSpeed, player.maxMoveSpeed);
            if (Input.anyKey == false) //감속
            {
                if (_moveSpeed > 0)
                {
                    _moveSpeed += -player.acceleration * Time.deltaTime; // 양수일 경우 음수 가속도 적용
                    player.transform.Translate(_moveSpeed * Time.deltaTime, 0, 0);
                }
                else if (_moveSpeed < 0)
                {
                    _moveSpeed += player.acceleration * Time.deltaTime; // 음수일 경우 양수 가속도 적용
                    player.transform.Translate(_moveSpeed * Time.deltaTime, 0, 0);
                }
            }
            if (Mathf.Abs(_moveSpeed) < 0.01f && Input.anyKey == false)//idle
            {
                player.ChangeState(new Idle(player, playerUI));
            }
            if (playerUI.isMovingLeft || Input.GetKey(KeyCode.A))
            {
                hithead = Physics2D.Raycast(player.transform.position + Vector3.left * 0.8f, Vector2.down, 1.4f, ~LayerMask.GetMask("Character", "Bound"));
                hittail = Physics2D.Raycast(player.transform.position + Vector3.right * 0.8f, Vector2.down, 1.4f, ~LayerMask.GetMask("Character", "Bound"));
                Debug.DrawRay(player.transform.position + Vector3.left * 0.8f, Vector2.down, Color.red, 2.0f);
                Debug.DrawRay(player.transform.position + Vector3.right * 0.8f, Vector2.down, Color.red, 2.0f);

                float distanceDifference = Mathf.Abs(hithead.distance - hittail.distance);

                if (distanceDifference > maxDistanceDifference && !isRotated && hittail.collider != null && hithead.collider != null)
                {
                    if (hithead.distance > hittail.distance)
                    {
                        slopeRotation = Quaternion.Euler(0, 0, player.slopeAngle);
                    }
                    else if (hithead.distance < hittail.distance)
                    {
                        slopeRotation = Quaternion.Euler(0, 0, -player.slopeAngle);
                    }

                    isRotated = true;
                }
                else if (distanceDifference <= maxDistanceDifference && isRotated)
                {
                    // 회전 초기화
                    slopeRotation = Quaternion.identity;
                    isRotated = false;
                }
                player.transform.rotation = slopeRotation;

                player.GetComponent<SpriteRenderer>().flipX = true;
                Vector2 capcol = player.GetComponent<CapsuleCollider2D>().offset;
                capcol.x = -Mathf.Abs(capcol.x);
                player.GetComponent<CapsuleCollider2D>().offset = capcol;
                _moveSpeed += -player.acceleration * Time.deltaTime; //���ӵ��� �ӵ�����
                player.transform.Translate(_moveSpeed * Time.deltaTime, 0, 0);
            }
            if (playerUI.isMovingRight || Input.GetKey(KeyCode.D))
            {
                hittail = Physics2D.Raycast(player.transform.position + Vector3.left * 0.8f, Vector2.down, 2.0f, ~LayerMask.GetMask("Character", "Bound"));
                hithead = Physics2D.Raycast(player.transform.position + Vector3.right * 0.8f, Vector2.down, 2.0f, ~LayerMask.GetMask("Character", "Bound"));

                Debug.DrawRay(player.transform.position + Vector3.left * 0.8f, Vector2.down, Color.red, 2.0f);
                Debug.DrawRay(player.transform.position + Vector3.right * 0.8f, Vector2.down, Color.red, 2.0f);

                float distanceDifference = Mathf.Abs(hithead.distance - hittail.distance);

                if (distanceDifference > maxDistanceDifference && !isRotated && player.OnGround && hittail.collider != null && hithead.collider != null)
                {
                    if (hithead.distance > hittail.distance)
                    {
                        slopeRotation = Quaternion.Euler(0, 0, -player.slopeAngle);
                    }
                    else if (hithead.distance < hittail.distance)
                    {
                        slopeRotation = Quaternion.Euler(0, 0, player.slopeAngle);
                    }

                    isRotated = true;
                }
                else if (distanceDifference <= maxDistanceDifference && isRotated && player.OnGround)
                {
                    // 회전 초기화
                    slopeRotation = Quaternion.identity;
                    isRotated = false;
                }
                player.transform.rotation = slopeRotation;

                player.GetComponent<SpriteRenderer>().flipX = false;
                Vector2 capcol = player.GetComponent<CapsuleCollider2D>().offset;
                capcol.x = Mathf.Abs(capcol.x);
                player.GetComponent<CapsuleCollider2D>().offset = capcol;
                _moveSpeed += player.acceleration * Time.deltaTime;
                player.transform.Translate(_moveSpeed * Time.deltaTime, 0, 0);
            }
            if ((playerUI.isJumpTriggerd || Input.GetKeyDown(KeyCode.Space)) && player.OnGround)
            {
                player.ChangeState(new Jump(player, playerUI));
            }
            if (playerUI.isItemUseTriggerd)
            {
                player.ChangeState(new UseItem(player, playerUI));
            }
        }
        public void Exit()
        {
            player.currentSpeed = _moveSpeed;
            player.transform.rotation = slopeRotation;
        }
    }
    public class Jump : State
    {
        private Character player;
        private PlayerControl playerUI;
        private float _moveSpeed;
        private int _jumpCount;//���� �̷��� ���� ������ �ʿ� ���µ� Ȥ�ó� Character class ���� ������ �Ǹ� �ʿ���
        private float _jumpPower;

        public Jump(Character character, PlayerControl characterUI)
        {
            player = character;
            playerUI = characterUI;
            _jumpCount = player.jumpCount;
            _jumpPower = player.jumpPower;
        }
        public void Enter()
        {
            player.transform.rotation = Quaternion.identity;
            GameMNG.Sound.Play("Effect/09_Jump_4");
            playerUI.isJumpTriggerd = false;
            player.GetComponent<Animator>().SetTrigger("IsJumping");
            player.GetComponent<Animator>().SetBool("OnGround", false);
            player.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(_moveSpeed, _jumpPower);
            player.OnGround = false;
            _jumpCount--;
        }

        public void Update()
        {
            _moveSpeed = Mathf.Clamp(_moveSpeed, -player.maxMoveSpeed, player.maxMoveSpeed);
            if (playerUI.isMovingLeft || Input.GetKey(KeyCode.A))
            {
                player.GetComponent<SpriteRenderer>().flipX = true;
                Vector2 capcol = player.GetComponent<CapsuleCollider2D>().offset;
                capcol.x = -Mathf.Abs(capcol.x);
                player.GetComponent<CapsuleCollider2D>().offset = capcol;
                _moveSpeed += -player.acceleration * Time.deltaTime;
                player.transform.Translate(_moveSpeed * Time.deltaTime, 0, 0);
            }
            if (playerUI.isMovingRight || Input.GetKey(KeyCode.D))
            {
                player.GetComponent<SpriteRenderer>().flipX = false;
                Vector2 capcol = player.GetComponent<CapsuleCollider2D>().offset;
                capcol.x = Mathf.Abs(capcol.x);
                player.GetComponent<CapsuleCollider2D>().offset = capcol;
                _moveSpeed += player.acceleration * Time.deltaTime;
                player.transform.Translate(_moveSpeed * Time.deltaTime, 0, 0);
            }
            if ((playerUI.isJumpTriggerd || Input.GetKeyDown(KeyCode.Space)) && _jumpCount > 0)
            {
                GameMNG.Sound.Play("Effect/09_Jump_4");
                player.currentSpeed = player.GetComponent<Rigidbody2D>().linearVelocity.x;
                playerUI.isJumpTriggerd = false;
                player.GetComponent<Animator>().Play("Jump", 0, 0);
                player.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, _jumpPower);
                _jumpCount--;
            }
            if (_jumpCount == 0)
            {
                playerUI.isJumpTriggerd = false;
            }
            if (player.OnGround)
            {
                player.ChangeState(new Move(player, playerUI));
            }
            if (playerUI.isItemUseTriggerd)
            {
                player.ChangeState(new UseItem(player, playerUI));
            }
        }
        public void Exit()
        {

        }
    }
    public class UseItem : State
    {
        private Character player;
        private PlayerControl playerUI;
        private float _itemUseDelay;
        public UseItem(Character character, PlayerControl characterUI)
        {
            player = character;
            playerUI = characterUI;
            _itemUseDelay = player.itemUseDelay;
        }
        public void Enter()
        {
            if (new Jump(player, playerUI) == player.priviosState) //�����ѵ� �������� ������� ��� �ִϸ��̼�+Ű�Է����� ����
            {
                return;
            }
            player.IgnoreInput(_itemUseDelay, player, playerUI); //����� �����ۿ� ���� ������ �ް� if���� ���� �ִϸ��̼ǰ� �ִ°Ͱ� ���°� ���� �ʿ�
            playerUI.isItemUseTriggerd = false;

            if (playerUI.ItemName == "Yarn")
            {
                GameMNG.Resource.MyInstantiate("Items/UsingYarn"); //털실소환
                Debug.Log("UseYarn");
                
            }
            if (playerUI.ItemName == "Mirror")
            {
                player.GetComponent<Raycast>().isActive= true;
                Debug.Log("UseMirror");
            }
            if (playerUI.ItemName == "Ball")
            {
                GameMNG.Resource.MyInstantiate("Items/UsingBall");
                Debug.Log("UseBall");
            }

            playerUI.ShowItem();

        }
        public void Update()
        {

        }
        public void Exit()
        {

        }
    }
    public class Die : State
    {
        private Character player;
        private PlayerControl playerUI;
        private GameObject gameoverUI;
        public Die(Character character, PlayerControl characterUI)
        {
            player = character;
            playerUI = characterUI;
        }
        public void Enter()
        {
            GameMNG.Sound.Play("Effect/08_GameOver_02 1");
            Quaternion slopeRotation = Quaternion.identity;
            player.GetComponent<Animator>().CrossFade("Death", 0);
            player.StartCoroutine(player.Fade());
            player.gameObject.layer = LayerMask.NameToLayer("Immune");
        }
        public void Update()
        {

        }
        public void Exit()
        {
            player.GetComponent<Animator>().CrossFade("Idle", 0);
        }

    }



    //�÷��̾� ��ȣ�ۿ� ����
    public class GetHit : State
    {
        private Character player;
        private PlayerControl playerUI;
        private float _hitDelay;
        private float _pushForce;
        private float _pushJump;
        private float _blinkTime;
        public GetHit(Character character, PlayerControl characterUI)
        {
            player = character;
            playerUI = characterUI;
            _hitDelay = player.hitDelay;
            _pushForce = player.pushForce;
            _pushJump = player.pushJump;
            _blinkTime = player.blinkTime;
        }
        public void Enter()
        {
            GameMNG.Sound.Play("Effect/13_Bump_01");
            Vector2 direction = (Vector2)player.transform.position - contactPoint;
            direction.Normalize();
            Vector2 pushForce = new Vector2(direction.x * _pushForce, _pushJump);
            player.GetComponent<Rigidbody2D>().AddForce(pushForce, ForceMode2D.Impulse);
            player.GetComponent<Animator>().CrossFade("Hit", 0);
            player.GetComponent<LIfe>().LifeOut();
            playerUI.LifeChange();
            if (player.GetComponent<LIfe>().GetLife() == 0)
            {
                player.ChangeState(new Die(player, playerUI));
            }
            else
            {
                player.SetImmune(_hitDelay, _blinkTime);
                player.IgnoreInput(_hitDelay-1, player, playerUI);
            }
        }
        public void Update()
        {

        }
        public void Exit()
        {

        }
    }
    public class GetStun : State
    {
        private Character player;
        private PlayerControl playerUI;
        private float _hitDelay;
        private float _pushForce;
        private float _pushJump;
        private float _stunDelay;
        private float _blinkTime;
        public GetStun(Character character, PlayerControl characterUI)
        {
            player = character;
            playerUI = characterUI;
            _hitDelay = player.hitDelay;
            _pushForce = player.pushForce;
            _pushJump = player.pushJump;
            _stunDelay = player.stunTime;
            _blinkTime = player.blinkTime;
        }
        public void Enter()
        {
            GameMNG.Sound.Play("Effect/13_Bump_01");
            player.GetComponent<Animator>().CrossFade("Stun", 0);
            Vector2 direction = (Vector2)player.transform.position - contactPoint;
            direction.Normalize();
            Vector2 pushForce = new Vector2(direction.x * _pushForce, _pushJump);
            player.GetComponent<Rigidbody2D>().AddForce(pushForce, ForceMode2D.Impulse);
            player.GetComponent<LIfe>().LifeOut();
            playerUI.LifeChange();
            if (player.GetComponent<LIfe>().GetLife() == 0)
            {
                player.ChangeState(new Die(player, playerUI));
            }
            else
            {
                player.SetImmune(_hitDelay + _stunDelay, _blinkTime);
                player.IgnoreInput(_hitDelay + _stunDelay -1, player, playerUI);
            }
        }
        public void Update()
        {

        }
        public void Exit()
        {

        }
    }
    public class MonsterKill : State
    {
        private Character player;
        private PlayerControl playerUI;
        private float _moveSpeed;
        private float _jumpPower;
        public MonsterKill(Character character, PlayerControl characterUI)
        {
            player = character;
            playerUI = characterUI;
            _jumpPower = player.jumpPower;
        }
        public void Enter()
        {
            GameMNG.Sound.Play("Effect/12_1_AttackMonster_02");
            player.GetComponent<Animator>().CrossFade("JumpKill", 0);
            player.GetComponent<Animator>().SetBool("OnGround", false);
            player.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, _jumpPower);
            player.OnGround = false;
        }
        public void Update()
        {
            if (playerUI.isMovingLeft || Input.GetKey(KeyCode.A))
            {
                player.GetComponent<SpriteRenderer>().flipX = true;
                Vector2 capcol = player.GetComponent<CapsuleCollider2D>().offset;
                capcol.x = -Mathf.Abs(capcol.x);
                player.GetComponent<CapsuleCollider2D>().offset = capcol;
                _moveSpeed += -player.acceleration * Time.deltaTime;
                player.transform.Translate(_moveSpeed * Time.deltaTime, 0, 0);
                _moveSpeed = Mathf.Clamp(_moveSpeed, -player.maxMoveSpeed, player.maxMoveSpeed);
            }
            if (playerUI.isMovingRight || Input.GetKey(KeyCode.D))
            {
                player.GetComponent<SpriteRenderer>().flipX = false;
                Vector2 capcol = player.GetComponent<CapsuleCollider2D>().offset;
                capcol.x = Mathf.Abs(capcol.x);
                player.GetComponent<CapsuleCollider2D>().offset = capcol;
                _moveSpeed += player.acceleration * Time.deltaTime;
                player.transform.Translate(_moveSpeed * Time.deltaTime, 0, 0);
                _moveSpeed = Mathf.Clamp(_moveSpeed, -player.maxMoveSpeed, player.maxMoveSpeed);
            }
            if (player.OnGround)
            {
                if (playerUI.isMovingLeft || playerUI.isMovingRight || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                {
                    player.ChangeState(new Move(player, playerUI));
                }
                else player.ChangeState(new Idle(player, playerUI));
            }
        }
        public void Exit()
        {

        }
    }
    public class GetFirstItem : State
    {
        private Character player;
        private PlayerControl playerUI;
        private Item currentItem;
        private GameObject itemSprite;
        private bool itemflip;
        public GetFirstItem(Character character, PlayerControl characterUI)
        {
            player = character;
            playerUI = characterUI;
            itemSprite = GameObject.Find("ItemSprite");
        }
        public void Enter()
        {
            player.GetComponent<Animator>().CrossFade("FirstItemGet", 0);
            GameMNG.Sound.Play("Effect/02_Item_string");
            currentItem = playerUI.GetItemName(); //획득한 아이템 정보
            itemSprite.GetComponent<SpriteRenderer>().sprite = currentItem.itemImage;//imageSprite의 Sprite
            itemSprite.GetComponent<SpriteRenderer>().enabled = true;
            if (player.GetComponent<SpriteRenderer>().flipX == true)
            {
                itemSprite.transform.Translate(1.1f, 0, 0);
                itemflip = true;
            }
            player.SetNoBlinkImmune(1f);
        }
        public void Update()
        {

        }
        public void Exit()
        {
            if (itemflip == true)
            {
                itemSprite.transform.Translate(-1.1f, 0, 0);
                itemflip = false;
            }
            itemSprite.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    #endregion
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Hedgehog") //���Ϳ� �浹
        {
            ChangeState(new GetStun(player, playerUI));
        }
        if (collision.gameObject.CompareTag("Item"))
        // 충돌한 오브젝트의 태그가 Item이라면
        {
            nowitem = collision.gameObject.GetComponent<ItemPickUp>().item; // 현재 부딪힌 아이템 갖고옴
            collision.gameObject.GetComponent<AudioSource>().Play();
            // 아이템을 주우면 AcquireItem()을 호출하여 아이템을 인벤토리 슬롯에 업데이트

            Debug.Log(nowitem.itemName + "획득");
            //Destroy(collision.gameObject);
            CheckAcquired();
 

            if (nowitem.itemSaveType == Item.ItemSaveType.canSave) // itemtype이 cansave면
            {
                theInventory.AcquireItem(nowitem); // 인벤토리에 넣기
            }

            playerUI.ShowItem();
        }
        if (collision.gameObject.tag == "Monster" || collision.gameObject.CompareTag("BlackSlime")) //���Ϳ� �浹
        {
            contactPoint = collision.GetContact(0).point;
            if (gameObject.GetComponent<Rigidbody2D>().linearVelocity.y < -0.01f && gameObject.transform.position.y > collision.transform.position.y)
            {
                ChangeState(new MonsterKill(player, playerUI));
            }
            else ChangeState(new GetHit(player, playerUI));
        }
    }
    public void CheckAcquired() // nowitem의 아이템슬랏
    {
        if (!nowitem.IsAcquired) // itemslot[3]이 켜짐?
		{
			theitemlist.AcquireItemList(nowitem);
			ChangeState(new GetFirstItem(player, playerUI));//최초 획득 상태
        }

	}
	private void OnTriggerEnter2D(Collider2D collision)
    {
        /*if (collision.gameObject.tag == "MonsterHead") //���� �Ӹ� ��� ����
        {
            ChangeState(new MonsterKill(player, playerUI));
        }*/
        if (collision.gameObject.tag == "Finish")
        {
            GameMNG.Sound.Play("Effect/07_Stage_Clear 1");
            GameMNG.Scene.LoadCutScene(Define.Scenes.CutScene, currentStage);//현재 스테이지에 해당하는 컷씬 재생 (다음 스태이지 넘어가는 부분임)
        }
        if (collision.gameObject.tag == "CharacterBound")
        {
            if (newSpawnPoint == null)
            {
                newSpawnPoint = GameObject.FindWithTag("Respawn").transform;
            }
            player.transform.position = newSpawnPoint.position;
            player.GetComponent<LIfe>().LifeOut();
            playerUI.LifeChange();
            SetImmune(1, blinkTime);
            if (player.GetComponent<LIfe>().GetLife() == 0)
            {
                player.ChangeState(new Die(player, playerUI));
            }
        }
        if (collision.gameObject.tag == "BoundException")
        {
            player.transform.position = new Vector3(5.5f, -2.5f, 0);
            player.GetComponent<LIfe>().LifeOut();
            playerUI.LifeChange();
            SetImmune(1, blinkTime);
            if (player.GetComponent<LIfe>().GetLife() == 0)
            {
                player.ChangeState(new Die(player, playerUI));
            }
        }
    }
    public void IgnoreInput(float IgnoreTime, Character player, PlayerControl playerUI)
    {
        StartCoroutine(Ignore(IgnoreTime, player, playerUI));
    }
    private IEnumerator Ignore(float IgnoreTime, Character player, PlayerControl playerUI)
    {
        yield return new WaitForSeconds(IgnoreTime);
        ChangeState(new Move(player, playerUI));
    }
    public void SetNoBlinkImmune(float duration)
    {
        player.gameObject.layer = LayerMask.NameToLayer("Immune");
        StartCoroutine(Immune(duration));
    }
    private IEnumerator Immune(float duration)
    {
        yield return new WaitForSeconds(duration);
        player.gameObject.layer = LayerMask.NameToLayer("Character");
        ChangeState(new Idle(player, playerUI));
    }
    public void SetImmune(float duration, float blinkTime)//��������
    {
        StartCoroutine(Blink(duration, blinkTime));
        player.gameObject.layer = LayerMask.NameToLayer("Immune");
    }
    private IEnumerator Blink(float duration, float blinkTime)
    {
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            player.GetComponent<SpriteRenderer>().enabled = !player.GetComponent<SpriteRenderer>().enabled;
            yield return new WaitForSeconds(blinkTime);
            timeElapsed += blinkTime;
        }
        player.GetComponent<SpriteRenderer>().enabled = true;
        player.gameObject.layer = LayerMask.NameToLayer("Character");
    }
    private void FixedUpdate() //�������� �ӵ� ����
    {
        player.GetComponent<Rigidbody2D>().AddForce(Vector3.down * Gravity);
    }
    public PlayerControl GetUI()
    {
        return playerUI;
    }
    private IEnumerator Fade()
    {
        yield return new WaitForSeconds(2f);
        GameMNG.UI.ShowPopupUI<GameOver>();
    }
}
