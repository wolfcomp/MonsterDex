using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using DeepDungeonDex.Models;
using DeepDungeonDex.Storage;
using ImGuiNET;

namespace DeepDungeonDex.Windows
{
    internal class Debug : Window
    {
        private readonly StorageHandler _storage;
        private readonly Debug _instance;

        public Debug(StorageHandler storage, CommandHandler handler) : base("DeepDungeonDex Debug Window", ImGuiWindowFlags.NoCollapse)
        {
            _storage = storage;
            _instance = this;
            handler.AddCommand("debugwindow", () => _instance.IsOpen = true, show: false);
        }

        public override void Draw()
        {
            ImGui.PushFont(Font.RegularFont);

            ImGui.BeginTable("##JsonList", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Value");
            ImGui.TableHeadersRow();
            foreach (var (key, value) in _storage.JsonStorage)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text(key);
                ImGui.TableNextColumn();
                if (value != null)
                    ImGui.Text(value.GetType().ToString());
            }
            ImGui.EndTable();

            ImGui.BeginTable("##YmlList", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Value");
            ImGui.TableHeadersRow();
            foreach (var (key, value) in _storage.YmlStorage)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text(key);
                ImGui.TableNextColumn();
                switch (value)
                {
                    case Locale locale:
                    {
                        if (locale.TranslationDictionary != null)
                            foreach (var translationDictionaryKey in locale.TranslationDictionary.Keys)
                            {
                                ImGui.Text($"{translationDictionaryKey}");
                            }

                        break;
                    }
                    case Storage.Storage storage:
                        ImGui.Text(storage.Name);
                        switch (storage.Value)
                        {
                            case FloorData floor:
                                if(floor.FloorDictionary != null)
                                    foreach (var floorDictionaryKey in floor.FloorDictionary.Keys)
                                    {
                                        ImGui.Text($"{floorDictionaryKey}");
                                    }
                                break;
                            case MobData mob:
                                if (mob.MobDictionary != null)
                                    foreach (var mobDictionaryKey in mob.MobDictionary.Keys)
                                    {
                                        ImGui.Text($"{mobDictionaryKey}");
                                    }
                                break;
                            case JobData job:
                                if (job.JobDictionary != null)
                                    foreach (var jobDictionaryKey in job.JobDictionary.Keys)
                                    {
                                        ImGui.Text($"{jobDictionaryKey}");
                                    }
                                break;
                        }
                        ImGui.Text("  " + storage.Value.GetType());
                        break;
                }
            }
            ImGui.EndTable();

            ImGui.BeginTable("##LanguageKeyList", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Value");
            ImGui.TableHeadersRow();
            foreach (var (key, value) in _storage.GetInstances<LocaleKeys>().SelectMany(t => t.LocaleDictionary))
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text(key);
                ImGui.TableNextColumn();
                ImGui.Text(value);
            }
            ImGui.EndTable();

            ImGui.PopFont();
        }
    }
}
