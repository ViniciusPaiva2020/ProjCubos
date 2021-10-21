using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlePlayer : MonoBehaviour
{
    public CharacterController controller;

	[SerializeField]
    public float speed = 1f;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

	[SerializeField]
    GameObject CameraDummy;

	[SerializeField]
	Vector3 relative;

    void Start()
    {
                
    }


    public void SetCameraDummy(GameObject dummy)
    {
        CameraDummy = dummy;
    }

    void Update()
    {

        //dire��o do movimento
        Vector3 Movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        //faz a dire��o do movimento ser baseada na rota��o da camera
        if (CameraDummy) Movement = CameraDummy.transform.TransformDirection(Movement);

        // transform the world forward into local space:
        relative = transform.InverseTransformDirection(Vector3.forward);
        Debug.Log(relative);
        


        //moviment���o basica funcionando
        float xDirection = Input.GetAxis("Horizontal");
        float zDirection = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(xDirection, 0.0f, zDirection);

        transform.position += moveDirection * speed;
        
       
        
        
        //movimenta��o basica antiga
		//float horizontal = Input.GetAxisRaw("Horizontal");
        //float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(zDirection, 0f, xDirection),normalized;
        


        if (direction.magnitude >= 0.1f)
        {
            
            // movimenta��o de acordo com a dire��o da camera
            float targetAngle = Mathf.Atan2(zDirection, xDirection) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            controller.Move(direction * speed * Time.deltaTime);
        }
    }
}
