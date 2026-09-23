using UnityEngine;

public class Pipe : MonoBehaviour
{
    public float Speed = 2.5f;
    public float DestroyX = -12f;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        transform.position += Vector3.left * Speed * Time.deltaTime;

        if (transform.position.x < DestroyX)
        {
            Destroy(gameObject);
        }
    }
}
