public static class GuardActionParser
{
    public static bool TryParse(string action, out GuardAction guardAction)
    {
        switch(action)
        {
            case "allow_entry":
                guardAction = GuardAction.AllowEntry;
                return true;

            case "request_more_info":
                guardAction= GuardAction.RequestMoreInfo;
                return true;

            case "deny":
                guardAction = GuardAction.Deny;
                return true;

            default:
                guardAction = default;
                return false;
              
        }
    }
}