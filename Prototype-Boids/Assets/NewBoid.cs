﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBoid : MonoBehaviour
{

    public struct BoidData
    {
        public NewBoid boid;
        public NewBoid aligningWithBoidDat;
        public bool isAlphaDat;

        public BoidData(NewBoid b)
        {
            boid = b;
            aligningWithBoidDat = b.BoidImAligningWithRb ? b.BoidImAligningWithRb.GetComponent<NewBoid>() : null;
            isAlphaDat = b.IsAlpha;
        }
    }

    private Collider2D MyTrig;
    private Rigidbody2D MyRb;
    private Rigidbody2D ClosestBoidRb;
    private NewBoid ClosestBoid;
    private Rigidbody2D BoidImAligningWithRb;
    private float MaxAngleCutOff = 7.5f;

    [SerializeField]
    private List<NewBoid> NearBoids = new List<NewBoid>();
    [SerializeField]
    private bool IsAlpha = default; 
    [SerializeField]
    private float moveSpeed = default;
    [SerializeField]
    public float rotSpeed = default;
    [SerializeField]
    public float alignDistance = default, avoidDistance = default;

    [Header("Generated on Start()")]
    [SerializeField]
    private float AngleCutoff = default;


    void Awake()
    {
        MyRb = GetComponent<Rigidbody2D>();
        if (!MyRb) Debug.LogError(gameObject + " has no Rigidbody2D.");
        MyTrig = GetComponent<Collider2D>();
        if (!MyTrig) Debug.LogError(gameObject + " has no Collider2D");
    }

    void Start()
    {
        AngleCutoff = Random.Range(0, MaxAngleCutOff);
    }

    void FixedUpdate()
    {
        // Align
        NewBoid alphaBoid = null;
        float closestAlphaDist = float.MaxValue;

        float closestDist = float.MaxValue;
        // Check for near boids
        for (int i = 0; i < NearBoids.Count; i++)
        {
            float dist = (NearBoids[i].transform.position - transform.position).sqrMagnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                ClosestBoid = NearBoids[i];
            }
            ClosestBoidRb = ClosestBoid != null ? ClosestBoid.GetRigidbody2D() : null;

            BoidData bD = new BoidData(ClosestBoid);
            if (bD.isAlphaDat)
            {
                alphaBoid = ClosestBoid;
                closestAlphaDist = dist < closestAlphaDist ? dist : closestAlphaDist;
            }     
        }
        
        if (ClosestBoid)
        {
            if (ShouldIAvoid(closestDist))
            {
                HandleAvoiding();
            }
            else if (ShouldIAlign(closestDist))
            {
                HandleAligning();
            }   

        }

        MyRb.AddForce(transform.up * moveSpeed * Time.fixedDeltaTime);
    }

    private void HandleAvoiding()
    {
        Vector2 myDir = transform.up;
        Vector2 meToThem = new Vector2(ClosestBoid.transform.position.x - transform.position.x, ClosestBoid.transform.position.y - transform.position.y);
        
        float angl = Vector2.SignedAngle(meToThem, myDir);

        float step = angl * rotSpeed * Time.deltaTime;

        if (Mathf.Abs(angl) > Mathf.Abs(AngleCutoff))
        {
            // if next step will exceed target angle + cutoff
            if (Mathf.Abs(AngleCutoff) + Mathf.Abs(step) > Mathf.Abs(angl))
            {
                MyRb.rotation += angl + AngleCutoff;
            }
            else 
            {
                MyRb.rotation += step;
            }
        }
    }

    private void HandleAligning()
    {
        BoidImAligningWithRb = ClosestBoidRb;
        Vector2 myDir = transform.up;
        Vector2 theirDir = ClosestBoidRb.transform.up;

        float angl = Vector2.SignedAngle(myDir, theirDir);
        
        float step = angl * rotSpeed * Time.fixedDeltaTime;

        if ((step > 0 && step + AngleCutoff > angl) || (step < 0 && step - AngleCutoff < angl ))
        {
            MyRb.rotation += angl;
        }
        else
        {
            MyRb.rotation += step;
        }

        if (Mathf.Abs(angl) > Mathf.Abs(AngleCutoff))
        {
            // if next step will exceed target angle + cutoff
            if (Mathf.Abs(AngleCutoff) + Mathf.Abs(step) > Mathf.Abs(angl))
            {
                MyRb.rotation += angl + AngleCutoff;
            }
            else 
            {
                MyRb.rotation += step;
            }
        }
        
    }

    private void OnTriggerEnter2D (Collider2D coll)
    {
        NewBoid b = coll.GetComponent<NewBoid>();
        if (b && !NearBoids.Contains(b))
        {
            NearBoids.Add(b);
        }
            
    }
    private void OnTriggerExit2D(Collider2D coll)
    {
        NewBoid b = coll.GetComponent<NewBoid>();
        NearBoids.Remove(b);
    }

    private bool ShouldIAvoid(float closestDist)
    {
        bool shouldIAvoid = false;

        shouldIAvoid = closestDist < avoidDistance * avoidDistance;

        return shouldIAvoid;
    }

    private bool ShouldIAlign(float closestDist)
    {
        bool shouldIAlign = false;

        BoidData bD = ClosestBoid.GetBoidData();
        
        shouldIAlign = (!bD.aligningWithBoidDat == MyRb || bD.isAlphaDat ? true : false);
        shouldIAlign = IsAlpha ? false : true;
        shouldIAlign = closestDist < alignDistance * alignDistance;

        return shouldIAlign;
    }

    // Getter for other boids to get the state of boids around them. 
    public BoidData GetBoidData()
    {
        BoidData dat = new BoidData(this);
        return dat;
    }

    public Rigidbody2D GetRigidbody2D()
    {
        return MyRb;
    }
}

   