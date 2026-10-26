public static class GuardIntentParser
{
    public static bool TryParse(string intent, out GuardIntent guardIntent)
    {
        switch (intent)
        {
            case "heal":
                guardIntent = GuardIntent.Heal;
                return true;

            case "harmful":
                guardIntent = GuardIntent.Harmful;
                return true;

            case "unclear":
                guardIntent = GuardIntent.Unclear;
                return true;

            default:
                guardIntent = default;
                return false;
        }
    }
}