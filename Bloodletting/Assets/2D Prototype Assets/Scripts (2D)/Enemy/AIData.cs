using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class AIData : MonoBehaviour
{
    public List<Transform> targets = null;
    public Collider2D[] obstacles = null;

    public Transform currentTarget = null;

    public int GetTargetsCount() => targets == null ? 0 : targets.Count;
}
