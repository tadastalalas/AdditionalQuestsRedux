namespace AdditionalQuestsRedux.Core;

// Vanilla conversation tokens and menu ids the quests hook into.
internal static class DialogTokens
{
    internal const string IssueClassicQuestStart = "issue_classic_quest_start";
    internal const string QuestDiscuss = "quest_discuss";
    internal const string Start = "start";

    // Vanilla default is 100; quest-character lines must win over generic "start" lines.
    internal const int QuestCharacterPriority = 125;
}
