using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CatGame
{
    public static class PrefsManager
    {
        // Layers are stored in a line by line format:
        /* 
         * Layer 1
         * Layer 2
         * Layer 3
         */
        // The above would mean the player completed all layers
        public static string AvailableLayers
        {
            get => PlayerPrefs.GetString("AvailableLayers", "");
            private set
            {
                PlayerPrefs.SetString("AvailableLayers", value);
                PlayerPrefs.Save();
            }
        }

        public static IEnumerable<string> GetAvailableLayers()
        {
            StringReader reader = new(AvailableLayers);

            string layer = reader.ReadLine();
            while (layer != null)
            {
                yield return layer;
                layer = reader.ReadLine();
            }
        }

        public static string GetHighestAvailableLayer()
            => GetAvailableLayers().OrderByDescending(val => val).FirstOrDefault();

        public static void AddAvailableLayer(string newLayer)
        {
            StringBuilder builder = new();

            foreach (string layer in GetAvailableLayers())
            {
                if (layer != newLayer)
                {
                    builder.Append(layer);
                    builder.AppendLine();
                }
            }

            builder.Append(newLayer);

            AvailableLayers = builder.ToString();
        }

        public static float GetGroupVolume(string group)
        {
            return PlayerPrefs.GetFloat(group, 0.5f);
        }

        public static void SetGroupVolume(string group, float volume)
        {
            PlayerPrefs.SetFloat(group, volume);
            PlayerPrefs.Save();
        }
    }
}
