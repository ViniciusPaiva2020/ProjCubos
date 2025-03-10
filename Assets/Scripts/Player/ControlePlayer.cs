using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlePlayer : MonoBehaviour
{
	[Header("Velocidade")]
	[SerializeField] float walkSpd;//velocidade andando
	[SerializeField] float runSpd;//velocidade correndo
	[SerializeField] float currSpd;//velocidade atual
	
	[Header("Pulo")]
	[SerializeField] float jumpForce;//for�a do pulo
	[SerializeField] float maxJumpForce;//for�a do pulo carregado
	[SerializeField] float tapJumpForce;//for�a do tap jump
	[SerializeField] ForceMode jumpFM;//modo da for�a do pulo
	[SerializeField] int currentJumps;//pulos feitos
	[SerializeField] int maxJumps;//n�mero de pulos m�ximo
	[SerializeField] float jumpTimer;
	
	[Header("Inputs")]
	[SerializeField] bool jumping;//input de pulo
	[SerializeField] bool sprintInput;//input de correr
	[SerializeField] float xInput, zInput;//input de movimento
	
	[Header("Outros")]
	//dire��o que a camera est� apontando
	[SerializeField] Transform CamParent;
	[SerializeField] string groundLayerMask;//layer do ch�o
	[SerializeField] float raycastDist;
	
	Rigidbody rigid;
	RaycastHit rayHit;
	
	Vector3 groundLocation;//localiza��o do ch�o
	int groundLM;//layer do ch�o
	[SerializeField] float distFromGround;//dist�ncia entre o player e o ch�o
	[SerializeField] float distToJump;//dist�ncia m�xima at� o ch�o pro player poder pular
	
	bool grounded;//se o player est� no ch�o
	bool jumpStart;//se o player come�ou a pular
	
	[SerializeField] PlayerHealth PH;//script de HP
	
	[SerializeField] Animator anim;
	
	[Header("Audio")]
	[SerializeField] AudioSource AS_walk;
	
	//setta vari�veis
    void Start()
    {
		rigid = GetComponent<Rigidbody>();
		groundLM = LayerMask.GetMask(groundLayerMask);
		
		jumpStart = true;
    }

	//pega inputs e checa se o player est� no ch�o
    void Update()
    {
		//inputs de movimento
		xInput = Input.GetAxis("Horizontal");//movimento horizontal
		zInput = Input.GetAxis("Vertical");//movimento vertical
		jumping = Input.GetButton("Jump");//pulo
		sprintInput = Input.GetButton("Sprint");//se o jogador est� correndo
		
		//if(sprintInput) currSpd = runSpd; else currSpd = walkSpd;
		currSpd = sprintInput ? runSpd : walkSpd;
		
		//checa se o jogador est� pr�ximo do ch�o e sua dist�ncia do ch�o
		if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down),
						  out rayHit, raycastDist, groundLM))
		{
			groundLocation = rayHit.point;
			distFromGround = transform.position.y - groundLocation.y;
		}
		//debug: desenha o Raycast na tela
		Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * raycastDist, Color.blue);
		
		//anima��o e sound effect do player andando
		//andando
		if(!AS_walk.isPlaying && (xInput != 0 || zInput != 0))
		{
			//anima��o
			anim.SetBool("Walk", true);
			
			//sound effect
			AS_walk.Play();
		}
		//parando
		else if(AS_walk.isPlaying && (xInput == 0 && zInput == 0))
		{
			//anima��o
			anim.SetBool("Walk", false);
			
			//sound effect
			AS_walk.Stop();
		}
    }
	
	//movimenta��o
	void FixedUpdate()
	{
		if(!PH.dead)//se o jogador n�o est� morto
		{
			//rotaciona o player de acordo com a camera
			transform.rotation = Quaternion.Euler(transform.rotation.x, 
												  CamParent.rotation.eulerAngles.y - 90, 
												  transform.rotation.z);
			
			//move o player
			rigid.MovePosition(transform.position + Time.deltaTime * currSpd
							   * transform.TransformDirection(xInput, 0, zInput));
			
			//se o player est� no ch�o
			grounded = (distFromGround <= distToJump);//true se distFromGround <= distToJump
			
			//se o player pode pular
			if(jumping && jumpStart && (grounded || (maxJumps > currentJumps)))
			{
				jumpTimer += Time.deltaTime;
				
				if(jumpTimer >= 0.15f)
				{
					jumpTimer = 0;
					
					jumpForce = maxJumpForce;
					
					//inicia o pulo
					StartCoroutine(ApplyJump());
				}
			}
			else if(!jumping && jumpTimer > 0)
			{
				jumpTimer = 0;
				
				jumpForce = tapJumpForce;
				
				//inicia o pulo
				StartCoroutine(ApplyJump());
			}
			
			//se o player est� no ch�o
			if(grounded)
			{
				currentJumps = 0;//reseta os pulos feitos
			}
			
			//anima��es
			anim.SetBool("Grounded", grounded);
			anim.SetFloat("YSpd", rigid.velocity.y);
		}
	}
	
	//aplica a for�a do pulo
	void Jump(float jumpF, ForceMode fMode)
	{
		rigid.AddForce(jumpF * rigid.mass * Time.deltaTime * Vector3.up, fMode);
	}
	
	//pulo
	IEnumerator ApplyJump()
	{
		//anima��o
		anim.SetTrigger("Jump");
		
		//for�a do pulo
		Jump(jumpForce, jumpFM);
		
		//pro if do pulo funcionar
		grounded = false;
		jumpStart = false;
		
		//espera at� o jogador soltar o bot�o de pulo
		yield return new WaitUntil(() => !jumping);
		
		//bugfix pro tapjump gastar um pulo
		int i = 0;
		while(i < 3)
		{
			i++;
			yield return null;
		}
		
		currentJumps++;//adiciona um pulo feito
		jumpStart = true;//pro if do pulo funcionar
		
		StopCoroutine("ApplyJump");
	}
}