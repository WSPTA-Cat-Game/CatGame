using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CatGame
{
    public static class PrefsManager
    {
        public static string CompletedLayers
        {
            // TODO: DON'T PUT THIS IN THE FINAL GAME IDIOT
            get => PlayerPrefs.GetString("CompletedLayers", "Layer 1\nLayer 2\nLayer 3");
            private set
            {
                PlayerPrefs.SetString("CompletedLayers", value);
                PlayerPrefs.Save();
            }
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

        public static float GetGroupVolume(string group)
        {
            return PlayerPrefs.GetFloat(group, 0);
        }

        public static void SetGroupVolume(string group, float volume)
        {
            PlayerPrefs.SetFloat(group, volume);
        }
    }
}
