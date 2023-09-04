using System.Numerics;

namespace DeepDungeonDex.Windows;

internal class Debug : Window
{
    private readonly StorageHandler _storage;
    private readonly Debug _instance;
    private readonly Requests _requests;

    public Debug(StorageHandler storage, CommandHandler handler, Requests requests) : base("DeepDungeonDex Debug Window", ImGuiWindowFlags.NoCollapse)
    {
        _storage = storage;
        _instance = this;
        _requests = requests;
        handler.AddCommand("debug_window", () => _instance.IsOpen = true, "Shows all the data loaded into the plugin.");
    }

    public override void Draw()
    {
        ImGui.PushFont(Font.RegularFont);

        if (_requests.IsRequesting)
        {
            ImGui.TextUnformatted("Requesting data from Github...");
            goto End;
        }

        ImGui.BeginTable("##JsonList", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Value");
        ImGui.TableHeadersRow();
        foreach (var (key, value) in _storage.JsonStorage)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.TextUnformatted(key);
            ImGui.TableNextColumn();
            if (value != null)
                ImGui.TextUnformatted(value.GetType().ToString());
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
            ImGui.TextUnformatted(key);
            ImGui.TableNextColumn();
            switch (value)
            {
                case Locale locale:
                {
                    if (locale.TranslationDictionary != null)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1,0,0,1));
                        ImGui.TextUnformatted("Only displaying the first 30 characters in translation!");
                        ImGui.PopStyleColor();
                        foreach (var (translationKey, translationValue) in locale.TranslationDictionary)
                        {
                            ImGui.TextUnformatted($"{translationKey}: {translationValue[..Math.Min(30, translationValue.Length)]}");
                        }
                    }

                    break;
                }
                case Storage.Storage storage:
                    ImGui.TextUnformatted(storage.Name);
                    switch (storage.Value)
                    {
                        case FloorData floor:
                            if(floor.FloorDictionary != null)
                                foreach (var (floorKey, floorValue) in floor.FloorDictionary)
                                {
                                    ImGui.TextUnformatted($"{floorKey} mapped to {floorValue}");
                                }
                            break;
                        case MobData mob:
                            if (mob.MobDictionary != null)
                                foreach (var (mobKey, mobValue) in mob.MobDictionary)
                                {
                                    ImGui.TextUnformatted($"{mobKey}");
                                    ImGui.Indent();
                                    ImGui.TextUnformatted($"Name: {mobValue.Name}");
                                    ImGui.TextUnformatted($"Aggro: {mobValue.Aggro}");
                                    ImGui.TextUnformatted($"Threat: {mobValue.Threat}");
                                    ImGui.TextUnformatted($"Weakness: {mobValue.Weakness}");
                                    ImGui.TextUnformatted($"Description:");
                                    ImGui.Indent();
                                    foreach (var description in mobValue.Description)
                                    {
                                        ImGui.TextUnformatted($"{string.Join(" ", description)}");
                                    }
                                    ImGui.Unindent();
                                    ImGui.Unindent();
                                }
                            break;
                        case JobData job:
                            if (job.JobDictionary != null)
                                foreach (var (jobKey, jobValue) in job.JobDictionary)
                                {
                                    ImGui.TextUnformatted($"{jobKey}: {jobValue}");
                                }
                            break;
                    }
                    ImGui.TextUnformatted("  " + storage.Value.GetType());
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
            ImGui.TextUnformatted(key);
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(value);
        }
        ImGui.EndTable();

        End:
        ImGui.PopFont();
    }
}