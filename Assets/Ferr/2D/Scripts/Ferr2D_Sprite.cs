using UnityEngine;
using System.Collections;

/// <summary>
/// This is really just a placeholder class. I recommend making or finding a better one.
/// </summary>
[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class Ferr2D_Sprite : MonoBehaviour
{
    public Color   spriteColor   = Color.white;
    public Rect    UV            = new Rect(0,0,1,1);
    public Vector2 meshScale     = new Vector2(1, 1);
    public float   pixelsPerUnit = 32;
    public bool    Edged         = false;
    public float   EdgeSize      = 0.1f;
    
    public float Width {
        get {
            Vector2 ppuScale = new Vector2(1,1);
            if (renderer.sharedMaterial.mainTexture != null)
            {
                ppuScale = new Vector2(
                    renderer.sharedMaterial.mainTexture.width  / pixelsPerUnit,
                    renderer.sharedMaterial.mainTexture.height / pixelsPerUnit);
            }
            return ppuScale.x * meshScale.x * UV.width/2;
        }
    }
    public float Height {
        get {
            Vector2 ppuScale = new Vector2(1,1);
            if (renderer.sharedMaterial.mainTexture != null)
            {
                ppuScale = new Vector2(
                    renderer.sharedMaterial.mainTexture.width  / pixelsPerUnit,
                    renderer.sharedMaterial.mainTexture.height / pixelsPerUnit);
            }
            return ppuScale.y * meshScale.y * (UV.height/2);
        }
    }

	void Start () {
        Rebuild();
	}

    private string GetMeshName()
    {
        return "SpriteMesh" + gameObject.GetInstanceID();
    }
    public  void   Rebuild    ()
    {
        Mesh m = GetComponent<MeshFilter>().sharedMesh;
        string name = GetMeshName();
        if (m == null || m.name != name)
        {
            m = GetComponent<MeshFilter>().sharedMesh = new Mesh();
            m.name = name;
        }
        Vector2 ppuScale = new Vector2(1,1);
        if (renderer.sharedMaterial.mainTexture != null)
        {
            ppuScale = new Vector2(
                renderer.sharedMaterial.mainTexture.width  / pixelsPerUnit,
                renderer.sharedMaterial.mainTexture.height / pixelsPerUnit);
        }
        m.Clear();
        if (!Edged)
        {
            m.vertices = new Vector3[] {
                new Vector3(-UV.width/2 * meshScale.x * ppuScale.x, -UV.height/2 * meshScale.y * ppuScale.y, 0),
                new Vector3( UV.width/2 * meshScale.x * ppuScale.x, -UV.height/2 * meshScale.y * ppuScale.y, 0),
                new Vector3( UV.width/2 * meshScale.x * ppuScale.x,  UV.height/2 * meshScale.y * ppuScale.y, 0),
                new Vector3(-UV.width/2 * meshScale.x * ppuScale.x,  UV.height/2 * meshScale.y * ppuScale.y, 0)
            };
            m.uv = new Vector2[] {
                new Vector2(UV.x,    UV.y),
                new Vector2(UV.xMax, UV.y),
                new Vector2(UV.xMax, UV.yMax),
                new Vector2(UV.x,    UV.yMax)
            };
            m.colors = new Color[] {
                spriteColor,
                spriteColor,
                spriteColor,
                spriteColor
            };
            m.triangles = new int[] {
                2, 1, 0,
                3, 2, 0
            };
        }
        else
        {
            float edgeX = UV.width  * EdgeSize;
            float edgeY = UV.height * EdgeSize;

            m.vertices = new Vector3[] {
                new Vector3((-UV.width/2 * meshScale.x),         (-UV.height/2 * meshScale.y), 0),
                new Vector3((-UV.width/2 * meshScale.x) + edgeX, (-UV.height/2 * meshScale.y), 0),
                new Vector3((-UV.width/2 * meshScale.x) + edgeX, (-UV.height/2 * meshScale.y) + edgeY, 0),
                new Vector3((-UV.width/2 * meshScale.x),         (-UV.height/2 * meshScale.y) + edgeY, 0),

                new Vector3(( UV.width/2 * meshScale.x) - edgeX, (-UV.height/2 * meshScale.y), 0),
                new Vector3(( UV.width/2 * meshScale.x),         (-UV.height/2 * meshScale.y), 0),
                new Vector3(( UV.width/2 * meshScale.x),         (-UV.height/2 * meshScale.y) + edgeY, 0),
                new Vector3(( UV.width/2 * meshScale.x) - edgeX, (-UV.height/2 * meshScale.y) + edgeY, 0),

                new Vector3(( UV.width/2 * meshScale.x) - edgeX, ( UV.height/2 * meshScale.y) - edgeY, 0),
                new Vector3(( UV.width/2 * meshScale.x),         ( UV.height/2 * meshScale.y) - edgeY, 0),
                new Vector3(( UV.width/2 * meshScale.x),         ( UV.height/2 * meshScale.y), 0),
                new Vector3(( UV.width/2 * meshScale.x) - edgeX, ( UV.height/2 * meshScale.y), 0),

                new Vector3((-UV.width/2 * meshScale.x),         ( UV.height/2 * meshScale.y) - edgeY, 0),
                new Vector3((-UV.width/2 * meshScale.x) + edgeX, ( UV.height/2 * meshScale.y) - edgeY, 0),
                new Vector3((-UV.width/2 * meshScale.x) + edgeX, ( UV.height/2 * meshScale.y), 0),
                new Vector3((-UV.width/2 * meshScale.x),         ( UV.height/2 * meshScale.y), 0)
            };
            m.uv = new Vector2[] {
                new Vector2(UV.x,            UV.y),
                new Vector2(UV.x + edgeX,    UV.y),
                new Vector2(UV.x + edgeX,    UV.y + edgeY),
                new Vector2(UV.x,            UV.y + edgeY),

                new Vector2(UV.xMax - edgeX, UV.y),
                new Vector2(UV.xMax,         UV.y),
                new Vector2(UV.xMax,         UV.y + edgeY),
                new Vector2(UV.xMax - edgeX, UV.y + edgeY),

                new Vector2(UV.xMax - edgeX, UV.yMax - edgeY),
                new Vector2(UV.xMax,         UV.yMax - edgeY),
                new Vector2(UV.xMax,         UV.yMax),
                new Vector2(UV.xMax - edgeX, UV.yMax),

                new Vector2(UV.x,            UV.yMax - edgeY),
                new Vector2(UV.x + edgeX,    UV.yMax - edgeY),
                new Vector2(UV.x + edgeX,    UV.yMax),
                new Vector2(UV.x,            UV.yMax)
            };
            m.colors = new Color[] {
                spriteColor,
                spriteColor,
                spriteColor,
                spriteColor,

                spriteColor,
                spriteColor,
                spriteColor,
                spriteColor,

                spriteColor,
                spriteColor,
                spriteColor,
                spriteColor,

                spriteColor,
                spriteColor,
                spriteColor,
                spriteColor
            };
            m.triangles = new int[] {
                2,  1,  0,   3,  2,  0,
                7,  4,  1,   2,  7,  1,
                6,  5,  4,   7,  6,  4,
                9,  6,  7,   8,  9,  7,
                10, 9,  8,   11, 10, 8,
                14, 11, 13,  11, 8,  13,
                15, 14, 12,  14, 13, 12,
                12, 13, 3,   13, 2,  3,
                13, 8,  2,   8,  7,  2
            };
        }
        m.RecalculateNormals();
        m.RecalculateBounds();
    }
}
