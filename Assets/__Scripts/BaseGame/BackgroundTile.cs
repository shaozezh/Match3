using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundTile : MonoBehaviour
{
    public int hitPoints;
    public Sprite breakPhase1Image;
    public Sprite breakPhase2Image;
    private SpriteRenderer sprite;
    private int totalHealth;
    public Animator anim;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        totalHealth = hitPoints;
    }

    public void TakeDamage(int damage)
    {
        hitPoints -= damage;
        anim.SetInteger("Damage", hitPoints);
        //StartCoroutine(ChangeSprite());
        //Debug.Log(totalHealth - hitPoints + " damage");
    }

    private IEnumerator ChangeSprite()
    {
        yield return new WaitForSeconds(.1f);
        if (hitPoints <= 2  * totalHealth / 3)
        {
            sprite.sprite = breakPhase2Image;
        }
        if (hitPoints <= totalHealth / 3)
        {
            sprite.sprite = breakPhase1Image;
        }
    }

    public bool CheckHealth()
    {
        return hitPoints <= 0;
    }

}
