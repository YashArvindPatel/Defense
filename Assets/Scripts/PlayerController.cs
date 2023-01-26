using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour, IDamageHandler
{
    public NavMeshAgent agent;
    public Animator animator;

    public float hitPoints;
    public float currentHP;
    private Transform playerHealth;

    private TurretSpawn turretSpawn;
    private RopeMaker ropeMaker;
    public GameObject CROSS;

    private Camera mainCam;
    private ProceduralGeneration gen;

    void Start()
    {
        gen = FindObjectOfType<ProceduralGeneration>();
        mainCam = Camera.main;
        CalculateCamMinMaxXYZ();
        SetupHealthPoints();
        turretSpawn = FindObjectOfType<TurretSpawn>();
        ropeMaker = FindObjectOfType<RopeMaker>();      
    }

    void SetupHealthPoints()
    {
        currentHP = hitPoints;

        playerHealth = transform.Find("HealthBar/Health");
        ConstraintSource source = new ConstraintSource()
        {
            sourceTransform = mainCam.transform,
            weight = 1
        };
        playerHealth.parent.GetComponent<LookAtConstraint>().SetSource(0, source);
        playerHealth.parent.gameObject.AddComponent<HealthBar>();
        playerHealth.parent.gameObject.SetActive(false);
    }

    Vector3 touchStart;
    public float zoomMin = 15f;
    public float zoomMax = 35f;
    bool movePlayer = false;
    Vector2 firstTouch;
    Vector3 hitPointOnTouch;
    bool turretTouched = false;

    void Update()
    {     
        playerHealth.localScale = new Vector3(currentHP / hitPoints, 1);

        if (Vector3.Distance(transform.position, agent.destination) < agent.stoppingDistance)
        {
            animator.SetBool("Move", false);
        }

        if (Input.GetMouseButtonDown(0))                                               //For testing on PC
        //if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)       //For testing on Mobile
        {
            //Touch touch = Input.GetTouch(0);                                           //For testing on Mobile          

            if (!EventSystem.current.IsPointerOverGameObject())                        //For testing on PC
            //if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))          //For testing on Mobile
            {
                //touchStart = mainCam.ScreenToWorldPoint(touch.position);

                Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);            //For testing on PC
                //Ray ray = mainCam.ScreenPointToRay(touch.position);                //For testing on Mobile

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (turretSpawn.turretSpawnClicked)
                    {
                        //Activate Turret Spawning

                        if (LayerMask.LayerToName(hit.collider.gameObject.layer) != "Ground")
                        {
                            HandleCROSSBehaviour();
                            return;
                        }

                        CROSS.SetActive(false);

                        turretSpawn.TurnOffTurretUI();
                        turretSpawn.turretSpawnClicked = false;
                        turretSpawn.SpawnUI(hit);

                        turretTouched = true;

                        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.WINDOW_BTN);
                    }
                    else if (ropeMaker.makeRope)
                    {
                        //Activate Rope Making

                        if (LayerMask.LayerToName(hit.collider.gameObject.layer) != "Ground")
                        {
                            HandleCROSSBehaviour();
                            return;
                        }

                        CROSS.SetActive(false);

                        ropeMaker.RopeFirstPoint(hit.point);

                        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.WINDOW_BTN);
                    }
                    else
                    {
                        if (turretSpawn.turretSelectUI.activeSelf || turretSpawn.turretStatsUI.activeSelf)
                        {
                            //Turn off all open UI (Current only turret UI)

                            turretSpawn.TurnOffTurretUI();

                            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.REVERSE_WINDOW);
                        }
                        else
                        {
                            if (hit.collider.tag == "Turret")
                            {
                                //Click on spawned Turret to see different options
                                turretTouched = true;

                                turretSpawn.TurretStats(hit.collider.GetComponentInParent<TurretController>());

                                SFX.instance.PlaySingleSoundClip((int)SoundIndexes.WINDOW_BTN);
                            }
                            else
                            {
                                movePlayer = true;
                                //firstTouch = touch.position;
                                hitPointOnTouch = hit.point;
                            }
                        }
                    }
                }
            }
            else
            {
                touchStart = Vector3.zero;
            }               
        }
        else if (Input.GetMouseButtonUp(0))                                              //For testing on PC       
        //else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)    //For testing on Mobile
        {
            //if (movePlayer && Vector2.Distance(firstTouch, Input.GetTouch(0).position) < 1f)
            ////if (movePlayer && Input.GetTouch(0).deltaPosition.magnitude < 0.1f)
            //{
            //    //Move the Player around

            //    agent.SetDestination(hitPointOnTouch);
            //    animator.SetBool("Move", true);

            //    SFX.instance.PlaySingleSoundClip((int)SoundIndexes.MOVE_AROUND);
            //    movePlayer = false;
            //    firstTouch = Vector3.zero;
            //}

            if (ropeMaker.makeRope)
            {
                ropeMaker.RopeSecondPoint(ropeMaker.mousePointer.transform.position);

                SFX.instance.PlaySingleSoundClip((int)SoundIndexes.REVERSE_WINDOW);
            }

            turretTouched = false;
        }

        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            if (!EventSystem.current.IsPointerOverGameObject(touchZero.fingerId) && !EventSystem.current.IsPointerOverGameObject(touchOne.fingerId) && !ropeMaker.makeRope && !turretSpawn.turretSpawnClicked && !turretTouched)
            {
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                float difference = currentMagnitude - prevMagnitude;

                if (turretSpawn.turretSelectUI.activeSelf || turretSpawn.turretStatsUI.activeSelf)
                {
                    //Turn off all open UI (Current only turret UI)

                    turretSpawn.TurnOffTurretUI();

                    SFX.instance.PlaySingleSoundClip((int)SoundIndexes.REVERSE_WINDOW);
                }

                mainCam.orthographicSize = Mathf.Clamp(mainCam.orthographicSize - difference * 0.01f, zoomMin, zoomMax);
                touchStart = Vector3.zero;
            }
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved && touchStart != Vector3.zero && !ropeMaker.makeRope && !turretSpawn.turretSpawnClicked && !turretTouched)
        {
            if (turretSpawn.turretSelectUI.activeSelf || turretSpawn.turretStatsUI.activeSelf)
            {
                //Turn off all open UI (Current only turret UI)

                turretSpawn.TurnOffTurretUI();

                SFX.instance.PlaySingleSoundClip((int)SoundIndexes.REVERSE_WINDOW);
            }

            Vector3 direction = touchStart - mainCam.ScreenToWorldPoint(Input.GetTouch(0).position);
            mainCam.transform.position += direction;
            float num = (mainCam.orthographicSize - 25f) * 2f;
            Vector3 camPos = mainCam.transform.position;
            Vector3 camPosClamped = new Vector3(Mathf.Clamp(camPos.x, minX + num, maxX - num), Mathf.Clamp(camPos.y, minY + num, maxY - num), Mathf.Clamp(camPos.z, minZ + num, maxZ - num));
            mainCam.transform.position = camPosClamped;
        }
    }

    private float minX, minY, minZ;
    private float maxX, maxY, maxZ;

    void CalculateCamMinMaxXYZ()
    {
        Vector3 camPos = mainCam.transform.position;
        minX = camPos.x - (25f * (gen.width - 2));
        maxX = camPos.x + (25f * (gen.width - 2));
        minY = camPos.y - 25f;
        maxY = camPos.y + 25f;
        minZ = camPos.z - (25f * (gen.height - 2));
        maxZ = camPos.z + (25f * (gen.height - 2));
    }

    void HandleCROSSBehaviour()
    {
        //CROSS.transform.position = Input.mousePosition;                   //For testing on PC
        CROSS.transform.position = Input.GetTouch(0).position;            //For testing on Mobile
        CROSS.SetActive(true);
        CROSS.LeanScale(Vector3.one, 1f).setFrom(Vector3.zero).setEaseShake();

        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.ERROR_BTN);
    }

    public void DamageCheck(float amount)
    {
        if (currentHP <= 0)
            return;

        playerHealth.parent.gameObject.SetActive(true);

        currentHP -= amount;
    }
}
