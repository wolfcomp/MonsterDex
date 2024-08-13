using System.Numerics;

namespace DeepDungeonDex.Windows;

internal class Debug : Window, IDisposable
{
    private StorageHandler _storage;
    private Debug _instance;
    private Requests _requests;

    public Debug(StorageHandler storage, CommandHandler handler, Requests requests) : base("MonsterDex Debug Window", ImGuiWindowFlags.NoCollapse)
    {
        _storage = storage;
        _instance = this;
        _requests = requests;
        handler.AddCommand("debug_window", () => _instance.IsOpen = true, "Shows all the data loaded into the plugin.");
    }

    public void Dispose()
    {
        _instance = null!;
        _storage = null!;
        _requests = null!;
    }

    public override void Draw()
    {
        using var _ = Font.Font.RegularFont.Push();

        var config = _storage.GetInstance<Configuration>()!;
        if (!config.LoadAll)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
            ImGui.TextUnformatted("All font load is not enabled. Some glyphs will not be available.");
            ImGui.PopStyleColor();
        }

        if (_requests.IsRequesting)
        {
            ImGui.TextUnformatted("Requesting data from Github...");
        }

        ImGui.TextUnformatted("Language list:");
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

        ImGui.TextUnformatted("Data:");
        ImGui.BeginTable("##StorageList", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Type");
        ImGui.TableSetupColumn("Value");
        ImGui.TableHeadersRow();
        foreach (var (key, value) in _storage.Storage.OrderBy(t => t.Value.GetType().Name.GetHashCode()))
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.TextUnformatted(key);
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(value.GetType().Name);
            ImGui.TableNextColumn();
            switch (value)
            {
                case FloorData floor:
                    if (floor.FloorDictionary != null)
                        foreach (var (floorKey, floorValue) in floor.FloorDictionary)
                        {
                            ImGui.TextUnformatted($"{floorKey} mapped to {floorValue}");
                        }
                    break;
                case MobData mob:
                    foreach (var (mobKey, mobValue) in mob.MobDictionary)
                    {
                        ImGui.TextUnformatted($"{mobKey}");
                        ImGui.Indent();
                        ImGui.TextUnformatted($"Name: {mobValue.Name}");
                        ImGui.SameLine(0, 14);
                        ImGui.TextUnformatted($"Aggro: {mobValue.Aggro}");
                        ImGui.SameLine(0, 14);
                        ImGui.TextUnformatted($"Threat: {mobValue.Threat}");
                        ImGui.SameLine(0, 14);
                        ImGui.TextUnformatted($"Weakness: {mobValue.Weakness}");
                        ImGui.SameLine(0, 14);
                        ImGui.TextUnformatted($"ContentType: {mobValue.InstanceContentType}");
                        ImGui.Unindent();
                    }
                    break;
                case Locale locale:
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                    ImGui.TextUnformatted("Only displaying the first 30 characters in translation!");
                    ImGui.PopStyleColor();
                    foreach (var (translationKey, translationValue) in locale.TranslationDictionary)
                    {
                        ImGui.TextUnformatted($"{translationKey}: {translationValue}");
                    }

                    break;
                case Territories territories:
                    foreach (var (name, ids) in territories.TerritoryDictionary)
                    {
                        ImGui.TextUnformatted($"{name}");
                        ImGui.Indent();
                        foreach (var id in ids)
                        {
                            ImGui.TextUnformatted($"{id[0]} - {id[1]}");
                            ImGui.SameLine(0, 14);
                        }
                        ImGui.Unindent();
                        ImGui.NewLine();
                    }

                    break;
            }
        }
        ImGui.EndTable();
    }
}