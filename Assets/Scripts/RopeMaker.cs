using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeMaker : MonoBehaviour
{
    public GameObject ropeEnd1, ropeEnd2;
    public RopeBridge rope;
    public bool makeRope;
    public Camera cam;
    public Transform mousePointer;
    public Vector3 firstP, secondP;
    public TurretSpawn turretSpawn;
    public bool firstPointSet = false;
    public GameObject lightningParticle;

    public void Start()
    {
        cam = Camera.main;
        turretSpawn = FindObjectOfType<TurretSpawn>();
    }

    public void StartRope()
    {
        makeRope = true;

        if (turretSpawn.turretSpawnClicked)
            turretSpawn.turretSpawnClicked = false;
    }

    public void RopeFirstPoint(Vector3 firstPoint)
    {
        if (GoldManager.instance.GoldCount - 100 >= 0)
        {
            GoldManager.instance.UpdateGoldCount(-100);

            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.WINDOW_BTN);
        }
        else
        {
            //Indicate player no gold left to use ability
            TextPopup.instance.GeneratePopup("Not enough coins to purchase rope");

            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.ERROR_BTN);

            makeRope = false;

            return;
        }

        firstP = firstPoint;
        GameObject ropeEnd = Instantiate(ropeEnd1, new Vector3(firstPoint.x, 3f, firstPoint.z), Quaternion.identity);
        rope = ropeEnd.GetComponent<RopeBridge>();
        rope.firstPoint = ropeEnd.transform;
        rope.secondPoint = mousePointer;
        firstPointSet = true;
    }

    public void RopeSecondPoint(Vector3 secondPoint)
    {
        if (!firstPointSet)
            return;

        secondP = secondPoint;
        GameObject ropeEnd = Instantiate(ropeEnd2, new Vector3(secondPoint.x, 3f, secondPoint.z), Quaternion.identity);
        ropeEnd.transform.LookAt(firstP);
        ropeEnd.transform.localEulerAngles = new Vector3(0, ropeEnd.transform.localEulerAngles.y - 90, 0);
        rope.secondPoint = ropeEnd.transform;
        rope.turnOffNow = true;
        makeRope = false;
        BoxCollider collider = ropeEnd.AddComponent<BoxCollider>();
        collider.center = new Vector3(Vector3.Distance(firstP, secondP) / 10, -0.35f, 0);
        collider.size = new Vector3(Vector3.Distance(firstP, secondP) / 5, 1f, 0.01f);
        collider.isTrigger = true;
        Barrier barrier = ropeEnd.AddComponent<Barrier>();
        barrier.lightningParticle = lightningParticle;
        
        firstPointSet = false;

        if (Tutorial.tutorialOn)
            FindObjectOfType<Tutorial>().PlayGameTutorial(5);
    }

    void Update()
    {
        if (makeRope)
        {
            //Ray ray = cam.ScreenPointToRay(Input.mousePosition);                   //For testing on PC
            Ray ray = cam.ScreenPointToRay(Input.GetTouch(0).position);                   //For testing on Mobile

            if (Physics.Raycast(ray,out RaycastHit hit))
            {
                mousePointer.position = hit.point;
            }
        }
    }
}
