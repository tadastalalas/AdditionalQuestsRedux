using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace AdditionalQuestsRedux.Core;

internal static class QuestText
{
    // Sets {TAG.NAME}, {TAG.LINK}, {?TAG.GENDER} etc. on this text and returns it for chaining.
    internal static TextObject WithCharacter(this TextObject text, string tag, CharacterObject character)
    {
        StringHelpers.SetCharacterProperties(tag, character, text);
        return text;
    }
}