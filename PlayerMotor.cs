namespace Bos.SplitLiner {

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    //using TouchScript;

    public class PlayerMotor : GameBehaviour {

	#if UNITY_IOS
	private const float ACC_VALUE = 125f; 
	#else
	private const float ACC_VALUE = 125f; 
	#endif

	public GameObject playButtonUI;
	public GameObject[] Rockets;
	    
	public bool buttonEnabled = true;
	public bool isPaused = false;
	public Rigidbody rb;

	private Animator animator;

	private CharacterController controller;
	public Vector3 moveVector;

	public bool gameOver = false;
	public bool isRight = false;
	public bool isLeft = true;
	public bool flagR = false;
	public bool flagL = false;

	public bool needBounce = false;

	private float speed = 20.0f;
	public float verticalVelocity = 20f;
	private float gravity = 10f;
	public float horizontalVelocity = 0f;
	private Vector3 horizontalPosition;
	public Vector3 tempPos;

	public bool needCamAcc = false;
	
	public Vector3 cameraRelative;

	public GameObject explosionPrefab;
	public TrailRenderer playerTrail;

	private AudioSource explosionAudio;
	private ParticleSystem explosionParticles;

	private SplitLinerGameManager gm;
	private SplitLinerAudioManager am;
	public Transform cam;

    public GraphicRaycaster graphicRaycaster;

	private float LeftRightXBorder {
		get {
			#if UNITY_IOS
			return 10.0f;
			#else
			return 10.0f;
			#endif
		}
	}

    private bool isOnBase = false;
        private float onBaseSpeed = 20;
        private float onBaseAcceleration = 200;
        private float onBaseTimer = 0;
        private bool isEndCalled = false;

        public bool isLookedAt = false;
        private bool wasTapped = false;

	    private float deccelerationTimer = 1;

	    private void ResetDeccelerationTimer() {
		    deccelerationTimer = 1;
	    }

	    private void SetVertVelocity(float val) {
		    verticalVelocity = val;
		    ResetDeccelerationTimer();
	    }

	public override void Start () {
		
		foreach (var rocket in Rockets)
		{
			rocket.SetActive(false);
		}

		var RocketLevel = Services.SplitService.GetLevel();
		Rockets[RocketLevel - 1].SetActive(true);
		
		controller = GetComponent<CharacterController> ();
		controller.detectCollisions = false;

		animator = GetComponentInParent<Animator> ();
		rb = GetComponent<Rigidbody> ();

		//cam = Camera.main.transform;
		gm = FindObjectOfType<SplitLinerGameManager>();
		am = FindObjectOfType<SplitLinerAudioManager>(); 
		explosionParticles = Instantiate (explosionPrefab).GetComponent<ParticleSystem> ();
		explosionAudio = explosionParticles.GetComponent<AudioSource> ();
		explosionParticles.gameObject.SetActive (false);
		isLookedAt = false;
		
	}

	public override void OnEnable()
	{
		gameOver = false;
	}

    
    private bool IsWasClickOnButton(Vector2 screenPosition) {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = screenPosition;
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerEventData, results);
            foreach(RaycastResult result in results) {
                if(result.gameObject.name == "BaseButton") {
                    return true;
                }
            }
            return false;
        }

	    private bool IsClickOnBase() {
		    if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null) {
			    if (EventSystem.current.IsPointerOverGameObject() &&
			        EventSystem.current.currentSelectedGameObject.name == "BaseButton") {
				    return true;
			    }
		    }
		    if(IsWasClickOnButton(Input.mousePosition)) {
			    return true;
		    }

		    return false;
	    }

    public override void Update () 
	{
		tempPos = transform.position;

        if(isOnBase) {
                animator.SetBool("goRight", false);
                animator.SetBool("goLeft", false);
                animator.SetBool("BounceRight", false);
                animator.SetBool("BounceLeft", false);

                onBaseSpeed += onBaseAcceleration * Time.deltaTime;
                controller.Move(Vector3.up * onBaseSpeed * Time.deltaTime);
                onBaseTimer += Time.deltaTime;
                if(onBaseTimer >= 1.5f) {
                    End(true);
                }
                return;
        }

		am.reloadScore = false;

		if (buttonEnabled == true)
		{
			controller.Move ((Vector3.up * 20f) * Time.deltaTime);
		}
		else
		{
			playButtonUI.SetActive (false);
			Vector3 cameraRelative = cam.InverseTransformPoint (transform.position);
			needCamAcc = (cameraRelative.y > 0);
                //Debug.Log($"position Y relative to cam => {cameraRelative.y}");

            animator.SetBool("goRight", false);
            animator.SetBool("goLeft", false);
            animator.SetBool("BounceRight", false);
            animator.SetBool("BounceLeft", false);

#if UNITY_IOS || UNITY_ANDROID
                if(Input.touchCount > 0 ) {
                    foreach(var touch in Input.touches) {
                        if(touch.phase == TouchPhase.Began) {
                            if (!isOnBase) {
	                            if (IsClickOnBase()) {
		                            return;
	                            }
                                wasTapped = true;
                            }
                        }
                    }
                }
#endif

			if ( (wasTapped || Input.GetMouseButtonDown (0)) && transform.position.x < 0)
			{

				if (!isOnBase) {
					if(IsClickOnBase()) {
						return;	                    
					}
					SetVertVelocity(speed);
					animator.SetBool("goRight", true);

					horizontalVelocity = 50f;
					flagR = true;
					flagL = false;

					isLeft = false;
					isRight = true;
				}
			}
			
			if ( ( wasTapped || Input.GetMouseButtonDown (0)) && transform.position.x > 0)
			{
				if (!isOnBase) {                    
					if(IsClickOnBase()) {
						return;	                    
					}
					SetVertVelocity(speed);
					animator.SetBool("goLeft", true);
					horizontalVelocity = -50f;
					flagL = true;
					flagR = false;

					isLeft = true;
					isRight = false;
				}
			}


			if (needBounce == true)
			{
				if (!isOnBase) {
					horizontalVelocity = -horizontalVelocity;
					if (horizontalVelocity > 0) {
						GoRight();
					} else
					if (horizontalVelocity < 0) {
						GoLeft();
					}
					needBounce = false;
				}
			}


                if (!isOnBase) {

	                deccelerationTimer -= Time.deltaTime;
	                
                    if (Mathf.Approximately(horizontalVelocity, 0) && ( verticalVelocity > 5 || deccelerationTimer < 0) ) {
	                    if (verticalVelocity > 0) {
		                    verticalVelocity -= gravity * 0.5f * Time.deltaTime;
                            
	                    }
	                    else {
                            float mult = 0.05f;
                            if(verticalVelocity < 0 ) {
                                mult = 0.005f;
                            }
		                    verticalVelocity -= gravity * mult * Time.deltaTime;
	                    }
                    }

                    playerTrail.time = Mathf.InverseLerp(0, 20, verticalVelocity) * 5;
                    //print(verticalVelocity);


                   // playerTrail.
	                //playerTrail.gameObject.ToggleActivity(verticalVelocity > 0 );
	
	                /*
                    if (verticalVelocity <= 5) {
                        verticalVelocity = 5;
                    }*/

	                /*
	                if (verticalVelocity <= 0f) {
		                //OnDeath(false);
		                
	                }
	                
	                print(verticalVelocity);
	                print(cameraRelative);*/

                    if (transform.position.x >= 0 && flagR == true) {
                        if (horizontalVelocity <= 0 /* || (horizontalVelocity > 0 && transform.position.x > LeftRightXBorder)*/) {
                            horizontalVelocity = 0;
                            //					tempPos.x = 10;
                            //					transform.position = tempPos;
                            flagR = false;
                        } else {
                            horizontalVelocity -= (ACC_VALUE * Time.deltaTime);
                        }

                    } else
                    if (transform.position.x <= 0 && flagL == true) {
                        if (horizontalVelocity >= 0 /* || (horizontalVelocity < 0 && transform.position.x < -LeftRightXBorder)*/) {
                            horizontalVelocity = 0;
                            flagL = false;
                        } else {
                            horizontalVelocity += (ACC_VALUE * Time.deltaTime);
                        }

                    }

                    if (transform.position.x > LeftRightXBorder) {
						//horizontalVelocity = 0;
                        tempPos.x = LeftRightXBorder;
                        transform.position = Vector3.Lerp(transform.position, tempPos, Time.deltaTime);
                    }

                    if (transform.position.x < -LeftRightXBorder) {
						//horizontalVelocity = 0;
                        tempPos.x = -LeftRightXBorder;
                        transform.position = Vector3.Lerp(transform.position, tempPos, Time.deltaTime);
                    }

                    //x - left & right
                    moveVector.x = horizontalVelocity;



                    //y - up & down
                    moveVector.y = verticalVelocity;

                    //z - Forward & back
                    moveVector.z = 0f;


                    //controller.Move ((Vector3.up * speed ) * Time.deltaTime);
                    controller.Move(moveVector * Time.deltaTime);
                    wasTapped = false;
                }
		}
	}

        


	public void GoRight()
	{
		verticalVelocity = speed;
		animator.SetBool ("BounceRight", true);
		horizontalVelocity = 50f;
		flagR = true;
		flagL = false;

		isLeft = false;
		isRight = true;
	}

	public void GoLeft()
	{
		verticalVelocity = speed;
		animator.SetBool ("BounceLeft", true);

		horizontalVelocity = -50f;
		flagL = true;
		flagR = false;

		isLeft = true;
		isRight = false;
	}

	public void enableDisableButton(bool whatever)
	{
		buttonEnabled = whatever;
            wasTapped = false;
	}


	public void OnDeath(bool isPlayFx = true)
	{
		if (isPlayFx) {
			explosionParticles.transform.position = transform.position;
			explosionParticles.gameObject.SetActive(true);

			explosionParticles.Play();
			explosionAudio.Play();
		}

		End(true);

		/*
		if (am.allowChance == true)
		{
			saveMe.SetActive (true);
			isLookedAt = true;
		}
		else
		{
			gm.EndTheGame ();
		}*/
		
		
			

	}
       

        private void End(bool success) {
            if (!isEndCalled) {
                gameObject.SetActive(false);
                gm.EndTheGame(success);
                isEndCalled = true;
            }
        }

    public void GoOnBase() {
            if(!isOnBase) {
                //StartCoroutine(GoOnBaseImpl());
                animator.SetBool("goRight", false);
                animator.SetBool("goLeft", false);
                animator.SetBool("BounceRight", false);
                animator.SetBool("BounceLeft", false);
                flagL = false;
                flagR = false;
                horizontalVelocity = 0;
                isOnBase = true;
            }
        }

    private IEnumerator GoOnBaseImpl() {
            yield return new WaitUntil(() => (flagL == false) && (flagR == false) && Mathf.Approximately(horizontalVelocity, 0));
            isOnBase = true;
    }

	public void OnSaveMe()
	{
		isLookedAt = false;
		//saveMe.SetActive (false);
	}



}

}