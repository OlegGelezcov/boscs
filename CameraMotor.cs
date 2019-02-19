namespace Bos.SplitLiner {
    
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CameraMotor : MonoBehaviour 
    {
        private CharacterController controller;
        public float speed = 10f;
        public PlayerMotor motor;
        private Transform lookAt;
        private Vector3 startOffset;
        private Vector3 moveVector;


        public bool needAcc;
        public Vector3 tempPos;

        private void Start () 
        {
            controller = GetComponent<CharacterController> ();
            lookAt = GameObject.FindGameObjectWithTag ("Player").transform;
            startOffset = transform.position - lookAt.position;

            needAcc = motor.needCamAcc;

        }
	

        private void Update () 
        {

            if (motor.buttonEnabled == true)
            {
                transform.position = lookAt.position + startOffset;
            }
            else
            {
                if (!lookAt)
                {
                    return;
                }
                tempPos = transform.position;

                
                if (motor.needCamAcc == true)
                {
                   // print("accumulate camera");
                    tempPos.y = lookAt.position.y;
                    this.transform.position = Vector3.Lerp (this.transform.position, tempPos, Time.deltaTime);
                }

                controller.Move ((Vector3.up * speed) * Time.deltaTime);

                Vector3 cameraRotation = transform.eulerAngles;
                if (!Mathf.Approximately(cameraRotation.z, 0)) {
                    transform.eulerAngles = Vector3.Lerp(cameraRotation, Vector3.zero, 10 * Time.deltaTime);
                    
                }else {
                    transform.eulerAngles = new Vector3 (0, 0, 0);
                }

                
            }
        }
    }


}