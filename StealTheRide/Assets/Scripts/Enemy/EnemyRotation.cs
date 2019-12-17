﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyRotation : MonoBehaviour 
{
    //public GameObject player;
    public bool enemyTriggered;
    public float range;
	public float enemySpeed;
    public float stoppingDistance;
    public float retreatDistance;
    public float nextWaypointDistance = 0.3f;

    public Animator animator;
    private GameObject player;
    private Transform playerToFollow;

    public bool EnemyTriggered
    {
        get { return enemyTriggered; }
        set { enemyTriggered = value; }
    }

    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private Seeker seeker;
    private Rigidbody2D rb;
    private Vector2 force;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerToFollow = player.transform;
        animator = GetComponentInChildren<Animator>();

        enemyTriggered = false;
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        InvokeRepeating("UpdatePath", 0f, 0.5f);
        seeker.StartPath(rb.position, playerToFollow.position, OnPathComplete);

        force = new Vector2(0.0f, 0.0f);
    }

    void Update()
    {
        if (PauseMenu.IsPaused)
            return;

        CheckIfEnemySeePlayer();

        if (enemyTriggered == true)
        {
            if (path == null)
            {
                return;
            }

            if (currentWaypoint >= path.vectorPath.Count)
            {
                reachedEndOfPath = true;
                return;
            }
            else
            {
                reachedEndOfPath = false;
            }

            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;

            if (Vector2.Distance(transform.position, playerToFollow.position) < range)
            {
                if (Vector2.Distance(transform.position, playerToFollow.position) > stoppingDistance)
                {
                    //transform.position = Vector2.MoveTowards(transform.position, playerToFollow.position, enemySpeed);
                    animator.SetBool("Movement", true);
                    force = direction * enemySpeed;
                }
                else if (Vector2.Distance(transform.position, playerToFollow.position) < stoppingDistance && Vector2.Distance(transform.position, playerToFollow.position) > retreatDistance)
                {
                    //transform.position = this.transform.position;
                    animator.SetBool("Movement", false);
                    force = direction * 0;
                }
                else if (Vector2.Distance(transform.position, playerToFollow.position) < retreatDistance)
                {
                    //transform.position = Vector2.MoveTowards(transform.position, playerToFollow.position, -enemySpeed);
                    animator.SetBool("Movement", true);
                    force = direction * enemySpeed;
                }
            }
            else if (Vector2.Distance(transform.position, playerToFollow.position) >= range)
            {
                //transform.position = this.transform.position;
                animator.SetBool("Movement", false);
                force = direction * 0;
            }
            animator.SetInteger("Section", CalculateSection());

            rb.AddForce(force);

            float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

            if (distance < nextWaypointDistance)
            {
                currentWaypoint++;
            }
        }

    }
    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }
    void UpdatePath()
    {
        if (seeker.IsDone())
            seeker.StartPath(rb.position, playerToFollow.position, OnPathComplete);
    }
    Vector2 EnemyAfterPlayer(Vector2 direction, Vector2 force)
    {
        if (Vector2.Distance(transform.position, playerToFollow.position) < range)
        {
            if (Vector2.Distance(transform.position, playerToFollow.position) > stoppingDistance)
            {
                force = direction * enemySpeed;
                //transform.position = Vector2.MoveTowards(transform.position, playerToFollow.position, enemySpeed);
                //animator.SetBool("Movement", true);
            }
            else if (Vector2.Distance(transform.position, playerToFollow.position) < stoppingDistance && Vector2.Distance(transform.position, playerToFollow.position) > retreatDistance)
            {
                force = direction * 0;
                //transform.position = this.transform.position;
                //animator.SetBool("Movement", false);
            }
            else if (Vector2.Distance(transform.position, playerToFollow.position) < retreatDistance)
            {
                force = direction * -enemySpeed;
                //transform.position = Vector2.MoveTowards(transform.position, playerToFollow.position, -enemySpeed);
                //animator.SetBool("Movement", true);
            }
        }
        else if (Vector2.Distance(transform.position, playerToFollow.position) >= range)
        {
            force = direction * 0;
            //transform.position = this.transform.position;
            //animator.SetBool("Movement", false);
        }
        return force;
    }
    void CheckIfEnemySeePlayer()
    {
        var heading = playerToFollow.position - transform.position;
        var distance = heading.magnitude*0.5f;
        var direction = (heading / distance);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction);
        //Debug.Log(hit.collider.gameObject);
        Debug.DrawRay(transform.position, direction);

        if (hit.collider != null && hit.collider.gameObject == player)
        {
            enemyTriggered = true;
        }
    }

    void RunAway()
    {

    }

    private float GetRotationAngle()
    {
        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
        currentPosition.x = playerToFollow.position.x - currentPosition.x;
        currentPosition.y = playerToFollow.position.y - currentPosition.y;

        return Mathf.Atan2(currentPosition.y, currentPosition.x) * Mathf.Rad2Deg;
    }

    private int CalculateSection()
    {
        float angle = GetRotationAngle();
        angle = GetRotationAngle();
        if (angle < 0)
            angle = 360 + angle;
        int section = 0; ;
        if (angle >= 330 || angle < 30)
            section = 0;
        else if (angle >= 30 && angle < 90)
            section = 1;
        else if (angle >= 90 && angle < 150)
            section = 2;
        else if (angle >= 150 && angle < 210)
            section = 3;
        else if (angle >= 210 && angle < 270)
            section = 4;
        else if (angle >= 270 && angle < 330)
            section = 5;

        return section;
    }
}