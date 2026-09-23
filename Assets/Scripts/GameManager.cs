using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Ready,
        Playing,
        GameOver
    }

    [Header("Game State")]
    public GameState CurrentState = GameState.Ready;
    public int Score = 0;
    public int HighScore = 0;

    [Header("References")]
    public BirdController Bird;
    public PipeSpawner Spawner;
    public GroundScroller Ground;
    public FlappyUI UI;

    private AudioSource audioSource;
    private AudioClip flapClip;
    private AudioClip scoreClip;
    private AudioClip hitClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        HighScore = PlayerPrefs.GetInt("FlappyHighScore", 0);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        GenerateProceduralAudio();
    }

    private void Start()
    {
        CurrentState = GameState.Ready;
        Score = 0;
        if (UI != null)
        {
            UI.UpdateScore(Score);
            UI.ShowReadyScreen();
        }
        if (Bird != null)
        {
            Bird.SetReadyMode();
        }
        if (Spawner != null)
        {
            Spawner.enabled = false;
        }
    }

    public void StartGame()
    {
        if (CurrentState != GameState.Ready) return;

        CurrentState = GameState.Playing;
        Score = 0;

        if (UI != null)
        {
            UI.ShowPlayingScreen();
            UI.UpdateScore(Score);
        }

        if (Bird != null)
        {
            Bird.StartFlying();
        }

        if (Spawner != null)
        {
            Spawner.enabled = true;
            Spawner.ResetSpawner();
        }
    }

    public void AddScore()
    {
        if (CurrentState != GameState.Playing) return;

        Score++;
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt("FlappyHighScore", HighScore);
            PlayerPrefs.Save();
        }

        if (UI != null)
        {
            UI.UpdateScore(Score);
        }

        PlayScoreSound();
    }

    public void TriggerGameOver()
    {
        if (CurrentState == GameState.GameOver) return;

        CurrentState = GameState.GameOver;
        PlayHitSound();

        if (Bird != null)
        {
            Bird.OnDeath();
        }

        if (Spawner != null)
        {
            Spawner.StopSpawning();
        }

        if (Ground != null)
        {
            Ground.StopScrolling();
        }

        if (UI != null)
        {
            UI.ShowGameOverScreen(Score, HighScore);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Procedural Audio Generation
    private void GenerateProceduralAudio()
    {
        flapClip = CreateSynthClip("Flap", 0.12f, (t, dur) =>
        {
            // Upward pitch sweep
            float progress = t / dur;
            float freq = Mathf.Lerp(300f, 650f, progress);
            float env = 1f - progress;
            return Mathf.Sin(2 * Mathf.PI * freq * t) * env * 0.4f;
        });

        scoreClip = CreateSynthClip("Score", 0.25f, (t, dur) =>
        {
            // Two-tone bell ding
            float freq = (t < 0.1f) ? 880f : 1320f;
            float env = Mathf.Exp(-t * 12f);
            return Mathf.Sin(2 * Mathf.PI * freq * t) * env * 0.4f;
        });

        hitClip = CreateSynthClip("Hit", 0.25f, (t, dur) =>
        {
            // Low thud with noise
            float progress = t / dur;
            float freq = Mathf.Lerp(160f, 40f, progress);
            float env = Mathf.Exp(-t * 15f);
            float noise = (Random.value * 2f - 1f) * 0.2f;
            return (Mathf.Sin(2 * Mathf.PI * freq * t) + noise) * env * 0.6f;
        });
    }

    private AudioClip CreateSynthClip(string clipName, float duration, System.Func<float, float, float> generator)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            samples[i] = Mathf.Clamp(generator(t, duration), -1f, 1f);
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void PlayFlapSound()
    {
        if (audioSource != null && flapClip != null)
            audioSource.PlayOneShot(flapClip);
    }

    public void PlayScoreSound()
    {
        if (audioSource != null && scoreClip != null)
            audioSource.PlayOneShot(scoreClip);
    }

    public void PlayHitSound()
    {
        if (audioSource != null && hitClip != null)
            audioSource.PlayOneShot(hitClip);
    }
}
