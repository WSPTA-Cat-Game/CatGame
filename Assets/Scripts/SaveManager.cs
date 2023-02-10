using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CatGame
{
    public static class SaveManager
    {
        public static string CompletedLayers
        {
            // TODO: DON'T PUT THIS IN THE FINAL GAME IDIOT
            get => PlayerPrefs.GetString("CompletedLayers", "Layer 1\nLayer 2");
            private set => PlayerPrefs.SetString("CompletedLayers", value);
        }

        public static IEnumerable<string> GetCompletedLayersEnumerator()
        {
            StringReader reader = new(CompletedLayers);

            string layer = reader.ReadLine();
            while (layer != null)
            {
                yield return layer;
                layer = reader.ReadLine();
            }
        }

        public static string GetHighestCompletedLayer()
            => GetCompletedLayersEnumerator().OrderByDescending(val => val).First();

        public static void AddCompletedLayer(string newLayer)
        {
            StringBuilder builder = new();

            foreach (string layer in GetCompletedLayersEnumerator())
            {
                if (layer != newLayer)
                {
                    builder.Append(layer);
                    builder.AppendLine();
                }
            }

            builder.Append(newLayer);

            CompletedLayers = builder.ToString();
        }
    }
}
