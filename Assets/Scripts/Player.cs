using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    void FixedUpdate()
    {
        Vector2 moveDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        MoveTo(moveDir);

        if (Input.GetButtonDown("Fire1"))
        {
            Attack();
        }
    }
}
