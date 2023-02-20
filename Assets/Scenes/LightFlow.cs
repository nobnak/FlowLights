using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class LightFlow : MonoBehaviour {

    public Links links = new Links();
    public Tuner tuner = new Tuner();

    protected Random rand;
    protected List<Light> lights = new List<Light>();

    #region unity
    private void OnEnable() {
        rand = Random.CreateFromIndex((uint)GetInstanceID());

        links.plight.gameObject.SetActive(false);

        var bounds = links.room.bounds;

        for (var i = 0; i < tuner.light_count; i++) {
            var l = Instantiate(links.plight);
            l.transform.SetParent(transform, false);
            l.gameObject.hideFlags = HideFlags.DontSave;

            l.transform.position = bounds.Sample();

            lights.Add(l);
            l.gameObject.SetActive(true);
        }
    }
    private void OnDisable() {
        System.Action<Object> dest = (Application.isPlaying ? Object.Destroy : Object.DestroyImmediate);
        foreach (var l in lights) {
            dest(l.gameObject);
        }
        lights.Clear();
    }
    private void Update() {
        var bounds = links.room.bounds;
        var idir = (int)tuner.dir;
        var dt = Time.deltaTime;

        foreach (var l in lights) {
            var tr = l.transform;
            float3 pos = tr.position;
            float3 bsize = bounds.size;
            float3 bmin = bounds.min;

            pos[idir] += dt * tuner.speed;

            if (!bounds.Contains(pos)) {
                pos = math.fmod(pos + bsize - bmin, bsize) + bmin;
            }

            tr.position = pos;
        }
    }
    #endregion

    #region declarations
    public enum DirectionMode {
        X = 0,
        Y = 1,
        Z = 2,
    }

    [System.Serializable]
    public class Links {
        public Light plight;
        public Collider room;
    }
    [System.Serializable]
    public class Tuner {
        public int light_count = 1;
        public float speed = 1f;
        public DirectionMode dir = DirectionMode.X;
    }
    #endregion
}

public static class BoundsExtension {

    static Random rand;

    static BoundsExtension() {
        rand = Random.CreateFromIndex(31);
    }

    public static float3 Sample(this Bounds b) {
        float3 min = b.min;
        float3 size = b.size;
        return min + size * rand.NextFloat3();
    }
}
