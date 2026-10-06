[System.Serializable]
public class GuardResponseSchema
{
    public string type;
    public GuardResponseProperties properties;
    public string[] required;
}

[System.Serializable]
public class GuardResponseProperties
{
    public GuardStringProperty reply;
    public GuardIntentProperty intent;
    public GuardActionProperty action;
}

[System.Serializable]
public class GuardStringProperty
{
    public string type;
}

[System.Serializable]
public class GuardIntentProperty
{
    public string type;
    public string[] @enum;
}

[System.Serializable]
public class GuardActionProperty
{
    public string type;
    public string[] @enum;
}
