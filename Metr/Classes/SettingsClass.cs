using System.Collections.Generic;
using System.Linq;

namespace Metr.Classes
{
    public static class SettingsClass
    {
        public static string status { get; set; }
        public static List<string> prelogin { get; set; } = new List<string>();
        public static List<EClass> EPresets { get; set; } = new List<EClass>();
        static string saveFile = @"./settings.txt";
        static string saveText = "";
        static bool fEx = false;
        public static void FileCheck()
        {
            fEx = System.IO.File.Exists(saveFile);
            if (!fEx)
            {
                var a = System.IO.File.Create(saveFile);
                a.Close();
            }

            saveText = System.IO.File.ReadAllText(saveFile);
            saveText = saveText.Contains('╟') ? saveText : "╟↔" + saveText;
            saveText = saveText.Contains('├') ? saveText : saveText + "├";
        }

        public static void LoadSettings(bool logSave = false)
        {
            FileCheck();


            prelogin.AddRange(saveText.Split('╟')[1][0] == '↔' ? new string[0] : saveText.Split('╟')[1].Split('├')[0].Split('╧'));

            foreach (string p in saveText.Split('├')[1].Split('█'))
            {
                try
                {
                    if (p != "")
                    {
                        EPresets.Add(new EClass()
                        {
                            Name = p.Split('▌')[0],
                            CHeader = p.Split('▌')[1].Split('▀').ToList(),
                            Field = p.Split('▌')[2].Split('▀').Select(int.Parse).ToList(),
                            Settings = p.Split('▌')[3].Split('▀').Select(int.Parse).ToList()
                        });
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        public static void UpdESets()
        {
            FileCheck();
            EPresets.Clear();
            foreach (string p in saveText.Split('├')[1].Split('█'))
            {
                try
                {
                    if (p != "")
                    {
                        EPresets.Add(new EClass()
                        {
                            Name = p.Split('▌')[0],
                            CHeader = p.Split('▌')[1].Split('▀').ToList(),
                            Field = p.Split('▌')[2].Split('▀').Select(int.Parse).ToList(),
                            Settings = p.Split('▌')[3].Split('▀').Select(int.Parse).ToList()
                        });
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        public static void SaveLogin(string LogString)
        {
            FileCheck();
            if (!saveText.Contains(LogString))
            {
                saveText=saveText.Replace("↔", "");
                saveText = saveText.Split('├')[0] + LogString + "╧" + "├" + saveText.Split('├')[1];
                System.IO.File.WriteAllText(saveFile, saveText);
            }
        }

        static string presetText = "";
        public static void SavePreset(EClass preset)
        {
            FileCheck();
            if (preset.Name + "" == "") preset.Name = "Иной";
            if (EClass.Presets.Any(p => p.Name == preset.Name))
                for (int i = 0; i!=-1; i++)
                {
                    if (!EClass.Presets.Any(p => p.Name == preset.Name + i))
                    {
                        presetText = "█" + preset.Name + i + "▌";
                        break;
                    }
                }
            else
                presetText = "█" + preset.Name + "▌";

            foreach (string Header in preset.CHeader) presetText += Header + "▀";
            presetText = presetText.Remove(presetText.Length - 1) + "▌";

            foreach (int Field in preset.Field) presetText += Field + "▀";
            presetText = presetText.Remove(presetText.Length - 1) + "▌";

            foreach (int Settings in preset.Settings) presetText += Settings + "▀";
            presetText = presetText.Remove(presetText.Length - 1) + "▌";

            System.IO.File.WriteAllText(saveFile, saveText+presetText);

            UpdESets();
            EClass.UpdPresets();
        }

        public static void DelPreset(string preset)
        {

            foreach (string p in saveText.Split('├')[1].Split('█'))
            {
                if (preset == p.Split('▌')[0])
                    saveText= saveText.Remove(saveText.IndexOf(p) - 1, p.Length + 1);
            }
            System.IO.File.WriteAllText(saveFile, saveText);

            UpdESets();
            EClass.UpdPresets();
        }
    }
}

