using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public enum Ferr2D_LoopMode {
    Stop,
    Loop,
    Next
}

[Serializable()]
public class Ferr2D_Frame
{
    public int   index;
    public float duration;
}

[Serializable()]
public class Ferr2D_Animation
{
    public string             name;
    public string             next;
    public float              maxTime;
    public List<Ferr2D_Frame> frames;
    public Ferr2D_LoopMode    loop;

    public Ferr2D_Animation(string aName)
    {
        name = aName;
        next = "";
        frames = new List<Ferr2D_Frame>();
    }

    public int GetFrame(float aTime)
    {
        if (loop == Ferr2D_LoopMode.Loop) aTime = aTime % maxTime;
        float curr = 0;
        for (int i = 0; i < frames.Count; i++)
        {
            curr += frames[i].duration;
            if (aTime < curr) return frames[i].index;
        }
        return frames[frames.Count - 1].index;
    }

    public bool UpdateTime(ref float aTime)
    {
        if (aTime > maxTime)
        {
            if (loop == Ferr2D_LoopMode.Loop) aTime -= maxTime;
            if (loop == Ferr2D_LoopMode.Stop) aTime  = maxTime;
            return true;
        }
        return false;
    }

    public void CalcMax()
    {
        maxTime = 0;
        for (int i = 0; i < frames.Count; i++)
        {
            maxTime += frames[i].duration;
        }
    }
}

/// <summary>
/// This is really just a placeholder class. I recommend making or finding a better one.
/// </summary>
[AddComponentMenu("Ferr2D/Animator"), RequireComponent(typeof(Ferr2D_Sprite))]
public class Ferr2D_Animator : MonoBehaviour {
    public Vector2                         cellSize;
    public List<Ferr2D_Animation>          animations = new List<Ferr2D_Animation>();
    public Action<Ferr2D_Animator, string> onAnimFinish;
    public Vector2 offset;

    Ferr2D_Animation anim;
    float            time;
    int              cellsX;
    
	void Start () {
        cellsX = (int)(1f / cellSize.x);
        for (int i = 0; i < animations.Count; i++) {
            animations[i].CalcMax();
        }
        if (HasAnim("default")) SetAnimation("default");
        if (HasAnim("idle"   )) SetAnimation("idle");
        renderer.material.SetTextureScale("_MainTex", cellSize);
	}
	
	void Update () {
        time += Time.deltaTime;
        if (anim != null) {
            SetFrame(anim.GetFrame(time));
            bool update = anim.UpdateTime(ref time);

            if (update) {
                if (onAnimFinish != null              ) onAnimFinish(this, anim.name);
                if (anim.loop  == Ferr2D_LoopMode.Next) SetAnimation(anim.next);
            }
        }
	}

    public void SetFrame(int aFrame) {
        renderer.material.SetTextureOffset("_MainTex", GetPos(aFrame));
    }
    public void SetAnimation(string aName) {
        aName = aName.ToLower();
        if (anim != null && anim.name == aName) return;
        if (HasAnim(aName)) {
            anim = GetAnim(aName);
            time = 0;
        }
    } 
    public  bool             HasAnim(string aName) {
        for (int i = 0; i < animations.Count; i++) {
            if (animations[i].name == aName) return true;
        }
        return false;
    }
    public  Ferr2D_Animation GetAnim(string aName) {
        for (int i = 0; i < animations.Count; i++) {
            if (animations[i].name == aName) return animations[i];
        }
        return null;
    }
    private Vector2          GetPos (int    i    ) {
        return new Vector2((i / cellsX) * cellSize.x, (i % cellsX) * cellSize.y) + offset;
    }
}
