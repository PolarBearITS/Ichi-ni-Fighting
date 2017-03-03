﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Character : MonoBehaviour
{
    //Movement Vars
    private float jumpspeed;
    private float walkspeed;
    private float jumpMult = 1.5f;
    private int jumpsquat = 3;
    private bool canJump = true;
    private string player;
    private int player_num;
    private string other_player;
    private int orientation;
    private string state;
    private float camWidth;

    //Attack Vars
    private string[] states = {"idleR", "idleL",
                               "walkR", "walkL",
                               "jumpR", "jumpL",
                               "squatR", "squatL",
                               "punchGroundR", "punchGroundL",
                               "punchAirR", "punchAirL",
                               "punchSquatR", "punchSquatL",
                               "kickGroundR", "kickGroundL",
                               "kickAirR", "kickAirL",
                               "kickSquatR", "kickSquatL"};

    private PolygonCollider2D[] hurtboxes = { };
    private BoxCollider2D[] hitboxes = { };
    private bool hit = false;
    private bool moveCanHit = true;
    private float health = 100.0f;
    private float healthDrain = 2.5f;
    private GameObject healthBar;

    //Framecounters
    private int[] framecounters = new int[10];
    private int framecounter = 0;
    private int count = 0;

    //Controls
    private KeyCode up = KeyCode.W;
    private KeyCode down = KeyCode.S;
    private KeyCode left = KeyCode.A;
    private KeyCode right = KeyCode.D;
    private KeyCode punch = KeyCode.G;
    private KeyCode kick = KeyCode.H;

    //Moves
    private Move punchGround = new Move(2, 4, 15, 10);
    private Move punchAir = new Move(2, 4, 10, 11);
    private Move punchSquat = new Move(2, 4, 5, 12);
    private Move kickGround = new Move(2, 4, 20, 13);
    private Move kickAir = new Move(2, 4, 15, 14);
    private Move kickSquat = new Move(2, 4, 10, 15);
    private Move currentMove = new Move();

    //General
    Rigidbody2D rb;
    Animator anim;

    public Character()
    {
        walkspeed = 5f;
        jumpspeed = 35f;
        jumpsquat = 3;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        hurtboxes = GetComponents<PolygonCollider2D>();
        hitboxes = GetComponents<BoxCollider2D>();
    }

    public Character(float w, float j, int js, Rigidbody2D r, Animator a, string p, PolygonCollider2D[] hurtB, BoxCollider2D[] hitB)
    {
        walkspeed = w;
        jumpspeed = j;
        jumpsquat = js;
        rb = r;
        anim = a;
        player = p;
        player_num = Convert.ToInt32(player.Trim("player".ToCharArray()));
        other_player = "player" + (3 - player_num);
        hurtboxes = hurtB;
        hitboxes = hitB;
        healthBar = GameObject.Find("healthBar" + player_num);
    }

    public Character(KeyCode[] k, Character c)
    {
        walkspeed = c.walkspeed;
        jumpspeed = c.jumpspeed;
        jumpsquat = c.jumpsquat;
        rb = c.rb;
        anim = c.anim;
        player = c.player;
        player_num = c.player_num;
        other_player = c.other_player;
        hurtboxes = c.hurtboxes;
        hitboxes = c.hitboxes;
        healthBar = c.healthBar;
        
        if (k.Length == 6)
        {
            up = k[0];
            down = k[1];
            left = k[2];
            right = k[3];
            punch = k[4];
            kick = k[5];
        }
    }

    private bool isLow(float f)
    {
        float threshold = 0.005f;
        return Mathf.Abs(f) <= threshold;
    }

    private bool isOnEdge()
    {

        return false;
    }

    public void pollInput()
    {
        if (canJump)
        {
            if (Input.GetKeyDown(up) && Input.GetKey(left))
            {
                state = "jumpLeft";
                canJump = false;
            }
            if (Input.GetKeyDown(up) && Input.GetKey(right))
            {
                state = "jumpRight";
                canJump = false;
            }
            if (Input.GetKeyDown(up) && isLow(rb.velocity[0]))
            {
                state = "jump";
                canJump = false;
            }
        }
        if (Input.GetKeyDown(punch))
        {
            state = "punch";
        }
        if (Input.GetKeyDown(kick))
        {
            state = "kick";
        }
        
        if (GameObject.Find(player).transform.position.x <= GameObject.Find(other_player).transform.position.x)
        {
            orientation = 0;
        }
        else
        {
            orientation = 1;
        }
    }

    public void move()
    {
        //Left Jump
        if (state == "jumpLeft")
        {
            rb.velocity = new Vector2(-jumpMult*walkspeed, jumpspeed);
            anim.SetBool("jump", true);
        }

        //Right Jump
        if (state == "jumpRight")
        {
            rb.velocity = new Vector2(jumpMult*walkspeed, jumpspeed);
            anim.SetBool("jump", true);
        }

        //Vertical Jump
        if (state == "jump")
        {
            rb.velocity = new Vector2(0f, jumpspeed);
            anim.SetBool("jump", true);
        }

        state = "";

        anim.SetInteger("walk", orientation);

        if (canJump && !(anim.GetBool("punch") || anim.GetBool("kick")))
        {
            rb.velocity = new Vector2(0f, 0f);

            //Squat
            if (Input.GetKey(down))
            {
                anim.SetInteger("walk", 4 + anim.GetInteger("walk") % 2);
                anim.SetBool("squat", true);
            }
            else
            {
                anim.SetBool("squat", false);
                //Walk Left
                if (Input.GetKey(left))
                {
                    rb.velocity = new Vector2(-walkspeed, 0f);
                    anim.SetInteger("walk", 2+orientation);
                }

                //Walk Right
                if (Input.GetKey(right))
                {
                    rb.velocity = new Vector2(walkspeed, 0f);
                    anim.SetInteger("walk", 2+orientation);
                }
            }
        }
        
        //Single Jump only
        if (isLow(rb.velocity[1]))
        {
            canJump = true;
            anim.SetBool("jump", false);
        }
    }

    public void waitFrames(int frames)
    {

    }

    public float attack()
    {
        string moveString = "";

        if (state == "punch")
        { 
            anim.SetBool("punch", true);
        }
        if (state == "kick")
        {
            anim.SetBool("kick", true);
        }

        if (anim.GetBool("punch"))
        {
            if (anim.GetBool("squat"))
            {
                currentMove = punchSquat;
            }
            else if (canJump)
            {
                currentMove = punchGround;
            }
            else
            {
                currentMove = punchAir;
                anim.SetBool("landing", false);
            }
            moveString = "punch";
        }
        if (anim.GetBool("kick"))
        {
            if (anim.GetBool("squat"))
            {
                currentMove = kickSquat;
            }
            else if (canJump)
            {
                currentMove = kickGround;
            }
            else
            {
                currentMove = kickAir;
                anim.SetBool("landing", false);
            }
            moveString = "kick";
        }

        if (!anim.GetBool("punch") && !anim.GetBool("kick")) {
            count = framecounter;
            currentMove = new Move();
        }
        else
        {
            if (framecounter - count >= currentMove.Total)
            {
                anim.SetBool(moveString, false);
                anim.SetBool("landing", true);
                state = "";
                GameObject.Find(player).GetComponent<master>().canHit = true;
            }
        }
        return currentMove.Damage;
    }

    public void doDamage()
    {
        if (hit)
        {
            float hitDamage = GameObject.Find(other_player).GetComponent<master>().currentDamage;
            health -= hitDamage;
            if (health < 0) health = 0;
            hit = false;
            GameObject.Find(other_player).GetComponent<master>().canHit = false;
        }
        animateHealthBar(healthBar, health / 100);
    }

    private void animateHealthBar(GameObject h, float size)
    {
        float h_size = h.transform.localScale.x;
        if(h_size > size)
        {
            h_size -= 0.01f * healthDrain;
            if (h_size < size) h_size = size;
            h.transform.localScale = new Vector3(h_size, 1, 1);
        }
    }

    public void boxUpdate()
    {
        for (int i = 0; i < hurtboxes.Length; i++)
        {
            hurtboxes[i].enabled = anim.GetCurrentAnimatorStateInfo(0).IsName(states[i]);
        }
        for(int i = 0; i < hitboxes.Length; i++)
        {   
            hitboxes[i].enabled = anim.GetCurrentAnimatorStateInfo(0).IsName(states[i + 8]);
        }
    }

    public void ishit()
    {
        if (GameObject.Find(other_player).GetComponent<master>().canHit)
        {
            hit = true;
        }
    }

    public void advanceFrame()
    {
        framecounter++;
        state = "";
    }
}