using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombExplosion : MonoBehaviour
{
    public int speed;
    public float duration;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, 2f);
        //StartCoroutine(MoveToDestination());
    }

    public IEnumerator MoveToDestination(Vector2 destination)
    {
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            transform.position = Vector2.Lerp(transform.position, destination, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = destination;

        /*while (gameObject != null && Vector2.Distance(transform.position, destination) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            yield return null;
        }*/
        //yield return new WaitForSeconds(.2f);
    }
}
