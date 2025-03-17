using Unity.Netcode;
using UnityEngine;

public enum AudioClipID
{
    GunShoot,
    GunPew,
    GunReload,
    Footsteps,
    Jump,
    CollectCheese,
    TakeDamage,
    MouseDeath,
    GameOver
}

public class NetworkAudioManager : NetworkBehaviour
{
    // Singleton reference (optional, but convenient for global audio access).
    public static NetworkAudioManager Instance { get; private set; }

    [Header("Audio Source(s)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips (indexed by AudioClipID)")]
    [SerializeField] private AudioClip gunShootClip;
    [SerializeField] private AudioClip gunPewClip;
    [SerializeField] private AudioClip gunReloadClip;
    [SerializeField] private AudioClip footstepsClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip collectCheeseClip;
    [SerializeField] private AudioClip takeDamageClip;
    [SerializeField] private AudioClip mouseDeathClip;
    [SerializeField] private AudioClip gameOverClip;

    private AudioClip[] clipArray;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Put clips into an array so we can refer to them by an integer ID or enum.
        clipArray = new AudioClip[]
        {
            gunShootClip,
            gunPewClip,
            gunReloadClip,
            footstepsClip,
            jumpClip,
            collectCheeseClip,
            takeDamageClip,
            mouseDeathClip,
            gameOverClip
        };
    }

    /// <summary>
    /// Called from anywhere in your code. Tells the server to broadcast the clip to all clients.
    /// </summary>
    public void PlaySoundGlobal(AudioClipID clipID)
    {
        Debug.Log("PlayGlobalLocal called with clipID: " + clipID);
        // If we’re the server, play it immediately for everyone.
        if (IsServer)
        {
            PlaySoundClientRpc((int)clipID);
        }
        else
        {
            // If we're not the server, send an RPC to the server,
            // which will then call the ClientRpc for everyone.
            RequestPlaySoundServerRpc((int)clipID);
        }
    }

    /// <summary>
    /// Called if you only want to play a sound on this local client.
    /// Useful for local-only SFX like UI clicks, or your own footsteps if you prefer local-only handling.
    /// </summary>
    public void PlaySoundLocal(AudioClipID clipID)
    {
        Debug.Log("PlaySoundLocal called with clipID: " + clipID);
        var clip = clipArray[(int)clipID];
        if (clip && sfxSource)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPlaySoundServerRpc(int clipIndex)
    {
        // Validate clip index if needed
        PlaySoundClientRpc(clipIndex);
    }

    [ClientRpc]
    private void PlaySoundClientRpc(int clipIndex)
    {
        if (clipIndex < 0 || clipIndex >= clipArray.Length)
            return;

        var clip = clipArray[clipIndex];
        if (clip && sfxSource)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
