using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestTexture : MonoBehaviour
{
    public Vector2Int dimension = Vector2Int.one;
    public Texture2D texture;
    public RawImage rawImage;
    public int testCase = 0;

    // Start is called before the first frame update
    void Start()
    {
        texture = new Texture2D(dimension.x, dimension.y);
        //texture = Texture2D.blackTexture;
        dimension.x = texture.width;
        dimension.y = texture.height;

        if (testCase >= 3)
        {
            for (int x = 0; x < dimension.x; x++)
            {
                for (int y = 0; y < dimension.y; y++)
                {
                    UpdatePixel(x, y);
                    if (testCase == 4)
                    {
                        texture.Apply();
                        rawImage.texture = texture;
                    }
                }
                if (testCase == 1) 
                { 
                    texture.Apply();
                    rawImage.texture = texture;

                }
            }
            if (testCase == 5)
            {
                texture.Apply();
                rawImage.texture = texture;

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int x = 0; x < dimension.x; x++)
        {
            for (int y = 0; y < dimension.y; y++)
            {
                UpdatePixel(x, y);
                if (testCase == 0)
                {
                    texture.Apply();
                    rawImage.texture = texture;
                }
            }
            if (testCase == 1) 
            { 
                texture.Apply(); 
                rawImage.texture = texture;
            }
        }
        if (testCase == 2)
        {
            texture.Apply();
            rawImage.texture = texture;
        }
    }

    public void UpdatePixel(int x, int y)
    {
        Color color = texture.GetPixel(x, y);
        color.r+=10;
        color.g+=10;
        color.b += 10;
        texture.SetPixel(x, y, color);
    }
}
