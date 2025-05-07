using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling;
using Lumina.Excel.Sheets;

namespace Amateur;

public partial class Utils {
    public static SeString TranslateAction(uint actionId, ClientLanguage language)
    {
        var sheet = Service.DataManager.GetExcelSheet<Action>(language);
        if (sheet == null)
        {
            return SeString.Empty;
        }

        var action = sheet.GetRow(actionId);
        if ((object)action == null)
        {
            return SeString.Empty;
        }

        return new SeString().Append(action.Name.ToString());
    }
}

