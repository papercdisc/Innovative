using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

// Mostly AI generated code!!
public class PositionTrackingHUD : MonoBehaviour
{
    [SerializeField] RectTransform trackerPrefab;
    [SerializeField] float edgePadding = 40f;      // px kept between tracker and screen edge
    [SerializeField] bool rotateWhenOffScreen = true; // point an arrow sprite toward the knife

    readonly Dictionary<KnifeProjectile, RectTransform> trackers = new();
    readonly List<KnifeProjectile> toRemove = new();
    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate() // after the camera has moved this frame
    {
        List<KnifeProjectile> knives = PlayerCombatFP.Instance.SpawnedKnives;

        // 1. add trackers for new knives
        foreach (KnifeProjectile knife in knives)
        {
            if (knife != null && !trackers.ContainsKey(knife) && knife.State == KnifeProjState.CanPickup)
                AddTracker(knife);
        }

        // 2. remove trackers whose knife is gone
        toRemove.Clear(); // clear previous toRemove entries.
        foreach (var pair in trackers)
        {
            if (pair.Key == null || !knives.Contains(pair.Key)) // if the knife no longer exists, set to remove tracker-knife pair
                toRemove.Add(pair.Key);
        }
        foreach (KnifeProjectile knife in toRemove) // remove pairs
        {
            Destroy(trackers[knife].gameObject);
            trackers.Remove(knife);
        }

        // 3. position everything
        foreach (var pair in trackers) 
            UpdateTracker(pair.Value, pair.Key.transform.position);
    }

    void AddTracker(KnifeProjectile knife)
    {
        trackers.Add(knife, Instantiate(trackerPrefab, transform));
    }

    void UpdateTracker(RectTransform tracker, Vector3 worldPos)
    {
        Vector3 sp = cam.WorldToScreenPoint(worldPos);
        Vector2 center = new Vector2(Screen.width, Screen.height) * 0.5f;
        Vector2 p = sp; // default position to center of canvas

        // behind the camera: Unity returns a mirrored point, so flip it back through the center
        bool behind = sp.z < 0f;
        if (behind) p = center - (p - center);

        Vector2 dir = p - center;
        Vector2 half = center - Vector2.one * edgePadding;
        bool onScreen = !behind && Mathf.Abs(dir.x) <= half.x && Mathf.Abs(dir.y) <= half.y;

        if (!onScreen)
        {
            if (dir.sqrMagnitude < 0.001f) dir = Vector2.down; // dead-center behind you (point down)
            // scale the direction vector until it touches the padded screen rectangle
            float scale = Mathf.Min(half.x / Mathf.Abs(dir.x), half.y / Mathf.Abs(dir.y));
            p = center + dir * scale;
        }

        tracker.position = p; // in Overlay mode, position is in screen pixels (works with Canvas Scaler)

        if (rotateWhenOffScreen)
        {
            tracker.rotation = onScreen
                ? Quaternion.identity
                : Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f); // -90 assumes the sprite points up
        }
    }
}
