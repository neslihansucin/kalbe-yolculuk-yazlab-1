public class GuardResponseValidator
{
    public bool IsValid(
        GuardLlmResponse response,
        bool hasMemoryOrb)
    {
        if (response == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(response.reply) ||
            string.IsNullOrWhiteSpace(response.intent) ||
            string.IsNullOrWhiteSpace(response.action))
        {
            return false;
        }

        switch (response.intent)
        {
            case "harmful":
                return response.action == "deny";

            case "unclear":
                return response.action == "request_more_info";

            case "heal":
                if (hasMemoryOrb)
                {
                    return response.action == "allow_entry";
                }

                return response.action == "request_more_info";

            default:
                return false;
        }
    }
}