using UnityEngine;

public class GroundScroller : MonoBehaviour
{
    public float ScrollSpeed = 2.5f;
    public Transform GroundA;
    public Transform GroundB;
    public float TileWidth = 12f;

    private bool isScrolling = true;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.GameOver)
        {
            return;
        }

        if (!isScrolling) return;

        float moveAmount = ScrollSpeed * Time.deltaTime;

        if (GroundA != null)
        {
            GroundA.position += Vector3.left * moveAmount;
            if (GroundA.position.x <= -TileWidth)
            {
                GroundA.position = new Vector3(GroundB.position.x + TileWidth, GroundA.position.y, GroundA.position.z);
            }
        }

        if (GroundB != null)
        {
            GroundB.position += Vector3.left * moveAmount;
            if (GroundB.position.x <= -TileWidth)
            {
                GroundB.position = new Vector3(GroundA.position.x + TileWidth, GroundB.position.y, GroundB.position.z);
            }
        }
    }

    public void StopScrolling()
    {
        isScrolling = false;
    }

    public void ResumeScrolling()
    {
        isScrolling = true;
    }
}
