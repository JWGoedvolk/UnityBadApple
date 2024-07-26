using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace JW.BadApple.Images.Processor
{
    /// <summary>
    /// Process the images
    /// </summary>
    public class ImageProcessor : MonoBehaviour
    {
        #region Variables
        [Header("Images")]
        [SerializeField] private List<Texture2D> texture = new List<Texture2D>();
        [SerializeField] private Texture2D currentTexture;
        [SerializeField] private Texture2D processed;
        [SerializeField] private RawImage processedCanvas;
        [SerializeField] private RawImage frameCanvas;
        [SerializeField] private int currentIndex = 0;
        [SerializeField] private Vector2Int dimension = new Vector2Int(0, 0);
        [SerializeField] private Vector2Int pixelPos = new Vector2Int(0, 0);
        [SerializeField] private RectTransform preview;
        [SerializeField] private Vector3 previewScale = Vector3.one;

        [Header("Text")]
        [SerializeField] private TMP_FontAsset fontAsset;
        [SerializeField] private string text = "";
        [SerializeField] private List<string> texts = new List<string>() { };
        [SerializeField] private TextDisplayer textDisplayer;
        [SerializeField] private int charWidth = 0;
        [SerializeField] private int charHeight = 0;
        [SerializeField] private List<float> lumiMin = new List<float>(); // Minimum bound for luminance for each character
        [SerializeField] private List<float> lumiMax = new List<float>(); // Maximum bound for luminance for each character
        [SerializeField] private List<char> charSet = new List<char>(); // Characters to use when converting

        [Header("Audio")]
        [SerializeField] private AudioClip music;
        [SerializeField] private AudioSource musicSource;

        [Header("Misc")]
        [SerializeField] private bool isDone = false;
        [SerializeField] private bool usingRuntime = false;
        [SerializeField] private bool autoAdvancing = false;
        [SerializeField] private bool createPreview = false;
        [SerializeField] private bool framePreview = false;
        [SerializeField] private int multiProcess = 1;
        [SerializeField] private bool audioPlaying = false;
        [SerializeField] private int fps = -1;
        #endregion

        #region Public Functions
        public bool NextFrame()
        {
            currentIndex += 1;
            pixelPos.x = 0;

            // Get the texture of the current frame, otherwise make the text green.
            if (texture[currentIndex] != null)
            {
                currentTexture = texture[currentIndex];
            }
            else
            {
                textDisplayer.ShowDone();
            }
            if (currentIndex < texture.Count)
            {
                dimension.x = currentTexture.width;
                dimension.y = currentTexture.height;

                // Reset necesary variables
                isDone = false;
                pixelPos.y = dimension.y - charHeight;

                // Make the new frame
                if (framePreview)
                {
                    NewFrame();
                }

                // Return successfull
                return true;
            }
            else
            {
                currentIndex = 0;
                isDone = true;
                //Debug.Log(Time.time);
                // Return unsuccesfull
                return false;
            }
        }
        #endregion

        #region Unity Specific Functions
        // Start is called before the first frame update
        void Start()
        {
            Application.targetFrameRate = fps;

            currentIndex = -1;
            NextFrame();
            //musicSource.clip = music;
            if (audioPlaying) { musicSource.Play(); }
            // Can run the program at runtime or 1 character every frame
            if (usingRuntime)
            {
                do
                {
                    do
                    {
                        // Convert the area into grayscale, get the brightness, and add the corresponding character to the string
                        Color averageColor = GetAreaAverage(pixelPos.x, pixelPos.y, charWidth, charHeight);
                        ProcessArea(averageColor);

                        
                        // Update grayscaled texture
                        processed.SetPixels(pixelPos.x, pixelPos.y, charWidth, charHeight, FillColorAray(averageColor, charWidth, charHeight));
                        processed.Apply();
                    } while (!MovePixelPos(charWidth, charHeight)); // Increment pixel position to the next character area until we reached the end

                    // Add the finished text to the list and update the UI
                    texts.Add(text);
                    //textDisplayer.UpdateTextUI(text);
                    text = ""; // Clear the string for the next frame to be processed

                    
                } while (NextFrame());
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            

            if (!isDone) // We stop the program when we are done
            {
                for (int iter = 0; iter < multiProcess; iter++)
                {
                    // Convert the area into grayscale, get the brightness, and add the corresponding character to the string
                    Color averageColor = GetAreaAverage(pixelPos.x, pixelPos.y, charWidth, charHeight);
                    ProcessArea(averageColor);

                    
                    if (createPreview)
                    {
                        // Update grayscaled texture
                        processed.SetPixels(pixelPos.x, pixelPos.y, charWidth, charHeight, FillColorAray(averageColor, charWidth, charHeight));
                        processed.Apply();
                    }
                    

                    // Move the pixel position to the next point and check if we are done with the frame
                    isDone = MovePixelPos(charWidth, charHeight);
                }


            }
            else // We are done
            {
                textDisplayer.UpdateTextUI(texts[currentIndex]);

                if (currentIndex < texts.Count)
                {
                    currentIndex++;
                }

                if (!audioPlaying)
                {
                    musicSource.Play();

                    audioPlaying = true;
                }
            }

        }
        #endregion

        #region Private Functions
        private void NewFrame()
        {
            // Display info about the frame
            frameCanvas.texture = texture[currentIndex];
            string name = texture[currentIndex].name;
            //Debug.Log($"{name}: Width: {dimension.x}, Height: {dimension.y}");

            // Reset pixel position and preview position
            pixelPos.x = 0;
            pixelPos.y = (dimension.y - 1) - charHeight;
            UpdatePreview(pixelPos.x * previewScale.x, pixelPos.y * previewScale.y);

            if (createPreview)
            {
                // Creating an imagde to show progress
                processed = new Texture2D(dimension.x, dimension.y);
                processedCanvas.texture = processed;
            }
        }

        private bool MovePixelPos(int width, int height)
        {
            bool isFinished = false;
            //textDisplayer.UpdateTextUI(text);

            pixelPos.x += width;
            //UpdatePreview((pixelPos.x - charWidth) * previewScale.x, pixelPos.y * previewScale.y);
            if (pixelPos.x >= dimension.x - charWidth) // Did we complete the line
            {
                // Calculate new positions
                pixelPos.y -= height;
                pixelPos.x = 0;

                text = text + "\n"; // Go to the next line in the string
                //textDisplayer.UpdateTextUI(text);
                //UpdatePreview(pixelPos.x * previewScale.x, pixelPos.y * previewScale.y);

                // Stop when we reach the bottom
                if (pixelPos.y < 0)
                {
                    // Add the text string to the list of converted strings and clear it for the next image
                    textDisplayer.UpdateTextUI(text);
                    texts.Add(text);
                    textDisplayer.UpdateTextUI(text);
                    text = "";

                    //isFinished = true; // Set the done flag to true
                    if (autoAdvancing)
                    {
                        isFinished = !NextFrame();
                    }
                    else
                    {
                        isFinished = true;
                    }
                }
            }

            return isFinished;
        }

        private void ProcessArea(Color color)
        {
            // Get the brightness and get the character corresponding to it
            float luminance = CalculateLuminance(color);
            char areaChar = GetChar(luminance);

            // Add it to the string
            text = text + areaChar;
            //Debug.Log($"({pixelPos.x}, {pixelPos.y}): {luminance} = {areaChar}");
            //Debug.LogWarning($"|{text}|");
        }

        private Color GetAreaAverage(int x, int y, int cw, int ch)
        {
            // Get the pixels in the are
            Color[] pixels = currentTexture.GetPixels(x, y, cw, ch);

            // Go through the pixels and sum up each color individualy
            Vector3 average = Vector3.zero;
            foreach (var item in pixels)
            {
                average.x += item.r;
                average.y += item.g;
                average.z += item.b;
            }

            // Calculate the average
            average.x /= charWidth * charHeight;
            average.y /= charWidth * charHeight;
            average.z /= charWidth * charHeight;
            float averageSingle = (average.x + average.y + average.z) / 3;
            
            // Make the new average Color to return
            Color averageColor = new Color(averageSingle, averageSingle, averageSingle);

            return averageColor;
        }

        private float CalculateLuminance(Color color)
        {
            return (0.2126f * color.r) + (0.7152f * color.g) + (0.0722f * color.b); // Calculate and return the brightness of the color
        }

        private char GetChar(float luminance)
        {
            for (int i = 0; i < charSet.Count; i++) // Go through each character
            {
                if (luminance >= lumiMin[i] && luminance <= lumiMax[i]) // Check if the luminance is within that character's range
                {
                    return charSet[i]; // Return that character
                }
            }
            return charSet[0]; // No character found so return the first one, which should be the darkest
        }

        private void UpdatePreview(float x, float y, float z = 0f)
        {
            preview.localPosition = new Vector3(x, y, 0);
        }

        private Color[] FillColorAray(Color color, int width, int height)
        {
            // Create an array with the size of our area
            Color[] colors = new Color[width * height];

            // Go through each element and set its color
            for (int x = 0; x < colors.Length; x++)
            {
                colors[x] = color;
            }

            // Return the filled array
            return colors;
        }
        #endregion
    }
}
